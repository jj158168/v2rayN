# v2rayN Multi-Port Edition - 项目技能说明

## 项目概述

这是 v2rayN 的一个 fork 版本，主要添加了**多节点同时运行**功能，允许用户同时启动多个代理节点，每个节点使用独立的本地端口。

## 核心功能需求

### 1. 多节点同时运行
- 支持同时启动多个代理节点
- 每个节点运行独立的 Core 进程（Xray/sing-box）
- 每个节点使用独立的本地端口
- 与主节点（系统代理）相互独立

### 2. 自定义本地端口
- 在 ProfileItem 模型中添加 `CustomLocalPort` 属性
- 在编辑服务器窗口中显示端口输入框
- 未设置时自动分配可用端口（从基础端口+100开始）

### 3. 运行状态显示
- 服务器列表新增"状态"列（显示"运行中"或空）
- 服务器列表新增"本地端口"列（显示当前使用的端口）

### 4. 右键菜单操作
- 添加"启动节点"菜单项
- 添加"停止节点"菜单项
- 添加"批量启动选中节点"菜单项
- 添加"批量停止选中节点"菜单项

## 技术实现

### 关键文件

| 文件 | 作用 |
|------|------|
| `ServiceLib/Manager/MultiCoreManager.cs` | **新增** - 管理多个 Core 实例的单例管理器 |
| `ServiceLib/Handler/CoreConfigHandler.cs` | 添加 `GenerateClientMultiNodeConfig` 方法生成简化配置 |
| `ServiceLib/Models/ProfileItem.cs` | 添加 `CustomLocalPort` 属性 |
| `ServiceLib/Models/ProfileItemModel.cs` | 添加 `IsRunning`, `RunningStatus`, `LocalPortDisplay` 属性 |
| `ServiceLib/ViewModels/ProfilesViewModel.cs` | 添加 `StartNodeCmd`, `StopNodeCmd`, `StartSelectedNodesCmd`, `StopSelectedNodesCmd` 命令 |
| `ServiceLib/ViewModels/AddServerViewModel.cs` | 添加 `CustomLocalPortText` 属性处理端口绑定 |
| `ServiceLib/Handler/ConfigHandler.cs` | 在 `AddServer` 方法中保存 `CustomLocalPort` |
| `ServiceLib/Services/CoreConfig/V2ray/V2rayInboundService.cs` | 支持 `CustomLocalPort` |
| `ServiceLib/Services/CoreConfig/Singbox/SingboxInboundService.cs` | 支持 `CustomLocalPort` |
| `ServiceLib/Resx/ResUI.resx` | 添加本地化字符串 |
| `v2rayN/Views/ProfilesView.xaml` | WPF UI - 添加状态和端口列 |
| `v2rayN.Desktop/Views/ProfilesView.axaml` | Avalonia UI - 添加状态和端口列 |

### MultiCoreManager 核心逻辑

```csharp
public class MultiCoreManager
{
    // 单例模式
    private static readonly Lazy<MultiCoreManager> _instance = new(() => new());
    public static MultiCoreManager Instance => _instance.Value;

    // 存储运行中的节点: IndexId -> (ProcessService, LocalPort, Node, CoreType)
    private readonly ConcurrentDictionary<string, RunningNodeInfo> _runningNodes = new();

    // 状态变更事件
    public event Action<string, bool, int>? OnNodeStatusChanged;

    // 启动节点
    public async Task<bool> StartNode(ProfileItem node)
    {
        // 1. 确定本地端口（自定义或自动分配）
        // 2. 生成配置文件（使用 CoreConfigHandler）
        // 3. 启动 Core 进程（使用 ProcessService）
        // 4. 触发状态变更事件
    }

    // 停止节点
    public async Task<bool> StopNode(string indexId)
    {
        // 1. 停止进程
        // 2. 从字典中移除
        // 3. 触发状态变更事件
    }

    // 批量启动节点
    public async Task<(int success, int failed)> StartNodes(IEnumerable<ProfileItem> nodes)
    {
        // 遍历并启动每个节点
    }

    // 批量停止节点
    public async Task<(int success, int failed)> StopNodes(IEnumerable<string> indexIds)
    {
        // 遍历并停止每个节点
    }

    // 查找可用端口
    private async Task<int> FindAvailablePort()
    {
        // 从基础端口+100开始查找未使用的端口
    }
}
```

### 本地化字符串

```xml
<!-- ResUI.resx -->
<data name="LvRunningStatus"><value>Status</value></data>
<data name="LvLocalPort"><value>Local Port</value></data>
<data name="TipRunning"><value>Running</value></data>
<data name="TipStopped"><value>Stopped</value></data>
<data name="menuStartNode"><value>Start Node</value></data>
<data name="menuStopNode"><value>Stop Node</value></data>
<data name="menuStartSelectedNodes"><value>Start selected nodes</value></data>
<data name="menuStopSelectedNodes"><value>Stop selected nodes</value></data>
<data name="TbCustomLocalPort"><value>Custom Local Port</value></data>
<data name="TbCustomLocalPortTips"><value>Custom local port for this node</value></data>

<!-- ResUI.zh-Hans.resx -->
<data name="LvRunningStatus"><value>状态</value></data>
<data name="LvLocalPort"><value>本地端口</value></data>
<data name="TipRunning"><value>运行中</value></data>
<data name="TipStopped"><value>已停止</value></data>
<data name="menuStartNode"><value>启动节点</value></data>
<data name="menuStopNode"><value>停止节点</value></data>
<data name="menuStartSelectedNodes"><value>批量启动选中节点</value></data>
<data name="menuStopSelectedNodes"><value>批量停止选中节点</value></data>
<data name="TbCustomLocalPort"><value>自定义本地端口</value></data>
<data name="TbCustomLocalPortTips"><value>此节点的自定义本地代理端口</value></data>
```

