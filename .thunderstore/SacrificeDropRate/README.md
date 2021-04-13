This mod will let you change the drop rate for the sacrifice artifact.

This mod only takes into account the percent entered in the config, meaning it will not take XP and monster value into account, unlike the default behavior. There is a ModEnabled toggle in the config that lets you enable/disable the mod if you wish to get the original artifact behavior without removing the mod.

Configuration
------------
The mod config file can be found under `BepInEx/config/com.ChevRoR.AlteredSacrificeArtifactRate.cfg`.

**SacrificeArtifactDropRate** - The percent chance that a monster will drop an item on death. Default is 5, which is a 5% chance.

**CustomWeightsEnabled** - If custom weights are enabled. Weights change how often some tiers of items will drop versus others.

**Tier1Weight** - Weight of Tier 1 (white) items.

**Tier2Weight** - Weight of Tier 2 (lime) items.

**Tier3Weight** - Weight of Tier 3 (red) items.

**EquipmentWeight** - Weight of Equipment (orange) items.

**LunarWeight** - Weight of Lunar (cyan) items.

**BossWeight** - Weight of Boss (yellow) items.


Changelog
------------
1.1.0 - Added the ability to use custom weights for items in the config. Removed ModEnabled as it caused issues.
1.0.0 - Initial mod
