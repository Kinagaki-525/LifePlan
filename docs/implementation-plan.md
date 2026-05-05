# ライフプランシミュレーター実装方針

## 1. 概要

本ドキュメントは、`docs/index.md` で定義されたライフプランシミュレーター機能の実装方針を示す。

## 2. 画面構成

| 画面 | パス | 説明 |
| --- | --- | --- |
| TOPページ | `/` (Home/Index) | ライフプランシミュレーターへの遷移ボタンを配置 |
| シミュレーター | `/LifePlan` (LifePlan/Index) | 入力画面と結果表示 |

## 3. 実装範囲

| 機能 | 説明 |
| --- | --- |
| TOPページ | ライフプランシミュレーターへの遷移ボタン |
| 入力画面 | 家族構成、ライフイベント、貯蓄・収入・支出の各種入力 |
| 計算エンジン | 年次キャッシュフロー計算、貯蓄推移算出 |
| 結果表示 | キャッシュフロー表、シミュレーショングラフ |

## 4. アーキテクチャ設計

### 4.1 レイヤー構成

```
LifePlan/
├─ Controllers/
│  ├─ HomeController.cs          # TOPページ制御
│  └─ LifePlanController.cs      # シミュレーター制御
├─ Views/
│  ├─ Home/
│  │  └─ Index.cshtml            # TOPページ
│  └─ LifePlan/
│       └─ Index.cshtml          # 入力・結果画面
├─ ViewModels/
│  ├─ HomeViewModel.cs           # TOPページ用（不要なら省略可）
│  └─ LifePlanViewModel.cs       # シミュレーター画面データ（入力項目はdocs/index.md 3.入力仕様を参照）
├─ Application/
│  ├─ Interfaces/
│  │  └─ ILifePlanPageService.cs # シミュレーター画面サービス契約
│  ├─ Services/
│  │   └─ LifePlanPageService.cs # シミュレーター画面ロジック
│  ├─ Factories/
│  │   └─ LifePlanRateSelectOptionFactory.cs # 利率系選択肢の画面用生成
│  ├─ Validators/
│  │   └─ LifePlanInputValidator.cs # 画面入力のサーバー側検証
│  └─ Results/
│       └─ LifePlanSubmitResult.cs # Application Service の処理結果
├─ Domain/
│  ├─ Entities/
│  │  └─ LifePlanData.cs         # 業務データ
│  ├─ Logic/
│  │  └─ LifePlanCalculator.cs   # 計算ロジック（計算仕様はdocs/index.md 4.計算仕様を参照）
│  ├─ ReferenceData/
│  │  ├─ EducationCostMaster.cs  # 教育費マスタ
│  │  ├─ PensionReferenceData.cs # 年金参考データ
│  │  └─ RateOptionCatalog.cs    # 年収変化・想定インフレ率の選択肢
│  └─ Rules/
│       ├─ AgeRules.cs           # 年齢範囲ルール
│       └─ RateRules.cs          # 利率範囲ルール
├─ Infrastructure/
│  └─ (現時点ではDB不要)
├─ LifePlan.Tests/
│  └─ Domain/
│      └─ Logic/                 # Domain/Logic の単体テスト
└─ wwwroot/
    └─ js/
        └─ life-plan-chart.js    # シミュレーショングラフ描画（初期実装では追加ライブラリなし）
```

### 4.2 依存関係

- **Controllers** → Application/Services
- **Application** → Domain/Logic, Domain/ReferenceData, Domain/Rules
- **Views** → ViewModels
- **Domain** は他レイヤーへの依存を持たない

### 4.3 責務分担

| レイヤー | 責務 |
| --- | --- |
| Controller | HTTPリクエスト受付、画面遷移、ViewModel受け渡し |
| ViewModel | 画面表示用データ、入力バインド |
| Application Service | 画面単位の処理フロー、現在年の取得、計算実行 |
| Application Factory | ViewModel に渡す画面用オブジェクトや設定の組み立て |
| Application Validator | ViewModel の入力値を検証し、画面へ返す検証エラーを作成 |
| Application Result | Application Service の処理結果、再表示用データ、検証結果、計算結果の受け渡し |
| Domain/Logic | 純粋な計算ロジック（貯蓄計算、ローン返済額等） |
| Domain/ReferenceData | 計算や選択肢生成で参照する固定データ、基準値、参考値 |
| Domain/Rules | 業務ルール（年齢範囲、利率範囲等） |
| Tests | Domain/Logic を中心とした単体テスト |

