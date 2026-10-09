# Quickstart: 支出近似改善の検証

## 前提

- .NET 10 SDK
- リポジトリルートで実行する

## 1. ビルドとテスト

```powershell
dotnet build LifePlan.slnx -m:1
dotnet test LifePlan.slnx -m:1
```

期待：ビルド成功、テスト全件成功。NuGet 脆弱性データ取得の警告など、ネットワーク由来の警告は結果と分けて報告する。

## 2. 単体テストで確認する受入条件

| 受入条件 | 確認先 | 期待値 |
| --- | --- | --- |
| AT02、AT05、AT06、AT18〜AT21、年齢帯境界、最大4子 | Domain/Logic の子どもの生活費テスト | 正本 4.3、5.3。2026年0歳＝613,836円、6/12/15/18歳＝697,068／790,704／842,724／842,724円 |
| AT03、AT04 | 同上（試験用前提：価格基準年＝開始年、年36万円） | 出生年＝360,000×1.02²、初年度＝240万＋36万 |
| AT07〜AT10 | Domain/Logic の住宅維持費テスト | 50万→51万、50万×1.02⁵、完済後も継続、価格0は住宅購入なし（維持費0円・家賃継続） |
| AT11〜AT13 | Domain/Logic の物価係数テスト | 240万→244.8万、40万→40.8万、家賃等は不変 |
| AT16 | Domain/Logic の貯蓄テスト | 運用0%で自動費用の有無の残高差＝自動費用累計 |
| AT14、AT15 | Application/Services のテスト | 版欠落：試算なし・案内・入力保持。未定義版：検証エラー。改ざん値は計算に使わない |
| AT17、FR-017、FR-019 | Application/Mappers のテスト | キャッシュフロー表の「子どもの生活費」「住宅維持費」行。前提注記に年1%・年2%・終了年齢 |

データの形は [data-model.md](data-model.md)、POST の扱いは [contracts/form-post.md](contracts/form-post.md) を参照。

## 3. 画面での確認

```powershell
dotnet run --project LifePlan
```

`/LifePlan` を開き、次を確認する（表示位置は [contracts/result-view.md](contracts/result-view.md)）。

1. 支出タブに想定インフレ率の欄がない。基本生活費・その他支出・物価上昇の説明が常時表示される（AT01）
2. 第1子に0歳、住宅購入（初年度、頭金1,000万・借入4,000万）を入れて試算する
   - キャッシュフロー表に「子どもの生活費」「住宅維持費」の行があり、住宅維持費の初年度は50.0万円
   - 前提条件に年1%・年2%・終了年齢・自動計上しない費用・重複の注記がある
3. 第1子を −2 にして試算し、キャッシュフロー表の「子どもの生活費」が2年後から計上される（AT17）
4. 購入時期だけ入れ、頭金・借入額を0で試算し、住宅購入なしとして家賃が続き、住宅維持費・頭金・ローン返済が0になる（AT10）
5. 開発者ツールで hidden の `CalculationSpecVersion` を削除して送信し、入力が保持され案内が出る。値を `99` にすると検証エラーになる（AT14、AT15）
6. スマートフォン幅（390px 程度）で説明文が折り返して読める
7. PC 幅で、入力行の説明文が入力欄の列から右端まで表示され、ラベル列に回り込まない。基本生活費に負数を入れて送信すると、検証エラーが入力欄の直下（説明文より上）に出る

## 4. HTTP での回帰確認（Controller テストの代替）

Controller の自動テストがないため、改ざん送信は HTTP で確認する。

1. `GET /LifePlan` で Cookie と `__RequestVerificationToken` を取得する
2. 同じ Cookie とトークンで、`CalculationSpecVersion=2`、`Family.HusbandAge=30`、`Family.WifeAge=30`、`IncomeExpense.Expenses.MonthlyBasicLivingCostManYen=20`、`IncomeExpense.Expenses.InflationRatePercent=1` を `POST /LifePlan` する
3. 結果の「基本生活費」行が 240.0 → 244.8（年2%）であり、送信した1%が使われていないこと（AT15）
4. `CalculationSpecVersion` を省くと案内が出て結果表が出ないこと、`99` にすると検証エラーになること（AT14、AT15）
