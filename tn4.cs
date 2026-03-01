using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;
using System.Reflection;
using Range = SemanticVersioning.Range;
using WTTServerCommonLib.Models; 

namespace c11_tn_4;

public record ModMetadata : AbstractModMetadata
{
public override string ModGuid { get; init; } = "com.c11.truenorth4";
public override string Name { get; init; } = "True North";
public override string Author { get; init; } = "C11";
public override SemanticVersioning.Version Version { get; init; } = new("2.5.0");
public override Range SptVersion { get; init; } = new("~4.0.10");

public override string License { get; init; } = "MIT";
public override bool? IsBundleMod { get; init; } = true;

public override Dictionary<string, Range>? ModDependencies { get; init; } = new()
{
    { "com.wtt.commonlib", new Range("~2.0.15") },
    { "com.c11.spt22lr", new Range("~1.0.0") },
    { "com.epicrangetime.shaders", new Range("~1.0.1") }
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
    TraderIds.Add("trudy", "699f89c757994beece5cf7e1");

    // WTT ingestion
    await wttCommon.CustomItemServiceExtended.CreateCustomItems(assembly);
    await wttCommon.CustomLocaleService.CreateCustomLocales(assembly);
    await wttCommon.CustomAssortSchemeService.CreateCustomAssortSchemes(assembly);
    await wttCommon.CustomBotLoadoutService.CreateCustomBotLoadouts(assembly);
    await wttCommon.CustomWeaponPresetService.CreateCustomWeaponPresets(assembly);
    wttCommon.CustomRigLayoutService.CreateRigLayouts(assembly);

    // DEBUG: list what the server actually registered
    var layouts = wttCommon.CustomRigLayoutService.GetLayoutManifest();
    log.LogInformation("[True North] Rig layouts registered: {Layouts}", string.Join(", ", layouts));

    log.LogInformation("Welcome to the True North");
    await Task.CompletedTask;
}

}