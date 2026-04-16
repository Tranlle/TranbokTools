# T-Orbit

<p align="center">
  <strong>基于 .NET 10 与 Avalonia 12 的插件化桌面工具宿主</strong>
</p>

<p align="center">
  为内置工具、目录插件、统一设计系统、变量注入、快捷键系统和诊断链路提供稳定宿主能力。
</p>

<p align="center">
  <img alt=".NET 10" src="https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white">
  <img alt="Avalonia 12" src="https://img.shields.io/badge/Avalonia-12-0F172A?logo=avalonia&logoColor=white">
  <img alt="Desktop Host" src="https://img.shields.io/badge/App-Desktop%20Host-0F766E">
  <img alt="Plugin Architecture" src="https://img.shields.io/badge/Architecture-Plugin%20Driven-1D4ED8">
  <img alt="SQLite" src="https://img.shields.io/badge/Storage-SQLite-003B57?logo=sqlite&logoColor=white">
  <img alt="License" src="https://img.shields.io/badge/License-Repository%20Defined-64748B">
</p>

## 概览

T-Orbit 不是单一业务应用，而是一个可扩展的桌面工具宿主：

- 宿主管理插件发现、加载、生命周期、导航、设置、诊断和持久化。
- 插件提供功能页面、工具能力和业务逻辑。
- 所有插件共享统一设计系统、主题系统、变量管理和宿主工具接口。

## 目录

- [为什么是 T-Orbit](#为什么是-t-orbit)
- [当前能力](#当前能力)
- [内置与示例插件](#内置与示例插件)
- [快速开始](#快速开始)
- [项目结构](#项目结构)
- [架构摘要](#架构摘要)
- [开发插件](#开发插件)
- [测试](#测试)
- [文档](#文档)
- [许可证](#许可证)

## 为什么是 T-Orbit

| 方向 | 当前实现 |
| --- | --- |
| 插件宿主 | 同时支持内置插件和目录插件 |
| 生命周期 | 支持启动、停止、重启，并按插件串行化执行 |
| 启动与关闭 | 通过 `AppStartupCoordinator` 与 `AppShutdownCoordinator` 统一协调 |
| 变量管理 | 支持默认值、加密标记、运行期注入和插件级提醒 |
| 快捷键系统 | 支持注册、覆盖、持久化和运行期刷新 |
| 诊断链路 | 可追踪启动、迁移、插件发现与关闭阶段的问题 |
| 持久化 | 使用 SQLite 保存应用偏好、快捷键和插件变量 |
| UI 框架 | 宿主与插件页共享统一设计系统和工作台式布局框架 |

## 当前能力

### 宿主层

- 插件目录扫描与 Manifest 加载
- 插件目录注册与导航构建
- 统一设置页、监控页、快捷键页
- 应用级诊断收集与展示

### 插件层

- `Visual` 类型插件页面承载
- 插件变量声明、注入、校验状态发布
- 页面头部动作区扩展
- 插件能力标签与元数据展示

### 工程能力

- SQLite 持久化
- `PluginExecutionGate` 生命周期串行化
- 关闭流程与普通生命周期共享门控
- 运行期快捷键刷新

## 内置与示例插件

| 插件 | 说明 |
| --- | --- |
| `torbit.keymap` | 快捷键查看、编辑与覆盖管理 |
| `torbit.settings` | 主题、字体、窗口行为与插件变量管理 |
| `torbit.monitor` | 插件运行状态、能力声明与应用诊断查看 |
| `torbit.migration` | EF Core 迁移插件，支持默认连接串注入和多 Profile 配置 |
| `torbit.promptor` | 提示词优化插件示例 |

## 快速开始

### 环境要求

- `.NET 10 SDK`
- 可运行 Avalonia 桌面应用的图形环境

### 构建

```bash
dotnet build TOrbit.slnx
```

### 运行

```bash
dotnet run --project TOrbit.App/TOrbit.App.csproj
```

### 测试

```bash
dotnet test TOrbit.Core.Tests/TOrbit.Core.Tests.csproj
```

## 项目结构

```text
T-Orbit/
├─ TOrbit.App/                 # 桌面宿主入口、主窗口、导航与启动流程
├─ TOrbit.Core/                # 核心服务：插件、存储、变量、快捷键、诊断
├─ TOrbit.Designer/            # 统一设计系统、主题与复用控件
├─ TOrbit.Plugin.Core/         # 插件契约、基础模型、基类
├─ TOrbit.Plugin.KeyMap/       # 内置插件：快捷键管理
├─ TOrbit.Plugin.Settings/     # 内置插件：设置与变量管理
├─ TOrbit.Plugin.Monitor/      # 内置插件：插件监控与应用诊断
├─ TOrbit.Plugin.Migration/    # 外部插件：EF Core 迁移
├─ TOrbit.Plugin.Promptor/     # 外部插件：提示词优化示例
├─ TOrbit.Core.Tests/          # 核心测试
└─ TOrbit.wiki/                # Wiki 文档
```

## 架构摘要

### 启动

- 主窗口先创建，避免初始化阻塞首帧。
- 启动协调器异步执行：
  - 内置插件注册
  - 外部插件发现与加载
  - 插件变量注入
  - 快捷键覆盖加载

### 生命周期

- `PluginLifecycleService` 负责普通启停与重启。
- `PluginExecutionGate` 为每个插件提供独立串行化门控。
- `AppShutdownCoordinator` 在关闭流程中复用同一套门控，避免和运行中的启停操作冲突。

### 变量与诊断

- 插件通过 `PluginVariableDefinition` 声明变量元数据。
- 设置页保存不会被变量校验阻塞。
- 校验问题会以“插件级提醒”的方式展示在对应插件页头部。
- 启动、迁移、发现、关闭阶段的异常会汇总到应用诊断。

### 存储

- 数据库路径：`%APPDATA%/T-Orbit/t-orbit.db`
- 主要存储：
  - 应用偏好
  - 快捷键绑定
  - 插件变量

## 开发插件

一个最小可运行的插件通常包括：

1. 元数据类
2. 插件类
3. ViewModel
4. View

基础模式如下：

```csharp
public sealed class MyPlugin : BasePlugin, IVisualPlugin
{
    public override PluginDescriptor Descriptor { get; } =
        CreateDescriptor<MyPlugin>(MyPluginMetadata.Instance);

    public override Control GetMainView()
    {
        EnsureView();
        return _view!;
    }
}
```

## 测试

核心测试项目：

```bash
dotnet test TOrbit.Core.Tests/TOrbit.Core.Tests.csproj
```

## 文档

- Wiki 首页：[TOrbit.wiki](https://github.com/Tranlle/T-Orbit/wiki/Home)
- 架构说明：[Architecture](https://github.com/Tranlle/T-Orbit/wiki/Architecture)
- 插件系统：[Plugin System](https://github.com/Tranlle/T-Orbit/wiki/Plugin-System)
- 变量管理：[Plugin Variable Management](https://github.com/Tranlle/T-Orbit/wiki/Plugin-Variable-Management)
- 插件开发：[Creating a Plugin](https://github.com/Tranlle/T-Orbit/wiki/Creating-a-Plugin)

## 许可证

以仓库中的 [LICENSE](./LICENSE) 为准。
