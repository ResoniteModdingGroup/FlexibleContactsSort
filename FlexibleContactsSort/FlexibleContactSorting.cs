using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite;
using SkyFrost.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlexibleContactsSort
{
    [HarmonyPatch(typeof(ContactsDialog))]
    [HarmonyPatchCategory(nameof(FlexibleContactSorting))]
    internal sealed class FlexibleContactSorting : ConfiguredResoniteMonkey<FlexibleContactSorting, ContactsSortingConfig>
    {
        private static readonly Dictionary<ContactItem, string> _contactIds = new();
        public override string Name => "FlexibleContactsSort";

        protected override IEnumerable<IFeaturePatch> GetFeaturePatches() => Enumerable.Empty<IFeaturePatch>();

        private static int CalculateOrderScore(ContactItem contactItem)
        {
            var contact = contactItem.Contact;
            var pinned = ConfigSection.PinnedContacts.Contains(contact.ContactUserId);

            var score = contact.ContactUserId == contactItem.Cloud.Platform.AppUserId ? -1_002_000_000 : 0;
            score = contact.IsSelfContact ? -1_001_000_000 : score;

            score += HasUnreadMessages(contactItem) ? -120_000_000 : 0;
            score += pinned ? -110_000_000 : 0;

            score += IsOfflineContact(contactItem) && !IsHeadlessHost(contactItem) ? 100_000_000 : 0; // offline friends before results
            score += contact.ContactStatus == ContactStatus.SearchResult ? 105_000_000 : 0; // non-contact search results always at the end
            score += contact.IsPartiallyMigrated ? 110_000_000 : 0; // partially migrated to the end

            score += (IsOutgoingRequest(contact) ? 1 : 0) * ConfigSection.OutgoingContactRequestPriority;
            score += (IsIncomingRequest(contact) ? 1 : 0) * ConfigSection.IncomingContactRequestPriority;

            score += (IsInJoinableSession(contactItem) ? 1 : 0) * ConfigSection.JoinablePriority;
            score += (IsHeadlessHost(contactItem) ? 1 : 0) * ConfigSection.HeadlessPriority;
            score += GetOnlineStatusOrder(contactItem) * ConfigSection.OnlineStatusPriority;

            if (_contactIds.TryGetValue(contactItem, out var oldId) && oldId != contact.ContactUserId)
            {
                Debug(() => $"ContactItem UserId changed from {oldId} to {contact.ContactUserId}!");
            }

            return score;
        }

        private static int GetOnlineStatusOrder(ContactItem item)
        {
            return (item.Data?.CurrentStatus.OnlineStatus).GetValueOrDefault() switch
            {
                OnlineStatus.Online => 1,
                OnlineStatus.Away => 2,
                OnlineStatus.Busy => 3,
                _ => 4,
            };
        }

        private static bool HasUnreadMessages(ContactItem contactItem) => contactItem.HasMessages;

        private static bool IsHeadlessHost(ContactItem contactItem)
            => contactItem.Data?.CurrentStatus.SessionType == UserSessionType.Headless;

        private static bool IsIncomingRequest(Contact contact)
            => contact.ContactStatus == ContactStatus.Requested;

        private static bool IsInJoinableSession(ContactItem contactItem)
            => contactItem.Data?.CurrentSessionInfo is SessionInfo session
            && session.CompatibilityHash == Engine.Current.CompatibilityHash
            && !IsHeadlessHost(contactItem);

        private static bool IsOfflineContact(ContactItem contactItem)
        {
            var status = contactItem.Data?.CurrentStatus.OnlineStatus ?? OnlineStatus.Offline;

            return (contactItem.Contact.ContactStatus == ContactStatus.Accepted
                && contactItem.Contact.IsAccepted && status == OnlineStatus.Offline)
                || status == OnlineStatus.Invisible;
        }

        private static bool IsOutgoingRequest(Contact contact)
            => contact.ContactStatus == ContactStatus.Accepted && !contact.IsAccepted;

        [HarmonyPostfix]
        [HarmonyPatch("OnCommonUpdate")]
        private static void Postfix(bool __state, SyncRef<Slot> ____listRoot)
        {
            // Sort only if Resonite would have sorted (but we prevented it)
            if (!__state)
                return;

            ____listRoot.Target.SortChildren((slot1, slot2) =>
            {
                var contactItem1 = slot1.GetComponent<ContactItem>();
                var contactItem2 = slot2.GetComponent<ContactItem>();
                var contact1 = contactItem1?.Contact;
                var contact2 = contactItem2?.Contact;

                // nulls go last, no need to build score
                if (contact1 is null)
                    return contact2 is null ? 0 : 1;

                if (contact2 is null)
                    return -1;

                var score1 = CalculateOrderScore(contactItem1!);
                var score2 = CalculateOrderScore(contactItem2!);

                // Compare Username with UserId appended as tie-breaker
                var alphabetical = string.Compare($"{contact1.ContactUsername}\0{contact1.ContactUserId}", $"{contact2.ContactUsername}\0{contact2.ContactUserId}", StringComparison.InvariantCultureIgnoreCase);
                score1 += alphabetical * ConfigSection.AlphabeticPriority;
                score2 -= alphabetical * ConfigSection.AlphabeticPriority;

                return score1 - score2;
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ContactsDialog.OnCommonUpdate))]
        private static void Prefix(ref bool ___sortList, out bool __state)
        {
            // steal the sortList bool's value, and force it to false from Resonite's perspective
            __state = ___sortList;
            ___sortList = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch("UpdateSelectedContact")]
        private static void UpdateSelectedContactPostfix(ContactsDialog __instance, UIBuilder ___actionsUi)
        {
            if (__instance.SelectedContact is null || __instance.SelectedContactId == __instance.Cloud.Platform.AppUserId || __instance.SelectedContact.IsSelfContact)
                return;

            var pinButton = ___actionsUi.Button(ConfigSection.PinnedContacts.Contains(__instance.SelectedContactId) ? "Unpin Contact" : "Pin Contact");
            pinButton.LocalPressed += (button, data) =>
            {
                if (ConfigSection.PinnedContacts.Add(__instance.SelectedContactId))
                {
                    pinButton.LabelText = "Unpin Contact";
                    return;
                }

                ConfigSection.PinnedContacts.Remove(__instance.SelectedContactId);
                pinButton.LabelText = "Pin Contact";
            };
        }
    }
}