# DICOM StoreSCP 现场联调工具软件架构设计说明书

## 1. 文档目的

本文档用于定义 DICOM StoreSCP 现场联调工具的软件架构、模块划分、关键类职责、核心流程和工程组织方式，作为后续开发、测试和维护的技术依据。

## 2. 设计目标

软件架构设计应满足以下目标：

- 满足 Windows 桌面工具化使用场景
- 支持 StoreSCP、C-ECHO、Ping、端口检测等核心能力
- 支持日志追溯、配置持久化和后续功能扩展
- 保持模块边界清晰，便于开发和维护
- 具备较好的稳定性、可测试性和可扩展性

## 3. 总体架构

本系统采用分层架构设计，分为以下层次：

- 表现层
- 应用层
- 核心服务层
- 基础设施层

## 4. 分层说明

### 4.1 表现层

表现层负责与用户交互，主要由 WPF 界面组成，完成以下职责：

- 接收用户输入
- 展示系统运行状态
- 展示接收记录、网络测试结果和日志
- 调用应用层服务完成业务操作

主要页面包括：

- 首页/监听页
- 网络测试页
- 接收记录页
- 日志页
- 设置页

### 4.2 应用层

应用层负责组织业务流程，是界面与底层服务之间的协调层，主要职责包括：

- 启动和停止 StoreSCP
- 发起 C-ECHO、Ping、端口测试
- 统一处理参数校验
- 组织日志写入
- 组织配置保存与加载
- 汇总展示所需数据模型

### 4.3 核心服务层

核心服务层负责实现项目核心能力，主要包括：

- DICOM 接收服务
- DICOM 网络测试服务
- 基础网络诊断服务
- 日志服务
- 配置服务
- 文件存储服务
- 会话记录服务

### 4.4 基础设施层

基础设施层负责与外部库和操作系统能力交互，主要包括：

- fo-dicom 封装
- Serilog 封装
- SQLite 或本地文件存储
- Windows 文件系统访问
- 系统网络接口访问

## 5. 模块划分

### 5.1 UI 模块

职责：

- 提供图形界面
- 展示运行状态和接收数据
- 承载参数输入和操作按钮

建议内容：

- Views
- ViewModels
- Converters
- UI Models

### 5.2 DICOM 接收模块

职责：

- 建立并维护 StoreSCP 监听
- 接收 DICOM Association
- 处理 C-STORE 请求
- 完成文件落盘
- 输出接收结果与日志

建议子模块：

- StoreScpHost
- DicomAssociationHandler
- DicomStorageHandler
- ReceiveSessionManager

### 5.3 DICOM 网络测试模块

职责：

- 实现 C-ECHO SCU
- 检查与远端 DICOM 节点的通信能力
- 输出测试结果

建议子模块：

- DicomEchoClient
- EchoTestService

### 5.4 基础网络诊断模块

职责：

- 执行 Ping
- 检测端口连通性
- 获取本机网络信息

建议子模块：

- PingService
- PortCheckService
- LocalNetworkInfoService

### 5.5 日志管理模块

职责：

- 提供统一日志入口
- 记录操作日志、协议日志、错误日志
- 支持日志文件输出和界面实时展示

建议子模块：

- LogService
- UiLogBuffer
- LogFileProvider

### 5.6 配置管理模块

职责：

- 保存和加载本地配置
- 提供默认配置
- 提供配置合法性校验

建议子模块：

- AppConfigService
- ConfigValidator

### 5.7 文件存储模块

职责：

- 生成接收目录结构
- 保存 DICOM 文件
- 返回文件保存结果

建议子模块：

- FileStorageService
- PathStrategyProvider

### 5.8 会话记录模块

职责：

- 管理每次接收会话的信息
- 汇总会话统计数据
- 为界面提供历史展示数据

建议子模块：

- ReceiveSessionRepository
- ReceiveSessionService

### 5.9 报告导出模块

职责：

- 导出日志
- 导出测试结果摘要
- 生成联调报告

该模块可在增强阶段实现。

## 6. 核心类设计建议

### 6.1 AppConfig

职责：

- 定义应用配置结构

典型字段：

- LocalAeTitle
- ListenPort
- ReceiveDirectory
- LogDirectory
- EnableDetailedDicomLog
- ValidateCalledAe

### 6.2 StoreScpService

职责：

- 对外提供启动和停止监听接口
- 管理 DICOM SCP 生命周期

主要方法建议：

- StartAsync()
- StopAsync()
- GetStatus()

### 6.3 DicomReceiveContext

职责：

- 表示一次接收过程中的上下文信息

典型字段：

- SessionId
- CallingAe
- CalledAe
- RemoteIp
- StartTime
- ReceivedCount

### 6.4 DicomFileSaveResult

职责：

- 表示单个文件保存结果

