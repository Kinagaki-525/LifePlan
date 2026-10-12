---

description: "Task list for 004-cash-flow-sticky-header"
---

# Tasks: スマートフォン版キャッシュフロー表の年度ヘッダーに年齢を表示し、縦スクロール中も固定する

**Input**: Design documents from `specs/004-cash-flow-sticky-header/`

**Prerequisites**: [plan.md](plan.md)、[spec.md](spec.md)、[research.md](research.md)、[data-model.md](data-model.md)、[contracts/cash-flow-table-ui.md](contracts/cash-flow-table-ui.md)、[quickstart.md](quickstart.md)

**Tests**: spec.md の SC-011（Mapper の自動テスト）と SC-012〜SC-017（ブラウザの自動操作による確認）が要件のため、テスト・確認のタスクを含める。検証用スクリプトはスクラッチに置き、リポジトリに含めない（research.md R8）。

**Organization**: タスクはユーザーストーリーごとにまとめる。パスはリポジトリルートからの相対パス。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 並行して実行できる（別ファイルで、未完了のタスクに依存しない）
- **[Story]**: 対応するユーザーストーリー（US1〜US4）

---

## Phase 1: Setup（修正前の基準値と検証の準備）

**Purpose**: 横スクロールを壊していないことを比べるための、修正前の基準値を記録する（plan「実装の順序」2、quickstart 1）

- [ ] T001 `dotnet build LifePlan.slnx -m:1` と `dotnet test LifePlan.slnx -m:1` を実行し、修正前の結果（合格件数）を記録する。.NET SDK がない環境では導入してから実行する
- [ ] T002 スクラッチに Playwright（Node.js）＋ Chromium の検証用スクリプトを作る：アプリ（`http://localhost:5250`）の `/LifePlan` で夫・妻の年齢を入力してフォームを送信し、結果画面を表示する関数、幅 320 / 375 / 390 / 430px（`isMobile`・`hasTouch`、高さ 700px）と 1280px（高さ 800px）の設定、`scrollTo({ top, behavior: 'instant' })` によるページのスクロール、表の枠（`.life-plan-cash-flow`）の `scrollLeft` の設定と計測を含める（quickstart 3 の共通の設定。リポジトリに含めない）
- [ ] T003 `develop`（`cfb8f53`）の状態でアプリを起動し、T002 のスクリプトで修正前の基準値を記録する：5 幅それぞれの表の枠の `scrollWidth − clientWidth`、`tbody` 1 行目の各セルの幅、固定列（スマートフォン幅は「項目」、PC 幅は「区分」「項目」）の左端、スワイプ（CDP `Input.dispatchTouchEvent`）・ホイール（`page.mouse.wheel`）・矢印キーそれぞれの後の `scrollLeft`、1280px の表の枠のスクリーンショット 3 状態（縦スクロール前・縦スクロール後・横スクロール後）、JavaScript 無効時の 390px の表のセル文字、5 幅それぞれの表の全セルの文字（`thead`・`tbody`、夫 30・妻 26 と夫 26・妻 30）とグラフ用データ（`#life-plan-chart-data` の JSON）（quickstart A5、A6、A9、A13 の修正前の値）

**Checkpoint**: 修正前の基準値が記録され、以降の比較に使える

---

## Phase 2: Foundational（全ストーリーの前提）

**Purpose**: 年度ヘッダーの表示用データの型を変える。US1〜US4 のすべてが、変更後の `YearHeaders` を前提にする

