# Tasks: 神経衰弱（2人対戦・CPU対戦）

**Input**: `specs/001-memory-match/` の spec.md、plan.md、data-model.md、contracts/state-contract.md、research.md、quickstart.md

**進め方**: 各 Task は「失敗テスト（Red）→ 最小実装（Green）→ 全テスト再実行」で1つずつ進める。Task ごとに作業条件（working-rules.md）を守る。Core のテストは EditMode、実行は unity-cli。CPUはカーソルを使わず直接カードを指定する（plan.md）。

**表記**: `[P]` は並行可（別ファイルで依存なし）。`FR / AS / UT` は plan.md の対応の鎖。パスは `MemoryMatch/` を省略（`Assets/...` は `MemoryMatch/Assets/...`）。

## Phase 1: Setup

- [x] T001 `Assets/Scripts/Core/MemoryMatch.Core.asmdef`（Unity参照なし：`noEngineReferences: true`）、`Assets/Scripts/View/MemoryMatch.View.asmdef`（Core参照）、`Assets/Tests/EditMode/MemoryMatch.Tests.EditMode.asmdef`（Core参照、EditMode専用）を作り、コンパイルエラー0を確認する
- [x] T002 unity-cli で EditMode テストを実行する手順を確認する（空のスモークテスト1件が「1件・合格」と返る。0件・全スキップは未確認として扱う）。ファイル：`Assets/Tests/EditMode/SmokeTests.cs`

## Phase 2: Foundational（全ストーリーの前提）

- [x] T003 `Assets/Scripts/Core/` に、data-model.md の `Phase`、`Card.State`、`Player.Kind` の列挙型を作る（値は data-model.md の定義どおり。ルールは足さない）。`Match.Winner` の列挙（None/P1/P2/Draw）も同じ場所に置く
- [x] T004 [P] FR-002・FR-015・UT-01(一部)：同じシードから同じ数列が出て、違うシードでは違う数列になる失敗テストと、決定的な乱数 `DeterministicRandom` の最小実装を、`Assets/Tests/EditMode/DeterministicRandomTests.cs` と `Assets/Scripts/Core/DeterministicRandom.cs` に作る
- [x] T005 [P] FR-004・UT-02：6列×4行で、端から外へ動かそうとすると位置が変わらず、内側は上下左右に1つ動く失敗テストと、`CursorGrid` の最小実装を、`Assets/Tests/EditMode/CursorGridTests.cs` と `Assets/Scripts/Core/CursorGrid.cs` に作る

## Phase 3: User Story 1 - 2人でペアを取り合って勝敗を決める (P1)

**Goal**: 人間2人で、めくる → 判定 → 手番 → 終了まで遊べる（Core）。
**Independent Test**: シード固定で決まった順にめくり、取得数・手番・勝敗が AS-1〜5 と一致する。

- [x] T006 [US1] FR-001・FR-002・FR-003・AS US1-1(M1)・UT-01：新しいゲームが「24枚・各絵柄ちょうど2枚・全て裏向き・取得0/0・残り12・手番はP1・Phase=Playing」で、同シードなら同配置になる失敗テストと、`MatchEngine` の生成と配置（data-model.md `Card.Index`、`Card.Symbol`、`Card.State`、`Card.Owner`、`Match.RemainingPairs`、`Match.CurrentPlayer`）の最小実装を作る。`Assets/Tests/EditMode/MatchSetupTests.cs`、`Assets/Scripts/Core/MatchEngine.cs`
- [x] T007 [US1] FR-004・FR-005・AS US1-2(M2)・UT-03：裏向きカードを選ぶと表になり、1手番で2枚まで、2枚が表のあいだ3枚目を選んでも状態が変わらない失敗テストと、`Pick(index)` の最小実装を作る（`Match.Picks`）。`Assets/Tests/EditMode/PickTests.cs`
- [x] T008 [US1] FR-007・FR-008・AS US1-3(M3)・UT-05：同じ絵柄の2枚をめくると取得（Captured、取得者=手番、取得ペア+1、残り-1）になり、表のまま残り、手番が続く失敗テストと、成立判定の最小実装を作る。`Assets/Tests/EditMode/MatchTests.cs`
- [x] T009 [US1] FR-009・AS US1-4(M3)・US3-2・UT-06：違う絵柄の2枚をめくると Phase=Resolving になり、`Tick` で0.99秒ではまだ表、1.0秒で両方 Hidden に戻って手番が相手に移り、Resolving中の `Pick` が無視される失敗テストと、最小実装を作る（`Match.ResolveTimer`）。`Assets/Tests/EditMode/MismatchTests.cs`
- [x] T010 [US1] FR-010・FR-011・AS US1-5(M5)・UT-07：最後のペア取得で Phase=Finished になり、取得数が多い側が Winner、6対6なら Draw になり、以後の `Pick` が無視される失敗テストと、最小実装を作る。`Assets/Tests/EditMode/FinishTests.cs`
- [x] T011 [US1] T006〜T010 の全テストを再実行し、名前・件数・合否を記録する（Refactor が必要ならこの時点で整理。振る舞いは変えない）

