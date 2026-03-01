using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;

namespace c11_tn_4.Utilities;
// Shoutout EpicRangeTime for letting me use his code, and CJ and Drakia for creating said code

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 3)]
public class GlockSlideLogic(
    ILogger<GlockSlideLogic> logger,
    DatabaseService databaseService
) : IOnLoad
{
    public Task OnLoad()
    {
        EditFilters();
        return Task.CompletedTask;
    }

    private void EditFilters()
    {
        var items = databaseService.GetItems();


        // =====================================================================
        // SECTION 2: VZOR4 REMOTE LOGIC
        // =====================================================================

        // 1. DEFINE YOUR IDs
        MongoId vzorRemoteSwitchId = "696000ef0493e34c01446f6a"; 

        // 2. WHITELIST: Handguards allowed to use this remote
        var allowedHandguards = new HashSet<string>()
        {
            "ALLOWED_HANDGUARD_ID_1", // <--- REPLACE WITH REAL IDs
            "ALLOWED_HANDGUARD_ID_2",
            "ALLOWED_HANDGUARD_ID_3"
        };

        // 3. CATEGORY: We scan all "Foregrips/Handguards"
        var handguardCategory = new List<string>()
        {
            "55818a104bdc2db9688b4569" // Base Class: Foregrip (Used for almost all handguards)
        };

        // 4. GENERATE CONFLICTS
        // We find the Remote Switch item, then blacklist EVERY handguard NOT in the list above.
        if (items.TryGetValue(vzorRemoteSwitchId, out var remoteItem) && remoteItem.Properties != null)
        {
            var remoteConflicts = new HashSet<MongoId>(remoteItem.Properties.ConflictingItems ?? new HashSet<MongoId>());

            foreach (var kvp in items)
            {
                var item = kvp.Value;

                // Is this item a Handguard?
                if (IsChildOfAny(item, handguardCategory, items))
                {
                    // Is it NOT in our allowed list?
                    if (!allowedHandguards.Contains(item.Id.ToString()))
                    {
                        // Ban it (The remote cannot be installed if this handguard is on the gun)
                        remoteConflicts.Add(item.Id);
                    }
                }
            }

            remoteItem.Properties.ConflictingItems = remoteConflicts;
            logger.LogDebug($"[C11] Vzor Remote Conflicts Updated. Count: {remoteConflicts.Count}");
        }
        else
        {
            // Helpful warning if you forgot to put the ID in
            if (vzorRemoteSwitchId != "696000ef0493e34c01446f6a")
            {
                logger.LogError($"[C11] Could not find Vzor Remote Switch with ID: {vzorRemoteSwitchId}");
            }
        }
    }

    // --- Helpers ---
    private bool IsChildOfAny(TemplateItem item, List<string> parentIds, Dictionary<MongoId, TemplateItem> allItems)
    {
        foreach (var parentId in parentIds)
            if (IsChildOf(item, parentId, allItems)) return true;
        return false;
    }

    private bool IsChildOf(TemplateItem item, string targetParentId, Dictionary<MongoId, TemplateItem> allItems)
    {
        if (string.IsNullOrEmpty(item.Parent)) return false;
        if (item.Parent == targetParentId) return true;

        if (allItems.TryGetValue(new MongoId(item.Parent), out var parentItem))
            return IsChildOf(parentItem, targetParentId, allItems);
        return false;
    }

    private void ModifySlotFilters(TemplateItem item, int slotIndex, int filterIndex, List<MongoId> ids)
    {
        var slot = GetSlotAtIndex(item, slotIndex);
        var filter = GetSlotFilterAtIndex(slot, filterIndex);
        if (filter.Filter == null) filter.Filter = new HashSet<MongoId>();
        filter.Filter.UnionWith(ids);
    }

    private Slot GetSlotAtIndex(TemplateItem item, int index, bool isCartridge = false)
    {
        var slots = isCartridge ? item.Properties?.Cartridges?.ToArray() : item.Properties?.Slots?.ToArray();

        if (index >= 0 && index < slots?.Length)
        {
            return slots[index];
        }

        throw new IndexOutOfRangeException($"Index on item slot property `{item.Name}` is out of range");
    }

    private SlotFilter GetSlotFilterAtIndex(Slot slot, int index)
    {  
        var slotFilter = slot.Properties?.Filters?.ToArray() ?? [];

        if (index >= 0 && index < slotFilter.Length)
        {
            return slotFilter[index];
        }

        throw new IndexOutOfRangeException($"Index on slot property `{slot.Name}` is out of range");
    }
}