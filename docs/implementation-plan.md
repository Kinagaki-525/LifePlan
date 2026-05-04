# ライフプランシミュレーター実装方針

## 1. 概要

本ドキュメントは、`docs/index.md` で定義されたライフプランシミュレーター機能の実装方針とPR分割を示す。

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
│  │  └─ PensionReferenceData.cs # 年金参考データ
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
| Application Validator | ViewModel の入力値を検証し、画面へ返す検証エラーを作成 |
| Application Result | Application Service の処理結果、再表示用データ、検証結果、計算結果の受け渡し |
| Domain/Logic | 純粋な計算ロジック（貯蓄計算、ローン返済額等） |
| Domain/ReferenceData | 計算や選択肢生成で参照する固定データ、基準値、参考値 |
| Domain/Rules | 業務ルール（年齢範囲、利率範囲等） |
| Tests | Domain/Logic を中心とした単体テスト |

`LifePlanCalculator` は `DateTime.Now` を直接参照せず、`Calculate(input, currentYear)` のように現在年を引数で受け取る。画面実行時は Application Service が現在年を取得して渡し、単体テストでは固定の `currentYear` を渡して計算結果を検証できるようにする。

PR #6 完了時点では、`LifePlanCalculator` は `LifePlanCalculationResult` を返し、年次行 `AnnualRows` に試算年、夫・妻・子どもの年齢、夫・妻の収入内訳、支出内訳、支出合計を保持する。PR #7 以降はこの結果構造を段階的に拡張し、同じ年次行に収支差額、貯蓄残高を追加していく。`LifePlanSubmitResult` は画面処理結果として `CalculationResult` を持つが、Domain 入力である `LifePlanData` は外へ返さず、Application Service 内の計算入力として閉じる。

### 4.4 単位変換・端数処理方針

画面入力と画面表示は万円単位、Domain/Logic の内部計算は円単位に統一する。

- ViewModel は画面入力・画面表示に合わせて万円単位の値を持つ
- Application 層は ViewModel の万円入力を円単位へ変換し、Domain/Logic に渡す
- Domain/Logic は円単位のみを扱い、計算中に万円単位の値を混在させない
- ローン返済額など小数が発生する計算結果は、Domain/Logic で円単位に四捨五入する
- Application/Mappers は Domain/Logic の円単位の計算結果を、表示用に万円単位へ変換する
- View は表示用に変換済みの値を表示し、計算責務を持たない

### 4.5 バリデーション方針

入力検証は、画面操作上の制御とサーバー側の検証を併用する。UIでは入力不可・選択肢制御・補助メッセージで誤入力を抑制し、Domain/RulesではPOSTされた値を必ず検証する。

サーバー側の画面入力検証は `Application/Validators` に置き、Application Service は検証処理の呼び出しと結果に応じた画面フロー制御に集中する。Validator は ViewModel を入力として受け取り、Domain/Rules や Domain/ReferenceData を参照して、Controller が ModelState に反映できる検証エラーを返す。

- 必須項目は、夫年齢・妻年齢など試算に必要な最低限の項目に限定する
- 年齢、期間、利率、金額は、仕様に定義された範囲内かつ数値として解釈できることを検証する
- 金額、年収、退職金、年金、家賃、生活費などは0以上を基本とし、負数は許可しない
- 利率はパーセント入力として扱い、想定運用年利は0〜20%、インフレ率は0〜5%を許可する
- 開始/終了の組み合わせは、終了が開始以上であることを検証する
- 就労終了年齢は就労開始年齢以上、年金受取開始年齢は就労終了年齢より後とする
- 住宅購入時期が未入力の場合、頭金・借入額・ローン年数・想定金利は無効扱いとし、計算に含めない
- 住宅購入時期が入力されている場合、ローン年数は1年以上、借入額と頭金は0以上、想定金利は0%以上とする
- 自動車の初回購入年齢が未入力の場合、買い替え間隔は無効扱いとし、計算に含めない
- 自動車の初回購入年齢が入力されている場合、購入額は0以上、買い替え間隔は1年以上とする
- 子ども年齢が `-（なし）` の場合、その子どもの教育費入力は無効扱いとし、計算に含めない
- 子ども年齢が入力されている場合、教育費区分は定義済みマスタの値のみ許可する
- 旅行・その他は、開始年齢と終了年齢の両方が入力されている場合のみ期間支出として扱う
- 検証エラー時は計算を実行せず、入力値を保持したまま画面にエラーを表示する

