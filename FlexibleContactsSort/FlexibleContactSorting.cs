using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader.Resonite;
using SkyFrost.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FlexibleContactsSort
{
    [HarmonyPatch(typeof(ContactsDialog))]
    [HarmonyPatchCategory(nameof(FlexibleContactSorting))]
    internal sealed class FlexibleContactSorting : ConfiguredResoniteMonkey<FlexibleContactSorting, ContactsSortingConfig>
    {
        private static readonly ConditionalWeakTable<ContactItem, ActivityTracker> _activityTrackersByContact = [];
        private static readonly Dictionary<ContactItem, string> _contactIds = [];

        private static readonly LocaleString _pinString = Mod.GetLocaleString("Pin");
        private static readonly LocaleString _unpinString = Mod.GetLocaleString("Unpin");

        public override bool CanBeDisabled => true;

        internal static int Compare((ContactData?, bool) contactSortInfo1, (ContactData?, bool) contactSortInfo2)
        {
            var contact1 = contactSortInfo1.Item1?.Contact;
            var contact2 = contactSortInfo2.Item1?.Contact;

            // nulls go last, no need to build score
            if (contact1 is null)
                return contact2 is null ? 0 : 1;

            if (contact2 is null)
                return -1;

            var score1 = CalculateOrderScore(contactSortInfo1!);
            var score2 = CalculateOrderScore(contactSortInfo2!);

            // Compare Username with UserId appended as tie-breaker
            var alphabetical = string.Compare($"{contact1.ContactUsername}\0{contact1.ContactUserId}", $"{contact2.ContactUsername}\0{contact2.ContactUserId}", StringComparison.InvariantCultureIgnoreCase);
            score1 += alphabetical * ConfigSection.AlphabeticPriority;
            score2 -= alphabetical * ConfigSection.AlphabeticPriority;

            return score1 - score2;
        }

        private static int CalculateOrderScore((ContactData, bool) contactSortInfo)
        {
            var contactData = contactSortInfo.Item1;
            var contact = contactData.Contact;
            var pinned = ConfigSection.PinnedContacts.Contains(contact.ContactUserId);

            var score = contact.ContactUserId == Engine.Current.Cloud.Platform.AppUserId ? -1_002_000_000 : 0;
            score = contact.IsSelfContact ? -1_001_000_000 : score;

            score += contactSortInfo.Item2 ? -120_000_000 : 0;
            score += pinned ? -110_000_000 : 0;

            score += IsOfflineContact(contactData) && !IsHeadlessHost(contactData) ? 100_000_000 : 0; // offline friends before results
            score += contact.ContactStatus == ContactStatus.SearchResult ? 105_000_000 : 0; // non-contact search results always at the end
            score += contact.IsPartiallyMigrated ? 110_000_000 : 0; // partially migrated to the end

            score += (IsOutgoingRequest(contact) ? 1 : 0) * ConfigSection.OutgoingContactRequestPriority;
            score += (IsIncomingRequest(contact) ? 1 : 0) * ConfigSection.IncomingContactRequestPriority;

            score += (IsInJoinableSession(contactData) ? 1 : 0) * ConfigSection.JoinablePriority;
            score += (IsHeadlessHost(contactData) ? 1 : 0) * ConfigSection.HeadlessPriority;
            score += GetOnlineStatusOrder(contactData) * ConfigSection.OnlineStatusPriority;

            return score;
        }

        private static int Compare(Slot slot1, Slot slot2)
        {
            if (slot1.ActiveSelf ^ slot2.ActiveSelf)
                return slot1.ActiveSelf ? -1 : 1;

            var contactItem1 = slot1.GetComponent<ContactItem>();
            var contactItem2 = slot2.GetComponent<ContactItem>();

            return Compare((contactItem1?.Data, contactItem1?.HasMessages ?? false),
                    (contactItem2?.Data, contactItem2?.HasMessages ?? false));
        }

        private static int GetOnlineStatusOrder(ContactData contactData)
        {
            return (contactData?.CurrentStatus.OnlineStatus).GetValueOrDefault() switch
            {
                OnlineStatus.Sociable => 1,
                OnlineStatus.Online => 2,
                OnlineStatus.Away => 3,
                OnlineStatus.Busy => 4,
                _ => 5,
            };
        }

        private static bool HasUnreadMessages(ContactItem contactItem)
        {
            var readMessageTracker = _activityTrackersByContact.GetOrCreateValue(contactItem);

            if (contactItem.HasMessages)
            {
                readMessageTracker.HasMessages = true;
                return true;
            }

            readMessageTracker.HasMessages = false;

            return readMessageTracker.SecondsSinceRead < ConfigSection.ReadMessageCooldown;
        }

        private static bool IsHeadlessHost(ContactData contactData)
            => contactData?.CurrentStatus.SessionType == UserSessionType.Headless;

        private static bool IsIncomingRequest(Contact contact)
            => contact.ContactStatus == ContactStatus.Requested;

        private static bool IsInJoinableSession(ContactData contactData)
            => contactData?.CurrentSessionInfo is not null
            && !IsHeadlessHost(contactData);

        private static bool IsOfflineContact(ContactData contactData)
        {
            return contactData.Contact.ContactStatus == ContactStatus.Accepted
                && contactData.Contact.IsAccepted && contactData.CurrentStatus.OnlineStatus is OnlineStatus.Offline or OnlineStatus.Invisible;
        }

        private static bool IsOutgoingRequest(Contact contact)
            => contact.ContactStatus == ContactStatus.Accepted && !contact.IsAccepted;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(ContactsDialog.OnCommonUpdate))]
        private static void OnCommonUpdatePostfix(ContactsDialog __instance, bool __state)
        {
            if (!__state || !LagFreeContactsLoading.AllowSorting)
                return;

            if (string.IsNullOrWhiteSpace(__instance._searchBar.Target.Text.Content))
            {
                if (ConfigSection.HideOffline)
                {
                    foreach (var contactSlot in __instance._listRoot.Target.Children)
                    {
                        var contactItem = contactSlot.GetComponent<ContactItem>();

                        if (contactItem?.Data is null)
                            continue;

                        var activityTracker = _activityTrackersByContact.GetOrCreateValue(contactItem);
                        var isOffline = activityTracker.IsOffline = IsOfflineContact(contactItem.Data);

                        contactSlot.ActiveSelf = !isOffline || activityTracker.SecondsSinceOffline < ConfigSection.OfflineCooldown
                            || (ConfigSection.KeepPinnedOffline && ConfigSection.PinnedContacts.Contains(contactItem.Data.Contact.ContactUserId));
                    }
                }
                else
                {
                    foreach (var contactSlot in __instance._listRoot.Target.Children)
                        contactSlot.ActiveSelf = true;
                }
            }

            // Sort only if Resonite would have sorted (but we prevented it)
            __instance._listRoot.Target.SortChildren(Compare);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ContactsDialog.OnCommonUpdate))]
        private static void OnCommonUpdatePrefix(ContactsDialog __instance, out bool __state)
        {
            if (!Enabled)
            {
                __state = false;
                return;
            }

            // steal the sortList bool's value, and force it to false from Resonite's perspective
            __state = __instance.sortList;
            __instance.sortList &= !Enabled;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(ContactsDialog.UpdateSelectedContactUI))]
        private static void UpdateSelectedContactPostfix(ContactsDialog __instance, UIBuilder ___actionsUi)
        {
            if (__instance.SelectedContact is null || __instance.SelectedContactId == __instance.Cloud.Platform.AppUserId || __instance.SelectedContact.IsSelfContact)
                return;

            var pinButton = ___actionsUi.Button(ConfigSection.PinnedContacts.Contains(__instance.SelectedContactId) ? _unpinString : _pinString);
            pinButton.LocalPressed += (button, data) =>
            {
                if (ConfigSection.PinnedContacts.Add(__instance.SelectedContactId))
                {
                    ((Text)pinButton.LabelTextField.Parent).LocaleContent = _unpinString;
                    return;
                }

                ConfigSection.PinnedContacts.Remove(__instance.SelectedContactId);
                ((Text)pinButton.LabelTextField.Parent).LocaleContent = _pinString;
            };
        }

        private sealed class ActivityTracker
        {
            private bool _hasMessages;
            private bool _isOffline = true;

            private DateTime _offlineTime = DateTime.MinValue;
            private DateTime _readTime = DateTime.MinValue;

            public bool HasMessages
            {
                get => _hasMessages;
                set
                {
                    if (_hasMessages && !value)
                        _readTime = DateTime.UtcNow;

                    _hasMessages = value;
                }
            }

            public bool IsOffline
            {
                get => _isOffline;
                set
                {
                    if (_isOffline && !value)
                        _offlineTime = DateTime.UtcNow;

                    _isOffline = value;
                }
            }

            public double SecondsSinceOffline => (DateTime.UtcNow - _offlineTime).TotalSeconds;

            public double SecondsSinceRead => (DateTime.UtcNow - _readTime).TotalSeconds;
        }
    }
}