using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite;
using SkyFrost.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexibleContactsSort
{
    [HarmonyPatchCategory(nameof(ExtraContactColoring))]
    [HarmonyPatch(typeof(LegacyUIStyle), nameof(LegacyUIStyle.GetStatusColor))]
    internal class ExtraContactColoring : ResoniteMonkey<ExtraContactColoring>
    {
        public override string Name { get; } = "ExtraContactColoring";

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        private static void Postfix(Contact contact, ContactData status, bool text, ref colorX __result)
        {
            if (contact.ContactStatus == ContactStatus.Accepted && !contact.IsAccepted)
                __result = text ? RadiantUI_Constants.Hero.YELLOW : RadiantUI_Constants.MidLight.YELLOW;
        }
    }
}