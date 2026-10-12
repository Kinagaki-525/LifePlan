# Research: iOS端末でフォーム操作時に画面が自動拡大される問題の解消

入力：[spec.md](spec.md)（Clarifications 2026-10-12 を含む）、UI の正本 `docs/common/ui-implementation-guidelines.md`（6. フォーム実装ルール、12. 確認観点）、`develop`（`2f48e21`）の `LifePlan/wwwroot/css/site.css`・`life-plan.css`・`contact.css`、`LifePlan/Views/Shared/_Layout.cshtml`、`Views/LifePlan/Index.cshtml`、`Views/Contact/Index.cshtml`、Bootstrap 5.3.3。

Technical Context に NEEDS CLARIFICATION はない。以下は設計上の判断の記録。

## R1. タッチ操作が主の端末をどう判定するか

- **Decision**: CSS のメディア特性 `pointer: coarse`（主な入力装置の精度が粗い＝指で操作する）で判定する。画面幅は条件に入れない。
- **Rationale**:
  - `pointer` は「主な入力装置」の特性で、spec の「入力方法で判定する」（Clarifications）にそのまま対応する。
  - iPhone（縦・横）、iPad（トラックパッドなし）、Android のスマートフォン・タブレットは `coarse`、マウス・トラックパッドが主の PC は `fine` になる。タッチ画面付きのノート PC も、主な入力がトラックパッドなら `fine` で、PC と同じ扱いになる（spec Edge Cases）。
  - CSS だけで判定でき、JavaScript もサーバー側の端末判定（User-Agent）も要らない。
- **Alternatives considered**:
  - `hover: none`：主な入力装置がホバーできるかの判定で、多くの場合 `pointer: coarse` と一致する。ただし一部の Android 端末（ペン対応など）で `hover: hover` を返す例が知られており、判定がずれる恐れがある。
  - `any-pointer: coarse`：タッチ画面付きのノート PC も対象になり、PC の見た目が変わる（FR-012 に反する）。
  - 画面幅（`max-width: 767.98px`）：iPhone の横向き（多くの機種で 768px 以上）と iPad が対象から漏れる（spec「現状」）。
  - User-Agent で iOS を判別：iPadOS の Safari は Mac と同じ User-Agent を名乗るため判別できない。サーバー側の処理も増える。
  - `@supports (-webkit-touch-callout: none)` で iOS だけを狙う：非標準の機能に頼る裏技で、保守しにくい。iOS 以外（Android）も対象とする回答（Clarifications）とも合わない。
- **既知の限界**：トラックパッドやマウスをつないだ iPad が `fine` を返す場合は PC 扱いになり、自動拡大が起きうる。spec でユーザーが許容済み。実機で判定結果を記録する（[quickstart.md](quickstart.md) D3）。

## R2. 文字の大きさの値と単位

- **Decision**: 16px（固定の px 値）とする。`site.css` の `:root` に新しいトークン `--ui-font-size-control-touch: 16px;` を追加して参照する。
- **Rationale**:
  - iOS が自動拡大するかどうかは、計算後の文字の大きさが 16px 未満かで決まる。基準の文字の大きさ（`html`）が画面幅によって 14px / 16px と変わるため、`rem` では幅 768px 未満で 14px 相当になり、条件を満たせない。
  - 既存の `:root` にも、画面幅で切り替える基準の値として px のトークン（`--ui-font-size-root-sm: 14px` など）がある。px の値をトークンとして定義するのは既存の方針に沿う（UI ガイドライン 4）。
  - Bootstrap の `:root` に `-webkit-text-size-adjust: 100%` があり、iOS の横向きで文字が自動的に大きく補正されることはない。そのため、指定した 16px がそのまま計算後の値になる。
- **Alternatives considered**:
  - `max(16px, 1rem)`：基準の文字の大きさが px で固定されているため、利用者のブラウザ既定の文字の大きさは `rem` に反映されない。16px と結果が変わらず、記述だけが複雑になる。
  - 17px 以上：16px で条件を満たすため、必要以上に大きくしない（FR-009、変更の最小化）。
  - スマートフォン幅の基準（`html`）を 16px にする：フォーム以外の文字もすべて変わる（FR-011 に反する）。

## R3. どこに、どの詳細度で指定するか

