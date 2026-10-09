# Contract: シミュレーター画面の POST（/LifePlan）

既存の `POST /LifePlan`（`LifePlanController.Index(LifePlanViewModel)`、AntiForgery 必須）に対する変更点のみ記載する。

## 追加フィールド

| name | 型 | 必須 | 値 |
| --- | --- | --- | --- |
| `CalculationSpecVersion` | string（hidden） | 新画面では常に送る | 定義済み：`2` |

## 削除フィールド

| name | 扱い |
| --- | --- |
| `IncomeExpense.Expenses.InflationRatePercent` | 画面から削除。送られてもバインドされず、計算・検証に使わない |

## 応答

| 条件 | 試算 | 入力 | 表示 |
| --- | --- | --- | --- |
| `CalculationSpecVersion` 欠落・空 | しない | 保持 | 費用範囲が変わった旨の案内（エラー枠ではない）。hidden に `2` を入れて再表示 |
| 未定義の値（例：`1`、`99`） | しない | 保持 | 検証エラー（キー `CalculationSpecVersion`、文言は `LifePlanValidationMessages`） |
| `2` かつ他の検証エラーなし | する | 保持 | 結果を表示 |
| `2` かつ他の検証エラーあり | しない | 保持 | 既存どおり検証エラー |

- 物価上昇率・住宅維持費率・子どもの生活費はリクエストから受け取らない。サーバー側の `SimulationAssumptions.Current` のみを使う
- 欠落と未定義が両方の条件に当たることはない（欠落を先に判定する）
- 版の欠落と、ほかの入力の検証エラーが同時にある場合は、案内と検証エラーを両方表示し、試算しない
- 判定には送信された版の値を使う。再表示用に hidden へ現行版を入れるのは判定の後とする
