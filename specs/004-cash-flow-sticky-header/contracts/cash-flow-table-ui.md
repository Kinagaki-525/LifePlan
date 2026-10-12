# UI Contract: キャッシュフロー表（年度ヘッダーと固定用の見出し）

**Feature**: [../spec.md](../spec.md) | **Plan**: [../plan.md](../plan.md) | **Research**: [../research.md](../research.md)

本書は、View・CSS・JavaScript の間の約束（DOM 構造、クラス、属性、振る舞い）を定める。クラス名は実装時に既存の命名（`life-plan-cash-flow__*`）に合わせて最終化してよいが、変えた場合は本書を更新する。

## 1. DOM 構造

```html
<div class="life-plan-result-frame">
  <h2 id="cash-flow-table-title" …>キャッシュフロー表</h2>
  <p class="life-plan-cash-flow__scroll-hint" aria-hidden="true">表は左右にスクロールできます</p>

  <!-- 追加：固定の範囲（表の先頭〜終端）。overflow を指定しない -->
  <div class="life-plan-cash-flow-area" data-life-plan-cash-flow>

    <!-- JavaScript がスマートフォン幅だけで挿入する（サーバーは出力しない） -->
    <div class="life-plan-cash-flow__sticky-head" aria-hidden="true" inert>
      <table class="life-plan-cash-flow__table"><thead>…元の thead の複製…</thead></table>
    </div>

    <!-- 既存（変更しない） -->
    <div class="life-plan-cash-flow" role="region" aria-label="キャッシュフロー表" tabindex="0">
      <table class="life-plan-cash-flow__table">
        <caption class="visually-hidden">…</caption>
        <thead>
          <tr>
            <th scope="col">区分</th>
            <th scope="col">項目</th>
            <!-- 年度列（変更） -->
            <th scope="col">
              <span class="life-plan-cash-flow__year">2026<span class="life-plan-cash-flow__compact">年</span></span>
              <span class="life-plan-cash-flow__age life-plan-cash-flow__compact">夫27歳</span>
              <span class="life-plan-cash-flow__age life-plan-cash-flow__compact">妻26歳</span>
            </th>
            …
          </tr>
        </thead>
        <tbody>…変更なし…</tbody>
      </table>
    </div>
  </div>

  <p class="life-plan-result-frame__note">金額の単位は万円です。</p>
</div>
```

- 年度列の `span` の間には、テンプレート上の改行（空白）を入れる。
- 「区分」「項目」の見出しセル、`tbody`、`caption`、表の枠の属性（`role`、`aria-label`、`tabindex`）は変更しない。

## 2. 表示の出し分け（CSS）

| 対象 | PC 幅（576px 以上） | スマートフォン幅（575px 以下） |
| --- | --- | --- |
| `.life-plan-cash-flow__compact` | `display: none`（表示も読み上げもされない） | 表示 |
| `.life-plan-cash-flow__age` | — | `display: block`（年度の下に 1 段ずつ） |
| 年度列の見出しの表示 | 「2026」 | 「2026年」「夫27歳」「妻26歳」の 3 段 |
| 年度列の見出しの読み上げ | 「2026」 | 「2026年 夫27歳 妻26歳」 |
| `.life-plan-cash-flow__sticky-head` | 存在しない（JS が作らない） | `position: sticky; top: 0`、`overflow: hidden`、`pointer-events: none`、表の行より手前（`z-index`）、背景は不透明 |

- 既存のルール（`.life-plan-cash-flow`、`.life-plan-cash-flow__table` の `th`・`td`、固定列、`@media (max-width: 575.98px)` 内の既存ルール）は変更しない。追加のルールだけで実現する。
- `.life-plan-cash-flow-area` とその祖先に `overflow`・`contain`・`transform` を指定しない（`position: sticky` の基準が変わるため。CSS にコメントを残す）。
- 色・余白・枠線・角丸は既存のトークン（`--ui-color-*`、`--ui-border-width-sm`、`--ui-radius-sm` など）を使う。

## 3. JavaScript（`wwwroot/js/life-plan-cash-flow.js`）の振る舞い

| # | 契機 | 振る舞い |
| --- | --- | --- |
| J1 | 読み込み時 | `[data-life-plan-cash-flow]` がなければ何もしない。あれば `matchMedia('(max-width: 575.98px)')` を評価する |
| J2 | スマートフォン幅になったとき（読み込み時を含む） | 元の `thead` を複製し、`id` 属性を取り除いて、`.life-plan-cash-flow__sticky-head` に入れ、表の枠の直前に挿入する。J3・J4 を実行する |
| J3 | 列幅の同期（J2、元の表の大きさの変化） | 元の見出しの各セルの実測幅を、複製の対応するセルの `width`・`min-width`・`max-width` に設定し、複製の表の幅を元の表の実測幅にする。元の `thead` の高さを CSS 変数で渡し、表の枠を同じ高さだけ複製の下に重ねる |
| J4 | 横位置の同期（J2、表の枠の `scroll`） | 複製の入れ物の `scrollLeft` に、表の枠の `scrollLeft` を代入する。`scroll` イベント内で直接行う（`requestAnimationFrame` で遅らせない）。イベントは `passive` |
| J5 | 元の表の大きさの変化 | `ResizeObserver` で検知し、J3・J4 を実行する（回転、文字の拡大、結果エリアの表示・非表示、フォントの読み込み） |
| J6 | PC 幅になったとき | 複製を取り除き、J3 で設定した CSS 変数・余白を元に戻し、イベントと監視を解除する |

- 同期は表の枠 → 複製の一方向だけ。複製の `scrollLeft` を表の枠に書き戻さない。
- 業務ルール、計算、表示文言を持たない。見出しの内容は元の `thead`（サーバーが出力）の複製だけ。
- 既存の JS（`life-plan-tabs.js`、`life-plan-chart.js` ほか）と依存関係を持たない。

## 4. アクセシビリティ

| 項目 | 約束 |
| --- | --- |
| 見出しの読み上げ | 元の `thead` だけ。複製は `aria-hidden="true"` で支援技術から外す（FR-025） |
| フォーカス | 複製は `inert` で、フォーカスもクリックもできない。表の枠の `tabindex="0"` と矢印キーによる横スクロールは変わらない（FR-019） |
| ポインター | 複製は `pointer-events: none`。見出しの位置でのタッチ・ホイールは下の表の枠に届く |
| 表の構造 | `th scope="col"`、`th scope="row"`、`caption` は変更しない（FR-024） |

## 5. 確認との対応

| 約束 | 確認 |
| --- | --- |
| 2 の表示 | quickstart A1、A6、M1 |
| 3 の J2〜J4 | quickstart A2〜A4 |
| 3 の J5・J6 | quickstart A7、M4 |
| 既存の CSS を変えない | quickstart A5（修正前後の比較）、レビュー |
| 4 | quickstart A8、A9 |
