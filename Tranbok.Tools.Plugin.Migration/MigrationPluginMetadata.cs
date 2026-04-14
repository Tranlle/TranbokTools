using Tranbok.Tools.Plugin.Core.Base;
using Tranbok.Tools.Plugin.Core.Models;

namespace Tranbok.Tools.Plugin.Migration;

public sealed class MigrationPluginMetadata : PluginBaseMetadata
{
    public static MigrationPluginMetadata Instance { get; } = new();

    public override string Id => "tranbok.migration";

    public override string Name => "数据库迁移";

    public override string Version => "1.0.1";

    public override string Description => "管理 EF Core 迁移文件：新增、编辑、执行、撤回，支持 SqlServer / PostgreSQL / MySQL";

    public override string Author => "Tranbok";

    public override string Icon => "Database";

    public override string Tags => "database,efcore,migration";

    public override IReadOnlyList<PluginVariableDefinition> VariableDefinitions =>
    [
        new PluginVariableDefinition(
            Key: "TRANBOK_DB_CONNECTION",
            DefaultValue: "",
            DisplayName: "数据库连接字符串",
            Description: "执行迁移时传递给 dotnet ef 命令的数据库连接字符串。")
    ];
}
