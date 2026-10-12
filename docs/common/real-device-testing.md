# 実機での動作確認手順

## 1. 目的

このドキュメントは、開発中のアプリを、PC と同じ Wi-Fi につないだスマートフォン・タブレット（実機）で確認する手順を定める。

iOS のフォームの自動拡大のように、PC のブラウザや開発者ツールの端末の模擬では再現できない動きがある。そうした変更は、この手順で実機を確認する。

## 2. 実機での確認が必要な場面

- iOS・Android に固有の動きに関わる変更
  - 例：フォームにフォーカスしたときの自動拡大、ソフトウェアキーボードの表示、選択欄の標準の一覧、ピンチズーム
- タッチ操作に関わる変更
  - 例：タップ、指の横移動、縦スクロールとの区別
- 端末の向き（縦・横）で表示が変わる変更

PC で確かめられる内容（文字の大きさの計算値、レイアウト、横スクロールの有無など）は、先に PC で確認する。実機では、PC で確かめられない症状だけを見る。

## 3. 前提

- PC と実機を、同じ Wi-Fi（自宅などのプライベートなネットワーク）につなぐ。
- 公衆 Wi-Fi やゲスト用 Wi-Fi では行わない。端末どうしの通信が遮断されていることが多く、つながらない。
- インターネットに公開する方法（トンネルなど）は使わない。
- アプリは開発環境として起動する。お問い合わせフォームは、user-secrets に SMTP の認証情報がある PC では、開発環境でも実際にメールが送られる。確認では必須項目を空にして入力検証で止め、送信まではしない。
- 開発環境では、エラー時の詳しい例外画面も、同じネットワークの端末から見える。確認の間だけ起動し、終わったら止める。

## 4. 接続の手順

1. PC のネットワークの種類が「プライベート」であることを確認する。
   - `Get-NetConnectionProfile` の `NetworkCategory` が `Private` であること。
   - `Public` の場合は、Windows の設定（ネットワークとインターネット → Wi-Fi → 接続中のネットワークのプロパティ）で「プライベート ネットワーク」に変える。
2. PC の IP アドレスを調べる。
   - `ipconfig` の Wi-Fi の「IPv4 アドレス」を見る（例：`192.168.0.58`）。
   - `vEthernet (WSL …)` などの仮想アダプターのアドレスは使わない。
3. リポジトリのルートで、アプリをすべてのネットワークで待ち受けるように起動する。

   ```powershell
   dotnet run --project LifePlan/LifePlan.csproj --launch-profile http --urls http://0.0.0.0:5250
   ```

   - `Get-NetTCPConnection -LocalPort 5250 -State Listen` の `LocalAddress` が `0.0.0.0` であることを確認する。
   - `127.0.0.1` や `::1` の場合は、`--urls` が効いていない。
4. Windows ファイアウォールで、受信を許可する（5 章）。
5. 実機のブラウザで `http://<PC の IP アドレス>:5250/` を開く。
   - `https://` ではなく `http://` で開く。
6. 確認が終わったら、アプリを止める（`Ctrl+C`）。5 章で許可ルールを追加した場合は削除し、遮断ルールを無効にした場合は元に戻す。

## 5. Windows ファイアウォールの許可

外からの受信は、許可するルールがなければ遮断される。

初回の起動で「Windows セキュリティの重要な警告」が表示された場合は、「プライベート ネットワーク」だけにチェックを入れて許可する。

ただし、確認画面が出ないこともある（以前に閉じた場合など）。その場合は、次の順に対処する。

1. アプリ向けの遮断ルールがないか確認する。確認画面を「キャンセル」で閉じると、Windows がアプリの受信を遮断するルールを作ることがある。遮断ルールは許可ルールより優先されるため、プライベートのネットワークに効く遮断ルールが残っていると、手順 2 の許可ルールを追加してもつながらない。

   ```powershell
   # 確認（通常の PowerShell で実行できる）
   Get-NetFirewallApplicationFilter | Where-Object Program -match 'LifePlan|dotnet' | Get-NetFirewallRule | Select-Object Name, DisplayName, Action, Profile, Enabled
   ```

   `Action` が `Block`、`Enabled` が `True`、`Profile` が `Private` を含むか `Any` のルールがあれば、管理者の PowerShell でそのルールだけを一時的に無効にする。表示名で指定すると、同じ表示名の許可ルールやパブリック用のルールまで対象になるため、条件で絞り込む。

   ```powershell
   # プライベートに効く遮断ルールだけを無効にし、無効にしたルールを $disabled に残す
   $disabled = Get-NetFirewallApplicationFilter | Where-Object Program -match 'LifePlan|dotnet' | Get-NetFirewallRule |
       Where-Object { $_.Action -eq 'Block' -and $_.Enabled -eq 'True' -and ($_.Profile -match 'Private|Any') } |
       Disable-NetFirewallRule -PassThru
   $disabled | Select-Object Name, DisplayName, Profile

   # 確認が終わったら、同じ PowerShell の画面で全件を元に戻す
   $disabled | Enable-NetFirewallRule
   ```

   ルールは TCP・UDP などで複数あることがある（該当するルールが無い場合、`$disabled` は空で、戻すコマンドを実行しても何も起きない）。PowerShell の画面を閉じてしまった場合は、表示された `Name` をすべて、カンマ区切りで渡して戻す（例：`Enable-NetFirewallRule -Name "<Name1>", "<Name2>"`）。

