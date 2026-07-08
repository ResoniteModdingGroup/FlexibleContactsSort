using Elements.Quantity;
using FrooxEngine;
using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite.Configuration;

namespace FlexibleContactsSort
{
    internal sealed class ContactsSortingConfig : ConfigSection
    {
        private static readonly ConfigKeySubgroup _cooldownsSubgroup = new(-1_000_000_000, "Cooldowns");
        private static readonly ConfigKeyQuantity<int, Time> _cooldownTimeFormat = new(new UnitConfiguration("s", "0", " ", ["m", "s"]), null, 0, int.MaxValue);
        private static readonly ConfigKeySubgroup _offlineSubgroup = new(-2_000_000_000, "OfflineHiding");

        private readonly DefiningConfigKey<int> _alphabeticPriorityKey = new("AlphabeticPriority", "Priority of the contact's name. Set to 0 to ignore; negative to invert.", () => 1)
        {
            new ConfigKeyPriority(-1)
        };

        private readonly DefiningConfigKey<int> _headlessPriorityKey = new("HeadlessPriority", "Priority of the contact being an active headless host. Set to 0 to ignore; negative to invert.", () => 100_000)
        {
            new ConfigKeyPriority(-100_000)
        };

        private readonly DefiningConfigKey<bool> _hideOffline = new("HideOffline", "Hide offline contacts completely.", () => false)
        {
            _offlineSubgroup,
            new ConfigKeyPriority(1)
        };

        private readonly DefiningConfigKey<int> _incomingContactRequestPriorityKey = new("IncomingContactRequestPriority", "Priority of the contact being an incoming request. Set to 0 to ignore; negative to invert.", () => -1_000_000)
        {
            new ConfigKeyPriority(1_000_000)
        };

        private readonly DefiningConfigKey<int> _joinablePriorityKey = new("JoinablePriority", "Priority of the contact being in a session you can join. Set to 0 to ignore; negative to invert.", () => -10_000)
        {
            new ConfigKeyPriority(10_000)
        };

        private readonly DefiningConfigKey<bool> _keepPinnedOffline = new("KeepPinnedOffline", "Do not hide pinned contacts, even if they're offline.", () => true)
        {
            _offlineSubgroup
        };

        private readonly DefiningConfigKey<int> _offlineCooldown = new("OfflineCooldown", "Delay before a contact that has just gone offline is counted as such. Set to 0 to disable.", () => 120)
        {
            _cooldownsSubgroup,
            _cooldownTimeFormat
        };

        private readonly DefiningConfigKey<int> _onlineStatusPriorityKey = new("OnlineStatusPriority", "Priority of the contact's online status. Set to 0 to ignore; negative to invert.", () => 1_000)
        {
            new ConfigKeyPriority(-1_000)
        };

        private readonly DefiningConfigKey<int> _outgoingContactRequestPriorityKey = new("OutgoingContactRequestPriority", "Priority of the contact being an outgoing request. Set to 0 to ignore; negative to invert.", () => 1_000_000)
        {
            new ConfigKeyPriority(-1_000_000)
        };

        private readonly DefiningConfigKey<HashSet<string>> _pinnedContactsKey = new("PinnedContacts", "List of Contacts to always keep at the top.", () => [], internalAccessOnly: true);

        private readonly DefiningConfigKey<int> _readMessageCooldownKey = new("ReadMessageCooldown", "Delay before a contact with freshly-read messages is counted as such. Set to 0 to disable.", () => 120)
        {
            _cooldownsSubgroup,
            _cooldownTimeFormat,
            new ConfigKeyPriority(1)
        };

        public int AlphabeticPriority => _alphabeticPriorityKey;
        public override string Description => "Contains options for how to sort the Contacts list.";
        public int HeadlessPriority => _headlessPriorityKey;
        public bool HideOffline => _hideOffline;
        public override string Id => "ContactsSorting";
        public int IncomingContactRequestPriority => _incomingContactRequestPriorityKey;
        public int JoinablePriority => _joinablePriorityKey;
        public bool KeepPinnedOffline => _keepPinnedOffline;
        public override string Name => "Contact Sorting";
        public int OfflineCooldown => _offlineCooldown;
        public int OnlineStatusPriority => _onlineStatusPriorityKey;
        public int OutgoingContactRequestPriority => _outgoingContactRequestPriorityKey;
        public HashSet<string> PinnedContacts => _pinnedContactsKey!;
        public int ReadMessageCooldown => _readMessageCooldownKey;
        public override Version Version { get; } = new(1, 2, 0);

        public ContactsSortingConfig()
        {
            _keepPinnedOffline.Components.Add(new ConfigKeyEnabledSource<bool>(_hideOffline));
        }
    }
}