### 4.6 PR4以降の補足方針

PR #3 で `Application/Validators`、`Application/Results`、`Domain/ReferenceData`、`Application/Mappers` の責務分離を進めたため、PR #4 以降は以下の方針を適用する。

#### 入力解決責務

住宅購入、教育費、支出などの条件付き入力は、ViewModel から Domain Entity へ変換する前段で `InputNormalizer` が整理する。
- Mapper は ViewModel と Domain Entity の単純な詰め替え・単位変換を中心に保つ
- 手入力値とマスタ選択値の優先順位解決は Normalizer に寄せる
- 未入力時に関連項目を無効扱いにする処理は Normalizer に寄せる
- Mapper が検証済み入力を前提にする場合は、呼び出し側で検証済みであることを保証する
- 子ども入力と教育費入力のインデックス対応を維持するため、Mapper は子ども年齢が未入力の行も `Age = null` として Domain 入力へ残す。計算対象外判定は Domain/Logic 側で行う。

#### ReferenceData 方針

選択肢や固定マスタは `Domain/ReferenceData` に置き、Mapper や View に直接定義しない。Mapper は `Domain/ReferenceData` の値を ViewModel 用の選択肢へ変換する責務に限定する。
- 職業、年金参考値、教育費マスタなど、POST 値として検証対象になる選択肢は `Domain/ReferenceData` に集約する
- 住宅ローン年数、旅行期間、インフレ率候補など、PR #4 以降で追加する固定候補も同じ方針で扱う
- Validator は `Domain/ReferenceData` を参照し、POST された選択肢の値が定義済みかを必ず検証する

#### Validator 方針

PR #4 以降は条件付き検証が増えるため、Validator は UI 制御に頼らず POST 値を必ず検証する。
- 未入力なら無効扱いにする項目と、必須エラーにする項目を明確に分ける
- 条件付き項目は Normalizer 後ではなく、入力状態として Validator で検証する
- 検証エラー時は計算を実行せず、入力値を保持して画面へ戻す

#### 年収変化入力方針

PR #3 では年収変化をパーセントの数値入力として実装したため、PR #5 以降の収入計算でも数値入力値をそのまま年次変化率として扱う。プルダウン方式へ戻す場合は、計算実装前に ViewModel、ReferenceData、Validator、Mapper の方針を更新する。

## 5. PR分割方針

機能全体を以下のPRに分割して実装する。PR #2 完了時点で、LifePlan 画面の初期表示、ViewModel、Domain Entity、ReferenceData、Rules の基盤は作成済みとする。

### PR #1: TOPページ修正
- 既存HomeController修正
- 既存Views/Home/Index.cshtml修正
- ライフプランシミュレーターへの遷移ボタン配置

### PR #2: 基盤作成
- LifePlanController作成
- LifePlanViewModel作成
- LifePlanPageService作成
- Domain/Logic 基本構造
- 教育費マスタ、年金参考データ作成
- Application/Interfaces 契約定義

### PR #3: 家族構成・貯蓄・収入入力フォーム実装
- View 実装（家族構成、貯蓄、夫・妻の収入入力）
- Controller の POST 受け口作成
- Application/Mappers で対象入力の ViewModel から Domain Entity への変換を実装
- Domain/Rules を使った基本検証の実装
  - 夫・妻年齢、子ども年齢の範囲検証
  - 想定運用年利、年収、退職金、年金の範囲検証
