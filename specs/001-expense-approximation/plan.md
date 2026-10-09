# Implementation Plan: 支出近似改善（子どもの生活費・住宅維持費・物価上昇の自動計上）

**Branch**: `001-expense-approximation` | **Date**: 2026-10-10 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/001-expense-approximation/spec.md`（正本：`docs/simulator/index.md` v1.2）

## Summary

子どもの生活費（年齢帯マスタ・2024年価格）と住宅維持費（購入費用の年1%）を自動で支出に加え、物価上昇年2%を固定前提として対象費目に適用する。インフレ率の入力欄を撤去し、フォームに計算仕様バージョンを持たせる。結果に初年度サマリ、自動計上費用、前提注記を追加する。

技術方針：前提値とマスタは `Domain/ReferenceData` の `SimulationAssumptions` に集約し、`LifePlanCalculator` に注入できるようにする（試験用マスタのため）。計算は Domain/Logic、表示用の加工は Application/Mappers・Factories、版の判定は Validator と Service に置く（[research.md](research.md)）。

## Technical Context

**Language/Version**: C# / .NET 10（`net10.0`）

**Primary Dependencies**: ASP.NET Core MVC、Razor Views、Bootstrap（既存。追加なし）

**Storage**: N/A（保存なし）

**Testing**: xUnit（`LifePlan.Tests`）

**Target Platform**: Web サーバー（既存のデプロイ先）、PC・スマートフォンのブラウザ

**Project Type**: Web アプリケーション（単一 MVC プロジェクト＋テストプロジェクト）

**Performance Goals**: 既存と同等（1回の試算は最大約85年分の年次計算。追加の計算量は子ども4人×年数の定数倍）

**Constraints**: 新しいライブラリ・DB・外部APIを使わない。計算の正本を View・JavaScript に複製しない。内部計算は円、表示は万円

**Scale/Scope**: 変更対象は Domain 5ファイル程度（新規3）、Application 7ファイル程度、ViewModel 6ファイル程度（新規3）、View 1、テスト6ファイル程度

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | 照合結果 | 根拠 |
| --- | --- | --- |
| I. 仕様の正本は docs/ | ✅ | `docs/simulator/index.md` v1.2 と `implementation-plan.md` へ了承済みで反映。spec.md は正本を参照 |
| II. レイヤー責務と依存方向 | ✅ | 下表のとおり配置。Controller の変更なし。View には ViewModel のみ |
| III. 計算ロジックのテスト | ✅ | Domain/Logic の変更ごとに単体テスト。境界値（年齢帯・出生・終了・購入年）、丸め、最大4子を含む |
| IV. 最小変更と事前確認 | ✅ | 新規依存なし。既存の命名・配置に従う。既存テストの期待値変更は仕様変更による（R9） |
| V. 検証とメッセージの正本 | ✅ | 版の検証は Validator、文言は `LifePlanValidationMessages`。JavaScript に計算・検証を追加しない |

### 型ごとの配置（原則 II）

| 型 | 種別 | レイヤー |
| --- | --- | --- |
| `SimulationAssumptions`、`ChildLivingCostMaster`、`ChildLivingCostEntry` | 新規 | Domain/ReferenceData |
| `RateOptionCatalog` | 変更（`InflationRates` 削除） | Domain/ReferenceData |
| `ExpenseData` | 変更（`InflationRatePercent` 削除） | Domain/Entities |
| `ChildLivingCostCalculator` | 新規 | Domain/Logic |
| `LifePlanCalculator`、`LifePlanCalculationResult`（`AnnualExpense`、`HousingMaintenanceStatus`） | 変更・新規 | Domain/Logic |
| `LifePlanInputValidator`、`LifePlanValidationMessages` | 変更（版の検証、インフレ率の検証削除） | Application/Validators |
| `LifePlanPageService` | 変更（版欠落の分岐、説明 ViewModel の組み立て呼び出し） | Application/Services |
| `LifePlanExpenseGuidanceFactory` | 新規 | Application/Factories |
| `LifePlanRateSelectOptionFactory` | 変更（インフレ率選択肢削除） | Application/Factories |
| `LifePlanPageMapper`、`LifePlanAssumptionMapper` | 変更（行追加、サマリ、前提注記、インフレ率の詰め替え削除） | Application/Mappers |
| `LifePlanInputNormalizer` | 確認のみ（Expenses はそのまま渡すため変更不要の見込み） | Application/Normalizers |
| `LifePlanViewModel`、`ExpenseInputViewModel`、`LifePlanResultViewModel`、`LifePlanAssumptionsViewModel` | 変更 | ViewModels |
| `LifePlanExpenseGuidanceViewModel`、`LifePlanFirstYearSummaryViewModel`、`AutoCostSummaryViewModel` | 新規 | ViewModels |
| `Views/LifePlan/Index.cshtml` | 変更 | Views |

**Post-design re-check**: Phase 1 の設計（data-model・contracts）でも違反なし。`ChildLivingCostCalculator` を Application から呼ぶのは「Application → Domain/Logic」の既存の依存方向に沿う。

## Project Structure

### Documentation (this feature)

```text
specs/001-expense-approximation/
├── spec.md
├── docs-update-proposal.md   # 正本への反映案（反映済み）
├── plan.md                   # 本書
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── form-post.md
│   └── result-view.md
├── checklists/requirements.md
└── tasks.md                  # /speckit-tasks で作成
```

### Source Code (repository root)

```text
LifePlan/
├── Domain/
│   ├── ReferenceData/
│   │   ├── SimulationAssumptions.cs        # 新規
│   │   ├── ChildLivingCostMaster.cs        # 新規
│   │   ├── ChildLivingCostEntry.cs         # 新規
│   │   └── RateOptionCatalog.cs            # InflationRates 削除
│   ├── Entities/LifePlan/ExpenseData.cs    # InflationRatePercent 削除
│   └── Logic/
│       ├── ChildLivingCostCalculator.cs    # 新規
│       ├── LifePlanCalculator.cs           # 物価係数・子ども生活費・住宅維持費
│       └── LifePlanCalculationResult.cs    # 費目追加・状態追加
├── Application/
│   ├── Factories/
│   │   ├── LifePlanExpenseGuidanceFactory.cs  # 新規
│   │   └── LifePlanRateSelectOptionFactory.cs
│   ├── Mappers/
│   │   ├── LifePlanPageMapper.cs
│   │   └── LifePlanAssumptionMapper.cs
│   ├── Services/LifePlanPageService.cs
│   └── Validators/
│       ├── LifePlanInputValidator.cs
│       └── LifePlanValidationMessages.cs
├── ViewModels/LifePlan/
│   ├── LifePlanViewModel.cs
│   ├── ExpenseInputViewModel.cs
│   ├── LifePlanResultViewModel.cs
│   ├── LifePlanAssumptionsViewModel.cs
│   ├── LifePlanExpenseGuidanceViewModel.cs    # 新規
│   ├── LifePlanFirstYearSummaryViewModel.cs   # 新規
│   └── AutoCostSummaryViewModel.cs            # 新規
├── Views/LifePlan/Index.cshtml
└── wwwroot/css/life-plan.css                  # 必要な場合のみ（サマリの体裁）

