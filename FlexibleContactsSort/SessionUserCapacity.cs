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
    [HarmonyPatchCategory(nameof(SessionUserCapacity))]
    [HarmonyPatch(typeof(SessionItem), nameof(SessionItem.Update))]
    internal sealed class SessionUserCapacity : ConfiguredResoniteMonkey<SessionUserCapacity, SessionCapacityConfig>
    {
        public override bool CanBeDisabled => true;

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        private static void Postfix(SessionItem __instance, SessionInfo session)
        {
            if (!Enabled)
                return;

            var format = "{0} ({1})";

            if (ConfigSection.ShowUserCapacityInSessionList)
                format += " / {2}";

            if (ConfigSection.ShowUsageLevelWithColorGradient)
            {
                var usage = (float)session.JoinedUsers / session.MaximumUsers;

                var value = usage > 1 || !usage.IsValid() ?
                    new ColorHSV(ConfigSection.FullSessionColor).v - .2f
                    : MathX.Lerp(new ColorHSV(ConfigSection.EmptySessionColor).v, new ColorHSV(ConfigSection.FullSessionColor).v, usage);

                var color = MathX.Lerp(ConfigSection.EmptySessionColor, ConfigSection.FullSessionColor, usage).SetValue(value);

                format = $"<color={color.ToHexString()}>{format}</color>";
            }

            format = "Users: " + format;
            __instance._userCount.Target.Content.Value = string.Format(format, session.ActiveUsers, session.JoinedUsers, session.MaximumUsers);
        }
    }
}