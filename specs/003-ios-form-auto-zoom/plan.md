# Implementation Plan: iOS端末でフォーム操作時に画面が自動拡大される問題の解消

**Branch**: `003-ios-form-auto-zoom` | **Date**: 2026-10-12 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/003-ios-form-auto-zoom/spec.md`（正本：`docs/common/ui-implementation-guidelines.md` 6・12。GitHub Issue #56）

## Summary

主な入力装置がタッチの端末（`pointer: coarse`）では、文字を入力・選択するフォーム部品（入力欄・選択欄・本文欄）の文字の大きさを 16px にする。iOS のブラウザは、16px 未満の部品にフォーカスすると画面を自動で拡大する。この原因を解消し、画面幅によらず iPhone の縦・横向きと iPad を対象にする（[research.md](research.md) R1）。PC の見た目、ラベル・単位などの文字の大きさ、viewport の設定は変えない。

技術方針：`site.css` の `:root` にトークンを 1 つ追加し、メディアクエリで囲んだ 1 つの規則を加える。規則は、要素名とクラスを組み合わせた詳細度にして、画面ごとの CSS より優先させる（R2、R3）。Razor・JavaScript・C# は変更しない。

## Technical Context

**Language/Version**: CSS（ビルドなしで直接配信）。プロジェクトは C# / .NET 10 の ASP.NET Core MVC

**Primary Dependencies**: Bootstrap 5.3.3（`form-control`、`form-select` のクラス）。追加なし

**Storage**: N/A

**Testing**: xUnit（`LifePlan.Tests`、回帰確認のみ。C# の変更なし）。計算後の文字の大きさは Edge headless で確認し、自動拡大は実機で確認する（[quickstart.md](quickstart.md)）

**Target Platform**: iOS のブラウザ（Safari、Chrome など。iPhone の縦・横、iPad）、Android のブラウザ、PC のブラウザ（Edge、Chrome、Safari）

**Project Type**: Web アプリケーション（単一 MVC プロジェクト＋テストプロジェクト）

**Performance Goals**: N/A（CSS の規則 1 つの追加）

**Constraints**:
- viewport に拡大の制限を加えない（FR-004、FR-005、UI ガイドライン 6）。
- PC では、フォーム部品の文字の大きさ・大きさ・配置を、画面幅によらず変えない（FR-012）。
- ラベル・単位・補足文・エラー文、フォーム以外の文字の大きさを変えない（FR-009、FR-011）。
- 送信項目、入力値の正規化、入力検証を変えない（FR-006）。

**Scale/Scope**: 変更は `wwwroot/css/site.css`（実装時の確認で `life-plan.css` にも 1 か所追加。R4 の追記）。対象の部品は 2 画面の画面上 75 個。高さ・幅の調整は、確認で問題が出た場合だけ加える（R4）

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | 照合結果 | 根拠 |
| --- | --- | --- |
| I. 仕様の正本は docs/ | ✅ | 規則は、了承を得て UI ガイドライン 6・12 に反映済み（2026-10-12）。spec.md は正本を参照する。`docs/simulator/index.md` には文字の大きさの規定がなく、変更しない（R6） |
| II. レイヤー責務と依存方向 | ✅ | C# の型の追加・変更なし。見た目の変更は `wwwroot/css` に閉じる |
| III. 計算ロジックのテスト | ✅（対象外） | `Domain/Logic`・`Domain/Rules` の変更なし。既存テストで回帰を確認する |
| IV. 最小変更と事前確認 | ✅ | 新しい依存パッケージなし。文字の大きさの指定は `site.css` の 1 か所で、画面ごとの CSS には同じ意図の指定を重複させない（R3）。幅・並びの調整は、共通トークンの置き換えを `site.css` に、教育プランの 2 列を `life-plan.css` に置く（R4 の追記）。新しいトークンは、既存のスケール（`rem`）では条件を満たせないため追加する（R2、UI ガイドライン 4） |
| V. 検証とメッセージの正本 | ✅ | 入力検証・検証メッセージに関わらない |

技術的制約：`docs/common/ui-implementation-guidelines.md` の 4（トークンは `:root`）、6（今回追記した規則）、8（重複）に従う。改行コードは LF。

### 型・ファイルごとの配置（原則 II）

| 対象 | 種別 | 置き場所 | 内容 |
| --- | --- | --- | --- |
| `wwwroot/css/site.css` | 変更 | 静的ファイル | `:root` に `--ui-font-size-control-touch: 16px` を追加。`@media (pointer: coarse)` で `input.form-control`、`select.form-select`、`select.form-control`、`textarea.form-control` の文字の大きさを指定（[contracts/form-controls.md](contracts/form-controls.md)） |
| `wwwroot/css/site.css`（実装時の追加） | 変更 | 静的ファイル | `:root` に `--ui-period-control-width-touch: 120px` を追加し、同じメディアクエリで `--ui-period-control-width` を置き換える（年齢の横並びと教育プランの欄の幅。R4 の追記） |
| `wwwroot/css/life-plan.css` | 変更（実装時の追加） | 静的ファイル | `@media (pointer: coarse)` で教育プランを 2 列にする（幅 992px 以上で 4 列では収まらないため。992px 未満は既存どおり 1 列。R4 の追記） |
| `wwwroot/css/contact.css` | 変更なし | — | — |
| `Views/**/*.cshtml`、`_Layout.cshtml` | 変更なし | — | viewport の設定を含めて変えない |
| C#（ViewModel、Mapper、Validator など） | 変更なし | — | — |

**Post-Design 再確認（Phase 1 後）**: 違反なし。設計は CSS の 1 ファイルに収まり、C# の契約、計算、検証、送信項目に影響しない。

## Project Structure

### Documentation (this feature)

```text
specs/003-ios-form-auto-zoom/
├── spec.md              # 仕様（Clarifications 2026-10-12 を含む）
├── plan.md              # 本書
├── research.md          # Phase 0：設計判断 R1〜R6
├── data-model.md        # Phase 1：表示の条件と、部品ごとの文字の大きさ
├── quickstart.md        # Phase 1：検証手順（自動確認・画面確認・実機確認）
├── contracts/
│   └── form-controls.md # Phase 1：フォーム部品の文字の大きさの CSS の約束
├── checklists/
│   └── requirements.md  # 仕様の品質チェック
└── tasks.md             # Phase 2（/speckit-tasks で作成。本コマンドでは作らない）
```

### Source Code (repository root)

```text
LifePlan/
└── wwwroot/
    └── css/
        ├── site.css      # 変更：トークン 2 つと、タッチの端末向けの規則（文字の大きさ、年齢欄・教育プランの欄の幅）
        └── life-plan.css # 変更：タッチの端末で教育プランを 2 列にする

