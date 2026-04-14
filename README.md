# Tranbok.Tools

> 一个基于 **.NET 10 + Avalonia** 构建的插件化桌面工具宿主，用于承载 Tranbok 相关的开发辅助能力。

![Platform](https://img.shields.io/badge/platform-Desktop-blue)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)
![UI](https://img.shields.io/badge/UI-Avalonia-7C3AED)
![Architecture](https://img.shields.io/badge/Architecture-Plugin--based-0F766E)

## 概览

`Tranbok.Tools` 不是单一功能应用，而是一个面向内部开发场景的 **工具容器**。项目以插件机制为核心，将数据库迁移、插件管理、应用设置等能力统一承载在一个桌面壳中，提供一致的交互体验与后续可扩展基础设施。

当前仓库已经具备以下核心能力：

- 插件化宿主与插件目录发现机制
- 内置数据库迁移插件
- 插件管理页面
- 应用设置页面
- 统一的 Designer / Theme / Dialog 能力层
- 基于依赖注入的模块装配方式

---

## 主要特性

### 插件化桌面宿主

- 通过 `IPlugin` 定义插件生命周期契约
- 通过 `IVisualPlugin` 承载可视化页面
- 支持内置插件注册与外部插件目录发现
- 支持插件目录、描述、排序、启停状态等元数据管理

### 数据库迁移工具

内置的 Migration 插件面向 EF Core 迁移场景，当前支持：

- 多配置管理
- `SQL Server / PostgreSQL / MySQL` 数据库类型切换
- 迁移列表加载
- 新增迁移
- 执行 `database update`
- 删除最后一条迁移
- 查看与编辑迁移文件内容
- 输出日志与状态反馈

### 统一的桌面 UI 基础设施

项目内置了一套通用设计层，包含：

- 页面布局组件
- 表单与属性展示组件
- 对话框 / Sheet 能力
- 主题与调色板服务
- 可复用的 Designer 控件

### 设置与主题能力

- 深色 / 浅色主题切换
- 内置调色板切换
- 应用级偏好设置持久化
- 工作区与工具行为配置入口

---

## 技术栈

- **.NET 10**
- **Avalonia UI**
- **MVVM**
- **Microsoft.Extensions.DependencyInjection**
- **插件化架构**
- **EF Core CLI 集成**

---

## 项目结构

```text
TranbokTools/
├─ Tranbok.Tools.App/                    # 桌面应用入口（Avalonia 宿主）
├─ Tranbok.Tools.Core/                   # 核心服务、配置、宿主基础能力
├─ Tranbok.Tools.Designer/               # 设计系统、主题、对话框、通用控件
├─ Tranbok.Tools.Plugin.Core/            # 插件契约、插件模型、插件上下文
├─ Tranbok.Tools.Plugin.Migration/       # 数据库迁移插件
├─ Tranbok.Tools.Plugin.PluginManager/   # 插件管理插件
├─ Tranbok.Tools.Plugin.Settings/        # 设置插件
├─ Tranbok.Tools/                        # 历史/兼容目录（旧实现保留）
├─ themes/                               # 主题相关资源目录
├─ Tranbok.Tools.slnx                    # 解决方案
└─ README.md
```

---

## 核心模块说明

### 1. `Tranbok.Tools.App`

应用启动入口与宿主壳层。

职责：

- 初始化 Avalonia 应用
- 配置依赖注入容器
- 注册内置插件
- 发现并加载插件目录中的扩展插件
- 创建主窗口与主视图模型

关键文件：

- `Tranbok.Tools.App/App.axaml`
- `Tranbok.Tools.App/App.axaml.cs`
- `Tranbok.Tools.App/Views/MainWindow.axaml`

### 2. `Tranbok.Tools.Core`

提供应用层核心服务与基础设施。

典型职责：

- 宿主环境信息
- 应用偏好设置
- Shell 服务
- 依赖注入扩展

### 3. `Tranbok.Tools.Designer`

统一的桌面设计层。

典型能力：

- 通用页面布局
- Designer 控件
- 主题服务
- 调色板注册
- Dialog / Prompt / Confirm / Sheet 服务

### 4. `Tranbok.Tools.Plugin.Core`

插件系统基础契约层。

关键概念：

- `IPlugin`
- `PluginDescriptor`
- `PluginContext`
- `BasePlugin`
- 插件可视化扩展接口

### 5. `Tranbok.Tools.Plugin.Migration`

数据库迁移插件，是当前仓库最核心的业务插件。

当前职责：

- 管理数据库连接配置
- 解析项目路径与迁移上下文
- 调用 EF Core CLI
- 展示迁移列表与迁移文件
- 支持迁移执行与回滚相关操作

关键文件：

- `Tranbok.Tools.Plugin.Migration/MigrationPlugin.cs`
- `Tranbok.Tools.Plugin.Migration/ViewModels/MigrationViewModel.cs`
- `Tranbok.Tools.Plugin.Migration/Services/MigrationService.cs`
- `Tranbok.Tools.Plugin.Migration/Views/MigrationView.axaml`

### 6. `Tranbok.Tools.Plugin.PluginManager`

插件管理插件，用于查看和管理当前宿主中的插件信息。

### 7. `Tranbok.Tools.Plugin.Settings`

设置插件，用于管理主题、调色板、字体策略、工作区等应用级选项。

---

## 插件机制简介

插件通过统一契约接入宿主：

```csharp
public interface IPlugin : IAsyncDisposable
{
    PluginDescriptor Descriptor { get; }

    ValueTask InitializeAsync(PluginContext context, CancellationToken cancellationToken = default);
    ValueTask StartAsync(CancellationToken cancellationToken = default);
    ValueTask StopAsync(CancellationToken cancellationToken = default);
}
```

对可视化插件，宿主会进一步获取其主视图并渲染到主内容区。

当前插件来源分为两类：

1. **内置插件**
   - 启动时通过 DI 直接注册
2. **外部插件**
   - 从应用目录下 `Plugins` 目录发现并加载

---

## 快速开始

### 环境要求

- **.NET 10 SDK**
- 可正常运行的桌面图形环境
- 若使用迁移插件，需要本机可执行 `dotnet ef`
- 业务项目需具备可用的 EF Core 迁移上下文

### 构建

```bash
dotnet build Tranbok.Tools.slnx
```

### 运行

```bash
dotnet run --project Tranbok.Tools.App/Tranbok.Tools.App.csproj
```

也可以直接使用 Visual Studio / Rider 打开解决方案运行 `Tranbok.Tools.App`。

---

## 数据库迁移插件使用说明

迁移插件主要用于管理 EF Core 迁移流程。

典型使用流程：

1. 选择或创建数据库配置
2. 配置项目路径、数据库类型、DbContext、连接字符串
3. 刷新迁移列表
4. 根据需要执行：
   - 新增迁移
   - 执行更新
   - 删除最后一条迁移
5. 在右侧查看或编辑迁移文件内容

支持的数据库类型：

- SQL Server
- PostgreSQL
- MySQL

---

## 开发说明

### 构建目标

当前主应用项目为：

- `Tranbok.Tools.App/Tranbok.Tools.App.csproj`

目标框架：

- `net10.0`

### 代码组织建议

新增功能时建议遵循以下分层：

- **App**：宿主入口与窗口壳层
- **Core**：跨插件共享的核心服务
- **Designer**：UI 基础设施与主题层
- **Plugin.Core**：插件契约
- **Plugin.***：具体插件实现

### 新增插件的建议步骤

1. 新建 `Tranbok.Tools.Plugin.YourPlugin` 项目
2. 引用 `Tranbok.Tools.Plugin.Core` 与必要的 Designer/Core 模块
3. 实现插件描述与生命周期
4. 实现主视图与 ViewModel
5. 将插件注册为内置插件，或输出到 `Plugins` 目录进行发现加载

---

## 当前状态

该仓库已经具备一个可持续扩展的桌面工具平台雏形，当前最成熟的功能是数据库迁移工具链。整体代码结构已经形成较清晰的分层：

- 宿主层
- Core 层
- Designer 层
- Plugin Core 层
- 插件实现层

这意味着它不仅可以继续完善迁移管理能力，也适合继续扩展为更多开发辅助工具的统一入口。

---

## 后续可扩展方向

- 更多数据库开发工具插件
- SQL 脚本生成 / 导出
- 差异预览与变更分析
- 项目模板与代码生成类工具
- API 调试与内部服务辅助工具
- 更完善的插件加载、隔离与版本管理能力

---

## 许可证

本项目许可证以仓库中的 `LICENSE` 文件为准。

---

## 代码阅读入口

如果你希望快速理解项目，建议按以下顺序阅读：

1. `Tranbok.Tools.App/App.axaml.cs`
2. `Tranbok.Tools.App/Views/MainWindow.axaml`
3. `Tranbok.Tools.Plugin.Core/Abstractions/IPlugin.cs`
4. `Tranbok.Tools.Plugin.Migration/MigrationPlugin.cs`
5. `Tranbok.Tools.Plugin.Migration/ViewModels/MigrationViewModel.cs`
6. `Tranbok.Tools.Plugin.Migration/Services/MigrationService.cs`
7. `Tranbok.Tools.Plugin.Settings/SettingsPlugin.cs`
8. `Tranbok.Tools.Designer/Services/ThemeService.cs`

---

如果你计划将 `Tranbok.Tools` 持续演进为长期维护的内部工具平台，建议下一步优先补齐：

- 插件开发规范文档
- 发布与版本管理流程
- 配置与偏好项说明
- 插件加载/隔离策略说明
- 截图、演示与使用示例