- 検証エラー時は計算を実行せず、入力値を保持して画面へ戻す

### PR #4: ライフイベント・支出入力フォーム実装
- View 実装（結婚、住宅購入、自動車、教育費、旅行・その他、支出入力）
- `LifePlanData.LifeEvents` と `LifePlanData.IncomeExpense.Expenses` へ、PR #4 で追加する入力値を反映する
- 万円入力の円変換、手入力値とマスタ選択値の優先順位、未入力時の無効扱いを Mapper / Normalizer の責務分担に沿って実装する
- Domain/Rules を使った条件付き検証の実装
  - 住宅購入、自動車、教育費、旅行・その他の条件付き入力検証
  - 基本生活費、家賃、その他支出、インフレ率の範囲検証
- 検証エラー時のメッセージと表示位置を整理

### PR #4.5: 入力フォームUI調整
- PR #4 までで作成した入力フォームの見た目と操作性を整理する
- 入力仕様、ViewModel、Validator、Normalizer、Mapper、Domain Entity の契約変更は原則行わない
- セクションの表示順、見出し、余白、グルーピング、補助テキストを調整する
- 入力単位（万円、%、年齢、年）が分かりやすくなるようにラベルや表示を整理する
- 住宅購入、自動車、教育費など条件付き入力の操作性と無効状態の見え方を改善する
- 検証エラー時の視認性と表示位置を改善する
- レスポンシブ表示を確認し、スマートフォン幅でも入力しやすいレイアウトに整える
- 出力表示、計算ロジック、グラフ描画は扱わない

### PR #5: 年次計算基盤・収入計算実装
- PR #4 までで入力受付、検証、Normalizer による入力解決、Mapper による Domain 入力化は完了済みとする
- Domain/Logic の計算結果構造を拡張する
  - 年次キャッシュフロー行データを追加
  - 試算開始年/終了年、夫・妻・子どもの年齢推移を行データへ反映
  - 夫・妻それぞれの給与、退職金、年金、収入合計を円単位で計算
  - 給与は就労開始〜終了年齢に計上し、年収変化率を年次適用する
  - 退職金は就労終了年齢年に単発計上する
  - 年金は受給開始年齢以降に毎年計上する
- Application Service から計算処理を呼び出し、有効入力時に計算結果を保持する流れを作成する
- PR #5 時点では支出、貯蓄、結果表示は最小限のプレースホルダーまたは未表示のままとする

### PR #6: 支出計算実装
- PR #5 で作成済みの `AnnualCashFlowRow` を拡張し、年次行に `AnnualExpense` による支出内訳と支出合計を追加する
- Domain/Logic に支出計算を追加する
  - 基本生活費の年額化とインフレ率適用
  - 家賃、その他支出、結婚、自動車、旅行・その他の支出計算
  - 住宅購入後の家賃停止
  - 住宅ローンの年額返済額計算
  - 教育費マスタに基づく教育費計算
  - 支出合計の集計
- 支出内訳は収入と同様に円単位の結果型として保持し、表示用の万円変換は PR #8 の Application/Mappers で行う
- 住宅ローンは購入年を1年目としてローン年数分だけ返済を計上する
- 教育費は `EducationCostMaster` を参照し、初年度/次年度以降の金額差を対象年齢ごとに適用する
- `EducationCostMaster` の金額は万円単位のマスタ値なので、Domain/Logic の計算結果へ入れる前に円単位へ変換する
- 年金参考値の入力解決は PR #4 の Normalizer 側で完了済みのため、支出計算PRでは扱わない
- 子ども年齢が未入力の行も Domain 入力では保持し、教育費プランとのインデックス対応を維持する
- `LifePlan.Tests` を追加し、`Domain/Logic` の支出計算を xUnit で検証する
- ソリューションは標準形式の `LifePlan.sln` に統一する
- TOPページのライフプラン導線と `LifePlanController` の GET/POST を有効化し、画面から入力フォームへ遷移できる状態にする

