using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite;
using SkyFrost.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace FlexibleContactsSort
{
    [HarmonyPatchCategory(nameof(LagFreeContactsLoading))]
    [HarmonyPatch(typeof(ContactsDialog), nameof(ContactsDialog.OnAttach))]
    internal sealed class LagFreeContactsLoading : ResoniteMonkey<LagFreeContactsLoading>
    {
        internal static bool AllowSorting { get; private set; } = true;

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        private static void AddContactItems(ContactsDialog contactsDialog)
            => contactsDialog.StartTask(async () =>
            {
                AllowSorting = false;

                await default(ToBackground);

                var contacts = new List<ContactData>();
                contactsDialog.Engine.Cloud.Contacts.ForeachContactData(contacts.Add);

                var contactsUnread = contacts.AsParallel().Select(contactData => (contactsDialog.Engine.Cloud.Messages.GetUserMessages(contactData.UserId)?.UnreadCount ?? 0) > 0).ToArray();
                var contactsSortInfo = contacts.Zip(contactsUnread, (contactData, contactUnread) => (contactData, contactUnread)).ToArray();
                Array.Sort(contactsSortInfo, FlexibleContactSorting.Compare);

                await default(ToWorld);

                foreach (var (contactData, _) in contactsSortInfo)
                {
                    contactsDialog.UpdateContactItem(contactData);
                    await default(NextUpdate);
                }

                AllowSorting = true;
            });

        [HarmonyTranspiler]
        [HarmonyPatch("OnAttach")]
        private static IEnumerable<CodeInstruction> OnAttachTranspiler(IEnumerable<CodeInstruction> instructionsEnumerable)
        {
            var foreachContactDataMethod = AccessTools.Method(typeof(ContactManager), nameof(ContactManager.ForeachContactData));
            var addContactItemsMethod = AccessTools.Method(typeof(LagFreeContactsLoading), nameof(AddContactItems));

            var instructions = instructionsEnumerable.ToList();

            var callIndex = instructions.FindIndex(instruction => instruction.Calls(foreachContactDataMethod));

            instructions[callIndex] = new CodeInstruction(OpCodes.Call, addContactItemsMethod);
            instructions.RemoveRange(callIndex - 6, 6);

            return instructions;
        }
    }
}