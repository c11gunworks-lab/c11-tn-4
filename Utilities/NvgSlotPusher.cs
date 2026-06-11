using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;

namespace c11_tn_4.Utilities;[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 3)]
public class NvgSlotPusher(
    ILogger<NvgSlotPusher> logger,
    DatabaseService databaseService
) : IOnLoad
{
    public Task OnLoad()
    {
        var items = databaseService.GetItems();

        // 1. DEFINE MULTIPLE BASEGAME MOUNTS
        var mountIds = new List<MongoId>()
        {
            "5a16b8a9fcdbcb00165aa6ca", // e.g., Norotos Titanium Advanced Tactical Mount
            "689dbded6c7e684817080c29",
            "689b8883b49f27df1c0873f8"
        };

        // 2. DEFINE MULTIPLE CUSTOM NVGS
        var nvgIds = new List<MongoId>()
        {
            "69e3d5a8dffbd50412ff1889", // Your Custom NVG
            "69e3d5a9609333ebadff188a",
            // "another_nvg_id_1",
            // "another_nvg_id_2",
        };

        // 3. DEFINE BLACKLISTED MOUNTS (Specific mounts your NVGs should NOT attach to)
        var blacklistedMountIds = new List<MongoId>()
        {
            
                "5c0695860db834001b735461",
                "5a16b93dfcdbcbcae6687261",
                "5c11046cd174af02a012e42b"
            
        };

        int mountsUpdated = 0;
        int nvgsUpdated = 0;

        // 4. PUSH NVGS TO ALLOWED MOUNTS
        foreach (var mountId in mountIds)
        {
            if (items.TryGetValue(mountId, out var mountItem))
            {
                // Push custom NVGs to the mount's 1st slot
                ModifySlotFilters(mountItem, 0, 0, nvgIds);
                mountsUpdated++;
            }
            else
            {
                logger.LogWarning($"[c11-tn-4] Mount ID {mountId} not found in database.");
            }
        }

        // 5. APPLY BLACKLIST TO CUSTOM NVGS
        if (blacklistedMountIds.Count > 0)
        {
            foreach (var nvgId in nvgIds)
            {
                if (items.TryGetValue(nvgId, out var nvgItem) && nvgItem.Properties != null)
                {
                    // Grab existing conflicts or create a new set
                    var conflictList = new HashSet<MongoId>(nvgItem.Properties.ConflictingItems ?? new HashSet<MongoId>());
                    
                    // Add the blacklisted mounts to the conflict list
                    conflictList.UnionWith(blacklistedMountIds);
                    
                    // Save it back to the item
                    nvgItem.Properties.ConflictingItems = conflictList;
                    nvgsUpdated++;
                }
            }
        }

        logger.LogDebug($"[c11-tn-4] Successfully added {nvgIds.Count} NVG(s) to {mountsUpdated} basegame mount(s).");
        if (blacklistedMountIds.Count > 0)
        {
            logger.LogDebug($"[c11-tn-4] Applied {blacklistedMountIds.Count} blacklisted conflict(s) to {nvgsUpdated} NVG(s).");
        }

        return Task.CompletedTask;
    }

    // --- Helpers ---
    private void ModifySlotFilters(TemplateItem item, int slotIndex, int filterIndex, List<MongoId> ids)
    {
        // Safely get slots
        var slots = item.Properties?.Slots?.ToArray();
        if (slots == null || slotIndex < 0 || slotIndex >= slots.Length)
            return;

        var slot = slots[slotIndex];
        
        // Safely get filters
        var filters = slot.Properties?.Filters?.ToArray();
        if (filters == null || filterIndex < 0 || filterIndex >= filters.Length)
            return;

        var filter = filters[filterIndex];
        
        // Ensure HashSet exists and push IDs
        if (filter.Filter == null) 
            filter.Filter = new HashSet<MongoId>();
            
        filter.Filter.UnionWith(ids);
    }
}