典型字段：

- Success
- FilePath
- SopInstanceUid
- ErrorMessage

### 6.5 NetworkTestResult

职责：

- 表示 Ping、端口检测、C-ECHO 的统一结果

典型字段：

- Success
- TestType
- Target
- DurationMs
- Message

### 6.6 ReceiveSessionSummary

职责：

- 表示一次接收会话的摘要信息

典型字段：

- SessionId
- StartTime
- EndTime
- RemoteIp
- CallingAe
- TotalFiles
- Status

## 7. 数据流设计

### 7.1 监听启动数据流

1. 用户在界面输入 AE Title、端口、目录
2. ViewModel 将参数传给应用层
3. 应用层调用配置校验器
4. 校验通过后调用 StoreScpService 启动监听
5. 日志服务写入启动日志
6. 运行状态回传界面显示

### 7.2 图像接收数据流

1. CT 发起 Association
2. DICOM 接收模块接收连接
3. 创建接收上下文
4. 接收实例并调用文件存储模块落盘
5. 会话记录模块更新统计
6. 日志服务写入协议日志和接收结果
7. 接收摘要回传界面

### 7.3 网络测试数据流

1. 用户发起 Ping、端口检测或 C-ECHO
2. 应用层调用对应服务
3. 服务执行测试并返回结果
4. 日志服务记录测试结果
5. 测试结果显示在界面中

## 8. 工程目录建议

建议代码工程结构如下：

```text
StorescpTool/
  src/
    StorescpTool.App/
      Views/
      ViewModels/
      Models/
      Converters/
      App.xaml
      MainWindow.xaml
    StorescpTool.Application/
      Services/
      Contracts/
      Dtos/
    StorescpTool.Core/
      Entities/
      Interfaces/
      Enums/
    StorescpTool.Infrastructure/
      Dicom/
      Logging/
      Storage/
      Config/
      Network/
      Persistence/
  tests/
    StorescpTool.Tests/
  docs/
```

对于首版项目，也可先简化为单解决方案多项目结构，后续逐步演进。

## 9. 日志架构设计

日志采用统一入口、分级输出的方式。

### 9.1 日志级别

- Debug
- Info
- Warning
- Error

### 9.2 日志分类

- 应用运行日志
- DICOM 协议日志
- 网络测试日志
- 错误异常日志

### 9.3 关键字段建议

- Timestamp
- Level
- Module
- SessionId
- CallingAe
- CalledAe
- RemoteIp
- Port
- SopClassUid
- SopInstanceUid
- Message
- Exception

## 10. 配置设计

配置建议采用 JSON 文件保存。

建议配置文件内容包括：

- 本地 AE Title
- 监听端口
- 接收目录
- 日志目录
- 是否开启详细协议日志
- 默认 C-ECHO 参数
- 是否校验 Called AE

建议路径：

```text
config/appsettings.json
```

## 11. 存储设计

### 11.1 文件存储

接收文件按日期和检查组织目录，例如：

```text
received/
  2026-04-03/
    STUDY_1.2.840.xxxxx/
      IMG_0001.dcm
```

### 11.2 历史记录存储

MVP 阶段：

- 使用内存或本地简单文件存储

增强阶段：

- 使用 SQLite 存储接收会话和测试记录

## 12. 并发与线程设计

需要注意以下原则：

- 网络监听与界面线程分离
- 文件接收与界面更新异步处理
- 日志写入不阻塞 UI
- 状态更新通过线程安全方式回传 ViewModel

## 13. 异常处理设计

系统应统一处理以下异常：

- 配置异常
- 网络异常
- DICOM 通信异常
- 文件系统异常
- 未预期运行时异常

建议策略：

- 业务异常进行友好提示
- 技术异常记录详细日志
- 关键操作统一 try/catch 包装
- 启动、监听、接收、测试流程分别设置错误边界

## 14. 扩展性设计

为了支持后续扩展，架构上应预留以下能力：

- 增加 StoreSCU 测试能力
- 增加 DICOM 标签查看
- 增加图像预览
- 增加测试报告导出
- 增加数据脱敏能力

设计上应尽量做到：

- UI 与服务解耦
- 服务接口抽象化
- 配置模型可扩展
- 日志字段结构化

## 15. 测试建议

建议测试覆盖以下内容：

- 配置加载与校验测试
- Ping、端口检测、C-ECHO 功能测试
- StoreSCP 启动与停止测试
- 文件保存测试
- 异常路径测试
- 日志输出测试

## 16. 结论

本架构方案采用 WPF + 应用服务 + 核心能力服务 + 基础设施封装的分层设计，能够较好支撑 DICOM StoreSCP 现场联调工具的开发目标。该架构兼顾快速落地与后续扩展，适合作为本项目的正式开发架构基线。
