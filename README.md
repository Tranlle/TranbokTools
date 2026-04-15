<div align="center">

# T-Orbit

**基于 .NET 10 + Avalonia 12 构建的插件化桌面开发助手**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Avalonia](https://img.shields.io/badge/Avalonia-12.0-7C3AED?logo=avalonia&logoColor=white)](https://avaloniaui.net/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20macOS%20%7C%20Linux-0078D4)](https://github.com/AvaloniaUI/Avalonia)
[![License](https://img.shields.io/badge/License-See%20LICENSE-6B7280)](./LICENSE)

</div>

---

T-Orbit 是一个以**插件为核心**的桌面开发辅助平台，而非单一功能应用。宿主本身只负责插件加载、生命周期管理与导航渲染；所有业务能力均由可独立部署的插件提供。内置插件（设置、快捷键、插件管理）与目录扫描插件（如 EF Core 迁移工具）共享统一的设计系统与宿主服务，彼此相互隔离。

## 目录

- [功能特性](#功能特性)
- [快速开始](#快速开始)
- [项目结构](#项目结构)
- [技术栈](#技术栈)
- [内置插件](#内置插件)
- [开发新插件](#开发新插件)
- [Wiki 文档](https://github.com/Tranlle/TranbokTools/wiki)

---

## 功能特性

| 能力 | 说明 |
|---|---|
| **插件化架构** | 通过 `IPlugin` 契约定义统一生命周期，支持内置注册与目录扫描两种接入方式 |
| **动态导航** | 宿主根据已启用插件自动生成侧边导航，插件启停实时反映 |
| **统一设计系统** | 完整的主题、调色板、对话框、通用控件层（`Designer` 层），插件无需重复实现 |
| **插件变量管理** | 集中配置各插件所需的环境变量键值对，支持加密存储，插件自行读取与校验 |
| **自定义快捷键** | 全局快捷键注册与自定义覆盖，支持按插件分组管理 |
| **本地 SQLite 持久化** | 所有设置、变量、快捷键绑定统一存储于 `%APPDATA%/T-Orbit/t-orbit.db` |
| **自定义主题** | 支持内置调色板与 `themes/*.json` 外部主题，运行时热切换 |
| **EF Core 迁移工具** | 完整的迁移管理插件，支持 SQL Server / PostgreSQL / MySQL，多 Profile 配置 |
| **反向域名插件 ID** | ID 命名规范强制校验（`com.example.plugin`），注册时检测重复，开发期即失败 |

---

## 快速开始

### 环境要求

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- 可运行 Avalonia 桌面应用的图形环境
- （可选）`dotnet ef` 全局工具，仅 EF Core 迁移插件需要

### 构建

```bash
git clone <repo-url>
cd TranbokTools
dotnet build TOrbit.slnx
```

### 运行

```bash
dotnet run --project TOrbit.App/TOrbit.App.csproj
```

> 也可直接用 **Visual Studio** 或 **JetBrains Rider** 打开 `TOrbit.slnx` 后运行 `TOrbit.App`。

---

## 项目结构

```
TranbokTools/
├── TOrbit.App/                   # 桌面宿主入口：DI 配置、插件注册、主窗口
├── TOrbit.Core/                  # 宿主核心服务：插件目录、偏好持久化、变量存储
├── TOrbit.Designer/              # 统一设计系统：主题、控件、对话框
├── TOrbit.Plugin.Core/           # 插件契约层：接口、基类、模型
├── TOrbit.Plugin.KeyMap/         # 内置插件：快捷键管理
├── TOrbit.Plugin.Settings/       # 内置插件：应用设置
├── TOrbit.Plugin.PluginManager/  # 内置插件：插件管理
├── TOrbit.Plugin.Migration/      # 外部插件：EF Core 迁移工具
├── themes/                       # 自定义主题 JSON 文件
└── TOrbit.slnx
```

宿主在启动时会扫描 `<AppDir>/plugins/` 目录，递归发现所有实现 `IPlugin` 的程序集并自动加载。迁移插件的 `.csproj` 已配置将构建输出写入该目录。

---

## 技术栈

| 库 / 框架 | 版本 | 用途 |
|---|---|---|
| .NET | 10 | 运行时与基础类库 |
| Avalonia | 12.0 | 跨平台桌面 UI 框架 |
| CommunityToolkit.Mvvm | 8.4 | MVVM 源生成器、ObservableObject |
| Material.Avalonia | 3.15 | Material Design 主题基础 |
| Material.Icons.Avalonia | 3.0 | 图标库 |
| Microsoft.Data.Sqlite | 9.x | 本地 SQLite 持久化 |
| Microsoft.Extensions.DependencyInjection | 10.0 | 依赖注入容器 |

---

## 内置插件

### `torbit.keymap` — 快捷键

- 查看、自定义全局快捷键绑定
- 按插件分组显示，支持单条重置
- 快捷键冲突即时可见

### `torbit.settings` — 设置

- 深色 / 浅色主题切换
- 内置调色板与自定义 JSON 主题
- 全局字体方案
- **插件变量管理**：集中配置各插件的环境变量键值对（支持加密字段）

### `torbit.plugin-manager` — 插件管理

- 查看所有已注册插件（名称、版本、标签、内置状态）
- 调整导航排序
- 切换插件启用状态

### `torbit.migration` — EF Core 迁移工具（外部插件）

- 多 Profile 数据库连接配置
- 新增 / 执行 / 回滚迁移
- 支持 SQL Server / PostgreSQL / MySQL
- 迁移文件查看与编辑
- 详见 → [迁移插件使用指南](https://github.com/Tranlle/TranbokTools/wiki/Migration-Plugin)

---

## 开发新插件

### 1. 创建项目

```bash
dotnet new classlib -n TOrbit.Plugin.MyPlugin -f net10.0
```

在 `.csproj` 中添加引用：

```xml
<ProjectReference Include="..\TOrbit.Plugin.Core\TOrbit.Plugin.Core.csproj" />
<!-- 需要宿主服务时 -->
<ProjectReference Include="..\TOrbit.Core\TOrbit.Core.csproj" />
<!-- 需要 UI 控件时 -->
<ProjectReference Include="..\TOrbit.Designer\TOrbit.Designer.csproj" />
```

### 2. 声明元数据

```csharp
public sealed class MyPluginMetadata : PluginBaseMetadata
{
    public static MyPluginMetadata Instance { get; } = new();

    // ID 必须遵循反向域名约定，格式：{namespace}.{name}
    public override string Id => "com.example.my-plugin";
    public override string Name => "我的插件";
    public override string Version => "1.0.0";
    public override string Icon => "Star";

    // 可选：声明插件所需的环境变量
    public override IReadOnlyList<PluginVariableDefinition> VariableDefinitions =>
    [
        new("MY_API_KEY", "", "API 密钥", "调用外部服务所需的密钥"),
    ];
}
```

> **ID 格式规则**：全小写字母、数字、连字符，至少两段，以 `.` 分隔。违反规则将在 `CreateDescriptor` 阶段抛出异常。

### 3. 实现插件

```csharp
public sealed class MyPlugin : BasePlugin, IVisualPlugin
{
    public override PluginDescriptor Descriptor { get; } =
        CreateDescriptor<MyPlugin>(MyPluginMetadata.Instance);

    public override Control GetMainView() => new MyView { DataContext = _viewModel };
}
```

### 4. 接入宿主

**内置插件**：在 `App.axaml.cs` 中注册

```csharp
services.AddSingleton<MyPlugin>();
// 在 RegisterBuiltInPluginsAsync 中：
await plugin.InitializeAsync(context);
await plugin.StartAsync();
catalog.Register(plugin);
```

**目录插件**：配置 `.csproj` 输出到插件目录即可

```xml
<PropertyGroup>
  <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
  <OutputPath>../TOrbit.App/bin/$(Configuration)/net10.0/plugins/MyPlugin/</OutputPath>
</PropertyGroup>
```

---

## 许可证

本项目许可证以仓库中的 [`LICENSE`](./LICENSE) 文件为准。
