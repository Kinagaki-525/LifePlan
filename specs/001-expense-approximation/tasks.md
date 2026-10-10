---

description: "Task list for 支出近似改善"
---

# Tasks: 支出近似改善（子どもの生活費・住宅維持費・物価上昇の自動計上）

**Input**: Design documents from `specs/001-expense-approximation/`

**Prerequisites**: plan.md、spec.md、research.md、data-model.md、contracts/、quickstart.md

**Tests**: 憲章 原則 III により、Domain/Logic の変更には単体テストが必須。Application の分岐（版の判定、表示の加工）にもテストを付ける。各ストーリーのテストは実装前に書き、失敗を確認する。

**Organization**: spec.md のユーザーストーリー（US1〜US6）ごとに分ける。正本は `docs/simulator/index.md` v1.2。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 並行実行可（別ファイル、未完了タスクへの依存なし）
- **[Story]**: 対応するユーザーストーリー（US1〜US6）
- パスはリポジトリルートからの相対パス。本体は `LifePlan/`、テストは `LifePlan.Tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: 着手前の状態確認

- [X] T001 `dotnet build LifePlan.slnx -m:1` と `dotnet test LifePlan.slnx -m:1` を実行し、変更前のテスト件数と成功状態を記録する（ネットワーク由来の警告は分けて記録）

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: 全ストーリーが使う前提値・マスタ・計算結果の型を用意する

**⚠️ CRITICAL**: このフェーズが終わるまでストーリーに着手しない

- [X] T002 [P] `ChildLivingCostEntry` record（`int StartAge`、`int EndAge`、`long AnnualCostYen`、`string Basis`）を LifePlan/Domain/ReferenceData/ChildLivingCostEntry.cs に作成する
- [X] T003 [P] `ChildLivingCostMaster`（`Entries`：0〜2歳 590,000／3〜5歳 590,000／6〜11歳 670,000／12〜14歳 760,000／15〜17歳 810,000／18〜23歳 810,000、根拠区分は正本 5.3 の表のとおり、`PriceBaseYear = 2024`、`Source` に出典）を LifePlan/Domain/ReferenceData/ChildLivingCostMaster.cs に作成する
- [X] T004 `SimulationAssumptions` record を LifePlan/Domain/ReferenceData/SimulationAssumptions.cs に作成する。フィールドと本番値は data-model.md のとおり（`Version = "2026.10"`、`InflationRatePercent = 2`、`HousingMaintenanceRatePercent = 1`、`ChildSupportEndAge = 22`、`ChildSupportEndAgeWithGraduateSchool = 24`、`ChildLivingCosts = ChildLivingCostMaster.Entries`、`ChildLivingCostPriceBaseYear = ChildLivingCostMaster.PriceBaseYear`（計算が使う価格基準年はこの値のみ）、`SupportedCalculationSpecVersions = ["2"]`、`CurrentCalculationSpecVersion = "2"`）。静的プロパティ `Current` で本番値を公開する（T002、T003 に依存）
- [X] T005 [P] 年齢帯が0歳から23歳まで重複・欠落なく連続すること、各帯の年額が正本 5.3 と一致すること、`SimulationAssumptions.Current` の各値を検証するテストを LifePlan.Tests/Domain/ReferenceData/ChildLivingCostMasterTests.cs に作成する
- [X] T006 LifePlan/Domain/Logic/LifePlanCalculationResult.cs の `AnnualExpense` に `long ChildLivingCostYen` と `long HousingMaintenanceYen` を追加して `TotalExpenseYen` に含め、`HousingMaintenanceStatus` enum（`NotPlanned`／`Calculated`／`PriceMissing`）と、`LifePlanCalculationResult` の `HousingMaintenanceStatus`・`AssumptionsVersion` を追加する。本体のビルドは T007 まで通らないため、T006 と T007 は続けて行う
- [X] T007 LifePlan/Domain/Logic/LifePlanCalculator.cs に、既定コンストラクタ（`SimulationAssumptions.Current` を使う）と `LifePlanCalculator(SimulationAssumptions assumptions)` を追加する。新しい2費目は0、状態は `NotPlanned`、`AssumptionsVersion` は前提の `Version` を設定する（T004、T006 に依存）
- [X] T008 T006 の型変更に合わせて、既存テスト LifePlan.Tests/Application/Mappers/LifePlanPageMapperResultTests.cs の `AnnualExpense`・`LifePlanCalculationResult` の生成箇所を修正し、ビルドとテストが変更前と同じ結果になることを確認する

**Checkpoint**: 前提値と型がそろい、既存テストがすべて成功する

---

## Phase 3: User Story 1 - 子どもの生活費が自動で見込まれる (Priority: P1) 🎯 MVP

**Goal**: 子どもの年齢から生活費を年次支出に自動計上し、表・合計・貯蓄・グラフに反映する

**Independent Test**: 子どもを入力して試算し、年次表の「子どもの生活費」行が年齢帯の年額×1.02^(年−2024) で計上され、支出合計・収支・貯蓄に反映される

### Tests for User Story 1

- [X] T009 [P] [US1] LifePlan.Tests/Domain/Logic/ChildLivingCostCalculatorTests.cs を作成する。本番前提で：2026年0歳＝613,836円、2027年1歳＝626,113円（AT18、AT21）、2026年6／12／15／18歳＝697,068／790,704／842,724／842,724円（AT19、AT20）、年齢帯境界（2→3、5→6、11→12、14→15、17→18）、`null`・負の年齢は0、22歳は0（AT05）、大学院ありは23歳で計上・24歳で0（AT06）
- [X] T010 [P] [US1] LifePlan.Tests/Domain/Logic/LifePlanCalculatorChildLivingCostTests.cs を作成する。子どもなしは全年0（AT02）、試験用前提（`SimulationAssumptions.Current with { ChildLivingCosts = 全年齢帯360,000円, ChildLivingCostPriceBaseYear = 2026 }`）で子ども年齢−2の初年度・翌年は0、出生年は374,544円＝360,000×1.02²（AT03）、基本生活費月20万円＋子ども1人で初年度の基本生活費2,400,000円・子どもの生活費360,000円・控除なし（AT04）、4人分が子どもごとに丸めてから合算されること、大学院の判定は `GraduateSchoolOptionValue` が空でないこと

### Implementation for User Story 1

- [X] T011 [US1] `ChildLivingCostCalculator.CalculateAnnualCost(int? childAge, bool hasGraduateSchool, int year, SimulationAssumptions assumptions)` を LifePlan/Domain/Logic/ChildLivingCostCalculator.cs に実装する。`childAge` が null・負・終了年齢以上なら0、それ以外は該当帯の `AnnualCostYen × (1 + 率/100)^(year − assumptions.ChildLivingCostPriceBaseYear)` を円単位で四捨五入（`MidpointRounding.AwayFromZero`）。指数が負の場合も同じ式で扱う
- [X] T012 [US1] LifePlan/Domain/Logic/LifePlanCalculator.cs に子どもの生活費の計算を追加する。子どもごとに `ChildLivingCostCalculator` を `currentYear + yearOffset` で呼び、大学院の有無は同じ添字の `EducationPlans[i].GraduateSchoolOptionValue` で判定し、合計を `ChildLivingCostYen` に設定する（T011 に依存）
- [X] T013 [US1] LifePlan/Application/Mappers/LifePlanPageMapper.cs の `AddExpenseRows` で「基本生活費」の次に「子どもの生活費」行を追加し、LifePlan.Tests/Application/Mappers/LifePlanPageMapperResultTests.cs に行の位置と万円変換のテストを追加する

**Checkpoint**: 子どもの生活費が計上され、表とグラフ（支出合計）に反映される

---

## Phase 4: User Story 2 - 物価上昇が自動で反映される (Priority: P1)

**Goal**: インフレ率の入力をなくし、対象費目に年2%を自動適用する

**Independent Test**: その他支出・教育費・基本生活費が初年度は入力額、翌年以降は1.02^t倍。家賃・頭金・ローン・自動車・結婚は不変

### Tests for User Story 2

- [X] T014 [US2] LifePlan.Tests/Domain/Logic/LifePlanCalculatorExpenseTests.cs を更新する。`Calculate_AppliesInflationToBasicLivingCost` を固定2%の検証に書き換え（`InflationRatePercent` の設定を削除。1,200,000→1,224,000）、その他支出240万円→244.8万円（AT11）、教育費が初年度の翌年に40万円となる区分で408,000円（AT12）、旅行・その他の物価係数、年収変化なしの給与が2年目も変わらないこと（FR-011：物価上昇と連動しない）、家賃・頭金・ローン返済・自動車・結婚が増額されないこと（AT13）を追加する。2年目以降の支出を検証している既存テスト（旅行・その他、教育費、複数費目の合算など）の期待値を物価係数適用後の値へ更新し、初年度の期待値は変えない
- [X] T015 [P] [US2] LifePlan.Tests/Domain/Logic/LifePlanCalculatorSavingsTests.cs の、2年目以降に支出がある既存テストの期待値を物価係数適用後の値へ更新する

### Implementation for User Story 2

- [X] T016 [US2] LifePlan/Domain/Logic/LifePlanCalculator.cs で、物価係数 `(1 + InflationRatePercent/100)^yearOffset` を基本生活費・その他支出・教育費（子ども全員の基準額合計に適用）・旅行・その他に適用し、費目ごとに1回だけ円単位で四捨五入する。家賃・結婚・頭金・ローン返済・自動車には適用しない
- [X] T017 [US2] LifePlan/Domain/Entities/LifePlan/ExpenseData.cs から `InflationRatePercent` を削除し、LifePlan/Application/Mappers/LifePlanPageMapper.cs の `ToExpenseData` の詰め替えを削除する
- [X] T018 [US2] LifePlan/Domain/ReferenceData/RateOptionCatalog.cs の `InflationRates`、LifePlan/Application/Factories/LifePlanRateSelectOptionFactory.cs の `CreateInflationRateOptions`、LifePlan/Application/Services/LifePlanPageService.cs の `InflationRateOptions` 設定、LifePlan/ViewModels/LifePlan/LifePlanViewModel.cs の `InflationRateOptions` を削除する
- [X] T019 [US2] LifePlan/Application/Validators/LifePlanInputValidator.cs の想定インフレ率の検証を削除し、LifePlan/ViewModels/LifePlan/ExpenseInputViewModel.cs から `InflationRatePercent` を削除する。LifePlan.Tests/Application/Services/LifePlanPageServiceResultTests.cs の `CreateInitialPage_SetsInflationRateOptions`、`Submit_AcceptsDefinedInflationRate`、`Submit_RejectsUndefinedInflationRate` を削除する
- [X] T020 [US2] LifePlan/Views/LifePlan/Index.cshtml から `inflationRateOptions` 変数と想定インフレ率の入力行（label、select、help、validation）を削除する

**Checkpoint**: インフレ率の欄がなく、対象費目に年2%が適用される

---

## Phase 5: User Story 3 - 住宅維持費が自動で見込まれる (Priority: P2)

**Goal**: 購入費用の年1%を購入年から試算終了まで自動計上する

**Independent Test**: 住宅購入を入力して試算し、「住宅維持費」行が購入年から（頭金＋借入額）×1%×物価係数で計上される

### Tests for User Story 3

- [X] T021 [P] [US3] LifePlan.Tests/Domain/Logic/LifePlanCalculatorHousingMaintenanceTests.cs を作成する。初年度購入・頭金＋借入額5,000万円で初年度500,000円・翌年510,000円（AT07）、5年後購入で購入前0・購入年552,040円＝500,000×1.02⁵（四捨五入後の値で確認、AT08）、ローン完済後も維持費が続き返済は0（AT09）、購入時期あり・価格0で全年0かつ `HousingMaintenanceStatus.PriceMissing`（AT10）、購入なしで全年0かつ `NotPlanned`、購入時期が現在より前で現在年から計上、物価係数を使わない比較で家賃・頭金が変わらないこと

### Implementation for User Story 3

- [X] T022 [US3] LifePlan/Domain/Logic/LifePlanCalculator.cs に住宅維持費を追加する。購入時期があり `頭金 + 借入額 > 0` のとき、`rawHusbandAge >= PurchaseHusbandAge` の年に `(頭金 + 借入額) × HousingMaintenanceRatePercent/100 × 物価係数` を円単位で四捨五入して `HousingMaintenanceYen` に設定し、`HousingMaintenanceStatus` を判定して結果に設定する
- [X] T023 [US3] LifePlan/Application/Mappers/LifePlanPageMapper.cs の `AddExpenseRows` で「住宅ローン返済」の次に「住宅維持費」行を追加し、LifePlan.Tests/Application/Mappers/LifePlanPageMapperResultTests.cs に行の位置のテストを追加する

**Checkpoint**: 住宅維持費が計上され、未算定の状態が結果に残る

---

## Phase 6: User Story 4 - 自動計上の内容が結果で分かる (Priority: P2)

**Goal**: 結果上部の初年度サマリ、自動計上費用、前提注記を表示する

**Independent Test**: 子ども・住宅購入を含む条件で試算し、サマリ・自動費用・前提が contracts/result-view.md のとおり表示される

### Tests for User Story 4

- [X] T024 [P] [US4] LifePlan.Tests/Application/Mappers/LifePlanPageMapperResultTests.cs に追加する。初年度サマリ（収入合計・支出合計・年間収支の万円表示）、自動費用が初年度正なら初年度額、初年度0で将来発生なら「YYYY年から ○○.○万円」（AT17）、発生しないなら「計上なし」、`PriceMissing` なら「計上なし」と「住宅価格が未設定のため維持費を含めていません」
- [X] T025 [P] [US4] LifePlan.Tests/Application/Mappers/LifePlanAssumptionMapperTests.cs に、`AutoCostNotes` に対象費目（衣類・食費・生活用品）、終了年齢（22歳、大学院選択時24歳）、18歳以降は高校生の額を継続する仮定、住宅維持費年1%、物価上昇年2%と家賃・住宅ローン等が対象外、自動計上しない費用（携帯料金・小遣い・医療費、大学の下宿費）、「生活費と教育費の一部費目には重複が残る概算です」、前提バージョンが含まれ、数値が `SimulationAssumptions.Current` から作られることのテストを追加する

### Implementation for User Story 4

- [X] T026 [P] [US4] LifePlan/ViewModels/LifePlan/LifePlanFirstYearSummaryViewModel.cs（`TotalIncomeText`、`TotalExpenseText`、`AnnualBalanceText`）と LifePlan/ViewModels/LifePlan/AutoCostSummaryViewModel.cs（`Label`、`AmountText`、`Note`）を作成し、LifePlan/ViewModels/LifePlan/LifePlanResultViewModel.cs に `FirstYearSummary` と `AutoCosts` を、LifePlan/ViewModels/LifePlan/LifePlanAssumptionsViewModel.cs に `AutoCostNotes` を追加する
- [X] T027 [US4] LifePlan/Application/Mappers/LifePlanPageMapper.cs の `ToResultViewModel` で、初年度サマリと自動費用サマリ（各費目で最初に正となる年と額、`HousingMaintenanceStatus` による案内）を作る（T026 に依存）
- [X] T028 [US4] LifePlan/Application/Mappers/LifePlanAssumptionMapper.cs に `AutoCostNotes` の生成を追加する。率・年齢・バージョンは `SimulationAssumptions.Current` から組み立て、文に数値を直書きしない。家計の実態との一致や安全を断定する文言は入れない
- [X] T029 [US4] LifePlan/Views/LifePlan/Index.cshtml の結果エリア先頭（キャッシュフロー表の前）に初年度サマリと自動計上費用を、前提条件エリアに `AutoCostNotes` を表示する。必要な場合のみ LifePlan/wwwroot/css/life-plan.css に既存の淡色・枠のスタイルに合わせたクラスを追加する

**Checkpoint**: 結果だけで自動計上の内容と前提が分かる

---

## Phase 7: User Story 5 - 二重計上を避けて入力できる (Priority: P2)

**Goal**: 入力欄の常時表示の説明と、子どもの概算月額の参考表示

**Independent Test**: PC・スマートフォン幅で各説明が折りたたまれずに読め、送信前は年齢帯ごとの月額一覧、送信後の再表示では送信済みの子ども年齢による概算月額合計が出る（正本 3.5）

### Tests for User Story 5

- [X] T030 [P] [US5] LifePlan.Tests/Application/Factories/LifePlanExpenseGuidanceFactoryTests.cs を作成する。送信前は `ChildMonthlyReference` が null かつ年齢帯ごとの月換算参考額（百円単位で四捨五入）が6件、送信後で子どもなしは `ChildMonthlyReference` が null、送信後・開始年2026年・0歳1人で月額51,200円（613,836÷12＝51,153 → 百円単位 51,200）、−1歳の子は0として扱うこと、住宅欄の説明に年1%、物価の説明に年2%が `SimulationAssumptions.Current` から入ること

### Implementation for User Story 5

- [X] T031 [P] [US5] LifePlan/ViewModels/LifePlan/LifePlanExpenseGuidanceViewModel.cs を作成する（`BasicLivingCostNote`、`ChildNote`、`HousingNote`、`OtherCostNote`、`InflationNote`、`string? ChildMonthlyReference`、`ChildMonthlyReferenceByAgeBand`、`ChildMonthlyReferenceCaveat`）。LifePlan/ViewModels/LifePlan/LifePlanViewModel.cs に `ExpenseGuidance` を追加する
- [X] T032 [US5] LifePlan/Application/Factories/LifePlanExpenseGuidanceFactory.cs を作成する。文言は正本 3.5 の趣旨に沿い、率は `SimulationAssumptions.Current` から組み立てる。送信前は年齢帯ごとの一覧のみ、送信後の再表示では送信された子ども年齢と開始年で `ChildLivingCostCalculator` を呼び、年額合計÷12 を百円単位で四捨五入する（T031 に依存）
- [X] T033 [US5] LifePlan/Application/Services/LifePlanPageService.cs で、`CreateInitialPage` では送信前、`Submit` では送信後として `ExpenseGuidance` を設定する。開始年は試算と同じ `DateTime.Today.Year` を使う
- [X] T034 [US5] LifePlan/Views/LifePlan/Index.cshtml に説明を配置する。子ども年齢の下に `ChildNote`、住宅購入の下に `HousingNote`、基本生活費の下に `BasicLivingCostNote` と参考月額・注記、その他支出の下に `OtherCostNote`、支出カード下部に `InflationNote`。いずれも `life-plan-help` で常時表示し、折りたたまない

**Checkpoint**: 入力画面だけで費用範囲が分かり、参考月額が出る

---

## Phase 8: User Story 6 - 旧フォームからの送信で誤計算しない (Priority: P3)

**Goal**: 計算仕様バージョンで旧画面からの送信を判別する

**Independent Test**: 版欠落・未定義版・インフレ率を含む送信で contracts/form-post.md どおりの応答になる

### Tests for User Story 6

- [X] T035 [US6] LifePlan.Tests/Application/Services/LifePlanPageServiceResultTests.cs を更新する。既存の有効入力ヘルパーに `CalculationSpecVersion = "2"` を設定する。追加：版欠落（`CalculationSpecVersion` を null にして `Submit` を呼び、`PopulatePageDefaults` を通った後も欠落と判定されること）で `Result` が null・`SpecVersionNotice` が true・`Errors` が空・入力値保持・再表示の `CalculationSpecVersion` が `"2"`（AT14）、版 `"99"` で `CalculationSpecVersion` キーの検証エラー（AT15）、版欠落かつ他の検証エラーありで `SpecVersionNotice` が true・検証エラーも返り試算されないこと、版 `"2"` で試算され `AssumptionsVersion` が前提の `Version` と一致すること

### Implementation for User Story 6

- [X] T036 [US6] LifePlan/ViewModels/LifePlan/LifePlanViewModel.cs に `string? CalculationSpecVersion` と `bool SpecVersionNotice` を追加する
- [X] T037 [US6] LifePlan/Application/Validators/LifePlanValidationMessages.cs に未定義版の文言を追加し、LifePlan/Application/Validators/LifePlanInputValidator.cs で「値があり `SupportedCalculationSpecVersions` に含まれない」場合にキー `CalculationSpecVersion` のエラーを返す（欠落はエラーにしない）
- [X] T038 [US6] LifePlan/Application/Services/LifePlanPageService.cs の `Submit` で、送信された `CalculationSpecVersion` を `PopulatePageDefaults` より前に退避し、その値で判定する。欠落なら試算せず `SpecVersionNotice = true` とし、入力を保持する（他の検証エラーは併せて返す）。再表示用に `CalculationSpecVersion` を `CurrentCalculationSpecVersion` へ設定するのは、判定と検証の後、返却直前とする。`CreateInitialPage` でも現行版を設定する（T036、T037 に依存）
- [X] T039 [US6] LifePlan/Views/LifePlan/Index.cshtml のフォームに `CalculationSpecVersion` の hidden を追加し、`SpecVersionNotice` が true のとき検証エラー枠の近くに「計算の前提（子どもの生活費・住宅維持費・物価上昇の自動計上）が変わったため、内容を確認して再度実行してください」の趣旨の案内を表示する

**Checkpoint**: 旧画面からの送信で旧前提の結果が出ない

---

## Phase 9: Polish & Cross-Cutting Concerns

- [X] T040 [P] AT16 のテストを LifePlan.Tests/Domain/Logic/LifePlanCalculatorSavingsTests.cs に追加する。運用0%で、子ども・住宅購入あり／なしの2条件を同じ前提で計算し、最終年の残高差が子どもの生活費＋住宅維持費の累計と1円単位で一致する
- [X] T041 `rg -n -i "inflation|インフレ" LifePlan LifePlan.Tests` で残存参照がないことを確認する（`wwwroot/lib` を除く）
- [X] T042 `dotnet build LifePlan.slnx -m:1` と `dotnet test LifePlan.slnx -m:1` を実行し、T001 との差（追加・削除・更新したテスト）を整理する。ネットワーク由来の警告は成否と分けて記録する
- [ ] T043 specs/001-expense-approximation/quickstart.md の「画面での確認」1〜6 を実施する（スマートフォン幅 390px 程度を含む）
  - 2026-10-10 実施：HTTP で 1〜5 を確認済み（インフレ率欄なし・説明表示・試算結果・将来開始・未算定案内・版欠落・未定義版・改ざん値無視）。6（スマートフォン幅の見た目）はブラウザ未使用のため未確認
- [X] T044 docs/simulator/implementation-plan.md 4.1 の構成図に `ChildLivingCostEntry.cs`、`ChildLivingCostCalculator.cs`、`LifePlanExpenseGuidanceFactory.cs` を追記する案を示し、了承を得て反映する（憲章 原則 I）
- [X] T045 `/review-changes` で変更をレビューする（比較元 develop、未追跡ファイル一覧とビルド・テスト結果を渡す）
  - 2026-10-10 実施：change-reviewer で3回レビュー。1回目 中1・低2、2回目 低1、3回目 指摘なし。すべて修正済み（正本の文言2件はユーザー確認待ち）

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)** → **Foundational (Phase 2)** → 各ストーリー → **Polish (Phase 9)**

### User Story Dependencies

- **US1 (P1)**：Foundational のみに依存
- **US2 (P1)**：Foundational のみに依存。US1 と同じ LifePlan/Domain/Logic/LifePlanCalculator.cs を編集するため、同時に進めず US1 → US2 の順で行う
- **US3 (P2)**：Foundational に依存。物価係数を使うため US2 の T016 の後に行う
- **US4 (P2)**：US1・US3 の費目と状態を表示するため、両方の後に行う
- **US5 (P2)**：`ChildLivingCostCalculator`（T011）に依存。US4 とは独立
- **US6 (P3)**：Foundational のみに依存。View と Service を編集するため、US5 の T033・T034 と同時に編集しない

### Within Each User Story

- テストを先に書き、失敗を確認してから実装する
- ViewModel・型 → Domain 計算 → Mapper・Factory → Service → View の順

### Parallel Opportunities

- Phase 2：T002・T003・T005 は並行可
- US1：T009・T010 は並行可
- US2：T015 は T014 と並行可
- US4：T024・T025・T026 は並行可
- US5：T030・T031 は並行可。US5 全体は US3・US4 と並行可（編集ファイルが重ならない範囲）

---

## Parallel Example: User Story 4

```text
Task: "T024 初年度サマリ・自動費用のテスト（LifePlanPageMapperResultTests.cs）"
Task: "T025 前提注記のテスト（LifePlanAssumptionMapperTests.cs）"
Task: "T026 結果用 ViewModel の作成"
```

---

## Implementation Strategy

### MVP First

1. Phase 1・2 を完了する
2. US1（子どもの生活費）と US2（物価上昇）を完了する。US2 はインフレ率欄の撤去を含むため、両方そろって初めて正本 v1.2 と画面が一致する
3. **STOP and VALIDATE**：AT02〜AT06、AT11〜AT13、AT18〜AT21 を確認する

### Incremental Delivery

1. MVP（US1＋US2）
2. US3（住宅維持費）→ AT07〜AT10
3. US4（結果の内訳と前提）→ AT17
4. US5（入力欄の説明と参考月額）→ AT01
5. US6（計算仕様バージョン）→ AT14、AT15
6. Polish → AT16、回帰、画面確認、レビュー

本番へ出すのは US6 まで完了してからとする。US6 がないと、デプロイ直後に旧画面から送信されたとき二重計上を防げないため。

---

## Notes

- 既存テストの期待値更新は、物価係数の適用による2年目以降の値に限る。初年度の値が変わる場合は回帰として扱う
- 率・年齢・金額を View・JavaScript に直書きしない（`SimulationAssumptions` から ViewModel 経由で渡す）
- 検証エラーの文言は `LifePlanValidationMessages` に置く
- コミットはタスクまたは論理的なまとまりごとに行う

---

## 実装後の変更（2026-10-10、ユーザー判断）

- [X] T046 結果上部の「初年度の収支」の枠（T024、T026、T027、T029 の初年度サマリ・自動計上費用）を削除し、関連する ViewModel・CSS・テストを削除する
- [X] T047 住宅価格が0なら住宅購入なしとして扱う。LifePlan/Application/Normalizers/LifePlanInputNormalizer.cs で住宅購入の入力を無効化し、`HousingMaintenanceStatus` と未算定の案内を削除する。LifePlan.Tests/Application/Services/LifePlanPageServiceResultTests.cs に家賃継続のテストを追加
- [X] T048 正本 docs/simulator/index.md（3.2、4.3、6.1、6.4）と implementation-plan.md、specs（spec.md、contracts、data-model.md、research.md、quickstart.md）を更新する
