# Quickstart（検証手順）

## 前提
- Unity 6000.6.4f1 で `MemoryMatch/` を開く。`unity-cli --output json system ping` が `versionCheck: match`。

## 1. EditModeテスト
`unity-cli` のテスト実行で EditMode を走らせ、UT-01〜UT-12 の名前・件数・合否を確認する。0件・全スキップは未確認。

## 2. 画面確認（Edit Mode）
`MemoryMatch.unity` を開き、Hierarchy に Game、Card_00〜Card_23、HUD、設定画面があることを確認する。Game View で24枚が6×4で見える。

## 3. AIプレイ（Play）
1. `Game` の `UseSeed=true`、`Seed=<固定値>`、P1=Human、P2=Human で開始する。
2. `input_keyboard` でカーソル移動とSpaceを送り、前後で [state-contract](contracts/state-contract.md) の状態を取得する。
3. US1〜US3 の受け入れシナリオを順に実証し、`verification.md` に 操作・期待・実際・根拠・判定 を書く。
4. P2=CPU強で、人間の入力なしにCPUの手番が進むことを確認する。
5. 終了時：入力を解除し、Stopし、シーンとプロジェクトを保存する。