- [ ] T004 [P] `LifePlan/ViewModels/LifePlan/CashFlowYearHeaderViewModel.cs` を新規作成する：`Year`（`string`、例 `"2026"`、「年」は付けない）、`HusbandAgeText`（`string`、例 `"夫27歳"`、空なら `"夫 -"`）、`WifeAgeText`（`string`、例 `"妻26歳"`、空なら `"妻 -"`）。既存の `CashFlowTableRowViewModel` と同じ書き方（`namespace` ブロック、`= string.Empty` の初期値）にする（data-model.md 2）
- [ ] T005 `LifePlan/ViewModels/LifePlan/LifePlanResultViewModel.cs` の `YearHeaders` の型を `IReadOnlyList<string>` から `IReadOnlyList<CashFlowYearHeaderViewModel>` に変える（data-model.md 3）
- [ ] T006 `LifePlan/Application/Mappers/LifePlanPageMapper.cs` の `ToResultViewModel` で、`YearHeaders` を年次データ（`AnnualCashFlowRow`）の並びから 1 対 1 に作る：`Year = row.Year.ToString()`、`HusbandAgeText` は `HusbandAge` が値を持てば `"夫{年齢}歳"`、`null` なら `"夫 -"`（半角空白＋半角ハイフン）、妻も同じ規則。年齢は整数をそのまま文字列にし、計算しない（research.md R4、FR-002〜FR-004）
- [ ] T007 `LifePlan/Views/LifePlan/Index.cshtml` の年度の `th` を、新しい型の `Year` を出力するように直す（この時点では年齢を出さず、表示は修正前と同じ「2026」）。`dotnet build LifePlan.slnx -m:1` が通ることを確認する

**Checkpoint**: ビルドが通り、画面の表示は修正前と同じ

---

## Phase 3: User Story 1 - 年度ヘッダーで夫婦の年齢がわかる (Priority: P1) 🎯 MVP

**Goal**: スマートフォン幅の年度ヘッダーに「2026年」「夫27歳」「妻26歳」を 3 段で表示する。PC 幅は「2026」のまま

**Independent Test**: 幅 375px で結果を表示し、全年度列の見出しの年齢が、同じ列の「夫（年齢）」「妻（年齢）」行と一致する。夫 30・妻 26 で 2097〜2100 年が「夫 -」（quickstart A1）

### Tests for User Story 1

- [ ] T008 [P] [US1] `LifePlan.Tests/Application/Mappers/LifePlanPageMapperResultTests.cs` の `ToResultViewModel_CreatesYearHeadersFromAnnualRows` を新しい型に合わせて更新し（`Year` が `["2026", "2027"]`）、テストを追加する：年齢あり（quickstart U1）、夫の年齢が `null` で `"夫 -"`（U2）、妻の年齢が `null` で `"妻 -"`（U3）、`YearHeaders.Count` と各行の `Values.Count` が等しく、見出しの年齢と「夫（年齢）」「妻（年齢）」行の値が全列で対応する（U4）。既存の `CreateAnnualRow` ヘルパーを使い、必要なら年齢の引数を足す
- [ ] T009 [US1] `dotnet test LifePlan.slnx -m:1` を実行し、追加・更新したテストと既存テストがすべて合格することを確認する（SC-011、SC-008）

### Implementation for User Story 1

- [ ] T010 [US1] `LifePlan/Views/LifePlan/Index.cshtml` の年度の `th` を contracts/cash-flow-table-ui.md 1 の構造にする：`<span class="life-plan-cash-flow__year">@header.Year<span class="life-plan-cash-flow__compact">年</span></span>`、続けて `life-plan-cash-flow__age life-plan-cash-flow__compact` の `span` を夫・妻の順に 2 つ。`span` の間はテンプレート上で改行して空白を入れる。`th scope="col"`、「区分」「項目」の見出し、`tbody`、`caption` は変更しない
- [ ] T011 [US1] `LifePlan/wwwroot/css/life-plan.css` にルールを追加する（既存ルールは変更しない）：`.life-plan-cash-flow__compact { display: none; }`、`@media (max-width: 575.98px)` 内で `.life-plan-cash-flow__compact` を表示し、`.life-plan-cash-flow__age { display: block; }`。年度の段と年齢の段の文字の太さ・大きさ・行間は既存のトークンで整え、`white-space: nowrap` のまま列の最小幅（6.5rem）に収める（research.md R5、FR-006）
- [ ] T012 [US1] 検証用スクリプトで quickstart A1（5 幅のうちスマートフォン幅 4 つ、夫 30・妻 26 と夫 26・妻 30）と A10（幅 320px・文字 200%）を実行し、年齢の一致・「夫 -」「妻 -」・はみ出しなしを確認する。1280px で年度見出しが「2026」だけであることも確認する

