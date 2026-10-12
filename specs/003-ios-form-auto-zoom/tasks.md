---

description: "Task list for iOS端末でフォーム操作時に画面が自動拡大される問題の解消"
---

# Tasks: iOS端末でフォーム操作時に画面が自動拡大される問題の解消

**Input**: Design documents from `specs/003-ios-form-auto-zoom/`

**Prerequisites**: plan.md、spec.md、research.md、data-model.md、contracts/form-controls.md、quickstart.md

**Tests**: C# のコード・計算ロジックは変更しないため、単体テストは追加しない（憲章 原則 III は対象外）。既存テストで回帰を確認する。計算後の文字の大きさは、スクラッチに置く Edge headless の計測ページで確認する。計測ページはリポジトリに含めない。自動拡大そのものは実機で確認する（research.md R5、quickstart.md）。

**Organization**: spec.md のユーザーストーリー（US1〜US6）ごとに分ける。文字の大きさの指定は `site.css` の 1 か所で全ストーリーに共通するため（幅・並びの調整は research.md R4 の追記を参照）、Phase 2 で実装し、各ストーリーのフェーズではそのストーリーの確認と、必要な場合だけの調整を行う。正本は `docs/common/ui-implementation-guidelines.md` 6・12。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 並行実行可（別ファイル、未完了タスクへの依存なし）
- **[Story]**: 対応するユーザーストーリー（US1〜US6）
- パスはリポジトリルートからの相対パス。変更するのは原則 `LifePlan/wwwroot/css/site.css` だけ（plan.md）
- 計測ページ・スクリーンショットの置き場所は、セッションのスクラッチディレクトリ（以下「スクラッチ」）

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: 着手前の状態の記録と、検証の準備

- [X] T001 `dotnet build LifePlan.slnx -m:1` と `dotnet test LifePlan.slnx -m:1` を実行し、変更前のテスト件数と成功状態を記録する。VS のデバッガーが DLL をロックしている場合は `--artifacts-path <スクラッチ>` を付ける。ネットワーク由来の警告は分けて記録する
  - 2026-10-12 実施：ビルドは警告0・エラー0、テストは125件すべて合格。ネットワーク由来の警告なし
- [X] T002 アプリを起動し（`dotnet run --project LifePlan/LifePlan.csproj --launch-profile http`、`http://localhost:5250`）、次の HTML をスクラッチに保存する。各 HTML の `<head>` に `<base href="http://localhost:5250/">` を足す
  - 入力画面：`Invoke-WebRequest -SessionVariable s` で `/LifePlan` を GET する
  - 結果画面：同じセッション（`-WebSession $s`）で `/LifePlan` に POST する。送る値は、入力画面の `form.life-plan-form` の中のすべての `input`・`select`・`textarea` の `name` と現在値（`__RequestVerificationToken` を含む。`checkbox` はチェックされているものだけ）。結果画面に `#life-plan-chart-year` があることを確認する。ない場合は入力エラーなので、エラーの項目に値を入れて送り直し、使った値を記録する
  - お問い合わせ画面：`/Contact` を GET する（送信はしない。開発環境でもメールが送られるため）
  - 入力エラーの画面：シミュレーターは年齢を空にして POST、お問い合わせは必須項目をすべて空にして POST する（入力検証で止まり、送信処理に進まないことをコントローラーで事前に確認する）。T003 のエラー文の計測と、T011・T020 に使う
  - 2026-10-12 実施：初期値では夫・妻の年齢が空で入力エラーになるため、どちらも 30 を入れて試算した。お問い合わせは、必須項目と同意を空にし、会社名だけ入れて POST した。入力検証で止まり（`ContactPageService.Submit` は `hasBindingErrors` のとき送信前に戻る）、会社名が保持されることを確認した。メールは送っていない
- [X] T003 スクラッチに計測用スクリプトを作り、T002 の HTML に読み込ませる。スクリプトは次を JSON で `document.title`（または `<pre id="measure">`）に書き出す
  - `matchMedia('(pointer: coarse)').matches`
  - `document.documentElement.scrollWidth` と `innerWidth`
  - contracts/form-controls.md の「対象になる部品」（`input.form-control`、`select.form-select`、`select.form-control`、`textarea.form-control`。`type="hidden"` を除く）ごとの `id`／`name` と、`getComputedStyle(el).fontSize`、`offsetHeight`、`offsetWidth`
  - 比較用として、ラベル（`.life-plan-field-row__label`、`.form-group label` など）、単位（`.life-plan-unit`）、エラー文（`.life-plan-validation`、`.field-validation-error`）、チェックボックス（`.form-check-input`）の `fontSize`。エラー文は、T002 の入力エラーの画面で計測する（エラーがない画面には出ないため）
