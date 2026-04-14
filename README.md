# Tranbok.Tools

> 基于 **.NET 10 + Avalonia 12** 的插件化桌面工具宿主，当前提供插件管理、应用设置，以及通过外部插件加载的 EF Core 迁移工具。

![Platform](https://img.shields.io/badge/platform-Desktop-blue)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)
![UI](https://img.shields.io/badge/UI-Avalonia-7C3AED)
![Architecture](https://img.shields.io/badge/Architecture-Plugin--based-0F766E)

## 项目概览

`Tranbok.Tools` 是一个桌面工具容器，而不是单一功能应用。仓库按“宿主 + 核心层 + Designer 层 + 插件契约层 + 插件实现层”组织，主界面会根据已注册且启用的插件动态生成导航，并在内容区渲染插件页面。

当前仓库中已实现的核心能力：

- Avalonia 桌面宿主与动态插件导航
- 内置插件注册机制
- `plugins` 目录外部插件发现与加载
- 插件管理页面
- 应用设置页面
- 统一的主题、调色板、对话框与通用控件层
- EF Core 迁移插件（通过插件目录发现加载）

---

## 当前功能

### 1. 插件化桌面宿主

- 通过 `IPlugin` 定义插件生命周期
- 通过 `IPluginViewProvider` 提供插件主视图
- 通过 `IPluginHeaderActionsProvider` 提供页头动作按钮
- 宿主根据启用插件动态生成左侧导航
- 支持内置插件与目录扫描插件两种接入方式

### 2. 插件管理

- 查看当前已注册插件
- 展示插件名称、描述、版本、标签、内置状态
- 调整插件排序
- 切换插件启用状态

> 注意：当前“禁用插件”仅影响宿主导航显示与 `EnabledPlugins` 集合，不等于真正卸载程序集，也不会调用 `StopAsync` 结束插件生命周期。

### 3. 设置页面

- 切换深色 / 浅色主题
- 切换内置调色板
- 从 `themes/*.json` 加载高级自定义主题
- 切换字体方案
- 提供工作区路径、自定义主题目录、迁移 Workspace 选项的界面入口

> 当前代码中真正持久化保存的是 `app-preferences.json` 里的 `FontOptionKey`。主题与调色板会即时生效，但并未看到完整的持久化配置落盘。

### 4. EF Core 迁移插件

迁移插件是当前最完整的业务插件，支持：

- 配置多个数据库连接 Profile
- 选择 Domain 项目 `.csproj`
- 配置 `DbContext`
- 支持数据库类型：`SQL Server` / `PostgreSQL` / `MySQL`
- 刷新迁移列表
- 新增迁移
- 执行 `database update`
- 删除最后一条迁移
- 查看与编辑迁移文件内容
- 输出执行日志

---

## 技术栈

- **.NET 10 SDK**
- **Avalonia 12**
- **CommunityToolkit.Mvvm**
- **Material.Avalonia**
- **Microsoft.Extensions.DependencyInjection**
- **插件化架构**
- **EF Core CLI（迁移插件）**

---

## 项目结构

```text
TranbokTools/
├─ Tranbok.Tools.App/                    # 桌面应用入口与主窗口壳
├─ Tranbok.Tools.Core/                   # 宿主核心服务、配置与插件目录管理
├─ Tranbok.Tools.Designer/               # 主题、对话框、通用控件、设计层基础设施
├─ Tranbok.Tools.Plugin.Core/            # 插件契约、模型、基类
├─ Tranbok.Tools.Plugin.Migration/       # EF Core 迁移插件（输出到 plugins 目录）
├─ Tranbok.Tools.Plugin.PluginManager/   # 插件管理插件
├─ Tranbok.Tools.Plugin.Settings/        # 设置插件
├─ themes/                               # JSON 主题目录
├─ Tranbok.Tools.slnx                    # 解决方案
└─ README.md
```

---

## 模块说明

### `Tranbok.Tools.App`

桌面宿主入口。

职责：

- 初始化 Avalonia 应用
- 注册 DI 容器
- 注册内置插件：`PluginManager`、`Settings`
- 扫描 `AppContext.BaseDirectory/plugins` 目录并加载外部插件
- 创建主窗口与主视图模型

关键文件：

- `Tranbok.Tools.App/Program.cs`
- `Tranbok.Tools.App/App.axaml.cs`
- `Tranbok.Tools.App/ViewModels/MainViewModel.cs`
- `Tranbok.Tools.App/Views/MainWindow.axaml`

### `Tranbok.Tools.Core`

宿主共享核心服务层。

职责：

- 插件注册目录 `PluginCatalogService`
- 插件发现 `PluginDiscoveryService`
- 应用偏好持久化 `AppPreferencesService`
- 宿主信息 `AppShellService`

### `Tranbok.Tools.Designer`

统一 UI / 设计系统层。

能力包括：

- 主题服务与调色板注册
- 内置主题 + `themes` 目录 JSON 主题加载
- Confirm / Prompt / Sheet 对话框
- 通用页面布局与控件
- 插件默认视图

### `Tranbok.Tools.Plugin.Core`

插件契约层。

核心对象：

- `IPlugin`
- `IPluginViewProvider`
- `IPluginMetadata`
- `BasePlugin`
- `PluginDescriptor`
- `PluginContext`

### `Tranbok.Tools.Plugin.PluginManager`

插件管理插件。

### `Tranbok.Tools.Plugin.Settings`

设置插件。

### `Tranbok.Tools.Plugin.Migration`

EF Core 迁移插件。

---

## 插件机制

### 插件生命周期契约

```csharp
public interface IPlugin : IAsyncDisposable
{
    PluginDescriptor Descriptor { get; }

    ValueTask InitializeAsync(PluginContext context, CancellationToken cancellationToken = default);
    ValueTask StartAsync(CancellationToken cancellationToken = default);
    ValueTask StopAsync(CancellationToken cancellationToken = default);
}
```

### 插件视图接入

- 插件实现 `IPluginViewProvider` 后，宿主会将其主视图渲染到主内容区
- 插件实现 `IPluginHeaderActionsProvider` 后，宿主会将最多 3 个动作按钮显示在页头

### 当前实际加载方式

#### 内置插件
由 `App.axaml.cs` 直接注册：

- `PluginManagerPlugin`
- `SettingsPlugin`

#### 外部插件
宿主会扫描：

- `AppContext.BaseDirectory/plugins`

并递归查找所有 `*.dll`，使用反射寻找实现 `IPlugin` 的非抽象类型，实例化后调用：

1. `InitializeAsync`
2. `StartAsync`
3. 注册到 `PluginCatalogService`

### 当前插件系统边界

虽然 `Plugin.Core` 中存在 `PluginManifest`、依赖图、兼容性等类型，但当前加载流程实际采用的是：

- `AssemblyLoadContext.Default.LoadFromAssemblyPath(...)`
- `Activator.CreateInstance(...)`
- 反射发现实现 `IPlugin` 的类型

因此当前版本更接近“基础反射发现式插件系统”，而不是完整的隔离 / 依赖解析 / manifest 驱动插件平台。

---

## 运行与构建

### 环境要求

- **.NET 10 SDK**
- 可运行 Avalonia 桌面应用的图形环境
- 若使用迁移插件，需要本机可执行：
  - `dotnet`
  - `dotnet ef`

### 构建

```bash
dotnet build Tranbok.Tools.slnx
```

### 运行

```bash
dotnet run --project Tranbok.Tools.App/Tranbok.Tools.App.csproj
```

也可以直接用 Visual Studio / Rider 打开解决方案运行 `Tranbok.Tools.App`。

---

## 插件接入说明

### 哪些插件是内置的

当前主应用内置注册：

- `PluginManager`
- `Settings`

### 哪些插件通过目录加载

仓库里的迁移插件项目：

- `Tranbok.Tools.Plugin.Migration`

其 `.csproj` 配置会将构建输出写入：

- `Tranbok.Tools.App/bin/<Configuration>/net10.0/plugins/Migration/`

因此它在运行时是通过插件扫描目录被宿主加载，而不是由 `App` 直接源码注册。

---

## 迁移插件使用说明

### 前置要求

迁移插件对目标业务项目有明确要求，缺一不可：

1. 你需要选择一个 **Domain 项目 `.csproj`**
2. 该项目所在目录必须存在 `DesignSettings.json`
3. `DesignSettings.json` 必须至少提供：
   - `dotnetVersion`
   - `packages`
4. `packages` 中必须包含：
   - `Microsoft.EntityFrameworkCore.Design`
5. 你需要填写目标 `DbContext` 名称
6. 迁移操作依赖本机 `dotnet ef`

### 最小 `DesignSettings.json` 示例

```json
{
  "dotnetVersion": "net10.0",
  "packages": [
    {
      "packageName": "Microsoft.EntityFrameworkCore.Design",
      "version": "10.0.0"
    },
    {
      "packageName": "Microsoft.EntityFrameworkCore.SqlServer",
      "version": "10.0.0"
    }
  ]
}
```

> 数据库类型为 PostgreSQL / MySQL 时，需提供对应 Provider 包，代码也会在缺失时按 `Microsoft.EntityFrameworkCore.Design` 的版本补全必需 Provider 引用。

### 典型流程

1. 在迁移页面选择或新建一个配置
2. 选择 Domain 项目 `.csproj`
3. 填写 Profile 名称、数据库类型、`DbContext`、连接字符串
4. 刷新迁移列表
5. 执行需要的操作：
   - 新增迁移
   - 执行 `database update`
   - 删除最后一条迁移
6. 在右侧查看或编辑迁移文件内容

### 迁移插件实际工作方式

迁移插件不会直接在目标业务项目内执行所有操作，而是：

- 在插件工作目录下构建一个临时 Workspace 工程
- 自动生成 `DesignTimeFactory.cs`
- 调用 `dotnet restore` / `dotnet build` / `dotnet ef ...`
- 将迁移输出复制到插件输出目录

### 支持的数据库类型

- SQL Server
- PostgreSQL
- MySQL

---

## 配置文件与输出目录

### 应用偏好

宿主会在应用目录写入：

- `app-preferences.json`

当前已确认持久化字段：

- `fontOptionKey`

### 主题目录

自定义主题从以下目录加载：

- `themes/*.json`

仓库示例：

- `themes/emerald-dark.json`

### 迁移插件配置文件

迁移插件优先写入 / 读取：

- `Plugins/Migration/.tranbok-tools.json`
- `Plugins/Migration/setting.json`

同时兼容读取旧路径：

- `<Domain项目目录>/.tranbok-tools.json`
- `<Domain项目目录>/Migration/setting.json`

### 迁移插件工作目录

迁移插件会在应用目录下生成：

- `Plugins/Migration/WorkSpace/...`
- `Plugins/Migration/Output/...`

用途：

- `WorkSpace/`：临时生成的迁移执行工程
- `Output/`：迁移输出文件目录

> 如果你需要整理仓库或发布目录，建议把这些运行期生成内容视为构建/工具产物处理。

---

## 自定义主题 JSON 格式

代码会从 `themes` 目录读取所有 `.json` 文件，并反序列化为 `ThemePalette`。示例字段如下：

```json
{
  "key": "emerald-dark-custom",
  "label": "Emerald Dark Custom",
  "description": "示例主题",
  "baseVariant": "Dark",
  "accentBrush": "#36CFC9",
  "accentForegroundBrush": "#081B1A",
  "backgroundBrush": "#0B1212",
  "surfaceBrush": "#121A1A",
  "surfaceElevatedBrush": "#182323",
  "borderBrush": "#274240",
  "textPrimaryBrush": "#F1FBFA",
  "textSecondaryBrush": "#A7C7C4",
  "textMutedBrush": "#6B8B88",
  "badgeSuccessBackgroundBrush": "#1B3028",
  "badgeWarningBackgroundBrush": "#322A18",
  "badgeDangerBackgroundBrush": "#351D22"
}
```

实际可参考：

- `themes/emerald-dark.json`

---

## 开发说明

### 分层建议

新增功能建议按现有结构扩展：

- **App**：宿主入口、窗口壳与导航
- **Core**：跨插件共享服务
- **Designer**：主题、对话框、控件、通用页面能力
- **Plugin.Core**：插件契约与模型
- **Plugin.***：具体插件实现

### 新增插件建议步骤

1. 新建 `Tranbok.Tools.Plugin.YourPlugin` 项目
2. 引用 `Tranbok.Tools.Plugin.Core`，按需引用 `Core` / `Designer`
3. 实现插件元数据与 `IPlugin`
4. 如需 UI，提供主视图
5. 选择接入方式：
   - 作为内置插件在 `App.axaml.cs` 中注册
   - 构建输出到 `plugins` 目录，由宿主自动发现

---

## 代码阅读入口

如果你要快速理解项目，建议按以下顺序阅读：

1. `Tranbok.Tools.App/App.axaml.cs`
2. `Tranbok.Tools.App/ViewModels/MainViewModel.cs`
3. `Tranbok.Tools.Plugin.Core/Abstractions/IPlugin.cs`
4. `Tranbok.Tools.Plugin.Core/Base/BasePlugin.cs`
5. `Tranbok.Tools.Core/Services/PluginDiscoveryService.cs`
6. `Tranbok.Tools.Plugin.Settings/ViewModels/SettingsViewModel.cs`
7. `Tranbok.Tools.Plugin.PluginManager/ViewModels/PluginManagerViewModel.cs`
8. `Tranbok.Tools.Plugin.Migration/ViewModels/MigrationViewModel.cs`
9. `Tranbok.Tools.Plugin.Migration/Services/MigrationService.cs`

---

## 当前状态

当前仓库已经具备一个可继续扩展的桌面工具平台基础：

- 宿主壳层完整
- 插件目录机制可用
- 公共 Designer 层较完整
- 设置与插件管理功能已可使用
- EF Core 迁移插件已具备较完整业务流程

其中最成熟的业务模块是：

- `Tranbok.Tools.Plugin.Migration`

---

## 许可证

本项目许可证以仓库中的 `LICENSE` 文件为准。