LifePlan.Tests/
├── Domain/
│   ├── Logic/
│   │   ├── ChildLivingCostCalculatorTests.cs        # 新規
│   │   ├── LifePlanCalculatorExpenseTests.cs        # 物価係数・維持費・子ども生活費を追加、既存期待値更新
│   │   └── LifePlanCalculatorSavingsTests.cs        # AT16、既存期待値更新
│   └── ReferenceData/ChildLivingCostMasterTests.cs  # 新規（年齢帯の連続性・値）
└── Application/
    ├── Mappers/
    │   ├── LifePlanPageMapperResultTests.cs         # 行追加・サマリ・自動費用
    │   └── LifePlanAssumptionMapperTests.cs         # 前提注記
    ├── Factories/LifePlanExpenseGuidanceFactoryTests.cs  # 新規（参考月額）
    └── Services/LifePlanPageServiceResultTests.cs   # 版欠落・未定義・改ざん、インフレ率テスト削除
```

**Structure Decision**: 既存の単一 MVC プロジェクト構成（`LifePlan/` と `LifePlan.Tests/`）をそのまま使う。新規ファイルは既存フォルダに置き、テストは本体の名前空間・フォルダに対応させる。

## 実装順序

要件定義書 8章の順に、各段階でビルド・テストが通る単位に分ける。

1. **前提とマスタ**：`SimulationAssumptions`・`ChildLivingCostMaster`・テスト
2. **Domain 計算**：`ChildLivingCostCalculator`、Calculator の物価係数・子どもの生活費・住宅維持費、`InflationRatePercent` の削除、既存テストの期待値更新
3. **入力画面の簡素化**：インフレ率欄・選択肢・検証の削除、計算仕様バージョン、入力欄の説明と参考月額
4. **結果の内訳**：表の行、初年度サマリ、自動計上費用、前提注記
5. **回帰確認**：`dotnet build` / `dotnet test`、quickstart の画面確認

## リスクと対応

| リスク | 対応 |
| --- | --- |
| 既存テストの期待値更新で回帰を見落とす | 更新するテストは「物価係数の適用による変更」に限定し、初年度の値が変わらないことを別途確認する |
| 開始年が実行日に依存し、本番マスタの値の画面確認が年で変わる | 単体テストは `currentYear` を固定。画面確認は式（2024年からの補正）で照合する |
| デプロイ直後に旧画面から送信される | 版欠落時は試算せず案内を出す（contracts/form-post.md） |

## Complexity Tracking

憲章違反なし。記載事項なし。