- [X] T004 Edge headless（`msedge --headless --disable-gpu --window-size=<幅>,900 --dump-dom` または `--screenshot`）で、T003 の計測ページをタッチ相当（`--blink-settings=primaryPointerType=2,availablePointerTypes=2,primaryHoverType=1,availableHoverTypes=1`）で開き、`pointer: coarse` が `true` になることを確かめる。`false` の場合は、quickstart.md 2 の代替（開発者ツールのデバイスモード）に切り替え、その旨を記録する
  - 2026-10-12 実施：Edge 155 で `pointer: coarse` が `true` になった。ウィンドウ幅は約 500px 未満にできない（旧ヘッドレスも同じ）ため、指定幅の `iframe` に読み込む方式にした（research.md R5）
- [X] T005 変更前（`develop`、`2f48e21`）の基準値を記録する：PC 相当とタッチ相当の両方で、幅 320 / 375 / 390 / 430 / 667 / 852 / 1024 / 1280px の計測結果（T003）と、3 画面のスクリーンショット（幅 375・1280px は PC 相当、320・667・852px はタッチ相当）をスクラッチに保存する。対象の部品が 51 個（シミュレーター 45、お問い合わせ 6）あることも確認する
  - 2026-10-12 実施：2 条件×8 幅×5 画面（入力、結果、シミュレーターの入力エラー、お問い合わせ、お問い合わせの入力エラー）を計測し、スクリーンショット 25 枚を保存。値は spec「現状」の表と一致した。部品は画面上で 75 個（シミュレーター入力画面 68：入力欄 32・選択欄 36。子ども 4 人分に展開されるため。年度選択欄 1、お問い合わせ 6）で、51 個はテンプレート上の数だった。spec・plan・quickstart・data-model の数を直した

**Checkpoint**: 変更前の基準値（文字の大きさ・高さ・幅・ページの横幅・スクリーンショット）が揃っている

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: 全ストーリーに共通する CSS の変更（plan.md「型・ファイルごとの配置」）

**⚠️ CRITICAL**: このフェーズが終わるまでストーリーの確認に着手しない

- [X] T006 LifePlan/wwwroot/css/site.css の `:root` の文字の大きさのトークン群（`--ui-font-size-root-sm` などの並び）に、`--ui-font-size-control-touch: 16px;` を追加する（research.md R2）
- [X] T007 LifePlan/wwwroot/css/site.css の `html` の文字の大きさの規則（`@media (min-width: 768px)` の直後）の近くに、次の規則を追加する。理由（iOS は 16px 未満の部品にフォーカスすると自動で拡大すること、画面幅ではなく主な入力装置で判定すること、要素名とクラスの組み合わせで画面ごとの CSS より優先させていること）を、既存のコメントの密度に合わせて短く書く（research.md R1・R3、contracts/form-controls.md 約束 1・2）
  ```css
  @media (pointer: coarse) {
    input.form-control,
    select.form-select,
    select.form-control,
    textarea.form-control {
      font-size: var(--ui-font-size-control-touch);
    }
  }
  ```
  - 2026-10-12 実施：`site.css` にはコメントがないが、規則の理由（16px、要素名による優先）がコードから読み取れないため、`life-plan.css` と同じ形の 1 行コメントを付けた
- [X] T008 `grep` で、`LifePlan/wwwroot/css/` と `LifePlan/Views/**/*.css` に、対象の部品の文字の大きさを詳細度 0,1,1 以上で指定している規則がないことを確認する（contracts/form-controls.md 約束 3）。`LifePlan/Views/Shared/_Layout.cshtml` の viewport が `width=device-width, initial-scale=1.0` のままであることも確認する（約束 5）
  - 2026-10-12 実施：0,1,1 以上の詳細度で文字の大きさを指定している規則はなかった（`contact.css` の `textarea.form-control`、`.select-wrapper select.form-control` は文字の大きさを指定していない）。viewport は変更なし

**Checkpoint**: タッチ相当の計測で、対象の部品の `fontSize` が 16px になっている

---

## Phase 3: User Story 1 - iPhone でシミュレーターに入力しても画面が勝手に拡大されない (Priority: P1) 🎯 MVP

