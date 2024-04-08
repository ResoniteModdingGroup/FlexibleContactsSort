using Elements.Core;
using FrooxEngine;
using MonkeyLoader.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexibleContactsSort
{
    internal sealed class SessionCapacityConfig : ConfigSection
    {
        private readonly DefiningConfigKey<colorX> _emptySessionColorKey = new("EmptySessionColor", "Color of the user count when only the host is there.", () => RadiantUI_Constants.Hero.GREEN);
        private readonly DefiningConfigKey<colorX> _fullSessionColorKey = new("FullSessionColor", "Color of the user count when the session is full.", () => RadiantUI_Constants.Hero.RED);
        private readonly DefiningConfigKey<bool> _showUsageLevelWithColorGradientKey = new("ShowUsageLevelWithColorGradient", "Color the user count based on capacity usage.", () => true);
        private readonly DefiningConfigKey<bool> _showUserCapacityInSessionListKey = new("ShowUserCapacityInSessionList", "Show the user capacity of contacts' joinable sessions.", () => true);
        public override string Description => "Contains options for how to highlight contacts' session capacity.";
        public colorX EmptySessionColor => _emptySessionColorKey.GetValue();
        public colorX FullSessionColor => _fullSessionColorKey.GetValue();
        public override string Id => "SessionCapacity";
        public bool ShowUsageLevelWithColorGradient => _showUsageLevelWithColorGradientKey.GetValue();
        public bool ShowUserCapacityInSessionList => _showUserCapacityInSessionListKey.GetValue();
        public override Version Version { get; } = new(1, 0, 0);
    }
}