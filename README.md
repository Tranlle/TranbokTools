<div align="center">

# T-Orbit

**基于 .NET 10 + Avalonia 12 构建的插件化桌面工具宿主**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Avalonia](https://img.shields.io/badge/Avalonia-12.0-7C3AED?logo=avalonia&logoColor=white)](https://avaloniaui.net/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20macOS%20%7C%20Linux-0078D4)](https://github.com/AvaloniaUI/Avalonia)
[![License](https://img.shields.io/badge/License-See%20LICENSE-6B7280)](./LICENSE)

</div>

---

T-Orbit 是一个以**插件为核心**的桌面工具宿主，而不是单一业务应用。宿主本身负责插件加载、生命周期管理、导航和基础设施；具体能力由内置插件和外部插件提供，并共享统一的设计系统、配置存储和宿主工具。

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
| **动态导航** | 宿主根据已启用插件自动生成侧边导航，插件启停可实时反映 |
| **统一设计系统** | 提供主题、对话框和复用控件，插件无需重复实现基础 UI |
| **插件变量管理** | 集中管理插件变量，支持加密存储和运行时注入 |
| **自定义快捷键** | 提供全局快捷键注册、覆盖、冲突检测和持久化 |
| **本地 SQLite 持久化** | 设置、变量、快捷键绑定统一保存在 `%APPDATA%/T-Orbit/t-orbit.db` |
| **自定义主题** | 支持内置调色板和 `themes/*.json` 外部主题文件 |
| **EF Core 迁移工具** | 通过外部插件管理迁移文件、数据库更新和回滚流程 |
| **反向域名插件 ID** | 强制使用反向域名命名规范，降低冲突概率 |

---

## 快速开始

### 环境要求

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- 可运行 Avalonia 桌面应用的图形环境
- 可选：`dotnet ef` 全局工具，仅迁移插件需要

### 构建

```bash
git clone <repo-url>
cd T-Orbit
dotnet build TOrbit.slnx
```

### 运行

```bash
dotnet run --project TOrbit.App/TOrbit.App.csproj
```

也可以直接用 **Visual Studio** 或 **JetBrains Rider** 打开 `TOrbit.slnx` 后运行 `TOrbit.App`。

---

## 项目结构

```text
T-Orbit/
├── TOrbit.App/                    # 桌面宿主入口：DI、内置插件注册、主窗口
├── TOrbit.Core/                   # 宿主核心服务：持久化、插件发现、变量、快捷键
├── TOrbit.Designer/               # 设计系统：主题、控件、对话框
├── TOrbit.Plugin.Core/            # 插件契约层：接口、基类、模型
├── TOrbit.Plugin.KeyMap/          # 内置插件：快捷键管理
├── TOrbit.Plugin.Settings/        # 内置插件：应用设置
├── TOrbit.Plugin.Monitor/         # 内置插件：插件监控
├── TOrbit.Plugin.Promptor/        # 外部插件：提示词优化工具
├── TOrbit.Plugin.Migration/       # 外部插件：EF Core 迁移工具
├── themes/                        # 自定义主题 JSON
└── TOrbit.slnx
```

宿主启动时会扫描 `<AppDir>/plugins/` 目录，递归发现实现了 `IPlugin` 的程序集并自动加载。外部插件的 `.csproj` 已配置为将构建输出写入该目录。

---

## 技术栈

| 组件 / 框架 | 版本 | 用途 |
|---|---|---|
| .NET | 10 | 运行时与基础类库 |
| Avalonia | 12.0 | 跨平台桌面 UI |
| CommunityToolkit.Mvvm | 8.4 | MVVM 源生成器 |
| Material.Avalonia | 3.15 | Material Design 主题基础 |
| Material.Icons.Avalonia | 3.0 | 图标库 |
| Microsoft.Data.Sqlite | 9.x | 本地 SQLite 存储 |
| Microsoft.Extensions.DependencyInjection | 10.0 | 依赖注入 |

---

## 内置插件

### `torbit.keymap`：快捷键

- 查看和自定义全局快捷键绑定
- 按插件分组展示，并支持单条重置
- 检测快捷键冲突

### `torbit.settings`：设置

- 切换主题和调色板
- 支持内置主题与外部 JSON 主题
- 切换全局字体方案
- 统一管理插件变量和加密字段

### `torbit.monitor`：插件监控

- 查看插件名称、版本、状态和来源
- 启停、重启插件
- 调整启用状态与排序

### `torbit.migration`：EF Core 迁移工具

- 管理多 Profile 数据库连接
- 新增、执行、回滚迁移
- 支持 SQL Server / PostgreSQL / MySQL
- 浏览和编辑迁移文件

---

## 开发新插件

### 1. 创建项目

```bash
dotnet new classlib -n TOrbit.Plugin.MyPlugin -f net10.0
```

在 `.csproj` 中添加引用：

```xml
<ProjectReference Include="..\\TOrbit.Plugin.Core\\TOrbit.Plugin.Core.csproj" />
<ProjectReference Include="..\\TOrbit.Core\\TOrbit.Core.csproj" />
<ProjectReference Include="..\\TOrbit.Designer\\TOrbit.Designer.csproj" />
```

### 2. 声明元数据

```csharp
public sealed class MyPluginMetadata : PluginBaseMetadata
{
    public static MyPluginMetadata Instance { get; } = new();

    public override string Id => "com.example.my-plugin";
    public override string Name => "我的插件";
    public override string Version => "1.0.0";
    public override string Icon => "Star";

    public override IReadOnlyList<PluginVariableDefinition> VariableDefinitions =>
    [
        new("MY_API_KEY", "", "API 密钥", "调用外部服务时需要的密钥"),
    ];
}
```

插件 ID 必须使用小写反向域名格式，例如 `torbit.settings` 或 `com.example.tool`。

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

内置插件：在 `App.axaml.cs` 中注册，并在启动时初始化后加入目录。

目录插件：将输出路径指向 `TOrbit.App/bin/<Configuration>/net10.0/plugins/<PluginName>/`。

```xml
<PropertyGroup>
  <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
  <OutputPath>../TOrbit.App/bin/$(Configuration)/net10.0/plugins/MyPlugin/</OutputPath>
</PropertyGroup>
```

---

## 许可说明

以仓库中的 [`LICENSE`](./LICENSE) 文件为准。