`LifePlanCalculator` は `DateTime.Now` を直接参照せず、`Calculate(input, currentYear)` のように現在年を引数で受け取る。画面実行時は Application Service が現在年を取得して渡し、単体テストでは固定の `currentYear` を渡して計算結果を検証できるようにする。

`LifePlanCalculator` は `LifePlanCalculationResult` を返し、年次行 `AnnualRows` に試算年、夫・妻・子どもの年齢、収入内訳、支出内訳、支出合計、収支差額、貯蓄残高を保持する。`LifePlanSubmitResult` は画面処理結果として `CalculationResult` を持つが、Domain 入力である `LifePlanData` は外へ返さず、Application Service 内の計算入力として閉じる。

### 4.4 単位変換・端数処理方針

画面入力と画面表示は万円単位、Domain/Logic の内部計算は円単位に統一する。

- ViewModel は画面入力・画面表示に合わせて万円単位の値を持つ
- Application 層は ViewModel の万円入力を円単位へ変換し、Domain/Logic に渡す
- Domain/Logic は円単位のみを扱い、計算中に万円単位の値を混在させない
- ローン返済額など小数が発生する計算結果は、Domain/Logic で円単位に四捨五入する
- Application/Mappers は Domain/Logic の円単位の計算結果を、表示用に万円単位へ変換する
- View は表示用に変換済みの値を表示し、計算責務を持たない

### 4.5 バリデーション方針

入力検証は、画面操作上の制御とサーバー側の検証を併用する。UIでは入力不可・選択肢制御・補助メッセージで誤入力を抑制し、Domain/Rules や Domain/ReferenceData を参照して POST された値を必ず検証する。

サーバー側の画面入力検証は `Application/Validators` に置き、Application Service は検証処理の呼び出しと結果に応じた画面フロー制御に集中する。Validator は ViewModel を入力として受け取り、Domain/Rules や Domain/ReferenceData を参照して、Controller が ModelState に反映できる検証エラーを返す。

- 必須項目は、夫年齢・妻年齢など試算に必要な最低限の項目に限定する
- 年齢、期間、金額は、仕様に定義された範囲内かつ半角整数として解釈できることを検証する
- 金額、年収、退職金、年金、家賃、生活費などは0以上かつ整数を基本とし、負数と小数は許可しない
- 利率はパーセント入力として扱い、住宅ローンの想定金利は0%以上かつ小数第1位まで、想定運用年利は0〜20%を許可する
- 年収変化と想定インフレ率は定義済みの選択肢のみ許可し、POST 値が定義済みかを `Domain/ReferenceData` で検証する
- 開始/終了の組み合わせは、終了が開始以上であることを検証する
- 就労終了年齢は就労開始年齢以上、年金受取開始年齢は就労終了年齢より後とする
- 住宅購入時期が未入力の場合、頭金・借入額・ローン年数・想定金利は無効扱いとし、計算に含めない
- 住宅購入時期が入力されている場合、ローン年数は1年以上、借入額と頭金は0以上かつ整数、想定金利は0%以上かつ小数第1位までとする
- 自動車の初回購入年齢が未入力の場合、買い替え間隔は無効扱いとし、計算に含めない
- 自動車の初回購入年齢が入力されている場合、購入額は0以上、買い替え間隔は1年以上とする
- 子ども年齢が `-（なし）` の場合、その子どもの教育費入力は無効扱いとし、計算に含めない
- 子ども年齢が入力されている場合、教育費区分は定義済みマスタの値のみ許可する
- 旅行・その他は、開始年齢と終了年齢の両方が入力されている場合のみ期間支出として扱う
- 検証エラー時は計算を実行せず、入力値を保持したまま画面にエラーを表示する

### 4.6 入力実装の補足方針

入力受付、検証、参照データ、変換処理は以下の責務分担で実装する。

#### 入力解決責務

