# DICOM StoreSCP 现场联调工具使用说明

## 1. 工具用途

本工具用于在 Windows 笔记本上替代目标设备完成 DICOM 联调测试，主要支持：

- StoreSCP 接收
- Ping
- TCP 端口检测
- DICOM C-ECHO
- 本机 C-STORE 自测
- 诊断包导出

## 2. 启动方式

开发环境下可通过以下命令启动：

```powershell
dotnet run --project .\src\StorescpTool.App\StorescpTool.App.csproj
```

发布时可通过以下脚本生成可交付目录：

```powershell
.\scripts\Publish-StorescpTool.ps1
```

发布输出目录默认为：

```text
dist\win-x64
```

## 3. 基本使用流程

1. 启动工具
2. 设置本地 AE Title、监听 IP、监听端口、接收目录、日志目录
3. 点击“启动监听”
4. 先执行 Ping、TCP 检测、C-ECHO 验证网络和 DICOM 通信
5. 需要时可执行“本机 C-STORE 自测”验证本工具自身接收链路
6. 在 CT 上配置目标 AE 并发起图像推送
7. 在“会话统计”“接收记录”“实时日志”中查看结果
8. 如需反馈问题，可导出诊断包

## 4. 目录说明

运行过程中主要数据默认位于应用目录下：

```text
data\
  logs\
  received\
  state\
  export\
config\
```

## 5. 当前版本说明

当前版本已具备 MVP 级现场联调能力，但仍建议在真实 CT 或 DICOM 模拟器环境下做进一步联调验证。
