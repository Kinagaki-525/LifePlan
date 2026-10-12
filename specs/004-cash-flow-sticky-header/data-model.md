# Data Model: スマートフォン版キャッシュフロー表の年度ヘッダー

**Feature**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)

計算の入力・出力（Domain の型）は変更しない。追加・変更するのは画面表示用の ViewModel だけである。

## 1. 既存のデータ（変更なし）

`Domain/Logic` の年次データ `AnnualCashFlowRow`（抜粋）：

| フィールド | 型 | 内容 |
| --- | --- | --- |
| `Year` | `int` | 西暦（試算した年から 1 年ずつ） |
| `HusbandAge` | `int?` | 夫の年齢。100 歳を超えた年度は `null` |
| `WifeAge` | `int?` | 妻の年齢。100 歳を超えた年度は `null` |

試算は、夫婦のうち若いほうが 100 歳になる年まで続く。そのため、`null` になるのは年上の配偶者だけで、2 人とも `null` の年度はない。

## 2. CashFlowYearHeaderViewModel（新規、`ViewModels/LifePlan`）

キャッシュフロー表の年度列 1 列分の見出し。

| プロパティ | 型 | 例 | 規則 |
| --- | --- | --- | --- |
| `Year` | `string` | `"2026"` | `AnnualCashFlowRow.Year` の数字だけ。「年」は付けない（PC 幅の表示を変えないため。「年」はビューでスマートフォン幅だけに出す） |
| `HusbandAgeText` | `string` | `"夫27歳"` / `"夫 -"` | `HusbandAge` が値を持てば `"夫{年齢}歳"`、`null` なら `"夫 -"`（半角空白＋半角ハイフン。表本体の空の年齢と同じ「-」） |
| `WifeAgeText` | `string` | `"妻26歳"` / `"妻 -"` | 夫と同じ規則 |

- 作成：`LifePlanPageMapper.ToResultViewModel` で、`CashFlowRows` の年齢行と**同じ年次データの並び**から 1 対 1 に作る（FR-002、FR-003）。
- 数値の書式：年齢は整数をそのまま文字列にする（カルチャーに依存しない。既存の `ToAgeText` と同じ）。

## 3. LifePlanResultViewModel（変更）

| プロパティ | 変更前 | 変更後 |
| --- | --- | --- |
| `YearHeaders` | `IReadOnlyList<string>`（例：`["2026", "2027"]`） | `IReadOnlyList<CashFlowYearHeaderViewModel>` |
| `CashFlowRows`、`ChartPoints`、`Assumptions` | — | 変更なし |

不変条件：`YearHeaders.Count` は、`CashFlowRows` の各行の `Values.Count` と等しい（列の数が一致する）。

## 4. 画面の状態（スマートフォン幅の年度ヘッダー）

JavaScript が管理する表示上の状態。データは持たない。

| 状態 | 条件 | 見え方 |
| --- | --- | --- |
| 無効 | PC 幅（576px 以上）、または JavaScript が動かない | 修正前と同じ。複製の見出しはない |
| 通常 | スマートフォン幅で、表の先頭が画面上端より下 | 複製の見出しが元の見出しにぴったり重なり、表の先頭に表示される |
| 固定 | スマートフォン幅で、表の先頭が画面上端を過ぎ、表の終端（の見出しの高さ手前）がまだ画面上端より下 | 複製の見出しが画面上端に固定される |
| 押し出し・通過 | 表の終端が画面上端に近づく〜過ぎる | 複製の見出しが表の終端とともに上へ押し出され、画面外に出る |

- 通常・固定・押し出しの切り替えはブラウザ（`position: sticky`）が行い、JavaScript は関与しない。
- 横位置は、どの状態でも表の枠の横スクロール位置に合わせる。

状態の遷移：

```text
無効 ⇄（幅が 575px 以下になる / 576px 以上になる）⇄ 通常 ⇄ 固定 ⇄ 押し出し・通過
                                                （ページの縦スクロールで行き来）
```