**Goal**: シミュレーター入力画面の入力欄 32 個・選択欄 36 個（テンプレート上は 12 か所）で、iOS の自動拡大の条件（16px 未満）をなくす

**Independent Test**: タッチ相当の計測で 68 個がすべて 16px。幅 320px で配置が崩れない。iPhone 実機（縦向き）で全部品をタップして倍率が変わらない

- [X] T009 [US1] タッチ相当で、幅 320 / 375 / 390 / 430px の `/LifePlan` 入力画面を計測し、入力欄 32 個・選択欄 36 個がすべて `16px` であることを確認する（quickstart A1、FR-001・FR-002）。入力不要で操作できない欄（子どもの欄・教育プランの欄）も 16px であること（spec Edge Cases）
  - 2026-10-12 実施：全幅で 68 個すべて 16px（変更前 12.25px）。入力不要の欄も 16px
- [X] T010 [US1] タッチ相当で、幅 320px のスクリーンショットを撮り、入力欄と単位（「万円」「歳」「%」）が同じ行に収まり、開始年齢〜終了年齢の横並びが収まり、ページの横幅が画面幅を超えないことを確認する。7 桁の金額を入れた状態でも値が読めること（quickstart L1・L2、A3、FR-008）。入力欄の高さが変更前（T005）と同じ 40px であること（research.md R4）
  - 2026-10-12 実施：横スクロールなし、配置は変更前と同じ。入力欄の高さは 40px のまま。7 桁の金額も読める
- [X] T011 [US1] 入力エラーになる値で送信した画面をタッチ相当・幅 375px で撮り、エラー文の位置と大きさ、入力値の保持が変更前と同じであることを確認する（quickstart L6、FR-006）。あわせて、ブラウザで金額の欄に全角数字を入力し、半角に変わることを確認する（入力値の正規化。FR-006）
  - 2026-10-12 実施：エラー文（12.25px）の位置と大きさ、入力値の保持は変更前と同じ。既存の正規化は全角数字を半角に変換するのではなく、半角数字以外を取り除く処理だった（`life-plan-validation.js`）。タッチ相当で「1２3a」が「13」になり、既存どおり動くことを確認した。spec FR-006 の記述を実際の処理に合わせて直した
- [X] T012 [US1] T010・T011 で崩れが見つかった場合だけ、LifePlan/wwwroot/css/site.css の T007 と同じメディアクエリの中で、最小限の調整を加える。調整した場合は、理由を research.md R4 に追記する
  - 2026-10-12：幅 320〜430px では調整不要。年齢の横並びの切れは幅 768px 以上で起きたため、T022 で対応した

**Checkpoint**: シミュレーター入力画面で、タッチ相当の文字の大きさがすべて 16px になり、配置・エラー表示が変更前と同じ

---

## Phase 4: User Story 2 - iPhone でグラフの年度を選んでも画面が拡大されない (Priority: P1)

**Goal**: 結果画面の「表示する年度」選択欄で、自動拡大の条件をなくす。グラフとの同期は変えない

**Independent Test**: タッチ相当の計測で年度選択欄が 16px。幅 320px で見切れない。一覧で選んだ年度がグラフ・数値表示欄に反映される

- [X] T013 [P] [US2] タッチ相当で、幅 320 / 375 / 430 / 852px の結果画面を計測し、`#life-plan-chart-year` が `16px` であることを確認する（quickstart A1、FR-001・FR-002）
  - 2026-10-12 実施：全幅で 16px（変更前 14px / 768px 以上 16px）
- [X] T014 [US2] タッチ相当で、幅 320px の結果画面を撮り、「表示する年度」のラベルと選択欄が見切れず、グラフ・数値表示欄と重ならないことを確認する。選択欄の高さの増分が、research.md R4 の見積もり（約 3px）程度であること（quickstart L4、FR-008）
  - 2026-10-12 実施：見切れ・重なりなし。高さは 34px → 37px（幅 768px 未満）
- [X] T015 [US2] 結果画面で年度選択欄から年度を選び、グラフの縦線・マーカーと数値表示欄が選んだ年度になり、グラフのタップで固定した年度が選択欄に反映されることを確認する（FR-007。002 の quickstart M6 と同じ手順）
  - 2026-10-12 実施（タッチ相当の headless）：年度選択欄で 2028 年を選ぶと、数値表示欄が「2028年 固定中」になった。グラフのタップから選択欄への反映は、変更していない JavaScript の処理なので、T029 の実機確認で見る

**Checkpoint**: 年度選択欄がタッチ相当で 16px になり、同期と配置が変更前と同じ

