---

description: "Task list for シミュレーショングラフの操作性改善"
---

# Tasks: シミュレーショングラフの操作性改善（ホバー表示・凡例整理・操作案内）

**Input**: Design documents from `specs/002-chart-hover-usability/`

**Prerequisites**: plan.md、spec.md、research.md、data-model.md、contracts/chart-ui.md、quickstart.md

**Tests**: C# のコード・計算ロジックは変更しないため、単体テストの追加はない（憲章 原則 III は対象外）。既存テストで回帰を確認する。画面の挙動は、スクラッチに置く Edge headless のハーネスと手動確認で検証する（research.md R10、quickstart.md）。ハーネスはリポジトリに含めない。

**Organization**: spec.md のユーザーストーリー（US1〜US6）ごとに分ける。正本は `docs/simulator/index.md` v1.3 6.3。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 並行実行可（別ファイル、未完了タスクへの依存なし）
- **[Story]**: 対応するユーザーストーリー（US1〜US6）
- パスはリポジトリルートからの相対パス。変更するのは次の3ファイルだけ（plan.md）
  - `LifePlan/Views/LifePlan/Index.cshtml`
  - `LifePlan/wwwroot/js/life-plan-chart.js`
  - `LifePlan/wwwroot/css/life-plan.css`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: 着手前の状態確認と検証の準備

- [X] T001 `dotnet build LifePlan.slnx -m:1` と `dotnet test LifePlan.slnx -m:1` を実行し、変更前のテスト件数と成功状態を記録する。VS のデバッガーが DLL をロックしている場合は `--artifacts-path <スクラッチ>` を付ける。ネットワーク由来の警告は分けて記録する
  - 2026-10-12 実施：VS のデバッガーが DLL をロックしていたため `--artifacts-path` で実行。ビルドは警告0・エラー0、テストは125件すべて合格。ネットワーク由来の警告なし
- [X] T002 [P] スクラッチに検証用ハーネス（001 で使った harness.html・frames.html を基に、実 CSS・JS を `file://` で読み込み、合成 PointerEvent・KeyboardEvent を送り、結果を `document.title` に出す）を用意する。基準コミット `ef3435f` の状態で、既存シナリオ（tap、first、last、cancel、firstcancel、twofinger、none、two、keys、round、resize）が通ることを確認する。リポジトリには追加しない
  - 2026-10-12 実施：基準コミットと同じコードで既存シナリオがすべて通ることを確認済み。新しい画面構成に合わせて harness2.html を作り直し、シナリオを追加した（none、hover、hoverfixed、leave、esc、edges、buttons、touch、cancel、firstcancel、twofinger、series、keys、round、resize、sweep）

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: ツールチップと数値表示欄が共有する部品、重ね表示の土台を用意する

**⚠️ CRITICAL**: このフェーズが終わるまでストーリーに着手しない

- [X] T003 LifePlan/wwwroot/js/life-plan-chart.js の `renderDetail` から、項目一覧（`dl.life-plan-chart__detail-list`：色マーカー＋項目名＋金額）を作る処理を `createSeriesValueList(point, visibleSeries)` として切り出す。数値表示欄（US5）とツールチップ（US1）で共用する。表示内容と DOM は変えない
- [X] T004 LifePlan/wwwroot/css/life-plan.css の `.life-plan-chart__canvas-wrap` に `position: relative` を加え、内部に絶対配置の要素を重ねられるようにする。PC・スマートフォン幅でグラフの大きさが変わらないことを確認する（FR-024）

**Checkpoint**: ハーネスの既存シナリオがすべて基準コミットと同じ結果になる

---

## Phase 3: User Story 1 - PCでカーソルを合わせて金額を一時確認する (Priority: P1) 🎯 MVP

**Goal**: マウスのホバーで年度別の金額をツールチップに一時表示し、固定中の年度は変えない

**Independent Test**: quickstart.md の P1〜P8。2040年を固定した後に2050年へホバーし、ツールチップは2050年、数値表示欄と破線は2040年のままであること。枠外へ出す・Esc で消えること

### Implementation for User Story 1

