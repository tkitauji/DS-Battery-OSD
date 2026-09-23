# 機器の動作確認状況

[動作を報告する / Report a device](https://github.com/tkitauji/DS-Battery-OSD/issues/new?template=device-report.yml)

成功・失敗どちらの報告も歓迎します。GitHubアカウントが必要で、投稿内容は公開されます。
機種名、PCとの接続方法、アプリの入手元・バージョン、Windowsのバージョン、表示結果を教えてください。
MACアドレス、シリアル番号、機器ID、個人名を含む名前、未加工のログは投稿しないでください。画像も個人情報を隠してください。

## 実機で確認できたこと

以下は開発者の手元の機器・利用者確認に基づく限定的な記録です。
同型機の全個体、全ファームウェア、全Windows環境での動作や残量精度を保証しません。
一般周辺機器への対応は開発版の機能で、公開済みStore版と同じではありません。

| 機器 | 接続 | 確認できた範囲 | 未確認・制約 |
| --- | --- | --- | --- |
| Sony DualSense | Bluetooth / USB | 残量・充電状態の表示を実機確認 | 全個体・全環境での精度は未検証。FULLと段階的残量は別の情報 |
| Anker SoundCore 2（利用者申告名） | Bluetooth | 開発中の一覧表示とアイコンを利用者が確認 | 取得経路の特定、更新精度、再接続は未検証 |
| Sony WF-1000XM5 | Bluetooth | 開発中の一覧表示を利用者が確認 | 表示までの遅延報告あり。BLE直接取得での成功とは未確認 |
| STK-7039RG（利用者申告型番） | 未記録 | 開発時のWindows標準API診断で残量値を取得、一覧行を確認 | Bluetoothとしての確認記録には数えない。報告容量は実容量を保証しない |

確認時の正確なビルド番号が残っていない項目は、今後のバージョン別再確認で補います。
BLITZ 2は取得成功を確認できていません。接続条件も十分に切り分けていないため「非対応確定」とは扱いません。

## 実装済みの取得方式と未確認の範囲

これは機種の対応保証リストではありません。

| 取得方式 | 対象になる条件 | 実機検証の状況 |
| --- | --- | --- |
| Windowsの残量プロパティ | 接続中の機器についてWindowsが残量値を提供する | 機種ごとに確認が必要 |
| Windows.Gaming.Input | コントローラーが有効な電池情報を返す | STK-7039RGで取得例あり |
| BLE標準Battery Service | ペアリング・接続済みで標準サービスと単一の残量値を読み取れる | 合成テスト済み。直接読み取りの実機成功は未確認 |
| DualSense専用HID | 対象コントローラーが残量・状態を通知する | 上表のUSB / Bluetoothで確認 |

Bluetooth接続できることと、バッテリー情報を取得できることは別です。
残量を機器・Windowsが公開しない場合、共通方式では取得できません。取得できない値は推測しません。
イヤホンの左右・ケースなど、複数バッテリーの個別表示は保証しません。

## 報告の扱い

- 新しい報告はまず「利用者報告」として扱い、開発者の再現確認と区別します。
- 記録するのは機種・接続・アプリ版・Windows版・結果・公開Issueへの参照です。
- 一覧に出る、残量が更新される、再接続できる、充電状態が正しい、は別々に確認します。
- 自動送信やテレメトリーは追加しません。投稿は利用者自身の操作によります。

## English

Working and non-working reports are welcome through the link above. A GitHub account is required; reports are public.
Include the model, connection, app source/version, Windows version and result. Never post device identifiers, secrets, personal names or raw logs.
The table records limited observations, not universal compatibility. General peripheral support is in the development build, not necessarily the released Store version.
BLE standard Battery Service support has synthetic tests but no confirmed real-device direct-read success yet.