---

## Phase 5: User Story 5 - 利用者は自分で画面を拡大できる（ピンチズームの維持） (Priority: P1)

**Goal**: 自動拡大の対策で、利用者が自分で拡大する手段を失わせない

**Independent Test**: viewport の設定に拡大の制限がない。実機でピンチ拡大できる

- [X] T016 [P] [US5] `git diff develop -- LifePlan/Views` が空であること、`LifePlan/Views/Shared/_Layout.cshtml` の viewport に `maximum-scale`、`minimum-scale`、`user-scalable` がないことを確認する（FR-004・FR-005、contracts/form-controls.md 約束 5）
  - 2026-10-12 実施：`git diff develop -- LifePlan/Views` は空。viewport に拡大の制限なし
- [ ] T017 [US5] PC のブラウザで、3 画面を Ctrl＋プラスで 200% に拡大し、フォームの入力・送信ができることを確認する（quickstart D4 の PC 部分、SC-003）。スマートフォン実機でのピンチ拡大は T029 で確認する
  - 未実施（手動確認）：headless ではブラウザの拡大操作を再現できない。PC では CSS も HTML も変わらない（T023・T024）ため、変更前と同じ結果になる見込み

**Checkpoint**: 拡大を制限する変更がないことが確認できている

---

## Phase 6: User Story 3 - iPhone でお問い合わせフォームに入力しても画面が拡大されない (Priority: P2)

**Goal**: お問い合わせ画面の入力欄 4 個・選択欄 1 個・本文欄 1 個で、自動拡大の条件をなくす

**Independent Test**: タッチ相当の計測で 6 個が 16px。幅 320px で崩れない。送信・エラー表示が変わらない

- [X] T018 [P] [US3] タッチ相当で、幅 320 / 375 / 430 / 852px の `/Contact` を計測し、氏名・会社名・メール・件名の入力欄、種別の選択欄、本文欄がすべて `16px` であること、同意のチェックボックスは変更前と同じであることを確認する（quickstart A1、FR-001・FR-002）
  - 2026-10-12 実施：6 個すべて 16px（変更前 14px）。チェックボックスは 14px のまま
- [X] T019 [US3] タッチ相当で、幅 320px の `/Contact` を撮り、各欄が収まり、2 列の欄（氏名・会社名）が 480px 以下で 1 列になる既存の動きが変わらず、本文欄の高さが変更前と同じ（`min-height: 130px`）であることを確認する（quickstart L5、FR-008）
  - 2026-10-12 実施：各欄が収まり、横スクロールなし。入力欄・選択欄は 44px → 47px。本文欄は `rows="6"` で高さが決まるため 157px → 177px になり、「変更前と同じ」は満たさない。はみ出しはなく FR-008 は満たすので許容し、research.md R4 と quickstart L5 を実際に合わせて直した
- [X] T020 [US3] `/Contact` で必須項目の一部を空にして送り、エラー文の位置と大きさ、入力した値の保持が変更前と同じであることを確認する（FR-006、US3 シナリオ 2、SC-005）。すべての項目を埋めて送信はしない
  - 2026-10-12 実施：エラー文 6 件（12.25px）の位置と大きさは変更前と同じ。入力した会社名が保持された。送信はしていない

**Checkpoint**: お問い合わせ画面で、タッチ相当の文字の大きさがすべて 16px になり、送信・エラー表示が変更前と同じ

---

## Phase 7: User Story 4 - iPhone の横向きと iPad でも画面が拡大されない (Priority: P2)

**Goal**: 画面幅 768px 以上のタッチ端末（iPhone の横向き、iPad）でも、対象の部品を 16px にする

**Independent Test**: タッチ相当・幅 667 / 852 / 1024px の計測で 75 個がすべて 16px。選択欄の選択中の項目が読める

- [X] T021 [US4] タッチ相当で、幅 667 / 852 / 1024px の 3 画面（入力画面、結果画面、お問い合わせ）を計測し、対象の 75 個がすべて `16px` であることを確認する（quickstart A1、FR-001・FR-003）
  - 2026-10-12 実施：667 / 852 / 1024 / 1280px と、追加の 576 / 600 / 744 / 1180px で、すべて 16px。横スクロールなし