- [X] T005 [US1] LifePlan/Views/LifePlan/Index.cshtml の `.life-plan-chart__canvas-wrap` 内、Canvas の後に `<div class="life-plan-chart__tooltip" data-life-plan-chart-tooltip role="tooltip" hidden></div>` を追加する。`aria-hidden` は付けない（contracts/chart-ui.md「ツールチップ」）
- [X] T006 [US1] LifePlan/wwwroot/js/life-plan-chart.js に、ホバーの状態 `hoverIndex`（初期値なし）と `hoverDismissed`（初期値偽）を追加する。遷移は data-model.md「ホバー中の年度」の表のとおり実装する
  - Canvas の `pointermove` で、`pointerType === 'mouse'` かつ `buttons === 0` のときだけ `hoverIndex` を更新する
  - 年度が変わったら `hoverDismissed` を偽に戻す
  - グラフ枠（`.life-plan-chart__canvas-wrap`）の `pointerleave` で `hoverIndex` をなしにする
  - マウスの `pointerdown` で `hoverIndex` をなしにする
  - `document` の `keydown` で Esc が押され、ツールチップを表示中なら `hoverDismissed` を真にする
  - ResizeObserver による再描画では `hoverIndex` をなしにする
  - タッチ・ペンでは `hoverIndex` を変えない
  - `mouseenter`／`mouseleave` などの互換マウスイベントは使わない（research.md R2）
  - 年度が変わらない `pointermove` では再描画しない（R9）
- [X] T007 [US1] LifePlan/wwwroot/js/life-plan-chart.js の `drawChart` に、ホバーの描画 `drawHover` を追加する
  - 実線の細い縦線（`--ui-color-muted`、1px）と、系列色で塗りつぶした小さめのマーカーを描く
  - `drawSelection`（破線＋白抜き）より先に描き、固定表示を上に重ねる
  - `hoverIndex` がない、または `hoverDismissed` が真のときは描かない（research.md R4）
- [X] T008 [US1] LifePlan/wwwroot/js/life-plan-chart.js に `renderTooltip` を追加する
  - 内容：見出し「{年}年」＋ラベル「カーソル位置」、T003 の `createSeriesValueList`。表示項目がなければ「表示項目が選択されていません。」
  - 縦方向はプロット領域の上端に置く。横方向はホバー年度の x 座標の右に余白を空けて置き、グラフ枠の右端からはみ出す場合は左に置く。それでも収まらなければ枠内に詰める（research.md R3）
  - ホバーがない、または抑止中は `hidden` にする
  - チェックボックスの変更時は、表示中なら内容を更新する
- [X] T009 [US1] LifePlan/wwwroot/css/life-plan.css にツールチップのスタイル `.life-plan-chart__tooltip` を追加する
  - 絶対配置、`--ui-color-surface` の背景、`--ui-color-border` の枠、`--ui-radius-sm`、`--ui-shadow-sm`、`--ui-font-size-small` を使う
  - 最大幅をグラフ枠内に収める
  - 見出しの「カーソル位置」ラベルの見た目を付ける
  - カーソルがツールチップ上に乗っても消えないよう、`pointer-events` は有効のままにする（FR-005）
  - 新しいトークンは追加しない
- [X] T010 [US1] ハーネスにホバーのシナリオを追加して確認する
  - 未選択でのホバー
  - 固定後に別年度へホバー（固定が変わらない：SC-001）
  - ツールチップ上への移動で消えない
  - 枠の `pointerleave` で消える
  - Esc で消え、別年度へ動かすと再表示
  - 左端・右端でツールチップが枠内に収まる
  - ボタンを押している間は出ない
  - 項目を一部・全部外した状態
  - 1280px のスクリーンショットで、線種・マーカー・ツールチップの見た目を確認する
  - 2026-10-12 実施：すべて期待どおり。スクリーンショットで、ツールチップを上端に固定すると上半分の横移動でカーソルが乗り、年度の更新が止まることが分かった。そのため縦位置を「カーソルと反対側の半分」に変更した（research.md R3、contracts を更新）。sweep シナリオで5つの高さを横になぞり、カーソルが乗った回数は0回

**Checkpoint**: PCでホバーによる一時確認と、クリックによる固定が両立する

---

## Phase 4: User Story 2 - スマートフォンでタップして年度を固定する（既存操作の維持） (Priority: P1)

**Goal**: ホバーの追加後も、タップ・横移動・縦スクロール・ピンチの挙動が基準コミットと同じ

**Independent Test**: quickstart.md の M1〜M3。タッチ操作でツールチップが出ないこと

### Implementation for User Story 2

