# Research: 支出近似改善

Technical Context に NEEDS CLARIFICATION はない。既存コードとの接続で判断が必要な点を記録する。

## R1. 前提値とマスタの置き場所・差し替え

- **Decision**: `Domain/ReferenceData/SimulationAssumptions.cs` に前提をまとめた不変レコードを置く（物価上昇率2%、住宅維持費率1%、負担終了年齢22/24歳、子どもの生活費マスタ、価格基準年2024、前提バージョン）。本番値は静的プロパティ `SimulationAssumptions.Current` で公開する。子どもの年齢帯は `ChildLivingCostMaster.cs`（`ChildLivingCostEntry` の一覧）に置き、`Current` から参照する。`LifePlanCalculator` は既定コンストラクタで `Current` を使い、テスト用に `SimulationAssumptions` を受け取るコンストラクタを追加する。
- **Rationale**: AT03・AT04 は「価格基準年を開始年に置いた試験用マスタ」を要求する。静的マスタ直参照では差し替えられない。既存の `EducationCostMaster` と同じ配置で、DI 登録は不要（Calculator は Service 内で `new` している）。
- **Alternatives**: DI で `IOptions` 化 → 設定ファイル変更で前提が変わり再現性（FR-022）を損なうため不採用。静的マスタのみ → AT03・AT04 を本番マスタの値で書き換える必要があり要件から外れるため不採用。

## R2. 物価係数の適用単位と丸め

- **Decision**: 各費目の「物価補正前の年額（円・整数）」を求め、`× 1.02^t` を decimal で計算して費目ごとに1回だけ円単位で四捨五入する（`MidpointRounding.AwayFromZero`、既存 `RoundToYen`）。教育費は子ども全員の基準額合計に係数を掛ける。子どもの生活費は子どもごとに `基準年額 × 1.02^(Y−2024)` を丸めてから合算する。
- **Rationale**: 正本 4.5「各費目の年額を円単位で四捨五入」、4.3「子どもごとに円単位で四捨五入してから合算」に一致。既存の `ApplyAnnualChange`（decimal 累乗）を再利用でき、AT11（240万→244.8万）、AT12（40万→40.8万）、AT18〜AT21 の値が出る。
- **Alternatives**: 教育費を子どもごとに丸める → 正本に規定がなく、端数が子ども数に依存するため不採用。

## R3. 子どもの生活費の価格補正の起点

- **Decision**: 指数は `計算年 − 価格基準年`（本番 2024）。開始年の物価係数とは掛け合わせない。価格基準年が開始年より後になる場合（試験用マスタで開始年と同じ）も同じ式で、指数0から始まる。
- **Rationale**: 正本 4.3。AT21（613,836→626,113）で二重補正がないことを検証する。

## R4. 計算仕様バージョンの扱い

- **Decision**: `LifePlanViewModel` に `CalculationSpecVersion`（string）を追加し、フォームの hidden で送る。定義済みの値は `SimulationAssumptions` 側の `SupportedCalculationSpecVersions`（初期値 `"2"`）。
  - 欠落（null/空）：Validator はエラーにせず、Service が試算せずに `SpecVersionNotice` を立てて入力を保持した画面を返す。再表示時は hidden に現行版を入れる。
  - 未定義：Validator が `CalculationSpecVersion` キーで検証エラーを返す（文言は `LifePlanValidationMessages` に追加）。
  - `InflationRatePercent` は ViewModel から削除するため、旧フォームの送信値はモデルバインドされず計算に使われない。
- **Rationale**: 正本 3.4。欠落は「誤りではなく旧画面」なので、エラーではなく案内として扱う。既存の画面フロー（ModelState への追加は Controller）を変えずに済む。
- **Alternatives**: 欠落もエラー扱い → 正本の「新しい費用範囲の説明を表示」と文言の性質が合わないため不採用。

## R5. 子どもの概算月額合計の参考表示（FR-014）

- **Decision**: サーバー側で計算し表示する。Application 層（Factory）が Domain の `ChildLivingCostCalculator` を呼び、送信された子ども年齢から開始年の概算月額合計（年額合計÷12、表示用に百円単位で四捨五入。正本 5.3 の「約49,200円」と同じ粒度）を作る。初期表示（送信前）では年齢帯ごとの月換算参考額の一覧を出す（正本 3.5 を 2026-10-10 に改訂、ユーザー了承）。子ども年齢の変更に追従する JavaScript の再計算は行わない。
- **Rationale**: 憲章 V と正本（View・JavaScript に計算の正本を複製しない）。試算・検証エラー・旧版案内のいずれの再表示でも、送信済みの年齢による合計が出る。
- **Alternatives**: マスタを JSON で埋め込み JS で合計 → 計算の複製になるため不採用。

## R6. 住宅維持費の未算定状態（2026-10-10 廃止：価格0は住宅購入なしとして Normalizer で無効化）

- **Decision**: `LifePlanCalculationResult` に `HousingMaintenanceStatus`（`NotPlanned` / `Calculated` / `PriceMissing`）を追加する。判定は Calculator が行い、Mapper が案内文に変換する。
- **Rationale**: 業務判断を Domain に置く（憲章 II）。年次行の0円だけでは「購入なし」と「価格未設定」を区別できない。

## R7. 自動費用サマリ（初年度または開始年）（2026-10-10 廃止：結果上部の枠を削除）

- **Decision**: Mapper が年次行から「子どもの生活費」「住宅維持費」それぞれ最初に正の値となる年と額を求める。初年度が正なら初年度額、0で将来発生するなら開始年と開始年額、発生しないなら「計上なし」を表示する。
- **Rationale**: 年次行から一意に求まる表示用の加工であり、計算の正本を増やさない。

## R8. 文言と数値の出どころ

- **Decision**: 入力欄の説明・前提表示のうち、率や年齢を含む文は Application 層（`LifePlanExpenseGuidanceFactory`、`LifePlanAssumptionMapper`）で `SimulationAssumptions` から組み立てて ViewModel で渡す。数値を含まない固定文は既存の help 表示と同様に View に置いてよい。
- **Rationale**: 2%・1%・22歳などを View に直書きすると、前提改訂時に表示と計算がずれる。

## R9. 既存テストへの影響

- **Decision**: 2年目以降の支出を検証している既存テスト（`LifePlanCalculatorExpenseTests`、`LifePlanCalculatorSavingsTests`、`LifePlanPageMapperResultTests`、`LifePlanPageServiceResultTests`）は、物価係数の適用後の期待値へ更新する。インフレ率の選択肢・検証に関するテストは削除し、計算仕様バージョンのテストに置き換える。
- **Rationale**: 計算仕様の変更による期待値変更であり、回帰ではない。更新理由をテスト名と tasks に残す。