- **Decision**: `site.css` に、`@media (pointer: coarse)` で囲んだ次の 1 か所だけを追加する。

  | セレクター | 対象 |
  | --- | --- |
  | `input.form-control` | シミュレーターの入力欄 32 個、お問い合わせの入力欄 4 個 |
  | `select.form-select` | シミュレーターの選択欄 36 個（テンプレート上は 12 か所。子ども 4 人分に展開）、「表示する年度」選択欄 1 個 |
  | `select.form-control` | お問い合わせの種別 1 個 |
  | `textarea.form-control` | お問い合わせの本文 1 個 |

- **Rationale**:
  - 読み込み順は Bootstrap → `site.css` → 各ページの CSS（`life-plan.css`、`contact.css` は `Styles` セクションで後から読む）。後から読む `.life-plan-control { font-size: … }`（詳細度 0,1,0）と `contact.css` の `.form-control { font-size: … }`（0,1,0）に勝つには、要素名と組み合わせた詳細度（0,1,1）が必要。これにより、読み込み順に依存せず 1 か所で効く。
  - Bootstrap の部品クラスに対する規則なので、今後追加するフォームにも自動で効く。UI ガイドライン 6 に追記した規則の実装として一貫する。
  - 既存の CSS に、これらの部品の文字の大きさを 0,1,1 以上の詳細度で指定している規則はない（`grep` で確認済み。`.select-wrapper select.form-control` は文字の大きさを指定していない）。
  - チェックボックス（`.form-check-input`）とボタンは対象外になる（spec Assumptions）。
- **Alternatives considered**:
  - `life-plan.css` と `contact.css` のそれぞれにメディアクエリを書く：同じ意図の指定が 3 か所に分かれる（UI ガイドライン 8 の重複に当たる）。今後のフォームで書き忘れる恐れもある。
  - `!important`：詳細度で解決できるため使わない。
  - 要素だけ（`input, select, textarea`）：チェックボックスやラジオボタンも対象になり、クラス付きの指定（0,1,0）には負ける。

## R4. レイアウトへの影響の見積もり

- **Decision**: 高さ・幅の調整は、計画の段階では入れない。確認で問題が出た場合だけ、同じメディアクエリの中で最小限の調整を加える（FR-008）。
- **見積もり**（Bootstrap の `line-height: 1.5`、`padding-block: .375rem`）：

  | 部品 | 幅 768px 未満の高さ（前 → 後） | 理由 |
  | --- | --- | --- |
  | シミュレーターの入力欄・選択欄 | 40px → 40px（変わらない） | `--ui-control-min-height: 40px` が下限。16px でも中身は約 36.5px で、下限を超えない |
  | 「表示する年度」選択欄 | 約 33.5px → 約 36.5px | 下限の指定がなく、1 行の高さが 21px → 24px になる |
  | お問い合わせの入力欄・選択欄 | 約 44px → 約 47px | 同上。本文欄は `min-height: 130px` のまま |

- **幅で注意する箇所**：
  - 選択欄の選択中の項目は、狭い欄で末尾が切れうる。最長は職業の「専業主婦・主夫」（7 文字、16px で約 112px）。
  - 幅 576〜991px では、職業の選択欄が `--ui-control-width: 150px` に収まる。矢印と余白を除いた表示幅は約 102px で、最後の 1 文字程度が切れる見込み。iPhone の横向きと小型タブレットがこの幅に当たる。
  - 幅 576px 未満では 1 列配置で欄が全幅になり、切れない。
  - 選択するときは iOS・Android 標準の一覧に全文が出るため、選択はできる。切れ方を確認し、意味が分からなくなる場合だけ対処する（[quickstart.md](quickstart.md) L3）。
  - 年齢の欄（`--ui-period-control-width: 80px`）は 3 桁で、16px でも約 27px に収まる。
- **Rationale**: 見積もり上、横スクロールや配置の崩れは起きない。前もって幅を変えると、PC と同じ幅の規則を分岐させることになり、変更が広がる。

### R4 の実装時の追記（2026-10-12、計測とスクリーンショットによる）