**Checkpoint**: スマートフォン幅で年齢が表示され、PC 幅は変わらない。固定はまだない

---

## Phase 4: User Story 2 - 縦スクロール中も年度ヘッダーが画面上部に見え続ける (Priority: P1)

**Goal**: スマートフォン幅で、ページを縦にスクロールしている間、年度ヘッダーを画面上端に固定し、表の終端で解除する

**Independent Test**: 幅 375px で、表の前・先頭付近・途中・終端付近・後の 5 位置で、見出しの上端が「元の位置／画面上端／画面外」になる（quickstart A3、M1）

### Implementation for User Story 2

- [ ] T013 [US2] `LifePlan/Views/LifePlan/Index.cshtml` で、表の枠（`.life-plan-cash-flow`）を `<div class="life-plan-cash-flow-area" data-life-plan-cash-flow>` で囲む（スクロールの案内と単位の注記は囲みの外のまま）。結果があるときだけ `<script src="~/js/life-plan-cash-flow.js" asp-append-version="true"></script>` を `life-plan-chart.js` の隣で読み込む（contracts 1）
- [ ] T014 [US2] `LifePlan/wwwroot/js/life-plan-cash-flow.js` を新規作成する（既存の JS と同じく即時関数で囲む）。contracts 3 の J1〜J4・J6 を実装する：`[data-life-plan-cash-flow]` がなければ終了／`matchMedia('(max-width: 575.98px)')` が一致したら元の `thead` を `cloneNode(true)` で複製し、`id` 属性を取り除き、`class="life-plan-cash-flow__table"` の `table` に入れて、`aria-hidden="true"` と `inert` を付けた `.life-plan-cash-flow__sticky-head` に入れ、表の枠の直前に挿入する／元の見出しの各セルの実測幅（`getBoundingClientRect().width`）を複製のセルの `width`・`min-width`・`max-width` に、元の表の実測幅を複製の表の幅に設定し、元の `thead` の高さを CSS 変数（例：`--life-plan-cash-flow-head-height`）で渡す／表の枠の `scroll`（`passive`）で複製の入れ物の `scrollLeft` に表の枠の `scrollLeft` を直接代入する（`requestAnimationFrame` を使わない。複製から表の枠へは書き戻さない）／一致しなくなったら複製・CSS 変数・イベントを取り除く。業務ルール・計算・表示文言は持たない
- [ ] T015 [US2] `LifePlan/wwwroot/css/life-plan.css` に固定用の見出しのルールを追加する（既存ルールは変更しない）：`.life-plan-cash-flow__sticky-head` は `position: sticky; top: 0`、`overflow: hidden`、`pointer-events: none`、表の固定列より手前の `z-index`、不透明な背景。表の枠の上側の枠線・角丸と合わせ、表の枠は CSS 変数の高さだけ上へ重ねる（負の `margin-top`）。重ねる指定は、JS が複製を挿入したときに囲みへ付ける `life-plan-cash-flow-area--sticky` クラスがあるときだけ効かせる（contracts 2）。`.life-plan-cash-flow-area` とその祖先に `overflow`・`contain`・`transform` を指定しない旨のコメントを残す（research.md R3、R7）
- [ ] T016 [US2] 検証用スクリプトで quickstart A3 をスマートフォン幅 4 つで実行し、固定・解除・表の後（単位の注記、グラフ、前提条件の各位置）で画面外になることを確認する。表の前・先頭付近で、複製の上端が元の `thead` の上端と一致すること（R3 の試作で出た 2〜3px のずれが解消していること）を確認する。固定中の見出しの高さが画面の高さ（700px）の 25% 以下であること、表の最終行（残高推移）が見出しの直下に完全に表示される縦位置があることを確認する（FR-012）。計測は `document.fonts.ready` の後に行う（T017 の幅の監視の前でも結果が安定するように）

**Checkpoint**: スマートフォン幅で見出しが固定・解除される。横位置の同期は動くが、組み合わせの確認は US3 で行う

