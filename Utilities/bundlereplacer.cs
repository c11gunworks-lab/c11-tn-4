using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common; 
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;

namespace unpractical_tactical.Resources.Utilities;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 5)]
public class BundleReplacer(
    ILogger<BundleReplacer> logger,
    DatabaseService databaseService
) : IOnLoad
{
    public Task OnLoad()
    {
        // ---------------------------------------------------------------------
        // CONFIGURATION
        // new("Base_Game_Item_ID", "Your_New_Bundle_Path_From_Bundles_Json")
        // ---------------------------------------------------------------------
        var replacements = new List<ReplacementRequest>()
        {
            // new("5447a9cd4bdc2dbd208b4567", "assets/content/unpractical_tactical/weapons/m4a1/m4a1_custom.bundle"),

            // new("5ae30bad5acfc400185c2dc4", "assets/content/unpractical_tactical/mods/sights/carry_handle_custom.bundle"),
        };

        // ---------------------------------------------------------------------
        // EXECUTION
        // ---------------------------------------------------------------------
        
        // SPT 4.0: GetItems returns Dictionary<MongoId, TemplateItem>
        var items = databaseService.GetItems();

        foreach (var req in replacements)
        {
            ReplaceBundlePath(items, req);
        }

        return Task.CompletedTask;
    }

    private void ReplaceBundlePath(Dictionary<MongoId, TemplateItem> items, ReplacementRequest request)
    {
        // Convert the string ID from our config to a MongoId for the dictionary lookup
        if (items.TryGetValue(new MongoId(request.ItemId), out var item))
        {
            // Safety check: Ensure the item actually has a Prefab property to modify
            if (item.Properties?.Prefab != null)
            {
                // Overwrite the path
                item.Properties.Prefab.Path = request.NewBundlePath;
                logger.LogDebug($"[UPT] Replaced bundle for {item.Name} ({request.ItemId})");
            }
            else
            {
                logger.LogError($"[UPT] Item {request.ItemId} found, but it has no Prefab properties.");
            }
        }
        else
        {
            logger.LogError($"[UPT] Failed to find item {request.ItemId} to replace bundle.");
        }
    }

    // Helper Record
    private record ReplacementRequest(string ItemId, string NewBundlePath);
}