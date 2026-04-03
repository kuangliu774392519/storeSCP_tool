# StoreSCP Tool

Windows 平台下的 DICOM 现场联调工具，用于在笔记本上替代目标设备完成 `StoreSCP`、网络连通性和基础 DICOM 通信验证。

## 项目目标

这个工具主要面向医院现场调试场景，核心用途是：

- 作为 `StoreSCP` 接收 CT 推送的 DICOM 图像
- 快速验证网络连通性和 DICOM 连通性
- 记录日志、接收记录和接收会话
- 导出诊断包，便于问题反馈和追溯

## 当前能力

当前版本已经具备以下可运行能力：

- `StoreSCP` 启动与停止
- `Ping`
- TCP 端口检测
- `DICOM C-ECHO`
- 本机 `C-STORE` 自测
- 实时日志显示与本地日志落盘
- 接收记录展示与持久化
- 接收会话统计与持久化
- 诊断包导出
- 监听 IP 自动选择、下拉选择和刷新

## 当前默认参数

- `Local AE Title = RC120`
- `Listen Port = 5678`
- `Echo Calling AE = RC120`
- `Echo Called AE = CT_AE`
- `Listen IP` 首次启动时会自动选择本机优先 IPv4

注意：

- 运行目录中的 `config/appsettings.json` 会覆盖代码默认值

## 快速开始

### 方式一：直接运行发布版

标准发布目录：

```text
dist\win-x64
```

主程序：

```text
dist\win-x64\StorescpTool.App.exe
```

建议直接携带整个 `dist\win-x64` 目录，而不是只拿单个 exe。

### 方式二：源码运行

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' run --project '.\src\StorescpTool.App\StorescpTool.App.csproj'
```

### 运行测试

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' test '.\StorescpTool.sln' --no-restore
```

### 发布

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' publish '.\src\StorescpTool.App\StorescpTool.App.csproj' -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:IncludeNativeLibrariesForSelfExtract=true -o '.\dist\win-x64'
```

或使用脚本：

```powershell
.\scripts\Publish-StorescpTool.ps1
```

## 项目结构

```text
src/
  StorescpTool.App
  StorescpTool.Application
  StorescpTool.Core
  StorescpTool.Infrastructure

tests/
  StorescpTool.Tests

scripts/
  Publish-StorescpTool.ps1
```

## 关键入口

- `src/StorescpTool.App/App.xaml.cs`：程序启动入口
- `src/StorescpTool.App/ViewModels/MainViewModel.cs`：主界面核心逻辑
- `src/StorescpTool.Infrastructure/Bootstrapper/ServiceRegistration.cs`：依赖注入注册
- `src/StorescpTool.Infrastructure/Dicom/StoreScpService.cs`：StoreSCP 生命周期管理
- `src/StorescpTool.Infrastructure/Dicom/DicomStoreScp.cs`：DICOM 接收处理

## 文档导航

建议优先阅读：

- [项目交接总览](./01_AI交接总览.md)
- [开发环境与常用命令](./02_开发环境与常用命令.md)
- [代码结构与关键入口](./03_代码结构与关键入口.md)
- [当前开发状态与待办清单](./04_当前开发状态与待办清单.md)
- [验证记录与已知问题](./05_验证记录与已知问题.md)
- [绿色版交付说明](./07_绿色版交付说明.md)

完整导航见：

- [文档导航](./00_文档导航.md)

## 当前状态

当前项目处于：

`可运行 MVP 已完成，继续做现场可用性和交付完善`

优先待办：

1. 真实 CT 或 DICOM 模拟器联调验证
2. 接收目录/日志目录选择器
3. 日志筛选与导出体验优化
4. 接收记录详情查看
5. 交付说明完善

## 版本控制

当前仓库已经初始化 Git，并已推送到远端：

```text
git@github.com:kuangliu774392519/storeSCP_tool.git
```

当前默认分支：

```text
main
```

## 说明

本工具定位为现场联调辅助工具，当前版本适合开发测试和工程联调，不定位为诊断软件。