---

## Phase 5: User Story 3 - 縦・横のスクロールを組み合わせても列がずれない (Priority: P1)

**Goal**: どのスクロール状態でも見出しと金額の列が一致し、既存の横スクロールと「項目」列の固定が修正前と同じ

**Independent Test**: 横 3 位置 × 縦 5 位置の全組み合わせで列の差が 1px 以内、`scrollLeft` が不変、修正前と横スクロールの量・列幅・固定列の位置が一致（quickstart A2、A4、A5）

### Implementation for User Story 3

- [ ] T017 [US3] `LifePlan/wwwroot/js/life-plan-cash-flow.js` に contracts 3 の J5 を追加する：`ResizeObserver` で元の表の大きさの変化を検知し、列幅・表の幅・見出しの高さの設定と横位置の同期をやり直す（回転、文字の拡大、結果エリアの表示・非表示、フォントの読み込み）。J6 で監視を解除する
- [ ] T018 [US3] 検証用スクリプトで quickstart A2・A4 をスマートフォン幅 4 つで実行する（横 3 位置 × 縦 5 位置）。列の左端・幅の差が 1px 以内、縦スクロールの前後で `scrollLeft` が不変、「項目」列と「項目」見出しの左端が表の枠の内側の左端、ページの幅が画面幅以下であることを確認する
- [ ] T019 [US3] 検証用スクリプトで quickstart A5 を 5 幅で実行し、T003 の基準値と比べる：横スクロールできる量・各列の幅・固定列の左端が一致し、スワイプ・ホイール・矢印キーのいずれでも横にスクロールする。スマートフォン幅では、見出しの位置（固定前・固定中）でのスワイプでも表の枠がスクロールすることを確認する（FR-020、SC-013）。あわせて quickstart A13 を実行し、表の全セルの文字とグラフ用データが T003 と一致することを確認する（年度見出しは、修正後の 1280px で修正前と同じ「2026」、スマートフォン幅で年齢の段が増えたことだけが差になる。SC-008）
- [ ] T020 [US3] 検証用スクリプトで quickstart A7（390px → 1280px → 390px）、A11（夫婦とも 16 歳、夫婦とも 95 歳、子ども 2 人）、A12（再試算）を実行し、A2〜A4 が成り立つことを確認する

**Checkpoint**: 横スクロールが修正前と同じで、組み合わせ操作でも列がずれない

---

## Phase 6: User Story 4 - PC の表示・操作は修正前と変わらない (Priority: P2)

**Goal**: PC 幅の見た目と操作が修正前と同じ

**Independent Test**: 1280px の表の枠のスクリーンショット 3 状態が修正前と画素単位で一致（quickstart A6）

### Implementation for User Story 4

- [ ] T021 [US4] 検証用スクリプトで quickstart A6 を実行し、T003 の 1280px のスクリーンショット 3 状態と画素単位で比べる。差がある場合は原因（追加した囲みの要素、`__compact` の非表示など）を直し、再度比べる。1280px で `.life-plan-cash-flow__sticky-head` が存在しないことも確認する（FR-021、SC-014）

**Checkpoint**: PC 幅に差がない

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: アクセシビリティ、JavaScript 無効時、レビュー、記録

