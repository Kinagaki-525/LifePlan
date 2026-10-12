# Contract: フォーム部品の文字の大きさ（タッチ操作が主の端末）

UI ガイドライン（`docs/common/ui-implementation-guidelines.md`）の「6. フォーム実装ルール」を、このリポジトリの CSS でどう満たすかの約束。今後追加するフォームも、この約束に従えば自動で対象になる。

## 対象になる部品

文字を入力・選択する、Bootstrap のフォーム部品クラスを付けた要素。

| 要素とクラス | 例 |
| --- | --- |
| `input` ＋ `form-control` | 文字・数値の入力欄（`type="text"`、`type="number"`、`type="email"` など） |
| `select` ＋ `form-select` | 選択欄（シミュレーター、「表示する年度」） |
| `select` ＋ `form-control` | 選択欄（お問い合わせの種別） |
| `textarea` ＋ `form-control` | 本文欄 |

画面固有のクラス（`life-plan-control` など）を併用しても対象になる。

## 対象にならない部品

- チェックボックス・ラジオボタン（`form-check-input`）、ボタン、リンク、タブ
- `type="hidden"` の入力欄（表示されない）
- Bootstrap のフォーム部品クラスを付けていない入力欄（現時点では存在しない）。今後追加する場合は、Bootstrap のクラスを付けるか、同じ条件で 16px 以上にする。

## 約束

1. 主な入力装置がタッチの端末（`pointer: coarse`）では、対象の部品の文字の大きさを `--ui-font-size-control-touch`（16px）にする。画面幅によらない。
2. 指定は `wwwroot/css/site.css` の 1 か所に置く。要素名とクラスを組み合わせた詳細度にし、画面ごとの CSS（`life-plan.css`、`contact.css` など）の文字の大きさの指定より優先させる（[research.md](../research.md) R3）。
3. 画面ごとの CSS では、タッチの端末での対象部品の文字の大きさを上書きしない。上書きが必要な場合も 16px 未満にしない。
4. PC（`pointer: fine` など）では、対象の部品の文字の大きさを変えない。
5. viewport に `maximum-scale`、`minimum-scale`、`user-scalable` を加えない。
6. 文字が大きくなって欄に収まらない場合は、同じ条件（`pointer: coarse`）で欄の幅や並びを調整する。共通のトークンで済むものは `site.css` の同じメディアクエリで、画面固有の配置は画面ごとの CSS で行う（例：`--ui-period-control-width-touch`、教育プランの 2 列。[research.md](../research.md) R4 の追記）。