**Checkpoint**: US1 の Core は、人間2人の対戦をテストで通せる。

## Phase 4: User Story 2 - CPUと対戦する (P1)

**Goal**: 各プレイヤーを人間／CPU弱・普通・強にでき、CPUの手番が自動で進む（Core）。
**Independent Test**: P2をCPUにし、`Tick` だけで2枚めくりまで進み、確率と再現性が AS-2〜5 と一致する。

- [x] T012 [US2] FR-012・AS US2-1・UT-08：種別が Human / CpuWeak / CpuNormal / CpuStrong から選べ、既定が P1=Human、P2=CpuNormal である失敗テストと、設定の保持（`Player.Kind`、記憶確率 弱0.25／普通0.60／強1.00）の最小実装を作る。`Assets/Tests/EditMode/PlayerSetupTests.cs`
- [x] T013 [US2] FR-012・AS US2-1・UT-08(開始)：設定画面で選んだ種別のまま「開始」すると Phase が Setup から Playing に移り、開始前は `Pick` が無視される失敗テストと、最小実装を作る（data-model.md `Match.Phase`）。`Assets/Tests/EditMode/StartTests.cs`
- [x] T014 [US2] FR-014・UT-10(記憶)：カードが表になるたびに強度の確率で記憶に入り、取得済みは記憶から消え、強は常に記憶する失敗テストと、`CpuMemory`（data-model.md の CpuMemory）の最小実装を作る。`Assets/Tests/EditMode/CpuMemoryTests.cs`
- [x] T015 [US2] FR-014・AS US2-4・UT-10(選択)：覚えている組があれば1枚目にその片方、2枚目は1枚目と同じ絵柄を覚えていればそれを選び、なければ未確認のカードから選ぶ（FR-014の改訂版、AS US2-4・US2-6）失敗テストと、`CpuPlayer` の選択の最小実装を作る。`Assets/Tests/EditMode/CpuChoiceTests.cs`
- [x] T016 [US2] FR-013・AS US2-2・US2-3・UT-09：CPUの手番で、`Tick` が0.8秒進むごとに1枚ずつ計2枚をめくり、CPUの手番中の人間の `Pick` が無視される失敗テストと、`CpuTimer` による自動進行の最小実装を作る（data-model.md `Match.CpuTimer`）。`Assets/Tests/EditMode/CpuTurnTests.cs`
- [x] T017 [US2] SC-004・UT-10(確率)：固定シードで1,000手以上の記憶判定をしたとき、記憶した比率が 弱25%・普通60%・強100% に対して±5ポイント以内になるテストを作り、通ることを確認する。`Assets/Tests/EditMode/CpuMemoryRateTests.cs`
- [x] T018 [US2] FR-015・AS US2-5・SC-002・SC-003・UT-11：同シード・同強度で2回進めるとカード配置とCPUの選択列が一致し、CPU同士の対戦が `Tick` だけで Finished まで進む失敗テストと、必要な最小実装を作る。`Assets/Tests/EditMode/DeterminismTests.cs`
- [x] T019 [US2] T012〜T018 と既存の全テストを再実行し、名前・件数・合否を記録する

**Checkpoint**: US1＋US2 の Core が完成。

## Phase 5: User Story 3 - 誤操作を防ぎ、最初からやり直せる (P2)

**Goal**: 選べないカードの選択を防ぎ、R で完全に初期化する（Core）。
**Independent Test**: AS US3-1〜4 を `Pick` と `Restart` で再現して合格する。

- [x] T020 [US3] FR-006・AS US3-1(M4)・UT-04：取得済みのカード、表のカード、今の手番で選んだカードを `Pick` しても状態が変わらない失敗テストと、最小実装を作る。`Assets/Tests/EditMode/DoublePickTests.cs`
- [x] T021 [US3] FR-016・FR-003・AS US3-3・US3-4(M6)・UT-12：途中・終了後・外れ表示中（Resolving）のいずれからも `Restart` で、配置の作り直し・全カード Hidden・取得0/0・残り12・手番P1・Phase=Playing・CPUの記憶が空になり、シード指定時は同じ配置とCPUの選択列、未指定時は異なりうる、という失敗テストと、`Restart` の最小実装を作る（表示だけ戻す実装にしない）。`Assets/Tests/EditMode/RestartTests.cs`
- [x] T022 [US3] 全テスト（UT-01〜UT-12）を再実行し、plan.md の対応の鎖の各UTが1件以上あり合格していることを、名前・件数・合否で記録する

