# StoreSCP Tool

Windows 平台下的 DICOM 现场联调工具，用于在笔记本上替代目标设备完成 `StoreSCP` 接收、网络连通性验证和基础 DICOM 通信调试。

## 核心用途

这个工具主要用于医院现场联调，当前支持：

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

## 现场同事如何获取可运行版本

如果目标是“下载后直接运行”，**不要克隆源码仓库后找 exe**。

正确方式：

1. 打开仓库的 [Releases](https://github.com/kuangliu774392519/storeSCP_tool/releases) 页面
2. 下载绿色版压缩包
3. 解压整个目录
4. 运行 `StorescpTool.App.exe`

推荐下载的附件命名：

```text
StorescpTool_win-x64_portable_<version>.zip
```

不推荐的方式：

- `git clone` 后直接运行
- GitHub 页面 `Code -> Download ZIP` 后直接运行

原因：

- 源码仓库默认不包含发布产物 `dist\win-x64`
- 仓库里的源码主要用于开发，不是面向现场直接运行的交付包

## 当前默认参数

- `Local AE Title = RC120`
- `Listen Port = 5678`
- `Echo Calling AE = RC120`
- `Echo Called AE = CT_AE`
- `Listen IP` 首次启动时会自动选择本机优先 IPv4

注意：

- 运行目录里的 `config\appsettings.json` 会覆盖代码默认值

## 本地开发

### 运行

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' run --project '.\src\StorescpTool.App\StorescpTool.App.csproj'
```

### 测试

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' test '.\StorescpTool.sln' --no-restore
```

### 标准发布

```powershell
.\scripts\Publish-StorescpTool.ps1 -Configuration Release -Runtime win-x64
```

### 打绿色版压缩包

```powershell
.\scripts\Publish-StorescpTool.ps1 -Configuration Release -Runtime win-x64 -CreatePortableZip -PackageVersion 0.1.0
```

## GitHub 自动发布

仓库已新增 GitHub Actions 工作流：

```text
.github/workflows/portable-release.yml
```

行为如下：

- 推送 tag `v*` 时，自动：
  - restore
  - test
  - publish
  - 打绿色版 zip
  - 创建或更新 GitHub Release 附件
- 手动触发 workflow 时，自动生成构建 artifact

推荐发版方式：

```powershell
git tag v0.1.0
git push origin v0.1.0
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

- `src/StorescpTool.App/App.xaml.cs`
- `src/StorescpTool.App/ViewModels/MainViewModel.cs`
- `src/StorescpTool.Infrastructure/Bootstrapper/ServiceRegistration.cs`
- `src/StorescpTool.Infrastructure/Dicom/StoreScpService.cs`
- `src/StorescpTool.Infrastructure/Dicom/DicomStoreScp.cs`

## 文档导航

优先阅读：

- [文档导航](./00_文档导航.md)
- [AI 交接总览](./01_AI交接总览.md)
- [开发环境与常用命令](./02_开发环境与常用命令.md)
- [代码结构与关键入口](./03_代码结构与关键入口.md)
- [当前开发状态与待办清单](./04_当前开发状态与待办清单.md)
- [验证记录与已知问题](./05_验证记录与已知问题.md)
- [绿色版交付说明](./07_绿色版交付说明.md)
- [GitHub Releases 发布流程说明](./08_GitHub_Releases发布流程说明.md)

## 当前状态

当前项目处于：

`可运行 MVP 已完成，继续做现场可用性和交付完善`

当前优先任务：

1. 真实 CT 或 DICOM 模拟器联调验证
2. 接收目录/日志目录选择器
3. 日志筛选与导出体验优化
4. 接收记录详情查看
5. 首次 GitHub Release 工作流实跑验证

## 仓库信息

- 远端仓库：`git@github.com:kuangliu774392519/storeSCP_tool.git`
- 默认分支：`main`

## 说明

本工具定位为现场联调辅助工具，当前版本适合开发测试和工程联调，不定位为诊断软件。