- [X] T022 [US4] タッチ相当で、幅 667・852px の `/LifePlan` 入力画面で、職業に「専業主婦・主夫」を、教育プランの各段階で選択肢の文字が最も長いもの（「私立文系」「私立理系」など）を選び、スクリーンショットで選択中の項目が読めることを確認する（quickstart L3、research.md R4）。意味が分からないほど切れる場合だけ、LifePlan/wwwroot/css/site.css の T007 と同じメディアクエリの中で、該当する欄の幅を最小限調整し、research.md R4 に追記する
  - 2026-10-12 実施：職業の欄は幅 388〜400px で切れない。一方、次の 2 つが切れたため調整した（research.md R4 の追記）
    - 教育プランの選択欄（80px）が「私立」までしか見えず、文系・理系を見分けられない
    - 年齢の横並び（約 50px）で「100」が切れる（幅 768px 以上）
  - 調整 1：`site.css` の同じメディアクエリで `--ui-period-control-width` を `--ui-period-control-width-touch`（120px）に置き換えた
  - 調整 2：幅 992px 以上では、教育プランの 4 列が収まらず、選択欄が隣の項目名に接した。`life-plan.css` に `@media (pointer: coarse)` を加え、2 列にした（画面固有の配置のため `life-plan.css` に置いた）
  - 調整後：667 / 1024px で「私立文系」「100」が読め、重なりなし。PC には影響なし（T023・T024）

**Checkpoint**: 画面幅によらず、タッチ相当で対象の部品が 16px になっている

---

## Phase 8: User Story 6 - PC・Android の見た目と操作が意図せず変わらない (Priority: P2)

**Goal**: PC では見た目を一切変えない。Android では入力欄の文字が大きくなる以外の変化を出さない

**Independent Test**: PC 相当の計測値・スクリーンショットが変更前（T005）と一致する

- [X] T023 [P] [US6] PC 相当（起動オプションなし）で、幅 375・1280px の 3 画面を計測し、対象の部品の `fontSize`・`offsetHeight`・`offsetWidth` が T005 の基準値と一致することを確認する（375px：シミュレーター 12.25px・年度選択欄とお問い合わせ 14px、1280px：14px・16px。quickstart A4、FR-012、SC-006）
  - 2026-10-12 実施：PC 相当の全 8 幅・5 画面で、文字の大きさ・高さ・幅が基準値と一致（差分 0 件）
- [X] T024 [US6] PC 相当で、幅 375・1280px のスクリーンショットを T005 と見比べ、差がないことを確認する（quickstart A5、SC-006）
  - 2026-10-12 実施：通常の 10 枚と、最長の選択肢・長い値を入れた 2 枚が、変更前とバイト単位で同一
- [X] T025 [US6] タッチ相当で、ラベル・単位・補足文・エラー文・チェックボックスの `fontSize` が T005 の基準値と一致することを確認する（quickstart A2、FR-009・FR-011）
  - 2026-10-12 実施：全 8 幅・5 画面で一致

**Checkpoint**: PC の見た目に差がなく、タッチ相当でもフォーム部品以外の文字の大きさが変わっていない

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: ビルド・テスト、レビュー、実機確認、記録

- [X] T026 `dotnet build LifePlan.slnx -m:1` と `dotnet test LifePlan.slnx -m:1` を実行し、ビルドの警告・エラーがなく、テスト件数と結果が T001 と同じであることを確認する（SC-007）。ネットワーク由来の警告は分けて報告する
  - 2026-10-12 実施：ビルドは警告0・エラー0、テストは125件すべて合格。ネットワーク由来の警告なし
- [X] T027 `git diff develop --stat` で、変更が LifePlan/wwwroot/css/site.css、LifePlan/wwwroot/css/life-plan.css（T022 の調整）、`docs/common/ui-implementation-guidelines.md`、`specs/003-ios-form-auto-zoom/` だけであることを確認する。改行コードが LF であることも確認する
  - 2026-10-12 実施：変更は `site.css`（+16）、`life-plan.css`（+7）、UI ガイドライン（+3）と、未追跡の `specs/003-ios-form-auto-zoom/` だけ。すべて LF
- [X] T028 `change-reviewer` に変更レビューを依頼する。対象、比較元（`develop`）、差分、未追跡ファイル一覧、T026 と各ストーリーの確認結果を渡す。指摘があれば対応する
  - 2026-10-12 実施：不具合・回帰はなし。詳細度と読み込み順、`:root` の上書き、992px 未満で 1 列が保たれることは想定どおりと確認された。低の指摘 1 件（tasks.md・plan.md に「CSS の変更は 1 か所」という古い記述が残っていた）を直した。残る確認は実機（T029）、PC の 200% 拡大（T017）、グラフのタップから選択欄への同期（T015 の残り）
