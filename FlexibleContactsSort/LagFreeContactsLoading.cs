using Elements.Assets;
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
        private const int ContactsPerUpdate = 8;

        public override bool CanBeDisabled => true;

        internal static bool AllowSorting { get; private set; } = true;

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

                var segments = contactsSortInfo.Length / ContactsPerUpdate;
                for (var segment = 0; segment < segments; ++segment)
                {
                    for (var i = 0; i < ContactsPerUpdate; ++i)
                        contactsDialog.UpdateContactItem(contactsSortInfo[(ContactsPerUpdate * segment) + i].contactData);

                    await default(NextUpdate);
                }

                for (var i = segments * ContactsPerUpdate; i < contactsSortInfo.Length; ++i)
                    contactsDialog.UpdateContactItem(contactsSortInfo[i].contactData);

                await default(NextUpdate);

                AllowSorting = true;
            });

        private static void AddSearchResult(ContactsDialog contactsDialog, SkyFrost.Base.User user)
        {
            if (contactsDialog._contactItems.ContainsKey(user.Id)
                || contactsDialog._searchResultItems.Any(item => item.Contact.ContactUserId == user.Id)
                || user.Id == contactsDialog.Cloud.CurrentUserID)
                return;

            var contactItem = contactsDialog.AddContactItem();
            contactItem.Update(user);
            contactsDialog._searchResultItems.Add(contactItem);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(ContactsDialog.OnAttach))]
        private static IEnumerable<CodeInstruction> OnAttachTranspiler(IEnumerable<CodeInstruction> instructionsEnumerable)
        {
            if (!Enabled)
                return instructionsEnumerable;

            var foreachContactDataMethod = AccessTools.Method(typeof(ContactManager), nameof(ContactManager.ForeachContactData));
            var addContactItemsMethod = AccessTools.Method(typeof(LagFreeContactsLoading), nameof(AddContactItems));

            var instructions = instructionsEnumerable.ToList();

            var callIndex = instructions.FindIndex(instruction => instruction.Calls(foreachContactDataMethod));

            instructions[callIndex] = new CodeInstruction(OpCodes.Call, addContactItemsMethod);
            instructions.RemoveRange(callIndex - 6, 6);

            return instructions;
        }

        private static bool RemoveItem(ContactItem contact, string? searchTerm, bool clear)
        {
            if (clear || !(contact.Username.StartsWith(searchTerm)
                || contact.AlternateNames.Any(name => name.StartsWith(searchTerm, StringComparison.InvariantCulture))))
            {
                contact.Slot.Destroy();
                return true;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ContactsDialog.SearchTextChanged))]
        private static bool SearchTextChangedPrefix(ContactsDialog __instance, TextEditor editor)
        {
            if (!Enabled)
                return true;

            __instance.StartTask(async () =>
            {
                var searchTerm = editor.TargetString?.Trim().ToLower();
                var clear = string.IsNullOrWhiteSpace(searchTerm);

                var segments = __instance._searchResultItems.Count / ContactsPerUpdate;

                for (var i = __instance._searchResultItems.Count - 1; i >= segments * ContactsPerUpdate; --i)
                {
                    if (RemoveItem(__instance._searchResultItems[i], searchTerm, clear))
                        __instance._searchResultItems.RemoveAt(i);
                }

                for (var segment = segments - 1; segment >= 0; --segment)
                {
                    await default(NextUpdate);

                    for (var i = ContactsPerUpdate - 1; i >= 0; --i)
                    {
                        var x = (ContactsPerUpdate * segment) + i;

                        if (RemoveItem(__instance._searchResultItems[x], searchTerm, clear))
                            __instance._searchResultItems.RemoveAt(x);
                    }
                }

                await default(NextUpdate);

                foreach (var contactItem in __instance._contactItems)
                {
                    contactItem.Value.Slot.ActiveSelf = clear
                        || contactItem.Value.Username.StartsWith(searchTerm, StringComparison.InvariantCultureIgnoreCase)
                        || contactItem.Value.AlternateNames.Any(name => name.StartsWith(searchTerm, StringComparison.InvariantCulture));
                }
            });

            __instance.globalSearchTimer = 0.5;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ContactsDialog.SetSearchResults))]
        private static bool SetSearchResultsPrefix(ContactsDialog __instance, List<SkyFrost.Base.User> users)
        {
            if (!Enabled)
                return true;

            __instance.StartTask(async () =>
            {
                var segments = users.Count / ContactsPerUpdate;

                for (var segment = 0; segment < segments; ++segment)
                {
                    for (var i = 0; i < ContactsPerUpdate; ++i)
                        AddSearchResult(__instance, users[(ContactsPerUpdate * segment) + i]);

                    await default(NextUpdate);
                }

                for (var i = segments * ContactsPerUpdate; i < users.Count; ++i)
                    AddSearchResult(__instance, users[i]);

                await default(NextUpdate);

                __instance.sortList = true;
            });

            return false;
        }
    }
}