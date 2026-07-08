using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;
using SkyFrost.Base;

namespace FlexibleContactsSort
{
    [HarmonyPatchCategory(nameof(ExtraContactColoring))]
    [HarmonyPatch(typeof(LegacyUIStyle), nameof(LegacyUIStyle.GetStatusColor))]
    internal sealed class ExtraContactColoring : ResoniteMonkey<ExtraContactColoring>
    {
        public override bool CanBeDisabled => true;

        private static void Postfix(Contact contact, ContactData status, bool text, ref colorX __result)
        {
            if (Enabled && contact.ContactStatus == ContactStatus.Accepted && !contact.IsAccepted)
                __result = text ? RadiantUI_Constants.Hero.YELLOW : RadiantUI_Constants.MidLight.YELLOW;
        }
    }
}