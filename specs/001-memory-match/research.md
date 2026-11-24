# Research

NEEDS CLARIFICATION は 0 件。Plan の決定の根拠を残す。

## 1. パッケージ
- Decision: manifest を変更せず、既存の依存（inputsystem、test-framework、ugui）を使う。
- Rationale: `Packages/packages-lock.json` に3つとも入っている（ブリッジの依存）。作業条件が「パッケージ追加なし」。
- Alternatives: 旧 Input Manager（却下：unity-cli の入力が Input System 経由）。

## 2. 乱数
- Decision: 自前の決定的乱数（SplitMix系）。
- Rationale: 同シードで配置とCPUの選択が環境によらず一致する必要がある（FR-015, SC-002）。
- Alternatives: System.Random（却下：実装依存の可能性）、UnityEngine.Random（却下：Coreをテストできない）。

## 3. 時間の扱い
- Decision: Coreに `Tick(deltaSeconds)` を渡す。
- Rationale: 1.0秒・0.8秒を待たずにEditModeでテストできる。ViewはUpdateで `Time.deltaTime` を渡すだけ。
- Alternatives: コルーチンでタイマー（却下：ロジックがUnity依存になる）。

## 4. CPUの記憶
- Decision: カードが表になった時点で、強度の確率で記憶に入れるか1回だけ判定する。
- Rationale: 「見たカードを覚えている確率」を、観測できる比率（SC-004）として測れる。強は100%なので、常に完全記憶。
- Alternatives: 手番ごとに思い出し判定（却下：同じカードの結果が揺れ、テストしにくい）。

## 5. 絵柄の表現
- Decision: uGUI の旧Text に、12色×文字A〜Lで表示する。組み込みフォントを使う。
- Rationale: 外部アセット不要、フォント欠落の心配がない（ASCIIのみ）。
- Alternatives: TextMeshPro（却下：初回に追加リソースの取り込みが要る）。

## 6. 状態取得
- Decision: `Game` の公開フィールドを `get_component_values` で読む。裏向きカードの絵柄は出さない。
- Rationale: スライド11・15。覗きによる不正確認を防ぐ。
