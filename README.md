# 神経衰弱（Memory Match）

Unity で作った、2 人対戦の神経衰弱です。各プレイヤーを人間か CPU（弱・普通・強）にでき、ゲーム全体に制限時間（既定 180 秒）があります。

[GitHub Spec Kit](https://github.com/github/spec-kit) による仕様駆動開発（SDD）と TDD で作り、[unity-cli](https://github.com/akiojin/unity-cli) で Unity Editor を操作して、AI がプレイして受け入れ条件を実証しました。

## ルール

- 盤面は 24 枚（6 列 × 4 行、12 ペア）です。絵柄は色と文字（A〜L）で区別します。
- 手番のプレイヤーが 2 枚めくります。
  - **同じ絵柄**：そのペアを取得します。表のまま残り、同じプレイヤーがもう一度めくれます。
  - **違う絵柄**：1.0 秒見せたあと両方を裏に戻し、手番が相手に移ります。
- 全ペアが取られたら終了です。取ったペアが多いほうが勝ち、同数は引き分けです。
- 制限時間（既定 180 秒）が 0 になると、その時点の取得ペア数で勝敗を決めます。
- 各プレイヤーは、人間／CPU 弱／CPU 普通／CPU 強から選べます。CPU が見たカードを覚えている確率は、弱 25%、普通 60%、強 100% です。

## 遊び方

1. Unity **6000.6.4f1** で `MemoryMatch/` を開きます。
2. `Assets/Scenes/MemoryMatch.unity` を開いて、Play を押します。
3. 設定画面で各プレイヤーの種別を選び、Enter か Space（またはマウスで START）で開始します。

| 画面 | キー | 動作 |
|---|---|---|
| 設定 | ↑ ↓ | プレイヤーの行を選ぶ |
| 設定 | ← → | 種別を切り替える（既定：P1 = Human、P2 = CpuNormal） |
| 設定 | Enter / Space | 開始 |
| 対戦 | 矢印キー | カーソルを動かす |
| 対戦 | Space | カードをめくる（人間の手番のみ） |
| 対戦 | R | 配置をシャッフルし直して最初から（途中でも終了後でも可） |

人間 2 人の場合は、同じキーボードを交代で使います。

### マウスでも操作できます

| 画面 | 操作 | 動作 |
|---|---|---|
| 設定 | プレイヤーの行を左クリック／右クリック | 種別を次へ／前へ切り替える |
| 設定 | **START** をクリック | 開始 |
| 対戦 | カードの上にマウスカーソルを置く | 選択枠がそのカードに移る |
| 対戦 | 左クリック | カードをめくる（Space と同じ。人間の手番のみ） |

マウスとキーボードは混ぜて使えます。再開（R）は、キーボードのみです。

### 設定値（Play 前に `Game` オブジェクトで設定）

| 名前 | 意味 |
|---|---|
| `UseSeed` / `Seed` | シードを指定すると、配置と CPU の選択を再現できます。 |
| `TimeLimitSeconds` | 制限時間（秒）。既定は 180 です。 |

## テスト

Core（ルール、CPU、制限時間）は Unity に依存しない C# で、EditMode テストが 93 件あります。

```bash
unity-cli test --mode editmode --filter MemoryMatch.Tests
```

Unity Editor の Test Runner（Window → General → Test Runner → EditMode）からも実行できます。

## 構成

```text
MemoryMatch/Assets/
├── Scripts/Core/      # ルール・CPU・制限時間（Unity 参照なし）
├── Scripts/View/      # 画面・入力・HUD・設定画面
├── Tests/EditMode/    # Core のテスト
├── Editor/            # シーン構築（Edit Mode）
└── Scenes/MemoryMatch.unity
specs/001-memory-match/
├── interview.md       # 決定メモ（質問・回答・理由・SPEC 反映先）
├── spec.md            # 仕様（ユーザーストーリー、FR、受け入れシナリオ）
├── plan.md            # 実装方針と、FR → 受け入れ → テストの対応表
├── tasks.md           # Task 一覧（54 件）
├── data-model.md / contracts/state-contract.md   # データと状態取得の契約
├── verification.md    # 実証の記録
└── evidence/          # 根拠画像（Game View）
```

## 開発の進め方

1. **Interview**：決めること（人数、盤面、手番、勝敗、CPU、制限時間など）を 1 つずつ決めました。
2. **Specify / Analyze**：`spec.md` を書き、`/speckit-analyze` で要件・Plan・Task の整合を確認しました。
3. **Plan / Tasks**：ルールを Core に分離し、Task をテストまで分解しました。
4. **Implement**：Task ごとに失敗テスト → 最小実装 → 全テスト再実行（TDD）で進めました。
5. **実証**：unity-cli のキー入力と状態取得だけで AI がプレイしました。状態と Game View の画像を、`verification.md` に記録しています。
6. **後半の追加機能**：制限時間を、先に `spec.md` へ追記して承認を受けてから実装しました。

### 状態の取得（AI プレイ用）

```bash
unity-cli --output json raw get_component_values \
  --json '{"gameObjectName":"Game","componentType":"MemoryGame"}'
```

裏向きカードの絵柄は `FaceSymbols` に `?` で返るため、覗いてプレイすることはできません。項目の一覧は [`contracts/state-contract.md`](specs/001-memory-match/contracts/state-contract.md) にあります。

## 検証の結果

- EditMode 93 件、失敗 0 件。
- PlayMode：M1〜M6（初期状態、めくる、判定、二重防止、終了、R）、CPU 戦、制限時間（A1〜A5）を実証しました。
- マウス操作：設定画面、ホバー、クリック、マウスだけの通しプレイを実証しました（unity-cli の仮想マウス）。
- 未確認（[`verification.md`](specs/001-memory-match/verification.md) に記録）：
  - 実機のマウスでの操作
  - 初見の人が説明なしで遊べるか（SC-007）
  - 制限時間 180 秒を丸ごと待つ確認（SC-008）
  - PlayMode での 6 対 6 の引き分け

## 動作環境

- Unity 6000.6.4f1（Input System、Test Framework、uGUI は、unity-cli ブリッジの依存として導入済み）
- unity-cli 0.19.1（ブリッジ `com.akiojin.unity-cli-bridge` 0.19.1）
- 外部アセット、追加パッケージ、効果音はありません。実行ファイルのビルドは対象外です。

## 範囲外

効果音、オンライン対戦、ランキング、3 人以上の対戦、外部アセット、実行ファイルのビルド。
