# Azure リソース

`main.bicep` はサブスクリプションに Resource Group を作成し、`resources.bicep` がその中に以下を作成します。

| 用途 | App Service Plan | Web App | デプロイ元 |
| --- | --- | --- | --- |
| 本番 | Linux B1 | `lifeplan-prod-<固定サフィックス>` | `master` |

サフィックスはサブスクリプション ID と Resource Group 名から決まるため、同じ設定で再実行しても名前は変わりません。Web App 名が既に他の利用者に取得されている場合は、`main.bicepparam` の `resourceNamePrefix` を変更してください。

各 Web App は HTTPS を必須にし、FTP と SCM の基本認証を無効にします。後続の自動デプロイでは Azure の OIDC 認証を利用する想定です。

本番の B1 App Service Plan は稼働中、アクセスがなくても課金されます。初期状態では Azure の既定 URL のみ使用し、独自ドメインや DNS は設定しません。

## 初回作成

Azure CLI と Bicep CLI を利用できる環境で、サブスクリプションを選択してから実行します。`what-if` の内容を確認してから `create` を実行してください。

```powershell
az login
az account set --subscription '<subscription-id>'
az bicep build --file infra/main.bicep --stdout
az bicep build-params --file infra/main.bicepparam --stdout
az deployment sub what-if --location japaneast --template-file infra/main.bicep --parameters infra/main.bicepparam
az deployment sub create --location japaneast --template-file infra/main.bicep --parameters infra/main.bicepparam
```

テンプレートは App Service のアプリ設定を管理しません。作成後、Web App ごとの環境変数へ microCMS と SMTP の設定を入力します。本番アプリでは `ASPNETCORE_ENVIRONMENT=Production` を設定してください。API キーやパスワードをこのリポジトリへ保存しないでください。

動作確認はローカルで行い、microCMS と SMTP の設定はユーザーシークレットで管理します。

この段階の GitHub Actions はビルドとテストのみです。Web App への自動デプロイは、Azure 認証と環境別設定を用意した後に追加します。
