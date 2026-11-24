# Implementation Plan: 神経衰弱（2人対戦・CPU対戦）

**Branch**: `001-memory-match` | **Date**: 2026-11-24 | **Spec**: [spec.md](spec.md)

**Input**: `specs/001-memory-match/spec.md`（承認済み）

## Summary

ゲームのルールをUnityに依存しない純粋なC#（Core）に集め、EditModeテストでTDDする。Unity層（View）は、カード表示・カーソル・入力・設定画面だけを担当する。時間（外れ1.0秒、CPU 0.8秒間隔）はCoreが `Tick(秒)` で受け取り、テストで実時間を待たない。状態は `Game` オブジェクトの公開フィールドとして出し、unity-cli の `get_component_values` で取得する。

## Technical Context

**Language/Version**: C# （Unity 6000.6.4f1 付属）

**Primary Dependencies**: 追加なし。com.unity.inputsystem、com.unity.test-framework、com.unity.ugui は、ブリッジ導入時に依存として入っているものを使う（manifest は変更しない）。

**Storage**: なし

**Testing**: Unity Test Framework（EditMode）。unity-cli で実行し、テスト名・件数・合否を照合する。

**Target Platform**: Unity Editor（PlayMode）。実行ファイルのビルドは対象外。

**Project Type**: Unityゲーム（単一プロジェクト）

**Performance Goals**: 24枚の盤面で、入力から表示反映まで1フレーム以内。

**Constraints**: 外部アセットなし。絵柄は色と記号（文字）で区別する。spec.md にないルールは追加しない。

**Scale/Scope**: 2人、24枚、1シーン（MemoryMatch.unity）

## Constitution Check

`.specify/memory/constitution.md` は未記入のテンプレートのままで、適用するゲートがない。代わりに `working-rules.md` と、スライド4の順序（SPECにないルールを追加しない）を基準にする。

- テストファースト：Task ごとに先に失敗テストを書く。**合格**
- SPEC外のルール追加なし：下の「Planで決めたこと」はすべてSPEC外の実装判断で、ゲームのルールは増やしていない。**合格**
- パッケージ追加なし：**合格**

## Planで決めたこと（実装判断。ルールではない）

| 項目 | 決定 | 根拠 |
|---|---|---|
| 構成 | Core / View / Tests の3アセンブリ。CoreはUnity参照なし | スライド11 |
| 時間 | Coreが `Tick(deltaSeconds)` を受け取る。外れ表示1.0秒、CPU間隔0.8秒 | FR-009, FR-013、実時間を待たないテスト |
| 乱数 | 自前の決定的な乱数（シード→配置とCPU選択）。System.Random は使わない | FR-002, FR-015 |
| CPUの記憶 | カードが表になるたびに、強度の確率で「覚える」を1回判定して記憶に入れる | FR-014, SC-004 |
| CPUの選択 | FR-014（改訂済み）どおり。1枚目：覚えている組があればその片方、なければ未確認から。2枚目：1枚目と同じ絵柄を覚えていればそれ、なければ未確認から（なければ残りの裏向きから） | FR-014 |
| CPUの操作 | CPUはカーソルを使わず、カードを直接指定する（interview Q10） | FR-013 |
| 時間のテスト | 外れ表示1.0秒とCPU間隔0.8秒は、テストでは秒数ちょうどで判定する（spec の「約」は実画面の揺れの許容） | FR-009, FR-013 |
| プレイヤー表記 | 内部は0/1、画面とWinnerはP1/P2（0→P1、1→P2） | contracts/state-contract.md |
| 入力 | Input System（Keyboard）。unity-cli の `input_keyboard` と同じ経路 | スライド15 |
| 設定画面の操作 | 上下で行選択、左右で種別切替、Enter または Space で開始 | SPEC外のUI操作。変更したければ指示してください |
| シード指定 | `Game` の `UseSeed` と `Seed` を、Play前にCLIで設定する | SPECはUIを定めていない |
| 絵柄 | 12色×12文字（A〜L）。外部アセットなし | FR-018 |
| 制限時間 | Core が `RemainingTime` を持ち、`Tick` で減らす（Playing と Resolving の間）。既定180秒。`MatchEngine` のコンストラクタ引数で渡し、`Game` の `TimeLimitSeconds` から設定 | FR-019 |
| 時間切れ | `Tick` の先頭で時間を判定。0なら表向きの未取得カードを裏へ戻し、手番は変えず Finished（終了の理由=TimeUp）。ペア取得は `Pick` 内で確定するので、同時ならペア取得が先 | FR-020〜022 |
| 表示 | HUD に `Time m:ss`（切り上げ）。終了時は理由（TIME UP）と勝者 | FR-010 |
| 絵柄の非公開 | 状態取得では、裏向きカードの絵柄を返さない | working-rules（覗かない） |

