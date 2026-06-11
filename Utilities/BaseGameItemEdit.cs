using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using Microsoft.Extensions.Logging;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;

namespace c11_tn_4.Utilities;

[Injectable(typePriority: OnLoadOrder.PostDBModLoader + 3)]
public class BaseGameItemEdits(
    ILogger<BaseGameItemEdits> log,
    DatabaseService databaseService
):IOnLoad

{    private const string PpshWeaponId = "5ea03f7400685063ec28bfa8";

    
        public Task OnLoad()
        {


            EditFilters();
            log.LogDebug("BaseGameItemEdits Loaded");
            return Task.CompletedTask;
        }

        private void EditFilters()
        {
            var dbItems = databaseService.GetItems();
            foreach (var (id, item) in dbItems)
            {
                switch (id)
                {


                    case "695844d86dcde47c9021a1f1":
                        item.Properties!.ConflictingItems = [];
                        break; //Clear conflicting items on rear am180



                    case "695ffdaedd28a71ea5446f66":
                        item.Properties!.ConflictingItems = [];
                        break; //Clear conflicting items on vzor
                }
            }

            if (dbItems.TryGetValue(PpshWeaponId, out var ppsh) && ppsh?.Properties?.Prefab != null)
            {
                ppsh.Properties.Prefab.Path = "ppsh-weapon.bundle";
                ppsh.Properties.Prefab.Rcid = "";

                log.LogDebug($"[C11-TN-4] Successfully replaced {PpshWeaponId} with wtt ppsh bundle");
            }
            else
            {
                log.LogDebug("[C11-TN-4]  Could not find PPSH object in database!");
            }
        }

        private void ReplaceSlotFilters(TemplateItem item, int slotIndex, int filterIndex, HashSet<MongoId> ids)
        {
            var slot = GetSlotAtIndex(item, slotIndex);
            var filter = GetSlotFilterAtIndex(slot, filterIndex);

            filter.Filter = ids;
        }

        private void ModifySlotFilters(TemplateItem item, int slotIndex, int filterIndex, List<MongoId> ids,
            bool isCartridge = false)
        {
            var slot = GetSlotAtIndex(item, slotIndex, isCartridge);
            var filter = GetSlotFilterAtIndex(slot, filterIndex);

            filter.Filter!.UnionWith(ids);
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
