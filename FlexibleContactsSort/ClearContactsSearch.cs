using Elements.Core;
using FrooxEngine.UIX;
using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyLoader.Resonite;
using MonkeyLoader.Patching;

namespace FlexibleContactsSort
{
    [HarmonyPatchCategory(nameof(ClearContactsSearch))]
    [HarmonyPatch(typeof(ContactsDialog), nameof(ContactsDialog.OnAttach))]
    internal sealed class ClearContactsSearch : ResoniteMonkey<ClearContactsSearch>
    {
        private static void Postfix(ContactsDialog __instance)
        {
            var searchEditor = __instance._searchBar.Target;

            var ui = new UIBuilder(searchEditor.Slot.Parent);
            RadiantUI_Constants.SetupDefaultStyle(ui);

            ui.VerticalFooter(32, out var footer, out var content);

            __instance._searchBar.Target.Slot.Parent = content.Slot;
            content.OffsetMax.Value += new float2(-4, 0);

            ui.ForceNext = footer;
            ui.Button("∅").LocalPressed += (sender, args) =>
            {
                searchEditor.Text.Content.Value = "";
                searchEditor.Editor.Target.ForceEditingChangedEvent();
            };
        }
    }
}