- [ ] T029 実機確認（quickstart.md 4）を行う。実機は quickstart.md 4「実機からの接続」の手順で、同じ Wi-Fi の PC のアプリにつなぐ。ファイアウォールのルールを追加した場合は、確認後に削除する
  - D1：iPhone・Safari の縦・横で 75 個（子どもの年齢を入れて教育プランを操作できる状態にする）をタップし、倍率が変わらない。縦向きで確認した後、横向きに回してから続けてタップする
  - D2：iPhone・Chrome で各画面の代表的な部品をタップし、倍率が変わらない
  - D3：iPad・Safari。トラックパッドをつないだ場合の結果も記録する（倍率が変わっても許容）
  - D4：iPhone・Android でピンチ拡大できる。拡大したまま入力欄をタップしても、倍率が勝手に変わらない
  - D5：Android・Chrome で入力・送信できる
  - D6：同じ値で修正前後に試算し、結果が同じ。お問い合わせは必須項目を空にしてエラーを出し、入力した値が保持される（送信はしない）
  - 手元にない端末は「未確認」と記録する
- [X] T030 確認結果（T009〜T025 の計測値の要約、T029 の実機の結果と未確認の端末）を、PR の説明に書く。Issue #56 を参照する（`Closes #56` は、D1 を実機で確認できた場合だけ付ける）
  - 2026-10-12 実施：PR #60（`develop` 向け）を作成。実機確認は未実施のため、未確認の項目をチェックリストで書き、`Closes` ではなく `Refs #56` にした

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup（Phase 1）**：依存なし。T002 → T003 → T004 → T005 の順（T001 は並行可）
- **Foundational（Phase 2）**：Phase 1 の T005（変更前の基準値）の後。全ストーリーの確認がこれに依存する
- **User Stories（Phase 3〜8）**：Phase 2 の後。ストーリー間に依存はない。T012・T022 の調整が入った場合は、調整後に T023・T024（PC で変わらないこと）をやり直す
- **Polish（Phase 9）**：全ストーリーの後。T029（実機）は T028（レビュー）の指摘対応の後

### User Story Dependencies

- **US1・US2・US5（P1）**：Phase 2 の後に、互いに独立して確認できる
- **US3・US4・US6（P2）**：Phase 2 の後に、互いに独立して確認できる。US6 は、US1・US4 の調整（T012・T022）が入った場合に再確認する

### Parallel Opportunities

- Phase 2 の後、計測だけのタスク T013・T016・T018・T023 は、画面と条件が別なので並行して実行できる
- 同じ計測ページ・同じ画面幅で取れる値は、1 回の Edge headless の実行でまとめて取ってよい（例：T009・T021・T025 のタッチ相当の計測）

---

## Parallel Example: Phase 2 の後

```text
# 計測は条件ごとに Edge headless を並行して起動できる
Task: "T013 [US2] タッチ相当で結果画面の年度選択欄を計測"
Task: "T018 [US3] タッチ相当で /Contact を計測"
Task: "T023 [US6] PC 相当で 3 画面を計測し、基準値と比較"
```

---

## Implementation Strategy

### MVP First（US1 のみ）

1. Phase 1（基準値の記録）→ Phase 2（CSS の変更）
2. Phase 3（US1）：シミュレーター入力画面で、タッチ相当の計測と配置を確認する
3. **止めて確認**：iPhone 実機で、シミュレーターの数項目をタップして倍率が変わらないことを確かめる（最も効果の大きい部分）

### Incremental Delivery

1. 文字の大きさの指定は `site.css` の 1 か所なので、US1〜US6 は同時に効く（幅・並びの調整は research.md R4 の追記を参照）。確認を P1（US1・US2・US5）→ P2（US3・US4・US6）の順に進める
2. 各ストーリーの確認で崩れが見つかった場合だけ、同じメディアクエリの中で調整し、US6（PC で変わらないこと）を再確認する
3. Phase 9 でまとめて検証し、PR は 1 本にする（変更が小さいため分割しない）

---

## Notes

- 計測ページ・スクリーンショットはスクラッチに置き、リポジトリに含めない
- 実機でしか分からない自動拡大は、計測値（16px 以上か）で代替確認し、最終的に T029 で実機確認する
- タスクを終えたら `[X]` にし、実施日と結果を下に短く書く（002 の tasks.md と同じ書き方）
- 改行コードは LF（`.gitattributes`）
