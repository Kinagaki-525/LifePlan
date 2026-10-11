# Implementation Plan: シミュレーショングラフの操作性改善（ホバー表示・凡例整理・操作案内）

**Branch**: `002-simulation-chart-ui` | **Date**: 2026-10-12 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/002-chart-hover-usability/spec.md`（正本：`docs/simulator/index.md` v1.3 6.3）

## Summary

PCではマウスを合わせるとツールチップで年度別の金額を一時表示し、クリック・ドラッグ・キー操作による年度の固定とは分けて扱う。スマートフォンのタップによる固定は維持する。グラフ下部の凡例を削除し、表示項目チェックボックスに系列色のマーカーを付けて凡例の代わりにする。チェックボックスとグラフの間に入力方法別の操作案内を置き、数値表示欄の見出しと金額の強弱を整える。グラフと数値表示欄の間に、標準の年度選択欄（PC・スマートフォンとも常に表示。キーボード・読み上げソフトからも使う）を設け、Canvas は図としてフォーカス対象から外す（[research.md](research.md) R6）。

技術方針：既存の自前 Canvas 描画（`life-plan-chart.js`）を拡張し、外部ライブラリは導入しない。ツールチップはグラフ枠内に重ねる HTML 要素とし、ホバーは `pointerType` で判定する（[research.md](research.md) R1・R2）。C# のコード、ViewModel、計算ロジックは変更しない。

## Technical Context

**Language/Version**: JavaScript（ES2020 相当、ビルドなしで直接配信）、CSS、Razor（C# / .NET 10 のプロジェクト内）

**Primary Dependencies**: 既存の Canvas 2D API、Pointer Events、Bootstrap（チェックボックスのクラスのみ）。追加なし

**Storage**: N/A

**Testing**: xUnit（`LifePlan.Tests`、回帰確認のみ。C# の変更なし）。画面は Edge headless のハーネスと手動確認（[quickstart.md](quickstart.md)）

**Target Platform**: PC・スマートフォンのブラウザ（Edge、Chrome、Safari。iOS Safari・Android Chrome を含む）

**Project Type**: Web アプリケーション（単一 MVC プロジェクト＋テストプロジェクト）

**Performance Goals**: カーソルが対象年度の位置に移ってから1秒以内に表示（SC-002）。実際は年度が変わったときの再描画1回のみで、待ち時間なし（R9）

**Constraints**:
- Canvas 描画方式を維持し、外部グラフライブラリを導入しない（Clarifications、正本 6.3）。
- 計算ロジック・計算結果・キャッシュフロー表・系列の意味と色を変えない。
- PCのグラフの大きさとスマートフォン幅のグラフの高さを変えない（FR-024）。
- 業務ルール・計算を JavaScript に持たせない（憲章 原則 V）。JavaScript が行うのは、受け取った金額の表示書式の整形だけ（既存）。

**Scale/Scope**: 変更は3ファイル（`life-plan-chart.js`、`life-plan.css`、`Index.cshtml`）。C#・テストコードの変更なし

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| 原則 | 照合結果 | 根拠 |
| --- | --- | --- |
| I. 仕様の正本は docs/ | ✅ | 凡例・ホバー・操作案内の変更は、了承を得て `docs/simulator/index.md` v1.3 6.3 に反映済み（[docs-update-proposal.md](docs-update-proposal.md)）。spec.md は正本を参照 |
| II. レイヤー責務と依存方向 | ✅ | C# の型の追加・変更なし。View には既存の ViewModel（`ChartPoints`）のみを渡す。表示の振る舞いは `wwwroot/js`、見た目は `wwwroot/css` に置く |
| III. 計算ロジックのテスト | ✅（対象外） | `Domain/Logic`・`Domain/Rules` の変更なし。既存テストで回帰を確認する |
| IV. 最小変更と事前確認 | ✅ | 新しい依存パッケージなし。既存の関数（`bindYearSelection`、`renderDetail`、`drawChart`、`formatDetailAmount`）を拡張し、大規模な分割はしない。CSS は既存トークンを使う |
| V. 検証とメッセージの正本 | ✅ | 入力検証・検証メッセージに関わらない。案内文はビューに置く（入力検証メッセージではないため `LifePlanValidationMessages` の対象外。R5） |

技術的制約：`docs/common/ui-implementation-guidelines.md` の 4（トークン）、8（重複）、9（ARIA、ツールチップを `aria-hidden` にしない）に従う。改行コードは LF。

### 型・ファイルごとの配置（原則 II）

| 対象 | 種別 | 置き場所 | 内容 |
| --- | --- | --- | --- |
| `Views/LifePlan/Index.cshtml` | 変更 | View | 操作案内2文の追加、凡例要素の削除、ツールチップ要素の追加、年度選択欄（ラベルと `select`）の追加、Canvas の代替テキスト変更と `tabindex` の削除 |
| `wwwroot/js/life-plan-chart.js` | 変更 | 静的ファイル | ホバーの状態と描画、ツールチップの生成と配置、Esc、チェックボックスの色マーカー、凡例処理の削除、数値表示欄の見出し・文言、年度選択欄の選択肢生成・同期・←/→ の補完（Canvas のキー操作は削除）、操作案内の表示 |
| `wwwroot/css/life-plan.css` | 変更 | 静的ファイル | 操作案内の出し分け（`any-hover`/`any-pointer`）、ツールチップ、チェックボックスのマーカー、数値表示欄の強弱、凡例のスタイル削除 |
| `wwwroot/css/site.css` | 変更なし（予定） | — | 既存トークンで足りる。足りない場合は `:root` に追加し、理由を記録する |
| `LifePlanChartPointViewModel`、`LifePlanPageMapper` ほか C# | 変更なし | — | — |

**Post-Design 再確認（Phase 1 後）**: 違反なし。設計で追加した要素はすべて View・JS・CSS の中に収まり、C# の契約、計算、検証に影響しない。

## Project Structure

### Documentation (this feature)

```text
specs/002-chart-hover-usability/
├── spec.md                  # 仕様（Clarifications 2026-10-12 を含む）
├── docs-update-proposal.md  # 正本 v1.3 への反映内容（反映済み）
├── plan.md                  # 本書
├── research.md              # Phase 0：設計判断 R1〜R10
├── data-model.md            # Phase 1：画面の状態と遷移
├── quickstart.md            # Phase 1：検証手順
├── contracts/
│   └── chart-ui.md          # Phase 1：グラフ枠の構成・文言・ARIA・操作
├── checklists/
│   └── requirements.md      # 仕様の品質チェック
└── tasks.md                 # Phase 2（/speckit-tasks で作成。本コマンドでは作らない）
```

### Source Code (repository root)

```text
LifePlan/
├── Views/
│   └── LifePlan/
│       └── Index.cshtml          # 変更：グラフ枠の構成（操作案内・ツールチップ・説明文・ARIA、凡例の削除）
└── wwwroot/
    ├── js/
    │   └── life-plan-chart.js    # 変更：ホバー、ツールチップ、色マーカー、数値表示欄、ARIA
    └── css/
        └── life-plan.css         # 変更：操作案内、ツールチップ、マーカー、数値表示欄

