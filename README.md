# KULMS.Local
KULMSの授業資料をまとめてダウンロードするためのアプリです．

## ビルド・インストール手順
このリポジトリのソースコードからアプリケーションを自前でビルド・実行する手順です。

### 1. 前提条件

**.NET SDK** (10.0 推奨)
  - インストール確認:
    ```bash
    dotnet --version
    ```

### 2. リポジトリのクローン
```bash
git clone https://github.com/HaruhiroObora/KULMS.Local.git
cd KULMS.Local
```

### 3. 単一ファイルでの発行・ビルド
実行用のバイナリを生成するコマンドです。

Linux (x64)
```bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o ./dist/linux-x64
```

Windows (x64)
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./dist/win-x64
```

macOS (arm64)
```bash
dotnet publish src/YourApp.Desktop -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -o ./dist/osx-arm64
```
