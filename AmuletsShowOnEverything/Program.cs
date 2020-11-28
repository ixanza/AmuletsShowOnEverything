using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Mutagen.Bethesda.FormKeys.SkyrimSE;

namespace KhajiitEarsShow
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return SynthesisPipeline.Instance.Patch<ISkyrimMod, ISkyrimModGetter>(
                args: args,
                patcher: RunPatch,
                new UserPreferences()
                {
                    ActionsForEmptyArgs = new RunDefaultPatcher()
                    {
                        IdentifyingModKey = "AmuletsShowOnEverything.esp",
                        TargetRelease = GameRelease.SkyrimSE
                    }
                }
            );
        }

        public static void RunPatch(SynthesisState<ISkyrimMod, ISkyrimModGetter> state)
        {   
            foreach (var armorAddon in state.LoadOrder.PriorityOrder.WinningOverrides<IArmorAddonGetter>())
            {
                try
                {

                    if (armorAddon.BodyTemplate == null || !(armorAddon.BodyTemplate.FirstPersonFlags.HasFlag(BipedObjectFlag.Body) && armorAddon.BodyTemplate.FirstPersonFlags.HasFlag(BipedObjectFlag.Amulet))) continue;

                    // Ignore things that are probably skins like Miraak's hidden skin
                    if (armorAddon.BodyTemplate.FirstPersonFlags.HasFlag(BipedObjectFlag.Head) &&
                        armorAddon.BodyTemplate.FirstPersonFlags.HasFlag(BipedObjectFlag.Hair) &&
                        armorAddon.BodyTemplate.FirstPersonFlags.HasFlag(BipedObjectFlag.Hands)) continue;

                    // Ignore Naked Torso and Child Clothes
                    if (armorAddon.EditorID == null || armorAddon.EditorID.Contains("Naked") || armorAddon.EditorID.Contains("Child")) continue;

                    var modifiedArmorAddon = state.PatchMod.ArmorAddons.GetOrAddAsOverride(armorAddon);

                    if (modifiedArmorAddon.BodyTemplate == null)
                    {
                        modifiedArmorAddon.BodyTemplate = new BodyTemplate();
                    }

                    modifiedArmorAddon.BodyTemplate.FirstPersonFlags &= ~BipedObjectFlag.Amulet;
                }
                catch (Exception ex)
                {
                    throw RecordException.Factory(ex, armorAddon);
                }
            }
        }
    }
}