LifePlan.Tests/           # 変更なし（回帰確認のみ）
```

**Structure Decision**: 既存の単一 MVC プロジェクトの構成のまま、共通 CSS の `site.css` だけを変更する。新しいファイルは追加しない。

## 実装の段取り（tasks.md への入力）

1. **修正前の記録**：`develop`（`2f48e21`）で、quickstart の自動確認（A1〜A5 の計測とスクリーンショット）を行い、修正前の値を記録する。
2. **CSS の変更**：`site.css` にトークンと規則を追加する。
3. **自動確認**：タッチ相当で 75 個が 16px、PC 相当で修正前と同じ値・同じ見た目であることを確認する（A1〜A5）。
4. **画面確認**：幅 320px と iPhone の横向き相当の幅で、配置・切れ方・エラー表示を確認する（L1〜L6）。問題が出た場合だけ、`site.css` の同じメディアクエリの中で最小限の調整を加える。
5. **ビルド・テスト・変更レビュー**：`dotnet build`、`dotnet test`、`change-reviewer`。
6. **実機確認**：iPhone（Safari・Chrome、縦・横）、iPad、Android、PC（D1〜D6）。手元にない端末は「未確認」として PR に明記する。

## リスクと対応

| リスク | 対応 |
| --- | --- |
| 16px が自動拡大の境目であることは Apple の公式文書に明記がなく、iOS の更新で変わりうる | 実機確認（D1〜D3）を受け入れの条件にする（spec Assumptions） |
| トラックパッドをつないだ iPad が PC 扱いになり、自動拡大が残る | spec で許容済み。D3 で結果を記録する |
| `pointer: coarse` を正しく返さない端末がある | 主要な端末（iPhone、iPad、Android Chrome）は実機で確認する。判定が外れても、修正前と同じ状態（自動拡大が起きる）に戻るだけで、表示は崩れない |
| iPhone の横向き・小型タブレット（幅 576〜991px）で、職業などの選択欄の末尾が切れる | 選択は標準の一覧で全文を見てできる。L3 で切れ方を確認し、意味が分からなくなる場合だけ幅を調整する（R4） |
| 今後、画面ごとの CSS で `select.form-select` などに文字の大きさを指定すると、規則が負ける | 約束 3（[contracts/form-controls.md](contracts/form-controls.md)）と UI ガイドライン 6 で、16px 未満にしないことを明記している。変更レビューで確認する |
| 起動オプションでタッチを再現できず、自動確認ができない | 開発者ツールのデバイスモードで手動確認に切り替える（quickstart 2） |

## Complexity Tracking

憲章の違反はないため記載なし。