- [ ] T022 [P] 検証用スクリプトで quickstart A8（`ariaSnapshot()`、スマートフォン幅と PC 幅）を実行し、年度の見出しが列ごとに 1 回だけ、名前がスマートフォン幅で「2026年 夫27歳 妻26歳」、PC 幅で「2026」、複製が現れないことを確認する（SC-016、FR-025、FR-026）
- [ ] T023 [P] 検証用スクリプトで quickstart A9（`javaScriptEnabled: false`、390px）を実行し、T003 の基準値と比べて表の文字・横スクロール・「項目」列の固定が同じで、年齢が表示されることを確認する（SC-015）
- [ ] T024 `dotnet build LifePlan.slnx -m:1` と `dotnet test LifePlan.slnx -m:1` を実行し、結果を記録する。ネットワーク制限による警告は、ビルド・テストの結果と分けて記録する
- [ ] T025 `/review-changes`（または `change-reviewer`）で変更をレビューする。対象・比較元（`cfb8f53`）・差分・未追跡ファイル一覧・T012〜T024 の検証結果を渡す。指摘を直したら、影響する確認（T012、T016、T018〜T023）をやり直す
- [ ] T026 quickstart 4 の手動確認（M1〜M6、ブラウザの開発者ツールのデバイスモード。M5 ではブラウザの拡大 200% も確認する）を行い、結果を記録する。この環境で操作できない項目は、未確認として報告に含める
- [ ] T027 完了報告を作成する：変更したファイル、採用した方式と理由（research.md R1）、PC 版・スマートフォン版の変更結果、横スクロール・縦スクロールの確認結果（T012〜T023 の数値）、ビルド・自動テストの結果、実機で未検証の事項（quickstart 5 の D1〜D7。特に iPhone での慣性スクロール中の横の追従、アドレスバーの伸縮）、既存機能への影響、追加で判断が必要な事項。あわせて、実機確認（quickstart 5）をユーザーに依頼する

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup（Phase 1）**：依存なし。T003 は T002 の後
- **Foundational（Phase 2）**：Setup の後。US1〜US4 のすべてをブロックする（`YearHeaders` の型変更でビルドが通るまで）
- **US1（Phase 3）**：Foundational の後
- **US2（Phase 4）**：Foundational の後。年齢の 3 段表示（US1）がなくても動くが、見出しの高さは US1 の後に確定するため、**US1 の後に行う**
- **US3（Phase 5）**：US2 の後（同じ JS ファイルを拡張し、固定の状態で確認するため）
- **US4（Phase 6）**：US1・US2 の後（PC 幅に影響しうる変更がそろってから比べる）
- **Polish（Phase 7）**：すべてのストーリーの後

### User Story Dependencies

```text
Setup → Foundational → US1 → US2 → US3
                         └──────┴──→ US4 → Polish
```

### Within Each User Story

- US1：テスト（T008）→ 実装（T010、T011）→ 確認（T012）。T009 は T006・T008 の後
- US2：View（T013）→ JS（T014）→ CSS（T015）→ 確認（T016）
- US3：JS（T017）→ 確認（T018〜T020）

### Parallel Opportunities

- T004 は T005〜T007 と別ファイルだが、T005 が T004 の型に依存するため、実質は T004 → T005 → T006 → T007
- T008（テスト）は T010・T011（View・CSS）と別ファイルで、並行できる
- T022 と T023 は互いに独立した確認で、並行できる

## Parallel Example: User Story 1

```text
Task: "T008 [US1] LifePlanPageMapperResultTests.cs に年度ヘッダーのテストを追加"
Task: "T010 [US1] Index.cshtml の年度の th に年齢の span を出力"  （T011 は T010 の後）
```

## Implementation Strategy

### MVP First（User Story 1 のみ）

1. Phase 1（基準値）→ Phase 2（型の変更）→ Phase 3（年齢の 3 段表示）
2. **止めて確認**：スマートフォン幅で年齢が出て、PC 幅が変わらない。この時点で、固定はないが年度と年齢の対応は見出しで確認できる

### Incremental Delivery

1. US1（年齢の表示）→ 確認
2. US2（縦の固定）→ 確認
3. US3（横スクロールとの組み合わせ・修正前との比較）→ 確認
4. US4（PC 幅の比較）→ Polish → 実機確認の依頼

固定の仕組み（US2・US3）に問題が出た場合は、`life-plan-cash-flow.js` の読み込み 1 行を外せば、US1 の状態（年齢の表示あり・固定なし）に戻せる（plan「実装の順序」）。

## Notes

- 既存の表の枠・表・固定列の CSS ルールは変更しない。追加のルールだけで実現する（FR-020、contracts 2）
- 検証用スクリプト、計測値、スクリーンショットはスクラッチに置き、リポジトリに含めない
- 各フェーズの終わりにコミットする