**Checkpoint**: Core が spec の全 FR（FR-001〜016）をテストで固定した。

## Phase 6: View（Unity層）

**Goal**: Core を画面と入力につなぐ。ロジックは足さない。

- [x] T023 FR-017・contracts/state-contract.md：`Assets/Scripts/View/MemoryGame.cs` を作り、Game オブジェクトの公開フィールド（Phase、CurrentPlayer、PlayerKind0/1、Pairs0/1、RemainingPairs、Winner、CursorIndex、UseSeed、Seed、CardStates、FaceSymbols）を Core の状態から毎フレーム更新する。裏向きカードの絵柄は `?`。コンパイルエラー0を確認する
- [x] T024 FR-018・AS US1-1：`Assets/Scripts/View/CardView.cs` を作り、Card_00〜Card_23 が State、Owner を持ち、裏は伏せた見た目、表は色（12色）＋文字（A〜L）で区別できるようにする（外部アセットなし、組み込みフォント）
- [x] T025 FR-004・FR-016：`MemoryGame` に Input System のキー入力（矢印、Space、R）をつなぎ、Core の `CursorGrid`、`Pick`、`Restart` を呼ぶ。CPUの手番ではカーソルを使わず、Core が選んだカードを直接めくる。`Update` で `Time.deltaTime` を `Tick` に渡す。ルール判断はここに書かない
- [x] T026 FR-010・FR-011・AS US1-1・US1-5：`Assets/Scripts/View/Hud.cs` を作り、現在の手番、各自の取得ペア数、残りペア数、終了時の勝者／引き分けを表示する
- [x] T027 FR-012・AS US2-1：`Assets/Scripts/View/SetupMenu.cs` を作り、上下で行選択、左右で種別切替、Enter／Space で開始（plan.md の決定）。既定は P1=Human、P2=CpuNormal。`UseSeed`、`Seed` は Play 前に CLI で設定した値を使う

## Phase 7: シーン構築（unity-cli）

- [x] T028 Edit Mode で `Assets/Scenes/MemoryMatch.unity` を作成し、unity-cli で Game（MemoryGame）、24枚のカード（Card_00〜Card_23、6列×4行）、カーソル、HUD、設定画面を配置する。保存後に Hierarchy を再取得して、24枚と各オブジェクトがあることを確認する（判定ロジックは触らない）
- [x] T029 Game View で、24枚が6×4で見えることを画像で確認し、コンパイルエラー0と保存を確認する

## Phase 8: PlayMode 実証（verification.md）

**ルール**: 状態の直接書き換えと、裏の絵柄を覗いたプレイをしない。根拠は、取得した状態と Game 画像の2種類。確認結果は「確認済み／未成立／未確認」を分ける。

- [x] T030 [P] [US1] AS US1-1(M1)：Play に入り、初期状態（24枚裏、取得0/0、残り12、手番P1、勝敗表示なし）を取得して `verification.md` に記録する
- [x] T031 [US1] AS US1-2・US1-3・US1-4(M2・M3)：シード固定、P1/P2とも Human で、カーソル移動＋Space を送り、成立（手番継続）と外れ（1.0秒表示後に裏へ、手番交代）の前後の状態を取得して記録する。3枚目がめくれないことも確認する
- [x] T032 [US3] AS US3-1・US3-2(M4)：取得済み・表向きのカードと外れ表示中に Space を送り、状態が変わらないことを再取得で確認して記録する
- [x] T033 [US1] AS US1-5(M5)：最後までプレイして Finished、勝者／引き分けの表示と、以後の入力無効を確認して記録する（6対6の引き分けは Core テストで確認済みの旨を併記）
- [x] T034 [US2] AS US2-2・US2-3・US2-5・SC-003：P2=CPU強（さらにP1=CPU強）で、人間の入力なしに0.8秒間隔で進み最後まで終わること、同シードで2回の結果が一致することを記録する
- [x] T035 [US3] AS US3-3・US3-4(M6)：途中・終了後・外れ表示中に R を送り、全項目（配置・表裏・取得・残り・手番）が初期化されたことを前後の状態で確認して記録する
- [x] T036 `verification.md` に、条件ごとに 操作・期待・実際・根拠（状態＋Game画像）・判定（確認済み／未成立／未確認）を数行で書き、画像を読んだ確認方法も記す。SC-007：Game 画像で現在の手番と各自の取得数が読めることを、人の目で確認して記す

## Phase 9: Polish

- [x] T037 `/speckit-analyze` を実行し、FR・AS・UT・Task の対応に漏れや矛盾がないか確認する（実装前に実施するのが理想。実装後にも再確認）
- [x] T038 終了処理：入力を解除し、Stop し、シーンとプロジェクトを保存する。`.gitignore`（Library、Temp、Logs、UserSettings、個人設定）を整える

