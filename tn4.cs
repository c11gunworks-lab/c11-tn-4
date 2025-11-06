using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Range = SemanticVersioning.Range;

namespace c11_tn_4;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.c11.truenorth4";
    public override string Name { get; init; } = "True North";
    public override string Author { get; init; } = "C11";
    public override SemanticVersioning.Version Version { get; init; } = new("1.0.0");

    // Allow any 4.0.x 
    public override Range SptVersion { get; init; } = new("^4.0.0");

    public override string License { get; init; } = "MIT";
    public override bool? IsBundleMod { get; init; } = true;

    public override Dictionary<string, Range>? ModDependencies { get; init; } = new()
    {
        { "com.wtt.commonlib", new Range("~2.0.0") }
    };

    public override string? Url { get; init; }
    public override List<string>? Contributors { get; init; }
    public override List<string>? Incompatibilities { get; init; }
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 2)]
public class TrueNorth(
    WTTServerCommonLib.WTTServerCommonLib wttCommon,
    ILogger<TrueNorth> log
) : IOnLoad
{
    public async Task OnLoad()
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Log resource names once while wiring things up
        foreach (var name in assembly.GetManifestResourceNames())
            log.LogDebug("[TrueNorth] Embedded resource: {Res}", name);

        // WTT ingestion
        await wttCommon.CustomItemServiceExtended.CreateCustomItems(assembly);
        await wttCommon.CustomLocaleService.CreateCustomLocales(assembly);

        log.LogInformation("[TrueNorth] WTT data loaded.");
    }
}