- [X] T011 [US2] LifePlan/wwwroot/js/life-plan-chart.js で、タッチの `pointerdown`・`pointermove`・`pointerup`・`pointercancel` がホバーの状態を変えず、ツールチップを出さないことを確認する。必要なら `pointerType` の判定を補う（FR-007）
- [X] T012 [US2] ハーネスでタッチのシナリオ（tap、cancel、firstcancel、twofinger）を再実行し、基準コミットと同じ結果、かつツールチップが `hidden` のままであることを確認する。「タッチの後にマウスで移動」した場合にホバーが正しく動くことも確認する（SC-003）

**Checkpoint**: スマートフォンの操作が回帰していない

---

## Phase 5: User Story 3 - チェックボックスで系列の色と名前の対応が分かる（凡例の整理） (Priority: P2)

**Goal**: 表示項目チェックボックスに色マーカーを付け、グラフ下部の凡例を削除する

**Independent Test**: チェックボックスに4色のマーカーがあり、チェックを外しても残る。グラフ下部に凡例がない。チェックの切り替えがグラフ・数値表示欄・ツールチップに反映される

### Implementation for User Story 3

- [X] T013 [US3] LifePlan/wwwroot/js/life-plan-chart.js の `renderSeriesControls` で、各ラベルの項目名の前に色マーカー（`span.life-plan-chart__legend-marker`、`aria-hidden="true"`、系列色）を追加する。チェックを外してもマーカーは残す（FR-010、Clarifications Q8(c)）
- [X] T014 [US3] LifePlan/Views/LifePlan/Index.cshtml から凡例要素（`data-life-plan-chart-legend`）を削除し、LifePlan/wwwroot/js/life-plan-chart.js から `renderLegend` とその呼び出し、`legend` の取得を削除する（FR-011）
- [X] T015 [US3] LifePlan/wwwroot/css/life-plan.css から使われなくなった `.life-plan-chart__legend`・`.life-plan-chart__legend-item` を削除する。`.life-plan-chart__legend-marker` は数値表示欄・チェックボックス・ツールチップで使うため残す。チェックボックスのラベル内でマーカーが縮まず、項目名と並ぶよう調整する

**Checkpoint**: 項目名を並べる場所がチェックボックスと数値表示欄の2か所になる（SC-004）

---

## Phase 6: User Story 4 - グラフを操作できることに気づける（操作案内） (Priority: P2)

**Goal**: チェックボックスとグラフの間に、入力方法に合った操作案内を1つ表示する

**Independent Test**: PC幅（マウス）でPC向け、タッチ模擬でタッチ向けの文が1つだけ表示され、グラフ・数値表示欄に重ならない

### Implementation for User Story 4

- [X] T016 [US4] LifePlan/Views/LifePlan/Index.cshtml の表示項目 `fieldset` と `.life-plan-chart__canvas-wrap` の間に、操作案内を追加する
  - `<p class="life-plan-chart__guide" data-life-plan-chart-guide hidden>` の中に、PC向け `<span class="life-plan-chart__guide-pointer">` とタッチ向け `<span class="life-plan-chart__guide-touch">` を置く
  - PC向けの文言：「カーソルを合わせて金額を確認・クリックで年度を固定（←/→キーでも年度を選択できます）」
  - タッチ向けの文言：「グラフをタップして年度を選択」
- [X] T017 [US4] LifePlan/wwwroot/css/life-plan.css に操作案内のスタイルを追加する
  - `--ui-font-size-small`、`--ui-color-muted`、余白なしで1行。狭い幅では折り返す
  - 既定ではタッチ向けを表示し、PC向けを隠す
  - `@media (any-hover: hover) and (any-pointer: fine)` ではPC向けを表示し、タッチ向けを隠す（research.md R5）
- [X] T018 [US4] LifePlan/wwwroot/js/life-plan-chart.js の `initialize` で、グラフの初期化に成功したときだけ操作案内の `hidden` を外す。代替表示のときは表示しない
- [X] T019 [US4] ハーネスで操作案内を確認する
  - 1280px（Edge headless はマウス環境）でPC向けだけが表示される
  - 案内のある状態とない状態で、結果画面の高さの差が1行分程度に収まる（SC-006）
  - タッチ向けの表示は、Edge のタッチ模擬（`--touch-events`、または DevTools のデバイス模擬）で確認する。確認できない場合は手動確認（quickstart M4）に回し、その旨を記録する
  - 2026-10-12 実施：Edge headless はマウス環境として扱われ、1280px・各スマートフォン幅ともPC向けの文が1つだけ表示されることを確認した。タッチ向けの表示は headless では確認できなかったため、手動確認（quickstart M4）に回す

