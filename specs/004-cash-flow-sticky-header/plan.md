# Implementation Plan: スマートフォン版キャッシュフロー表の年度ヘッダーに年齢を表示し、縦スクロール中も固定する

**Branch**: `004-cash-flow-sticky-header` | **Date**: 2026-10-12 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/004-cash-flow-sticky-header/spec.md`（正本：`docs/simulator/index.md` 6.2。反映案は [docs-update-proposal.md](docs-update-proposal.md)）

## Summary

スマートフォン幅（575px 以下）で、キャッシュフロー表の年度ヘッダーを「2026年／夫27歳／妻26歳」の 3 段にし、ページを縦にスクロールしている間は画面上端に固定する。PC 幅は年度ヘッダー（西暦だけ）も操作も変えない。

技術方針（[research.md](research.md) R1）：**既存の表の枠・表・横スクロール・固定列の CSS と構造には手を入れない。** 表の枠の直前に、見出し行だけを複製した表を JavaScript で置く。複製の入れ物を `position: sticky; top: 0` で縦に固定し、横位置だけを表の枠の `scroll` イベントで合わせる。複製は読み上げ・フォーカス・ポインター操作の対象外（`aria-hidden`、`inert`、`pointer-events: none`）とし、見出しの位置でのスワイプも下の表の枠に届くようにする（R2）。JavaScript が動かない環境では複製を作らず、表は修正前と同じになる。

年齢の文字列は、表本体の年齢行と同じ年次データから `LifePlanPageMapper` で作り、`YearHeaders` を年度・夫の年齢・妻の年齢を持つ ViewModel の一覧に変える（R4）。計算ロジックは変更しない。

## Technical Context

**Language/Version**: C# / .NET 10（ASP.NET Core MVC、Razor）、JavaScript（ES2020 相当、ビルドなしで直接配信）、CSS

**Primary Dependencies**: 既存のみ（Bootstrap、ブラウザ標準の `position: sticky`・`matchMedia`・`ResizeObserver`）。追加なし

**Storage**: N/A

**Testing**: xUnit（`LifePlan.Tests`、Mapper のテストを追加・更新）。画面は Playwright（Node.js）＋ Chromium の検証用スクリプト（スクラッチ、リポジトリに含めない）と、手動確認・実機確認（[quickstart.md](quickstart.md)）

**Target Platform**: スマートフォン（iOS Safari・Chrome、Android Chrome）と PC のブラウザ（Edge、Chrome、Safari）

**Project Type**: Web アプリケーション（単一 MVC プロジェクト＋テストプロジェクト）

**Performance Goals**: 縦スクロール中の見出しの固定はブラウザの描画と同時に行い、JavaScript による遅れを生じさせない。横スクロールの同期は `scroll` イベントごとに 1 回の代入だけにする（R1、R7）

**Constraints**:

- 既存の横スクロールと固定列を壊さない（FR-020）。表の枠（`.life-plan-cash-flow`）・表・既存の固定列の CSS ルールを変更しない。
- 表の枠の中で縦スクロールさせない（FR-009）。
- PC 幅の見た目と操作を変えない（FR-021、SC-014）。
- 計算ロジック・計算結果・表の項目と金額を変えない（FR-023）。
- 業務ルール・計算を JavaScript に持たせない（憲章 原則 V）。JavaScript が行うのは、見出しの複製・列幅のコピー・横位置の同期だけ。

**Scale/Scope**: C# 4 ファイル（ViewModel 2、Mapper 1、テスト 1）、View 1、CSS 1、JS 1（新規）。表は最大 85 列程度（夫婦とも 16 歳で開始した場合）× 約 30 行

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | 照合結果 | 根拠 |
| --- | --- | --- |
| I. 仕様の正本は docs/ | ✅ | 年度ヘッダーの表示と縦固定を、了承を得て `docs/simulator/index.md` v1.4 6.2 に反映済み（[docs-update-proposal.md](docs-update-proposal.md)）。spec.md は正本を参照 |
| II. レイヤー責務と依存方向 | ✅ | 表示用文字列は Mapper で作る（R4）。View は ViewModel だけを使う。画面上の振る舞い（複製・同期）は `wwwroot/js` の機能別ファイルに置く。Domain の型は変えない |
| III. 計算ロジックのテスト | ✅（対象外） | `Domain/Logic`・`Domain/Rules` の変更なし。Mapper の表示用文字列はテストを追加する（SC-011） |
| IV. 最小変更と事前確認 | ✅ | 新しい依存パッケージなし。既存の表の CSS は変えず、追加のみ。`YearHeaders` の型変更は、使用箇所（View 1、テスト 1）に限られる |
| V. 検証とメッセージの正本 | ✅ | 入力検証・検証メッセージに関わらない。「年」「夫」「妻」「歳」の表記は Mapper と View に置き、JavaScript に文言を持たせない |

技術的制約：`docs/common/ui-implementation-guidelines.md`（トークン、レスポンシブ、ARIA）に従う。改行コードは LF。

### 型・ファイルごとの配置（原則 II）

| 対象 | 種別 | 置き場所 | 内容 |
| --- | --- | --- | --- |
| `ViewModels/LifePlan/CashFlowYearHeaderViewModel.cs` | 新規 | ViewModels | 年度列の見出しの表示用データ（`Year`、`HusbandAgeText`、`WifeAgeText`）。[data-model.md](data-model.md) |
| `ViewModels/LifePlan/LifePlanResultViewModel.cs` | 変更 | ViewModels | `YearHeaders` の型を `IReadOnlyList<CashFlowYearHeaderViewModel>` に変更 |
| `Application/Mappers/LifePlanPageMapper.cs` | 変更 | Application/Mappers | 年次データから年度・年齢の表示用文字列を作る。年齢が空なら「夫 -」 |
| `Views/LifePlan/Index.cshtml` | 変更 | Views | 年度の `th` に年度・年齢の `span` を出力。表の枠を囲む要素（固定の範囲）を追加。新しい JS の読み込み |
| `wwwroot/css/life-plan.css` | 変更 | 静的ファイル | 年齢の出し分け（PC 幅で非表示、スマートフォン幅で 3 段）、固定用の見出しの入れ物（sticky、重ね、枠線）。既存ルールは変更しない |
| `wwwroot/js/life-plan-cash-flow.js` | 新規 | 静的ファイル | スマートフォン幅だけで見出しを複製し、列幅をコピーし、横位置を同期する。幅の変化・画面幅の切り替えに追従。[contracts/cash-flow-table-ui.md](contracts/cash-flow-table-ui.md) |
| `LifePlan.Tests/Application/Mappers/LifePlanPageMapperResultTests.cs` | 変更 | テスト | 年度ヘッダーのテストを新しい型に更新し、年齢・空の年度のテストを追加 |
| `Domain/**`、計算ロジック、他の ViewModel | 変更なし | — | — |

**Post-Design 再確認（Phase 1 後）**: 原則 II〜V に違反なし。原則 I も反映済みで違反なし。

## Project Structure

### Documentation (this feature)

```text
specs/004-cash-flow-sticky-header/
├── spec.md                  # 仕様（Clarifications 2026-10-12 を含む）
├── docs-update-proposal.md  # 正本 v1.4 6.2 への反映内容（反映済み）
├── plan.md                  # 本書
├── research.md              # Phase 0：方式比較と設計判断 R0〜R8
├── data-model.md            # Phase 1：表示用データと画面の状態
├── quickstart.md            # Phase 1：検証手順（自動・手動・実機）
├── contracts/
│   └── cash-flow-table-ui.md  # Phase 1：表の構造・クラス・ARIA・JS の振る舞い
├── checklists/
│   └── requirements.md      # 仕様の品質チェック
└── tasks.md                 # Phase 2（/speckit-tasks で作成。本コマンドでは作らない）
```

### Source Code (repository root)

```text
LifePlan/
├── Application/Mappers/LifePlanPageMapper.cs          # 変更
├── ViewModels/LifePlan/
│   ├── CashFlowYearHeaderViewModel.cs                 # 新規
│   └── LifePlanResultViewModel.cs                     # 変更
├── Views/LifePlan/Index.cshtml                        # 変更
└── wwwroot/
    ├── css/life-plan.css                              # 変更（追加のみ）
    └── js/life-plan-cash-flow.js                      # 新規

LifePlan.Tests/
└── Application/Mappers/LifePlanPageMapperResultTests.cs  # 変更
```

**Structure Decision**: 既存の単一 MVC プロジェクトの構成に従う。ページ固有の JS は機能名のファイルに分ける（`.github/copilot-introduction.md` の wwwroot/js）。既存の `life-plan-chart.js` と同じく、結果があるときだけ読み込む。

## 実装の順序と横スクロールを守る手順

1. 正本への反映（2026-10-12 反映済み）。
2. 修正前の計測（quickstart の A 系を `cfb8f53` で実行し、横スクロールの量・列幅・固定列の位置・PC 幅のスクリーンショットを記録）。
3. C#：ViewModel・Mapper・テスト（SC-011）。
4. View・CSS：年齢の 3 段表示（この時点では固定なし）。PC 幅のスクリーンショットが修正前と一致することを確認。
5. JS・CSS：固定用の見出し。
6. 検証用スクリプトで SC-012〜SC-017、修正前との比較（SC-013、SC-014）。
7. レビュー（`/review-changes`）、手動確認、実機確認の依頼（SC-010）。

手順 4 と 5 を分けることで、横スクロールに問題が出た場合に、原因が年齢の表示か固定の仕組みかを切り分けられる。固定の仕組みは JS ファイルとその読み込み 1 行を外せば修正前の挙動に戻る。

## Complexity Tracking

違反なし（記載不要）。
