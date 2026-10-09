# Data Model: 支出近似改善

金額は Domain では円（long）、ViewModel では万円（decimal）または表示用文字列。

## Domain/ReferenceData

### ChildLivingCostEntry（新規 record）

| フィールド | 型 | 説明 |
| --- | --- | --- |
| StartAge | int | 年齢帯の下限（含む） |
| EndAge | int | 年齢帯の上限（含む） |
| AnnualCostYen | long | 1人あたり基準年額（価格基準年の価格） |
| Basis | string | 根拠区分（例：「2024年調査の0〜2歳から算出」「高校生の額を継続する仮定」） |

### ChildLivingCostMaster（新規 static）

- `Entries`：0〜2 / 3〜5 / 6〜11 / 12〜14 / 15〜17 / 18〜23 歳、590,000 / 590,000 / 670,000 / 760,000 / 810,000 / 810,000 円（正本 5.3）
- `PriceBaseYear`：2024（マスタの出典情報。計算では使わず、`SimulationAssumptions.Current` の初期値としてのみ参照する）
- `Source`：出典表記
- 検証ルール：年齢帯は0歳から連続し重複しない（単体テストで確認）

### SimulationAssumptions（新規 record）

| フィールド | 型 | 本番値 |
| --- | --- | --- |
| Version | string | `"2026.10"`（前提バージョン） |
| InflationRatePercent | decimal | 2 |
| HousingMaintenanceRatePercent | decimal | 1 |
| ChildSupportEndAge | int | 22 |
| ChildSupportEndAgeWithGraduateSchool | int | 24 |
| ChildLivingCosts | IReadOnlyList\<ChildLivingCostEntry\> | `ChildLivingCostMaster.Entries` |
| ChildLivingCostPriceBaseYear | int | `ChildLivingCostMaster.PriceBaseYear`（2024）。計算が使う価格基準年はこの値のみ |
| SupportedCalculationSpecVersions | IReadOnlyList\<string\> | `["2"]` |
| CurrentCalculationSpecVersion | string | `"2"` |

- `static SimulationAssumptions Current`：本番の前提
- 試験用には `Current with { ChildLivingCosts = …, ChildLivingCostPriceBaseYear = 開始年 }` で作る

### RateOptionCatalog（変更）

- `InflationRates` を削除。`AnnualIncomeChangeRates` は維持

## Domain/Entities

### ExpenseData（変更）

- `InflationRatePercent` を削除

その他の Entity は変更しない。大学院選択は既存の `ChildEducationData.GraduateSchoolOptionValue` が空でないことで判定する。

## Domain/Logic

### ChildLivingCostCalculator（新規 static）

- `long CalculateAnnualCost(int? childAge, bool hasGraduateSchool, int year, SimulationAssumptions assumptions)`
  - `childAge` が null・負・終了年齢以上なら0
  - 該当帯の `AnnualCostYen × (1 + 率)^(year − assumptions.ChildLivingCostPriceBaseYear)` を円単位で四捨五入
- 年次計算（LifePlanCalculator）と参考月額表示（Application）の両方から使う

### AnnualExpense（変更 record）

追加フィールド：`ChildLivingCostYen`、`HousingMaintenanceYen`。`TotalExpenseYen` に両方を加える。

### LifePlanCalculationResult（変更 record）

追加：`string AssumptionsVersion`

> 2026-10-10 ユーザー判断：住宅価格が0の場合は「住宅購入なし」として扱うことになり、未算定の状態（`HousingMaintenanceStatus`）は削除した。購入時期があっても頭金＋借入額が0なら、`LifePlanInputNormalizer` が住宅購入の入力を無効化する（家賃は購入時期以降も計上）。

### LifePlanCalculator（変更）

- コンストラクタ：`()`（`SimulationAssumptions.Current`）と `(SimulationAssumptions assumptions)`
- 物価係数 `1.02^t`（`t = yearOffset`）を適用：基本生活費、その他支出、教育費（合計）、旅行・その他、住宅維持費
- 適用しない：家賃、結婚、頭金、ローン返済、自動車
- 子どもの生活費：`ChildLivingCostCalculator` を子どもごとに呼んで合算（年は `currentYear + yearOffset`）
- 住宅維持費：`rawHusbandAge >= PurchaseHusbandAge` かつ価格 > 0 の年に `(頭金 + 借入額) × 1% × 物価係数` を丸め

## ViewModels/LifePlan

### LifePlanViewModel（変更）

| 変更 | フィールド | 説明 |
| --- | --- | --- |
| 追加 | CalculationSpecVersion : string? | hidden で送受信 |
| 追加 | SpecVersionNotice : bool | 旧版（欠落）送信の案内表示 |
| 追加 | ExpenseGuidance : LifePlanExpenseGuidanceViewModel | 入力欄の説明と参考月額 |
| 削除 | InflationRateOptions | |

### ExpenseInputViewModel（変更）

- `InflationRatePercent` を削除

### LifePlanExpenseGuidanceViewModel（新規）

| フィールド | 説明 |
| --- | --- |
| BasicLivingCostNote | 基本生活費の説明 |
| ChildNote | 子ども欄の説明 |
| HousingNote | 住宅購入欄の説明（率を含む） |
| OtherCostNote | その他支出の説明 |
| InflationNote | 物価上昇の説明（率を含む） |
| ChildMonthlyReference : string? | 送信済み年齢による子どもの概算月額合計（子どもなしは null） |
| ChildMonthlyReferenceByAgeBand | 年齢帯ごとの月換算参考額（初期表示用） |
| ChildMonthlyReferenceCaveat | 「実際の内訳とは一致しない」旨 |

> 2026-10-10 ユーザー判断：結果上部の「初年度の収支」の枠は不要となり、`LifePlanResultViewModel` の `FirstYearSummary`・`AutoCosts` と、`LifePlanFirstYearSummaryViewModel`・`AutoCostSummaryViewModel` は削除した。

### LifePlanAssumptionsViewModel（変更）

追加：`AutoCostNotes : IReadOnlyList<string>`（対象費目・終了年齢・18歳以降の仮定・年1%・年2%と対象外・自動計上しない費用・重複の注記・前提バージョン）

### キャッシュフロー表の行（変更）

支出区分に「子どもの生活費」（基本生活費の次）と「住宅維持費」（住宅ローン返済の次）を追加。

## 状態遷移（フォーム送信）

```text
POST
 ├─ CalculationSpecVersion 欠落 → 試算なし・SpecVersionNotice=true・入力保持・hidden に現行版
 ├─ 未定義の版 → 検証エラー（入力保持）
 ├─ その他の検証エラー → 検証エラー（入力保持）
 └─ 有効 → 試算・結果表示
```