**Checkpoint**: 操作案内が入力方法に合わせて表示される

---

## Phase 7: User Story 5 - 選択年度の金額を読み取りやすくする（数値表示欄の改善） (Priority: P2)

**Goal**: 表示内容を変えずに、年度の見出し・項目名・金額の強弱と未選択時の文言を整える

**Independent Test**: quickstart.md の M5。320〜430px と 1280px で、見出し「{年}年 固定中」、金額の太字・単位、0・マイナス・7桁の値が重ならずに表示される

### Implementation for User Story 5

- [X] T020 [US5] LifePlan/wwwroot/js/life-plan-chart.js の `renderDetail` を変更する
  - 見出しを「{年}年」＋ラベル「固定中」（`span.life-plan-chart__detail-status`）にする
  - 未選択時の文言を「年度が選択されていません」にする
  - 金額の「万円」を `span.life-plan-chart__unit` に分ける
  - 表示項目なしの「表示項目が選択されていません。」は維持する
  - `aria-live` の更新が変化時だけである既存の仕組み（`renderedDetailKey`）は維持する（FR-015〜017、FR-022）
- [X] T021 [US5] LifePlan/wwwroot/css/life-plan.css の数値表示欄のスタイルを調整する
  - 年度の見出しは `--ui-font-size-body` の太字、「固定中」ラベルは小さく補足色にする
  - 項目名は `--ui-font-size-small` の通常の太さにする
  - 金額は `--ui-font-size-body` の太字・等幅数字にし、単位は小さめにする
  - 既存の2列・1列のグリッドと、金額を次の行に右寄せで回す折り返しは維持する（FR-018）
  - ツールチップは T003 の一覧を共用するため、ツールチップ内では文字サイズを一段小さくする
- [X] T022 [US5] ハーネス（round、none、two、および 1,234,567.9・-123.5・0.0 を含むデータ）と、320 / 375 / 390 / 430 / 1280px のスクリーンショットで、重なり・はみ出し・横スクロールがないことを確認する（SC-006、SC-008）

**Checkpoint**: 数値表示欄の見出し・金額・単位が読み取りやすい

---

## Phase 8: User Story 6 - キーボードでも年度別の金額を確認できる (Priority: P2)

**Goal**: 標準の年度選択欄（フォーカス時だけ表示）で、キーボードと読み上げソフトから年度を選べる。Canvas は図としてフォーカス対象から外す（※表示仕様は Phase 11 で常時表示に変更）

**Independent Test**: quickstart.md の K1〜K6。Tab で年度選択欄が表示され、←/→/Home/End で固定でき、未選択は「年度を選択」と伝わる。マウス・タッチの利用者には表示されない（※「マウス・タッチの利用者には表示されない」は Phase 11 で撤回。現行の確認基準は quickstart の K1・K6・M6）

### Implementation for User Story 6

- [X] T023 [US6] LifePlan/Views/LifePlan/Index.cshtml を変更する
  - Canvas から `tabindex="0"` を削除し、`aria-label` を「収入、支出、貯蓄合計の年次推移グラフ。年度別の金額はグラフ下の表示する年度で選択して確認できます」に変える。`role="img"` は維持する
  - グラフ枠と数値表示欄の間に、年度選択欄を追加する。包む要素は `<div class="life-plan-chart__year-select visually-hidden-focusable" data-life-plan-chart-year-select-wrap hidden>` とし、中に `<label for="life-plan-chart-year">表示する年度</label>` と `<select id="life-plan-chart-year" class="form-select" data-life-plan-chart-year-select>` を置く（contracts/chart-ui.md「年度選択欄」）
- [X] T024 [US6] LifePlan/wwwroot/js/life-plan-chart.js に年度選択欄の処理を追加する
  - 初期化時に選択肢を作る。先頭は「年度を選択」（`value=""`、`disabled`、`selected`）、続けて各年度「{年}年」（値は年度の位置）。作成後に包む要素の `hidden` を外す
  - `change` でその年度を固定する（`selectIndex`）
  - `keydown` で ←/→/Home/End を処理する。未選択時の←/→は最初の年度とし、既定動作を止めて固定する。Home/End が既定動作で変わる場合も、結果として同じ年度になるようにする
  - `selectIndex` の後に `select.value` を同期する。プログラムからの変更では `change` を発火させない
  - 未選択へ戻す選択肢は作らない（FR-020）