- **見積もりと違った点**：
  - 職業・収入の変化率の選択欄は幅の広い行（`--ui-control-width-wide`）で、幅 576px 以上では 388〜400px あった。「専業主婦・主夫」は切れない。上の「幅で注意する箇所」の 150px の見積もりは誤り。
  - お問い合わせの本文欄は `rows="6"` で高さが決まるため、幅 768px 未満のタッチ相当で 157px → 177px になった（`min-height: 130px` を上回る）。横にははみ出さず、FR-008 を満たすので許容する。
  - 年齢の横並び（就労期間、旅行・その他の期間）の入力欄は、80px の列に「入力欄＋余白＋『歳』」が入るため約 50px しかない。16px では「100」が「10」付近で切れた（幅 768px 以上。PC の 14px ではぎりぎり収まる）。
  - 教育プランの選択欄は 80px で、16px では「私立文系」「私立理系」がどちらも「私立」までしか見えず、見分けられなかった（変更前の 12.25px では「私立文」まで見えた）。
- **Decision（調整）**：
  1. `site.css` の同じメディアクエリで、`--ui-period-control-width` を `--ui-period-control-width-touch`（120px）に置き換える。このトークンは年齢の横並びの列と教育プランの選択欄だけが使う（`life-plan.css`）。年齢欄は約 90px になり「100」が収まる。教育プランは「私立文系」が収まる。
  2. 幅 992px 以上（iPad の横向きなど）では教育プランが 4 列になり、120px の選択欄が隣の項目名に接し、右端が枠に届いた。`life-plan.css` に `@media (pointer: coarse)` を追加し、教育プランを 2 列にする（992px 未満は既存の規則で 1 列のまま）。教育プランの画面固有の配置なので、`site.css` ではなく `life-plan.css` に置く。
- **Alternatives considered**:
  - 教育プランの選択欄を 100px に抑えて 4 列を保つ：16px では「私立文」までしか見えず、変更前と同程度の切れ方が残る。
  - 年齢欄の単位「歳」を下の行に回す：配置の変更が大きい。
- **検証**：タッチ相当で幅 320〜1280px と 576 / 600 / 744 / 1180px で横スクロールなし。PC 相当の計測値とスクリーンショットは変更前と一致（バイト単位で同一）。

## R5. 検証の方法

- **Decision**:
  1. **自動確認（Edge headless）**：アプリを起動し、`/LifePlan`（入力画面と結果画面）と `/Contact` の HTML を取得する。`<base href>` と計測用のスクリプトを足した検証用 HTML をスクラッチに置く（リポジトリには含めない）。Edge headless を次の 2 条件で開き、対象 75 個の部品の計算後の文字の大きさと、ページの横幅を DOM に書き出して確認する。
     - Edge headless はウィンドウ幅を約 500px 未満にできないため、指定した幅の `iframe` に検証用 HTML を読み込んで計測・撮影する（メディアクエリは `iframe` の幅で判定される）。シミュレーター入力画面は、すべてのタブを表示した状態で計測する。
     - タッチ相当：`--blink-settings=primaryPointerType=2,availablePointerTypes=2,primaryHoverType=1,availableHoverTypes=1`
     - PC 相当：起動オプションなし
     - 画面幅：320 / 375 / 390 / 430 / 667 / 852 / 1024 / 1280px
     - 起動オプションでタッチを再現できない場合は、ブラウザの開発者ツールのデバイスモード（タッチ）で同じ確認を手動で行う。（2026-10-12：Edge 155 で起動オプションが効き、`pointer: coarse` が `true` になることを確認した）
  2. **スクリーンショット**：同じ条件で修正前後を撮り、PC 相当の条件で差がないこと（SC-006）と、タッチ相当の条件で崩れがないこと（SC-004）を確認する。
  3. **実機確認**：自動拡大は iOS の実機でしか再現できないため、iPhone（Safari・Chrome、縦・横）、iPad、Android で確認する（[quickstart.md](quickstart.md) D1〜D5）。
- **Rationale**: 自動拡大そのものはヘッドレスのブラウザでは再現できない。そのため、原因の指標（計算後の文字の大きさが 16px 以上か）を自動で網羅的に確かめ、症状は実機で確かめる。002 と同じく、検証用ファイルはリポジトリに含めない。
- **Alternatives considered**: Playwright などの導入は、新しい依存の追加になる（憲章 原則 IV）。今回は CSS 1 か所の変更なので見合わない。

## R6. 正本との関係

- **Decision**: 規則は UI ガイドライン 6・12 に反映済み（ユーザー了承、2026-10-12）。`docs/simulator/index.md` には画面の文字の大きさの規定がないため、変更しない。
- **Rationale**: 本機能は画面固有の仕様ではなく、フォーム部品に共通の実装規則である。
