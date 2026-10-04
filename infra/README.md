# Azure リソース

`main.bicep` はサブスクリプションに Resource Group を作成し、`resources.bicep` がその中に以下を作成します。

| 用途 | App Service Plan | Web App | デプロイ元 |
| --- | --- | --- | --- |
| 本番 | Linux B1 | `lifeplan-prod-<固定サフィックス>` | `master` |

サフィックスはサブスクリプション ID と Resource Group 名から決まるため、同じ設定で再実行しても名前は変わりません。Web App 名が既に他の利用者に取得されている場合は、`main.bicepparam` の `resourceNamePrefix` を変更してください。

本番 Web App は HTTPS を必須にし、FTP と SCM の基本認証を無効にします。自動デプロイでは Azure の OIDC 認証を利用します（[自動デプロイ](#自動デプロイ) を参照）。

本番の B1 App Service Plan は稼働中、アクセスがなくても課金されます。初期状態では Azure の既定 URL のみ使用し、独自ドメインや DNS は設定しません。

## 初回作成

Azure CLI をインストールし、リポジトリのルートから実行します。Bicep CLI は `az bicep build` の実行時に必要なら自動でインストールされます。対象のサブスクリプションを確認し、`what-if` の内容を確認してから `create` を実行してください。

```powershell
az login
az account list --output table
az account set --subscription '<subscription-id>'
az account show --output table
az bicep build --file infra/main.bicep --stdout
az bicep build-params --file infra/main.bicepparam --stdout
az deployment sub what-if --location japaneast --parameters infra/main.bicepparam
az deployment sub create --name main --location japaneast --parameters infra/main.bicepparam
```

テンプレートは App Service のアプリ設定を管理しません。作成後、本番 Web App の環境変数へ microCMS と SMTP の設定を入力します。本番アプリでは `ASPNETCORE_ENVIRONMENT=Production` を設定してください。API キーやパスワードをこのリポジトリへ保存しないでください。

動作確認はローカルで行い、microCMS と SMTP の設定はユーザーシークレットで管理します。

## 自動デプロイ

`.github/workflows/ci.yml` はビルド、テスト、Bicep の構文検証を行います。`.github/workflows/cd.yml` は `master` への push（または手動実行）でビルド、テスト、発行を行い、本番 Web App へデプロイします。

SCM の基本認証を無効にしているため、発行プロファイルは使わず、GitHub Actions の OIDC で Azure にログインします。初回のみ以下を設定してください。

```powershell
$subscriptionId = az account show --query id --output tsv
$tenantId = az account show --query tenantId --output tsv
$appName = az deployment sub show --name main --query properties.outputs.productionAppName.value --output tsv
$webAppId = az webapp show --resource-group rg-lifeplan-jpe --name $appName --query id --output tsv

# デプロイ用のアプリ登録とサービスプリンシパル
$clientId = az ad app create --display-name 'github-lifeplan-deploy' --query appId --output tsv
az ad sp create --id $clientId

# GitHub の production 環境からのトークンだけを信頼する
@'
{
  "name": "github-lifeplan-production",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:<owner>/<repo>:environment:production",
  "audiences": ["api://AzureADTokenExchange"]
}
'@ | Out-File -Encoding utf8 federated-credential.json
az ad app federated-credential create --id $clientId --parameters federated-credential.json
Remove-Item federated-credential.json

# 権限は本番 Web App のみに限定する
az role assignment create --assignee $clientId --role 'Website Contributor' --scope $webAppId
```

`<owner>/<repo>` は GitHub のリポジトリ名に置き換えます。デプロイ名 `main` で出力を取得できない場合は、`az deployment sub list --output table` でデプロイ名を確認するか、Azure Portal で Web App 名を確認してください。

GitHub リポジトリの Settings で `production` 環境を作成し、以下を登録します。必要に応じて、環境の保護ルールで承認者やデプロイ可能なブランチ（`master`）を設定してください。

| 種類 | 名前 | 値 |
| --- | --- | --- |
| Secret | `AZURE_CLIENT_ID` | `$clientId` |
| Secret | `AZURE_TENANT_ID` | `$tenantId` |
| Secret | `AZURE_SUBSCRIPTION_ID` | `$subscriptionId` |
| Variable | `AZURE_WEBAPP_NAME` | `$appName` |