- [X] T025 [US6] LifePlan/wwwroot/js/life-plan-chart.js から、Canvas の `keydown` の処理（`bindYearSelection` 内）を削除する。キー操作は T024 の年度選択欄に集約する
- [X] T026 [US6] LifePlan/wwwroot/css/life-plan.css に、フォーカス時に表示された年度選択欄の配置を追加する
  - ラベルと `select` を横に並べ、狭い幅では縦に並べる
  - 幅を抑える
  - 数値表示欄との間に `--ui-space-sm` 程度の余白を取る
  - フォーカスリングが他の要素に隠れないことを確認する（FR-019）
  - 不要になった `.life-plan-chart__canvas:focus-visible` のスタイルを削除する
- [X] T027 [US6] ハーネスで確認する
  - `select` に `focus()` すると表示され、`blur` で非表示になる
  - 未選択時の値が空で、先頭項目が選択不可である
  - ←/→/Home/End で固定と数値表示欄が更新され、既定動作が止まる（`defaultPrevented`）
  - `change` で固定される
  - クリック・タップで固定すると `select.value` が同期する
  - Canvas に `tabindex` がない
  - 画面幅の変更で数値表示欄が再生成されない（FR-022）

**Checkpoint**: キーボード・読み上げソフトで年度と金額を確認できる

---

## Phase 9: Polish & Cross-Cutting Concerns

- [X] T028 LifePlan/wwwroot/js/life-plan-chart.js・LifePlan/wwwroot/css/life-plan.css・LifePlan/Views/LifePlan/Index.cshtml に、削除した凡例・Canvas のキー操作の参照（`data-life-plan-chart-legend`、`renderLegend`、`.life-plan-chart__legend-item`、Canvas の `tabindex`）が残っていないことを、`rg` で確認する。改行コードが LF であることも確認する
- [X] T029 `dotnet build LifePlan.slnx -m:1` と `dotnet test LifePlan.slnx -m:1` を実行し、T001 と同じ件数ですべて合格することを確認する（SC-009）。ネットワーク由来の警告は分けて記録する
- [X] T030 ハーネスの全シナリオを再実行し、320 / 375 / 390 / 430 / 1280px のスクリーンショットを確認する。同じ年度で、ツールチップと数値表示欄の金額が一致し、キャッシュフロー表の値（`ToString("0.0")`、3桁区切りを除く）とも一致すること（SC-008）を確認する
- [X] T031 `change-reviewer` で変更をレビューする（比較元 `ef3435f`、差分、未追跡ファイル一覧、T029・T030 の結果を渡す）。指摘があれば修正し、指摘がなくなるまでレビューを繰り返す
  - 2026-10-12 実施：change-reviewer で4回レビューした。
    - 1回目：中1・低2
      - 中：狭い幅や年数が少ない場合に、ツールチップがカーソルの下に入る
      - 低：正本と SC-007 に年度選択欄の記述がない
      - 低：スタイラス対応の Android 端末で操作案内を誤判定する可能性
    - 2回目：中1（速いマウス移動で、同じ半分に置いた候補に乗る）
    - 3回目：低2（コメントと計測値の記録が実装に追いついていない）
    - 4回目：指摘なし
  - 修正はすべて完了。正本への追記（docs-update-proposal.md の A-3）だけ、ユーザーの了承待ち（T033）だった（その後、ユーザーの追加修正依頼で正本の更新が指示され、T033 で反映済み）
- [X] T032 specs/002-chart-hover-usability/quickstart.md の手動確認項目のうち、自動で確認できなかったものを、このタスクの下に記録する。ユーザーに実機確認を依頼する項目は次のとおり（レビュー指摘により、次の2点を追加）
  - スタイラス対応の Android 端末での操作案内の出し分け
  - 年度選択欄の ↑/↓・文字入力による選択
  - P3：ツールチップ上への実マウス移動
  - M1〜M4：iOS Safari・Android Chrome
  - K3〜K5：ナレーター・VoiceOver・TalkBack
  - P10（実際のマウス）：ツールチップのクリックで固定すること。金額をドラッグで選んで離しても固定しないこと。ダブルクリックで表示が乱れないこと
  - タッチとマウスの両方を使える端末：マウスのホバーでツールチップを出したまま、指でツールチップをタップしても不整合がないこと
  - 常に表示する年度選択欄（Phase 11）：iOS Safari でタップしてピッカーが開くこと（画面の自動拡大は Issue #56 で調査する。スマートフォン幅では選択欄の文字が14pxになるため、拡大される可能性がある）。ピッカー（iOS）・一覧（Android）・PC のドロップダウン（実際のマウス）で選んだ年度がグラフと数値表示欄に反映されること。タップで固定した年度が一覧に反映されること。読み上げソフトで「表示する年度、年度を選択」と読まれ、選んだ後の更新が1回だけ読まれること
  - 2026-10-12 記録：上記は Edge headless のハーネスでは確認できない（合成イベントでは、標準の select のキー操作、タッチ環境のメディアクエリ、支援技術を再現できない）。ユーザーの実機確認待ち
