using Elements.Core;
using FrooxEngine;
using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite.Configuration;

namespace FlexibleContactsSort
{
    internal sealed class SessionCapacityConfig : ConfigSection
    {
        private static readonly ConfigKeySubgroup _gradientSubgroup = new(-10, "Gradient");

        private readonly DefiningConfigKey<colorX> _emptySessionColorKey = new("EmptySessionColor", "Color of the user count when only the host is there.", () => RadiantUI_Constants.Hero.GREEN)
        {
            _gradientSubgroup,
            new ConfigKeyPriority(5)
        };

        private readonly DefiningConfigKey<colorX> _fullSessionColorKey = new("FullSessionColor", "Color of the user count when the session is full.", () => RadiantUI_Constants.Hero.RED)
        {
            _gradientSubgroup,
            new ConfigKeyPriority(1)
        };

        private readonly DefiningConfigKey<bool> _showUsageLevelWithColorGradientKey = new("ShowUsageLevelWithColorGradient", "Color the user count based on capacity usage.", () => true)
        {
            _gradientSubgroup,
            new ConfigKeyPriority(10)
        };

        private readonly DefiningConfigKey<bool> _showUserCapacityInSessionListKey = new("ShowUserCapacityInSessionList", "Show the user capacity of contacts' joinable sessions.", () => true);
        public override string Description => "Contains options for how to highlight contacts' session capacity.";
        public colorX EmptySessionColor => _emptySessionColorKey.GetValue();
        public colorX FullSessionColor => _fullSessionColorKey.GetValue();
        public override string Id => "SessionCapacity";
        public override string Name => "Session Capacity";
        public bool ShowUsageLevelWithColorGradient => _showUsageLevelWithColorGradientKey.GetValue();
        public bool ShowUserCapacityInSessionList => _showUserCapacityInSessionListKey.GetValue();
        public override Version Version { get; } = new(1, 0, 0);

        public SessionCapacityConfig()
        {
            _emptySessionColorKey.Components.Add(new ConfigKeyEnabledSource<colorX>(_showUsageLevelWithColorGradientKey));
            _fullSessionColorKey.Components.Add(new ConfigKeyEnabledSource<colorX>(_showUsageLevelWithColorGradientKey));
        }
    }
}