LifePlan.Tests/                   # 変更なし（回帰確認のみ）
```

**Structure Decision**: 既存の単一 MVC プロジェクトの構成のまま、結果画面のビューと、グラフ専用の JS・CSS だけを変更する。新しいファイルは追加しない。

## 実装の段取り（tasks.md への入力）

利用場面の優先度に合わせ、次の順で小さく確認しながら進める。

1. **凡例の整理（US3）**：チェックボックスの色マーカー、凡例の削除。ほかの変更の土台になるため最初に行う。
2. **数値表示欄（US5）と未選択時の文言**：見出し「固定中」、強弱、「年度が選択されていません」。
3. **ホバー（US1）**：状態、実線の縦線と塗りマーカー、ツールチップの生成と配置、終了条件（枠外・Esc・ボタン押下）、タッチの除外。
4. **スマートフォンの回帰確認（US2）**：タップ・横移動・縦スクロール・ピンチ。
5. **操作案内（US4）**：2文の出し分け、表示の条件。
6. **アクセシビリティ（US6）**：年度選択欄（常に表示※、未選択の先頭項目、同期、←/→/Home/End）、Canvas の代替テキストとフォーカス対象からの除外、読み上げの重複がないこと。 ※当初は「フォーカス時だけ表示」。ユーザー指示で常時表示に変更（research.md R6 の再改訂）
7. **検証**：ビルド・テスト、ハーネス、各画面幅のスクリーンショット、変更レビュー（`change-reviewer`）。

## リスクと対応

| リスク | 対応 |
| --- | --- |
| iOS Safari で `touch-action: pan-y pinch-zoom` の解釈に差があり、横移動の連続固定が効かない可能性 | 001 からの既知事項。タップでの固定は影響を受けない。実機確認の項目として quickstart M2・M3 に残す |
| タッチ後の互換マウスイベントでツールチップが残る | `pointerType` で判定し、`mouse*` イベントは使わない（R2）。ハーネスで、タッチの後にツールチップが出ないことを確認する |
| ツールチップがクリックを遮る | ツールチップはホバー年度の縦線の横に置き、カーソル位置を覆わない（R3） |
| キー操作のフォーカス先が Canvas から年度選択欄に移る | ←/→/Home/End の割り当ては維持し、Tab 順はチェックボックスの直後にする（R6）。キーボードで確認する（quickstart K1・K2） |
| 年度選択欄を常に表示するため、結果画面が1行分（狭い幅では折り返して2行分）縦に伸びる | FR-024 で許容する。320〜430px と 1280px のスクリーンショットで、見切れやはみ出しがないことを確認する（quickstart M6）。※当初の「フォーカスした間だけ数値表示欄が上下に動く」というリスクは、常時表示への変更でなくなった |
| スマートフォンの読み上げソフトでの操作 | 標準の `select` を使うため、ARIA の独自部品より確実。実機で確認する（quickstart K5） |

## Complexity Tracking

憲章の違反はないため記載なし。
