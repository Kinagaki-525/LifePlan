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
| 結果表示 | サマリー、キャッシュフロー表、貯蓄推移グラフ |

## 4. アーキテクチャ設計

### 4.1 レイヤー構成

```
rennsyu/
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
└─ wwwroot/
    └─ js/
        └─ life-plan-chart.js    # 貯蓄推移グラフ描画（初期実装では追加ライブラリなし）
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
| Application Result | Application Service の処理結果、再表示用データ、検証結果の受け渡し |
| Domain/Logic | 純粋な計算ロジック（貯蓄計算、ローン返済額等） |
| Domain/ReferenceData | 計算や選択肢生成で参照する固定データ、基準値、参考値 |
| Domain/Rules | 業務ルール（年齢範囲、利率範囲等） |

`LifePlanCalculator` は `DateTime.Now` を直接参照せず、`Calculate(input, currentYear)` のように現在年を引数で受け取る。画面実行時は Application Service が現在年を取得して渡し、単体テストでは固定の `currentYear` を渡して計算結果を検証できるようにする。

### 4.4 単位変換・端数処理方針

画面入力と画面表示は万円単位、Domain/Logic の内部計算は円単位に統一する。

- ViewModel は画面入力・画面表示に合わせて万円単位の値を持つ
- Application Service は ViewModel の万円入力を円単位へ変換し、Domain/Logic に渡す
- Domain/Logic は円単位のみを扱い、計算中に万円単位の値を混在させない
- ローン返済額など小数が発生する計算結果は、Domain/Logic で円単位に四捨五入する
- Application Service は Domain/Logic の円単位の計算結果を、表示用に万円単位へ変換する
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

### 4.6 TODO: 入力解決責務の分離

PR #4 以降で住宅購入、教育費、支出などの条件付き入力が増えた時点で、ViewModel から Domain Entity へ変換する前段に `InputResolver` または `InputNormalizer` 相当の責務を追加することを検討する。

- Mapper は ViewModel と Domain Entity の単純な詰め替え・単位変換を中心に保つ
- 手入力値とマスタ選択値の優先順位解決は Resolver / Normalizer に寄せる
- 未入力時に関連項目を無効扱いにする処理は Resolver / Normalizer に寄せる
- Mapper が検証済み入力を前提にする場合は、呼び出し側で検証済みであることを保証する
- 年金参考値のような「手入力が空ならマスタ値を使う」処理は、PR #4 以降で同種の処理が増えたら Mapper から切り出す

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
- Application/Mappers で対象入力の ViewModel から Domain Entity への変換を実装
- Domain/Rules を使った条件付き検証の実装
  - 住宅購入、自動車、教育費、旅行・その他の条件付き入力検証
  - 基本生活費、家賃、その他支出、インフレ率の範囲検証
- 検証エラー時のメッセージと表示位置を整理

### PR #5: 計算基盤・収入計算実装
- Domain/Logic 計算基盤実装
  - 引数で受け取った現在年を基準にした試算期間計算の拡張
  - 夫・妻・子どもの年齢推移計算
  - 万円入力から円内部計算への単位変換
  - 円単位の端数処理
  - 年次計算結果の行データ構造作成
  - 給与、退職金、年金の収入計算
- Application Service から計算処理を呼び出す流れを作成

### PR #6: 支出計算実装
- Domain/Logic 支出計算実装
  - ローン返済額計算
  - 教育費計算
  - 基本生活費の年額化とインフレ率適用
  - 家賃、その他支出、結婚、自動車、旅行・その他の支出計算
  - 住宅購入後の家賃停止
- ReferenceData の教育費マスタ、年金参考値を計算・選択肢生成で利用する

### PR #7: 貯蓄・サマリー計算実装
- Domain/Logic 貯蓄・サマリー計算実装
  - 収入合計、支出合計、収支差額の集計
  - 0%運用と想定年利運用の残高計算
  - 最終年残高、最低残高年、大型支出ピーク年の算出

### PR #8: 結果表示実装
- Application/Mappers で計算結果を表示用 ViewModel へ変換
- サマリー表示
- キャッシュフロー表表示
- 検証エラーと計算結果の表示状態を整理

### PR #9: グラフ描画実装
- `wwwroot/js/life-plan-chart.js` 作成
- 貯蓄推移グラフ描画（初期実装では追加ライブラリなし）
- 収入合計、支出合計、貯蓄合計（0%運用/想定年利運用）の描画

## 6. 実装優先順位

1. **ViewModel** - 入力項目のプロパティ定義
2. **Domain/Logic** - 計算ロジック（単体テスト可能な形で）
3. **Application Service** - 計算実行ロジック
4. **Controller + View** - 画面表示
5. **グラフ描画** - 追加ライブラリなしの簡易描画

## 7. 確認方法

各PRで以下を確認する：
- ビルド成功 (`dotnet build`)
- 画面が表示される
- 計算結果が仕様通り

計算ロジックは、可能な範囲で単体テストにより以下を確認する：

- 現在年から夫・妻の両方が100歳になる年まで試算期間が作成される
- 夫・妻・子どもの年齢が年ごとに+1される
- 負数の子ども年齢が将来出生予定として扱われ、対象年齢から教育費が計上される
- 給与が就労開始〜終了年齢の期間だけ計上される
- 退職金が就労終了年齢年に単発計上される
- 年金が受取開始年齢以降だけ計上される
- 基本生活費が月額×12で年額化され、インフレ率が年次適用される
- 住宅ローンの年額返済額が、金利0%と金利ありの両方で仕様通り算出される
- 住宅購入年から家賃が0になる
- 教育費の初年度金額と次年度以降金額が正しく切り替わる
- 0%運用と想定年利運用の貯蓄残高が仕様通り算出される
- 万円入力から円内部計算、円計算結果から万円表示への変換がずれない
- 小数が発生する計算結果が円単位で四捨五入される