## GitHub Actions 工作流

### 1. 检测上游更新 (`check-upstream-release.yml`)
- 每天 UTC 8:00 自动检查
- 检测到新版本时创建 Issue 提醒

### 2. 同步上游代码 (`sync-upstream.yml`)
- 手动触发
- 自动合并上游代码或报告冲突

### 3. 自动化测试 (`test-multiport.yml`)
- 在 push 和 PR 时自动运行
- 验证多节点功能的关键文件存在
- 在 Windows 和 Linux 上测试构建

### 4. 发布构建 (`release-multiport.yml`)
- 创建 Release 时自动触发
- 构建所有平台版本（Windows/Linux/macOS x64/x86/arm64）
- 自动生成详细发布说明

## 构建命令

```bash
# Windows x64
dotnet publish ./v2rayN/v2rayN.csproj -c Release -r win-x64 --self-contained -o publish/win-x64

# Windows x86
dotnet publish ./v2rayN/v2rayN.csproj -c Release -r win-x86 --self-contained -o publish/win-x86

# Windows ARM64
dotnet publish ./v2rayN/v2rayN.csproj -c Release -r win-arm64 --self-contained -o publish/win-arm64

# Linux x64
dotnet publish ./v2rayN.Desktop/v2rayN.Desktop.csproj -c Release -r linux-x64 --self-contained -o publish/linux-x64

# Linux ARM64
dotnet publish ./v2rayN.Desktop/v2rayN.Desktop.csproj -c Release -r linux-arm64 --self-contained -o publish/linux-arm64

# macOS x64
dotnet publish ./v2rayN.Desktop/v2rayN.Desktop.csproj -c Release -r osx-x64 --self-contained -o publish/osx-x64

# macOS ARM64
dotnet publish ./v2rayN.Desktop/v2rayN.Desktop.csproj -c Release -r osx-arm64 --self-contained -o publish/osx-arm64
```

## 常见问题修复

### 1. CustomLocalPort 无法保存
**原因**: `ConfigHandler.AddServer` 方法在更新服务器时没有复制 `CustomLocalPort` 字段
**修复**: 在 ConfigHandler.cs 添加 `item.CustomLocalPort = profileItem.CustomLocalPort;`

### 2. CustomLocalPort 绑定问题
**原因**: `int?` 类型直接绑定到 TextBox 的 `string` 类型失败
**修复**: 在 AddServerViewModel 中添加 `CustomLocalPortText` 字符串属性作为中间层

### 3. Core 日志不显示
**原因**: MultiCoreManager 初始化时 updateFunc 为 null
**修复**: 在 UpdateFunc 方法中添加 fallback 到 NoticeManager

### 4. 多节点运行时只有活动节点生效
**原因**: 使用 `GenerateClientConfig` 生成完整配置，包含路由规则导致冲突
**修复**: 新增 `GenerateClientMultiNodeConfig` 方法，使用 `GenerateClientSpeedtestConfig` 生成简化配置（无路由规则），让每个节点独立作为代理服务器工作

## 注意事项

1. **端口冲突**: 确保每个同时运行的节点使用不同的本地端口
2. **资源占用**: 同时运行多个节点会增加系统资源占用
3. **与主节点独立**: 多节点功能与主节点（系统代理）相互独立
4. **Core 类型**: 支持 Xray 和 sing-box 两种 Core

## 项目结构

```
v2rayN-fork/
├── .github/
│   ├── workflows/
│   │   ├── check-upstream-release.yml  # 检测上游更新
│   │   ├── sync-upstream.yml           # 同步上游代码
│   │   ├── test-multiport.yml          # 自动化测试
│   │   └── release-multiport.yml       # 发布构建
│   ├── ISSUE_TEMPLATE/
│   │   ├── bug_report.md
│   │   └── feature_request.md
│   └── PULL_REQUEST_TEMPLATE.md
├── v2rayN/
│   ├── ServiceLib/                     # 核心业务逻辑
│   │   ├── Manager/
│   │   │   └── MultiCoreManager.cs     # 多节点管理器
│   │   ├── Models/
│   │   │   ├── ProfileItem.cs          # 服务器配置模型
│   │   │   └── ProfileItemModel.cs     # UI 绑定模型
│   │   ├── ViewModels/
│   │   │   ├── ProfilesViewModel.cs    # 服务器列表视图模型
│   │   │   └── AddServerViewModel.cs   # 添加服务器视图模型
│   │   ├── Handler/
│   │   │   └── ConfigHandler.cs        # 配置处理
│   │   ├── Services/CoreConfig/
│   │   │   ├── V2ray/V2rayInboundService.cs
│   │   │   └── Singbox/SingboxInboundService.cs
│   │   └── Resx/                       # 本地化资源
│   ├── v2rayN/                         # WPF 版本 (Windows)
│   │   └── Views/
│   │       ├── ProfilesView.xaml
│   │       └── AddServerWindow.xaml
│   └── v2rayN.Desktop/                 # Avalonia 版本 (跨平台)
│       └── Views/
│           ├── ProfilesView.axaml
│           └── AddServerWindow.axaml
└── README.md
```
