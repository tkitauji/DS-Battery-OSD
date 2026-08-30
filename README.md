# DS Battery OSD

DualSense / DualSense Edgeの電池残量を、Windowsデスクトップ右上に表示する最小オーバーレイです。

## MVPの動作

- USB / Bluetooth接続を自動検出
- 10%刻みのバッテリー残量と充電状態を表示
- 切断時は自動的に非表示
- 常に最前面、枠なし、クリック透過
- DualSense以外のゲームやプロセスにはアクセスしない

## ビルド

.NET 8 SDKを導入後、次を実行します。

```powershell
dotnet build .\DsBatteryOsd.csproj -c Release
```

起動後、DualSenseを接続すると右上に `DS 70%` のように表示されます。

## 現時点の範囲

単一コントローラー向けのMVPです。設定画面、タスクトレイ、自動起動、複数台対応、インストーラーはまだ含みません。
