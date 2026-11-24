# Data Model

## Phase（進行状態）
`Setup` → `Playing` ⇄ `Resolving`（外れ表示中、1.0秒）→ `Playing` … → `Finished`（全ペア取得、または時間切れ）。R で `Playing`（初期状態）へ。

## Card
| 項目 | 内容 |
|---|---|
| Index | 0〜23（Card_00〜Card_23、6列×4行、行優先） |
| Symbol | 0〜11（各2枚） |
| State | Hidden / Revealed / Captured |
| Owner | 取得したプレイヤー（0 / 1 / なし） |

## Player
| 項目 | 内容 |
|---|---|
| Kind | Human / CpuWeak / CpuNormal / CpuStrong |
| Pairs | 取得ペア数 |
| 記憶確率 | 弱0.25 / 普通0.60 / 強1.00 |

## Match
| 項目 | 内容 |
|---|---|
| Phase | 上記 |
| CurrentPlayer | 0 / 1 |
| RemainingPairs | 12 から減る |
| Picks | 今の手番でめくった枚数（0〜2）と、そのCard |
| Winner | なし / プレイヤー0 / プレイヤー1 / 引き分け |
| Seed | 指定時のみ。配置とCPU選択を決める |
| ResolveTimer | 外れ表示の残り（1.0秒） |
| RemainingTime | 残り時間（秒）。既定180。Playing と Resolving の間だけ減る（FR-019） |
| EndReason | None / AllPairs / TimeUp（FR-020, 022） |
| CpuTimer | 次のCPU操作までの残り（0.8秒） |

## CpuMemory
覚えているカードの Index → Symbol の対応。カードが表になるたびに、強度の確率で1回判定して追加する。取得済みのカードは消す。

## 状態遷移（規則）
1. 裏向きカードを選ぶ → Revealed、Picks +1。
2. Picks = 2 で比較。同じ絵柄 → 2枚を Captured、Pairs +1、残り -1、手番は同じ。違う → Phase = Resolving。
3. Resolving が1.0秒経過 → 2枚を Hidden に戻し、手番交代、Phase = Playing。
4. 残り = 0 → Finished、Winner を決める。
5. Resolving 中と Finished 中は、めくり入力を受け付けない（R は別）。
6. Playing / Resolving の `Tick` で RemainingTime が減る。0になったら、表向き（未取得）のカードを Hidden に戻し、手番は変えず Finished、EndReason=TimeUp、Winner を取得数で決める。
7. 全ペア取得は EndReason=AllPairs。以後 RemainingTime は減らない。