2. 管理者の PowerShell で、ポートを指定した許可ルールを追加し、確認後に削除する。

   ```powershell
   # 追加（プライベート ネットワークだけ）
   New-NetFirewallRule -DisplayName "LifePlan dev 5250" -Direction Inbound -Protocol TCP -LocalPort 5250 -Profile Private -Action Allow

   # 確認
   Get-NetFirewallRule -DisplayName "LifePlan dev 5250" | Select-Object DisplayName, Enabled, Profile, Action

   # 削除（確認が終わったら）
   Remove-NetFirewallRule -DisplayName "LifePlan dev 5250"
   ```

   - `-Profile Private` を外さない。パブリックのネットワーク（外出先の Wi-Fi など）で開放しないため。
   - ルールは確認の間だけ置き、残さない。手順 1 で遮断ルールを無効にした場合は、許可ルールの削除とあわせて元に戻す。

## 6. つながらないとき

| 症状 | 主な原因 | 確かめ方・対処 |
| --- | --- | --- |
| 「サーバが応答を停止しています」（iOS Safari）、しばらく読み込んだ後に時間切れになる | ファイアウォールで遮断されている | 5 章の手順 1 で、プライベートに効くアプリ向けの遮断ルールがないかを確認し、あれば一時的に無効にする。5 章の手順 2 で許可ルールを追加する |
| 同上 | ネットワークの種類が「パブリック」 | 4 章の手順 1 |
| 同上 | ルーターが端末どうしの通信を遮断している（ゲスト用 Wi-Fi、プライバシーセパレーター） | ゲスト用ではない Wi-Fi につなぐ。ルーターの設定で端末間通信を許可する |
| 同上 | PC か実機が VPN につながっている | VPN を切る |
| 「サーバに接続できません」とすぐに出る | アプリが止まっている、または `localhost` だけで待ち受けている | 4 章の手順 3 の `LocalAddress` を確認する |
| 同上 | IP アドレスが違う（再接続で変わった、仮想アダプターのアドレスを使った） | 4 章の手順 2 をやり直す |
| ページは出るが、見た目が古い | ブラウザのキャッシュ、または別のブランチで起動している | 再読み込みする。起動しているブランチを確認する |

2026-10-12 に iPhone で「サーバが応答を停止しています」となった例：待ち受けは `0.0.0.0:5250`、ネットワークはプライベートで正しかった。`LifePlan.exe` を対象にしたファイアウォールのルールは、許可・遮断とも無かった。ファイアウォールでの遮断が最も疑わしいが、許可ルールを追加した後の接続は確かめていない。

## 7. 確認するときの注意

- **画面の倍率を等倍に戻してから確認する。**
  - iOS Safari：アドレスバーのページメニュー（iOS のバージョンにより「ぁあ」などの表示）で、ページの拡大縮小を 100% にし、ページを再読み込みする。
  - ピンチで拡大したままだと、自動拡大の有無を判断できない。
- **iOS のフォームの自動拡大の見分け方**
  - 入力欄や選択欄をタップした直後に、画面全体が拡大されるかを見る。
  - フォーカスを外しても拡大されたまま戻らなければ、自動拡大である。
  - 日本で配布されている iOS のブラウザは、Safari 以外（Chrome など）も同じ WebKit を使うため、同じ動きになる（EU では WebKit 以外のエンジンを使うブラウザもある）。
- **縦向き・横向きの両方を確認する。**
  - iPhone の横向きは、機種によって画面幅が 768px 以上になる。そのため、縦向きと違う CSS が効くことがある。
- **確認した端末・OS・ブラウザと結果を、PR に書く。**
  - 確認できなかった端末は「未確認」と書く。
  - 機能ごとの確認項目は、その機能の `specs/NNN-*/quickstart.md` を参照する。
