# DS Battery OSD

DualSense / DualSense Edgeの電池残量を、Windowsデスクトップ右上に表示する最小オーバーレイです。

![FINAL FANTASY XIVでの表示例](docs/images/ds-battery-osd-ffxiv.png)

## MVPの動作

- USB / Bluetooth接続を自動検出
- 10%刻みのバッテリー残量と充電状態を表示
- 切断時は自動的に非表示
- 常に最前面、枠なし、背景なし
- 表示領域全体をマウスドラッグで移動
- 20%未満は赤色で表示
- 丸みのあるNunito Boldをアプリへ同梱
- DualSense以外のゲームやプロセスにはアクセスしない

## ビルド

.NET 8 SDKを導入後、次を実行します。

```powershell
dotnet build .\DsBatteryOsd.csproj -c Release
```

起動後、DualSenseを接続すると右上に `🎮 70%` のように表示されます。充電中は `🎮 ⚡ 70%` になります。

## フルスクリーン表示について

ウィンドウモードと仮想（ボーダーレス）フルスクリーンに対応しています。

排他的フルスクリーンでは、Windowsの通常ウィンドウよりゲーム画面が優先されるため、本OSDは表示されません。仮想（ボーダーレス）フルスクリーンで使用してください。NVIDIA Appなどのオーバーレイへの統合や、ゲームへのDLL注入は行いません。

## 現時点の範囲

単一コントローラー向けのMVPです。設定画面、タスクトレイ、自動起動、複数台対応、インストーラーはまだ含みません。

## ライセンス

アプリ本体は[MIT License](LICENSE)です。

同梱するNunitoフォントはSIL Open Font License 1.1です。ライセンス全文は[Assets/OFL-Nunito.txt](Assets/OFL-Nunito.txt)を参照してください。
