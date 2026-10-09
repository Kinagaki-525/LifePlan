# LifePlan のレビュー参照先

ここに仕様を複製せず、リポジトリルートから以下の正本を読む。

| 対象 | 正本・参照先 |
| --- | --- |
| 作業ルール・優先順位 | `AGENTS.md`、`.github/copilot-introduction.md` |
| シミュレーター要件 | `docs/simulator/index.md` |
| シミュレーター実装方針 | `docs/simulator/implementation-plan.md` |
| 記事表示 | `docs/articles/feature-spec.md`、`docs/articles/implementation-plan.md` |
| UI・CSS | `docs/common/ui-implementation-guidelines.md` |
| テスト・CI | `LifePlan.Tests/`、`.github/workflows/ci.yml` |

仕様と実装方針の関係は `AGENTS.md` の優先順位に従う。仕様・責務・画面挙動・データ構造に影響する矛盾は、推測せず確認事項として報告する。分割先が未記入の場合は、シミュレーター要件の正本を使う。

## 変更別の確認

- **Domain/Logic・Domain/Rules**：仕様から求めた期待値、年齢や利率の境界、期間の包含、円単位の計算、丸め、固定年での再現性、関連するxUnitテストを確認する。
- **Validator・Normalizer・Mapper・Service**：入力制約、無効項目、単位変換、検証失敗時に計算しないこと、入力保持、責務と依存方向を確認する。分岐・画面フローへの影響に応じてテスト不足を判断する。
- **Controller・Razor・JavaScript**：ControllerのService interfaceへの依存、サーバー側検証、エラーメッセージの正本、入力保持、タブ状態、表示用データ、ARIAを確認する。UI・Controllerの新しいテスト基盤は一律に要求せず、具体的な検証観点を示す。
- **記事・外部API**：検索条件・順序・ページ境界、キャンセル、API失敗と記事なしの区別、HTMLサニタイズ、APIキーの扱いを確認する。HTTPを代替するテストと、実APIへの接続確認を区別する。
- **リンク・広告**：設定からViewModelへの受け渡し、URL全体の保持、`rel`属性、PR表示、空URL時の扱いを確認する。

ビルド・テストのコマンドと警告の扱いは `AGENTS.md` の Verification、テスト追加の方針は `.github/copilot-introduction.md` の Testing に従う。
