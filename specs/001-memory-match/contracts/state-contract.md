# 状態取得の契約（FR-017）

```text
unity-cli --output json raw get_component_values --json '{"gameObjectName":"Game","componentType":"MemoryGame"}'
```

内部の番号 0/1 は、画面表示の P1/P2 に対応する（0=P1、1=P2）。`Pairs0`=P1、`Pairs1`=P2。`CardStates` の `0`/`1` も同じ対応。

## Game（MemoryGame）の公開フィールド
| 名前 | 型 | 意味 |
|---|---|---|
| Phase | string | Setup / Playing / Resolving / Finished |
| CurrentPlayer | int | 0 または 1（表示は P1/P2） |
| PlayerKind0, PlayerKind1 | string | Human / CpuWeak / CpuNormal / CpuStrong |
| Pairs0, Pairs1 | int | 取得ペア数 |
| RemainingPairs | int | 残りペア数（初期12） |
| Winner | string | None / P1 / P2 / Draw |
| CursorIndex | int | 0〜23 |
| UseSeed, Seed | bool, int | シード指定 |
| TimeLimitSeconds | float | 制限時間（秒、既定180）。Play前に設定する |
| RemainingTime | float | 残り時間（秒） |
| EndReason | string | None / AllPairs / TimeUp |
| CardStates | string | 24文字。H=裏、R=表、0/1=取得したプレイヤー |
| FaceSymbols | string | 24文字。表または取得済みのみ A〜L、裏は `?`（絵柄を漏らさない） |

## カード（CardView、GameObject名 Card_00〜Card_23）
| 名前 | 意味 |
|---|---|
| State | Hidden / Revealed / Captured |
| Owner | 取得者または None |

## 入力（Play中）
```text
unity-cli --output json raw input_keyboard --json '{"key":"right","action":"press"}'
```
キー：`left` `right` `up` `down`（カーソル）、`space`（めくる）、`r`（再開）。設定画面は `up` `down` `left` `right` と `enter`。