- [X] T033 spec.md・contracts/chart-ui.md・正本 `docs/simulator/index.md` v1.3 6.3 と実装の文言・挙動が一致していることを確認する。不一致があれば、正本の変更は提案して了承を得てから反映する（憲章 原則 I）
  - 2026-10-12 実施：年度選択欄の記載（docs-update-proposal.md の A-3）は、ユーザーの追加修正依頼で正本の更新が指示されたため反映した。ツールチップのクリック（A-4）もあわせて反映し、spec・contracts と文言・挙動が一致することを確認した

---

## Phase 10: 実装後の追加修正（ツールチップのクリック、年度選択欄の表示仕様の明確化）

**Purpose**: ユーザーの追加修正依頼（2026-10-12）。REQ-01 はツールチップのクリックによる固定（A案）、REQ-02 は「表示する年度」の現仕様の維持と明確化

- [X] T034 LifePlan/wwwroot/js/life-plan-chart.js に `bindTooltipSelection` を追加する。ツールチップの `click`（左ボタン）で、表示中のホバー年度を `selectIndex` で固定する。一時表示を終了しているときと、ツールチップ内の文字を選択しているときは何もしない（FR-025）
- [X] T035 LifePlan/wwwroot/css/life-plan.css の `.life-plan-chart__tooltip` に `cursor: pointer` を追加する
- [X] T036 ハーネスに tipclick シナリオを追加し、次を確認する
  - ツールチップのクリックで表示中の年度が固定され、年度選択欄が同期する
  - ホバーだけでは変わらない
  - 右クリックと Esc の後は固定しない
  - グラフのクリック・ドラッグは維持される
  - 既存の16シナリオと sweep2 に回帰がない
  - 2026-10-12 実施：すべて期待どおり。ツールチップ 2053年をクリックすると固定 2053年、select 27。ホバーだけ、右クリック、Esc の後は変化なし。グラフのクリック・ドラッグは正常
- [X] T037 正本 `docs/simulator/index.md`（0. 位置づけ、6.3）、spec.md（Clarifications、US1 シナリオ8、FR-008・019・025）、research.md（R3a）、contracts/chart-ui.md、data-model.md、quickstart.md（P10、tipclick）、docs-update-proposal.md（A-3・A-4）を更新する。次の3点を明記する
  - ツールチップのクリックで固定できること
  - 年度選択欄はキーボードでフォーカスしたときだけ表示すること
  - スマートフォンでも常時表示しないこと
- [X] T038 `dotnet build LifePlan.slnx -m:1` と `dotnet test LifePlan.slnx -m:1` を実行する
- [X] T039 `change-reviewer` でレビューし、指摘がなくなるまで修正と再レビューを繰り返す
  - 2026-10-12 実施：1回目は低1件（文書）と要確認1件（文字の選択時の固定）。どちらも修正し、2回目は指摘なし

---

## Phase 11: 年度選択欄の常時表示（ユーザー指示による仕様変更）

**Purpose**: 「表示する年度」を、PC・スマートフォンとも常に表示する。以前の「フォーカスしたときだけ表示」は撤回する（spec.md Clarifications）