### PR #7: 貯蓄計算実装
- PR #6 で支出合計まで入った `AnnualCashFlowRow` を拡張し、収支差額と貯蓄残高を追加する
- Domain/Logic に貯蓄計算を追加する
  - 収入合計、支出合計、収支差額を年次行に反映
  - 0%運用の残高推移を計算
  - 想定年利運用の残高推移を計算
- 想定運用年利は現在の金融資産残高を起点とする資産全体へ年次適用する
- `LifePlan.Tests` に貯蓄計算の単体テストを追加する
  - 初年度の開始金融資産
  - 収支差額
  - 0%運用の残高推移
  - 想定年利運用の残高推移
- `LifePlanCalculator` がさらに肥大化する場合は、収入・支出・貯蓄の計算責務を分割するか検討する。ただし不要な抽象化は避け、PR #7 の実装量と可読性を見て判断する。
- PR #7 完了時点で、Domain/Logic は `docs/index.md` の計算仕様を一通り満たす状態とする

### PR #8: 結果表示実装
- Application/Mappers で `LifePlanCalculationResult` を表示用 ViewModel へ変換する
  - 円単位の計算結果を画面表示用の万円単位へ変換
  - 年次キャッシュフロー表表示用データを作成
- View にキャッシュフロー表を表示する
- キャッシュフロー表は Figma の結果表示エリアに合わせ、既存タブ領域内の緑枠フレームに表示する
- 列数が多い場合は枠内で横スクロールできるようにする
- キャッシュフロー表は区分ごとに淡い背景色を分け、濃い色は避ける
- 初期表示、検証エラー時、計算成功時の表示状態を整理する
- 計算成功時は入力フォームを初期表示せずキャッシュフロー表を表示し、入力タブはグレー表示のまま押下可能にして該当入力フォームへ戻れるようにする
- 入力フォームへ戻った後は、選択中の入力タブのみ有効色で表示する
- 入力タブへ戻った後に再度「結果を表示する」を押すと、再計算してキャッシュフロー表を表示する
- 検証エラー表示時は、エラーメッセージ枠と入力タブの間に適切な余白を設ける
- PR #6 で有効化済みの TOP 導線と `LifePlanController` の入力画面表示を前提に、計算成功時だけ結果エリアを表示する
- PR #8 ではグラフ描画は扱わず、シミュレーショングラフ枠は Coming soon 表示の必要最小限の表示枠に留める

### PR #9: グラフ描画実装
- `wwwroot/js/life-plan-chart.js` を作成する
- 外部グラフライブラリは初期実装では追加しない
- 収入合計、支出合計、貯蓄合計（0%運用/想定年利運用）を描画する
- PR #8 の結果表示 ViewModel から、グラフ描画に必要な年次データだけをJSONとして安全に渡す
- グラフ非対応環境でもキャッシュフロー表の確認ができる状態を保つ

## 6. 実装優先順位

1. **Domain/Logic** - 年次行、収入、支出、貯蓄を段階的に実装する
2. **Application Service** - 検証済み入力を計算へ渡し、計算結果を画面へ返す流れを作る
3. **Application/Mappers** - 計算結果を表示用 ViewModel へ変換し、円から万円へ表示単位変換する
4. **Controller + View** - 初期表示、検証エラー、計算成功時の表示状態を整理する
5. **グラフ描画** - 追加ライブラリなしの簡易描画

入力フォームのUI調整は PR #4.5 で行い、PR #5 以降の計算実装には混ぜない。

## 7. 確認方法

各PRで以下を確認する：
- ビルド成功 (`dotnet build LifePlan.sln -m:1`)
- 単体テスト成功 (`dotnet test LifePlan.sln -m:1`)
- 画面が表示される
- 計算結果が仕様通り
- PR #4 以降は、未入力時・不正値時・有効値時のフォーム送信を確認する
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