住宅購入、教育費、支出などの条件付き入力は、ViewModel から Domain Entity へ変換する前段で `InputNormalizer` が整理する。
- Mapper は ViewModel と Domain Entity の単純な詰め替え・単位変換を中心に保つ
- 手入力値とマスタ選択値の優先順位解決は Normalizer に寄せる
- 未入力時に関連項目を無効扱いにする処理は Normalizer に寄せる
- Mapper が検証済み入力を前提にする場合は、呼び出し側で検証済みであることを保証する
- 子ども入力と教育費入力のインデックス対応を維持するため、Mapper は子ども年齢が未入力の行も `Age = null` として Domain 入力へ残す。計算対象外判定は Domain/Logic 側で行う。

#### ReferenceData 方針

選択肢や固定マスタは `Domain/ReferenceData` に置き、Mapper や View に直接定義しない。Mapper や Factory は `Domain/ReferenceData` の値を ViewModel 用の選択肢へ変換する責務に限定する。
- 職業、年金参考値、教育費マスタなど、POST 値として検証対象になる選択肢は `Domain/ReferenceData` に集約する
- 住宅ローン年数、旅行期間、インフレ率候補などの固定候補も同じ方針で扱う
- 年収変化率や想定インフレ率のように定義済み候補から選ぶ項目は `Domain/ReferenceData` に候補を置き、Factory は ViewModel 用の選択肢へ変換する
- Validator は `Domain/ReferenceData` を参照し、POST された選択肢の値が定義済みかを必ず検証する

#### Validator 方針

Validator は UI 制御に頼らず POST 値を必ず検証する。
- 未入力なら無効扱いにする項目と、必須エラーにする項目を明確に分ける
- 条件付き項目は Normalizer 後ではなく、入力状態として Validator で検証する
- 検証エラー時は計算を実行せず、入力値を保持して画面へ戻す

#### 利率選択肢入力方針

年収変化と想定インフレ率は自由入力ではなくプルダウン方式とする。選択肢は `-（なし）`、`控えめ（年1%増）`、`標準（年2%増）` の3つとし、未選択は変化なしとして扱う。

候補値は `Domain/ReferenceData` に定義し、Factory は表示用の選択肢へ変換する。Validator は UI 制御に頼らず POST 値が定義済み候補かを検証する。

## 5. 確認方法

変更時は以下を確認する：
- ビルド成功 (`dotnet build LifePlan.sln -m:1`)
- 単体テスト成功 (`dotnet test LifePlan.sln -m:1`)
- 画面が表示される
- 計算結果が仕様通り
- 未入力時・不正値時・有効値時のフォーム送信を確認する
- 条件付き入力は、関連項目が未入力のときに無効扱いになること、関連項目が入力済みのときに必要な範囲検証が行われることを確認する
- 検証エラー時は計算を実行せず、入力値を保持したまま該当項目にエラーが表示されることを確認する

計算ロジックは、可能な範囲で単体テストにより以下を確認する：

- 現在年から夫・妻の両方が100歳になる年まで試算期間が作成される
- 夫・妻・子どもの年齢が年ごとに+1される
- 負数の子ども年齢が将来出生予定として扱われ、対象年齢から教育費が計上される
- 給与が就労開始〜終了年齢の期間だけ計上される
- 退職金が就労終了年齢年に単発計上される
- 年金が受取開始年齢以降だけ計上される
- 基本生活費が月額×12で年額化され、インフレ率が年次適用される
- 家賃が住宅購入前まで計上され、住宅購入年から0になる
- 住宅ローンの年額返済額が、金利0%と金利ありの両方で仕様通り算出される
- 住宅購入年に頭金と年額ローン返済額が計上され、借入額そのものは支出合計に直接計上されない
- 結婚、自動車、旅行・その他の支出が夫年齢基準で対象年に計上される
- 教育費の初年度金額と次年度以降金額が正しく切り替わる
- 教育費マスタの万円単位が円単位の支出計算結果へ正しく変換される
- 0%運用と想定年利運用の貯蓄残高が仕様通り算出される
- 万円入力から円内部計算、円計算結果から万円表示への変換がずれない
- 小数が発生する計算結果が円単位で四捨五入される