## Project Structure

### Documentation (this feature)

```text
specs/001-memory-match/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/state-contract.md
├── spec.md / interview.md / requirements-input.md / working-rules.md
├── tasks.md              # /speckit-tasks
└── verification.md       # 実証の記録
```

### Source Code

```text
MemoryMatch/Assets/
├── Scripts/
│   ├── Core/             # MemoryMatch.Core（Unity参照なし）
│   │   ├── MemoryMatch.Core.asmdef
│   │   ├── Board / MatchState / PlayerKind / Phase
│   │   ├── DeterministicRandom
│   │   ├── CursorGrid
│   │   ├── CpuMemory / CpuPlayer
│   │   └── MatchEngine   # 規則・手番・勝敗・Tick
│   └── View/             # MemoryMatch.View
│       ├── MemoryGame    # GameObject "Game"。状態の公開フィールド
│       ├── CardView      # Card_00〜Card_23
│       └── SetupMenu / Hud
├── Tests/EditMode/       # MemoryMatch.Tests.EditMode
└── Scenes/MemoryMatch.unity
```

**Structure Decision**: Core / View / Tests の3分割。Coreだけをテストで直接検証し、Viewは実物（Hierarchy、Game View、状態取得）で確かめる。

## 対応の鎖（FR → 受け入れ → テスト）

| FR | 受け入れ | テスト | 内容 |
|---|---|---|---|
| FR-001, 002, 003 | US1-1（M1） | UT-01 | 24枚・各絵柄2枚・同シード同配置・初期状態 |
| FR-004 | US1-2（M2） | UT-02 | カーソル移動（端で止まる） |
| FR-005 | US1-2（M2） | UT-03 | 3枚目はめくれない |
| FR-006 | US3-1（M4） | UT-04 | 取得済み・表向き・同じカードは選べない |
| FR-007, 008 | US1-3（M3） | UT-05 | 成立：取得・手番継続・残り-1 |
| FR-009 | US1-4（M3）, US3-2 | UT-06 | 不成立：1.0秒見せて裏へ・手番交代・その間の入力無効 |
| FR-010, 011 | US1-5（M5） | UT-07 | 終了：勝者・引き分け・以後の入力無効 |
| FR-012 | US2-1 | UT-08 | 種別の選択と既定値 |
| FR-013 | US2-2, 2-3 | UT-09 | CPUが0.8秒間隔で2枚めくる・人間入力を無視 |
| FR-014 | US2-4 | UT-10 | CPUの記憶と選択、確率（SC-004） |
| FR-015 | US2-5 | UT-11 | 同シード・同強度でCPUの選択が一致 |
| FR-016 | US3-3, 3-4（M6） | UT-12 | R再開：全項目を初期化（表示戻しだけにしない） |
| FR-019 | A1 | UT-13 | 残り時間の初期値と減少（設定画面・終了後は減らない） |
| FR-020, 011 | A2 | UT-14 | 時間切れで終了、取得数で勝敗・引き分け、以後の入力無効 |
| FR-021 | A5 | UT-15 | 時間切れで表向きは裏へ、手番は不変、外れ表示中も時間は減る |
| FR-022 | A3 | UT-16 | 全ペア取得で時計が止まる、同時ならペア取得優先 |
| FR-016, 003 | A4 | UT-17 | R で残り時間・終了の理由も初期化 |
| FR-023 | A2 | UT-18 | CPU同士でも制限時間で終了する |
| FR-017 | 全部 | PlayMode実証 | 状態取得（contracts/state-contract.md） |
| FR-018 | US1-1 | 画面確認 | 色と文字で12種が区別できる |
