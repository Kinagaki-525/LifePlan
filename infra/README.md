# Azure リソース

`main.bicep` はサブスクリプションに Resource Group を作成し、`resources.bicep` がその中に以下を作成します。

| 用途 | App Service Plan | Web App | デプロイ元 |
| --- | --- | --- | --- |
| 本番 | Linux B1 | `lifeplan-prod-<固定サフィックス>` | `master` |

サフィックスはサブスクリプション ID と Resource Group 名から決まるため、同じ設定で再実行しても名前は変わりません。Web App 名が既に他の利用者に取得されている場合は、`main.bicepparam` の `resourceNamePrefix` を変更してください。

本番 Web App は HTTPS を必須にし、FTP と SCM の基本認証を無効にします。自動デプロイでは Azure の OIDC 認証を利用します（[自動デプロイ](#自動デプロイ) を参照）。

本番の B1 App Service Plan は稼働中、アクセスがなくても課金されます。独自ドメインは `main.bicepparam` の `customHostNames` に指定します（[独自ドメイン](#独自ドメイン) を参照）。

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

## 独自ドメイン

`customHostNames` に指定したホスト名を本番 Web App にバインドし、App Service の無料マネージド証明書で HTTPS（SNI SSL）を有効にします。空の配列にすると独自ドメインは設定しません。

マネージド証明書の発行時にドメインの所有確認が行われるため、デプロイ前に DNS を設定してください。ドメインは お名前.com の DNS で管理しています。検証 ID と IP アドレスは以下で確認できます。

```powershell
$appName = az deployment sub show --name main --query properties.outputs.productionAppName.value --output tsv
az webapp show --resource-group rg-lifeplan-jpe --name $appName --query customDomainVerificationId --output tsv
Resolve-DnsName "$appName.azurewebsites.net" -Type A
```

| ホスト名 | 種類 | 値 |
| --- | --- | --- |
| `www` | CNAME | `<Web App 名>.azurewebsites.net` |
| `asuid.www` | TXT | 検証 ID |
| `@` | A | Web App の受信 IP アドレス |
| `asuid` | TXT | 検証 ID |

B1 プランの受信 IP アドレスは共有のため、プランの変更などで変わることがあります。`www` は IP アドレスに依存しない CNAME で設定してください。正規の URL は `www.futari-kakei.com` とし、ルートドメインへのアクセスはアプリ（`Program.cs`）で `www` へ恒久的にリダイレクト（308）します。

再デプロイ時は、ホスト名のバインドをいったん SSL なしで更新してから証明書を再設定するため、短時間 HTTPS が無効になることがあります。

## 自動デプロイ

`.github/workflows/ci.yml` はビルド、テスト、Bicep の構文検証を行います。`.github/workflows/cd.yml` は `master` への push（または手動実行）でビルド、テスト、発行を行い、本番 Web App へデプロイします。

`develop` から `master` への PR は、Squash ではなく **Create a merge commit** でマージし、`develop` ブランチは削除せずに残します。Squash でマージすると `develop` の各コミットが `master` に取り込まれた扱いにならないため、次に `master` を `develop` へ取り込むときに、同じ変更同士でコンフリクトが起きます。

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