## Phase 10: User Story 4 - 制限時間つきで遊ぶ (P2)（後半の追加機能）

**Goal**: ゲーム全体の制限時間（既定180秒）と時間切れ終了。影響する既存条件は M1・M5・M6。
**Independent Test**: AS US4-1〜5（A1〜A5）を Core のテストと PlayMode の状態取得で確認する。

- [x] T039 [US4] FR-019・AS US4-1(A1)・UT-13：新しいゲームの `RemainingTime` が制限時間（既定180、引数で変更可）で、Playing と Resolving のあいだ `Tick` で減り、Setup 中と Finished 後は減らない失敗テストと、最小実装を作る（data-model.md `Match.RemainingTime`）。`Assets/Tests/EditMode/TimeLimitTests.cs`、`Assets/Scripts/Core/MatchEngine.cs`
- [x] T040 [US4] FR-020・FR-011・AS US4-2(A2)・UT-14：残り時間が0になると Finished、EndReason=TimeUp、取得数が多い側が Winner（同数は Draw）になり、以後の `Pick` が無視される失敗テストと、最小実装を作る（data-model.md `Match.EndReason`、`Match.Winner`）。`Assets/Tests/EditMode/TimeUpTests.cs`
- [x] T041 [US4] FR-021・AS US4-5(A5)・UT-15：時間切れの瞬間に、1枚だけ表、または外れ2枚の表示中でも、未取得の表向きカードがすべて Hidden に戻り、手番が変わらず、外れ表示中も時間が減る失敗テストと、最小実装を作る。`Assets/Tests/EditMode/TimeUpTests.cs`
- [x] T042 [US4] FR-022・AS US4-3(A3)・UT-16：全ペア取得で EndReason=AllPairs になり時計が止まる。時間切れ寸前の最後のペア取得は、ペア取得が優先される失敗テストと、最小実装を作る。`Assets/Tests/EditMode/TimeUpTests.cs`
- [x] T043 [US4] FR-016・FR-003・AS US4-4(A4)・UT-17：途中、時間切れ後、全ペア取得後のどこから `Restart` しても、RemainingTime が制限時間に戻り、EndReason=None になる失敗テストと、最小実装を作る。`Assets/Tests/EditMode/RestartTests.cs`
- [x] T044 [US4] FR-023・UT-18：CPU同士（強）で制限時間を短く（例：10秒）すると、入力なしで時間切れ終了し、取得数で勝敗が決まる失敗テストと、必要な最小実装を作る。`Assets/Tests/EditMode/TimeLimitTests.cs`
- [x] T045 [US4] 全テスト（UT-01〜UT-18、SC-010 の回帰）を再実行し、名前・件数・合否を記録する
- [x] T046 [US4] FR-010・FR-017・contracts/state-contract.md：`MemoryGame` に `TimeLimitSeconds`、`RemainingTime`、`EndReason` を足し、Hud に `Time m:ss`（切り上げ）と時間切れ表示（TIME UP＋勝者）を足す。ロジックは足さない。コンパイルエラー0を確認する。`Assets/Scripts/View/MemoryGame.cs`、`Assets/Scripts/View/Hud.cs`
- [x] T047 [US4] PlayMode：A1〜A5 を実証する（確認を速くするため `TimeLimitSeconds` を短くして開始してよい）。状態と Game 画像を `verification.md` に記録する
- [x] T048 [US4] 回帰確認（SC-010）：M1〜M6 のシナリオ（A と B）を制限時間つきで再実行し、すべて成立することを `verification.md` に記録する。M1 は残り時間の初期値、M5 は終了の理由、M6 は残り時間の初期化を確認する
- [x] T049 [US4] 終了処理：入力を解除し、Stop し、シーンとプロジェクトを保存する

## Dependencies & Execution Order

- Phase 1 → 2 → （3, 4, 5 は Core。US1 を先に。US2 は T006〜T010 の `MatchEngine` に依存。US3 も同様）→ 6 → 7 → 8 → 9
- T004 と T005 は並行可。T030 は T031 以降と独立に記録できる。
- US3 の T020 は US1 の `Pick` に依存する。T021 は US1 と US2 の `Restart` 対象状態（乱数、CPU記憶）に依存する。

## Implementation Strategy

- **MVP**: Phase 1〜3（人間2人の対戦が Core で成立）＋ Phase 6〜8 の US1 分。ここで1回、実物で確認する。
- 次に US2（CPU）、US3（誤操作防止と再開）を足し、T037 の整合確認に戻る。
- 後半の1機能追加（制限時間など）は、承認済み spec.md への追記後に、別の Task として追加する。
