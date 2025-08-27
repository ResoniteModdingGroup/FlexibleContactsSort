using Elements.Quantity;
using FrooxEngine;
using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexibleContactsSort
{
    internal sealed class ContactsSortingConfig : ConfigSection
    {
        private static readonly ConfigKeyQuantity<int, Time> _cooldownTimeFormat = new(new UnitConfiguration("s", "0", " ", ["m", "s"]), null, 0, int.MaxValue);

        private readonly DefiningConfigKey<int> _alphabeticPriorityKey = new("AlphabeticPriority", "Priority of the contact's name. Set 0 to ignore; negative to invert.", () => 1);
        private readonly DefiningConfigKey<int> _headlessPriorityKey = new("HeadlessPriority", "Priority of the contact being an active headless host. Set 0 to ignore; negative to invert.", () => 100_000);
        private readonly DefiningConfigKey<bool> _hideOffline = new("HideOffline", "Hide offline contacts completely.", () => false);
        private readonly DefiningConfigKey<int> _incomingContactRequestPriorityKey = new("IncomingContactRequestPriority", "Priority of the contact being an incoming request. Set 0 to ignore; negative to invert.", () => -1_000_000);
        private readonly DefiningConfigKey<int> _joinablePriorityKey = new("JoinablePriority", "Priority of the contact being in a session you can join. Set 0 to ignore; negative to invert.", () => -10_000);

        private readonly DefiningConfigKey<bool> _keepPinnedOffline = new("KeepPinnedOffline", "Do not hide pinned contacts, even if they're offline.", () => true);

        private readonly DefiningConfigKey<int> _offlineCooldown = new("OfflineCooldown", "Delay before a contact that has just gone offline is counted as such. Set to 0 to disable.", () => 120)
        {
            _cooldownTimeFormat
        };

        private readonly DefiningConfigKey<int> _onlineStatusPriorityKey = new("OnlineStatusPriority", "Priority of the contact's online status. Set 0 to ignore; negative to invert.", () => 1_000);
        private readonly DefiningConfigKey<int> _outgoingContactRequestPriorityKey = new("OutgoingContactRequestPriority", "Priority of the contact being an outgoing request. Set 0 to ignore; negative to invert.", () => 1_000_000);
        private readonly DefiningConfigKey<HashSet<string>> _pinnedContactsKey = new("PinnedContacts", "List of Contacts to always keep at the top.", () => [], internalAccessOnly: true);

        private readonly DefiningConfigKey<int> _readMessageCooldownKey = new("ReadMessageCooldown", "Delay before a contact with freshly-read messages is counted as such. Set 0 to disable.", () => 120)
        {
            _cooldownTimeFormat
        };

        public int AlphabeticPriority => _alphabeticPriorityKey.GetValue();
        public override string Description => "Contains options for how to sort the Contacts list.";
        public int HeadlessPriority => _headlessPriorityKey.GetValue();
        public bool HideOffline => _hideOffline;
        public override string Id => "ContactsSorting";
        public int IncomingContactRequestPriority => _incomingContactRequestPriorityKey.GetValue();
        public int JoinablePriority => _joinablePriorityKey.GetValue();
        public bool KeepPinnedOffline => _keepPinnedOffline;
        public override string Name => "Contact Sorting";
        public int OfflineCooldown => _offlineCooldown;
        public int OnlineStatusPriority => _onlineStatusPriorityKey.GetValue();
        public int OutgoingContactRequestPriority => _outgoingContactRequestPriorityKey.GetValue();
        public HashSet<string> PinnedContacts => _pinnedContactsKey.GetValue()!;
        public int ReadMessageCooldown => _readMessageCooldownKey.GetValue();
        public override Version Version { get; } = new(1, 2, 0);
    }
}