- [X] T040 LifePlan/Views/LifePlan/Index.cshtml の年度選択欄を包む要素から `visually-hidden-focusable` を外す。初期化が終わるまで `hidden` にしておくのは維持する
- [X] T041 LifePlan/wwwroot/css/life-plan.css の `.life-plan-chart__year-select:focus-within` を、`.life-plan-chart__year-select`（常に flex）に変える。選択欄は `flex: 1 1 10rem`、`min-width: 0`、`max-width: 14rem` とし、狭い幅では折り返して見切れないようにする
- [X] T042 LifePlan/wwwroot/js/life-plan-chart.js の年度選択欄のコメントを、常時表示に合わせて直す。処理（選択肢の生成、相互同期、←/→/Home/End）は変えない
- [X] T043 正本 `docs/simulator/index.md`（0. 位置づけ、6.3）、spec.md（Clarifications、US6、FR-019・FR-024）、research.md（R6 の再改訂）、contracts/chart-ui.md、plan.md、quickstart.md（K1・K6・M6）、docs-update-proposal.md（A-5）を更新する
- [X] T044 ハーネスとスクリーンショットで確認する
  - 初期表示で一覧が表示され、「年度を選択」で、どの年度も固定されていない
  - 一覧の選択がグラフ・数値表示欄に反映され、グラフのクリック・タップが一覧に反映される
  - フォーカスが外れても表示が続く
  - 既存の全シナリオに回帰がない
  - 320 / 375 / 390 / 430 / 1280px で見切れない
  - 2026-10-12 実施：すべて期待どおり。320px でもラベルと選択欄が1行に収まる
- [X] T045 `dotnet build` / `dotnet test` を実行し、`change-reviewer` でレビューする。指摘がなくなるまで修正と再レビューを繰り返す
  - 2026-10-12 実施：build 警告0・エラー0、test 125件すべて合格。change-reviewer で2回レビューした。1回目は文書の低3件（撤回した旧仕様の記述が現行の内容として読める）、2回目は指摘なし

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)** → **Foundational (Phase 2)** → 各ストーリー → **Polish (Phase 9)**

### User Story Dependencies

3ファイルを共有するため、ストーリーは原則として順に進める。

- **US1 (P1)**：Foundational（T003 の一覧、T004 の重ね表示）に依存
- **US2 (P1)**：US1 のホバー処理の後に回帰を確認する（US1 に依存）
- **US3 (P2)**：Foundational のみに依存。ツールチップ・数値表示欄のマーカーは US3 の後もそのまま使える
- **US4 (P2)**：Foundational のみに依存
- **US5 (P2)**：T003 に依存。US1 のツールチップが一覧を共用するため、T021 のツールチップ内の文字サイズ調整は US1 の後に行う
- **US6 (P2)**：Foundational のみに依存。T025 は Canvas のキー操作を削除するため、T024 の完了後に行う

### Within Each User Story

- View（要素の追加・削除）→ JS（状態・描画・同期）→ CSS（見た目）→ ハーネスでの確認の順

### Parallel Opportunities

- Phase 1：T002 は T001 と並行可
- 実装タスクは同じ3ファイルを編集するため、並行は避ける。ストーリー内でも JS と CSS は別ファイルだが、表示の確認が JS の完了に依存するため順に行う

---

## Parallel Example: Phase 1

```text
Task: "T001 変更前のビルド・テスト結果の記録"
Task: "T002 スクラッチでの検証用ハーネスの準備"
```

---

## Implementation Strategy

### MVP First

1. Phase 1・2 を完了する
2. US1（ホバー）と US2（スマートフォンの回帰確認）を完了する。依頼の最重要方針「PC版：ホバーで一時確認、クリックで年度を固定／スマートフォン版：タップで年度を固定」が満たされる
3. **STOP and VALIDATE**：quickstart.md の P1〜P8、M1〜M3

### Incremental Delivery

1. MVP（US1＋US2）
2. US3（凡例の整理）→ SC-004、SC-005
3. US4（操作案内）→ SC-006
4. US5（数値表示欄）→ SC-006、SC-008
5. US6（年度選択欄）→ SC-007
6. Polish → 回帰、レビュー、手動確認の依頼

正本 v1.3 6.3 は凡例の削除・操作案内・ホバーをまとめて記載している。本番に出すのは US1〜US6 がすべてそろってからとする。

---

## Notes

- 計算ロジック、ViewModel、JSON の形式、キャッシュフロー表、系列の色・意味は変更しない
- 外部グラフライブラリを導入しない。Canvas 描画を維持する
- 色・余白・文字サイズは既存のトークン（`site.css` の `:root`）を使う。新しいトークンが必要な場合は理由を記録する
- 金額の書式は既存の `formatDetailAmount` を使い、ツールチップと数値表示欄で共用する
- コミットはストーリーまたは論理的なまとまりごとに行う
