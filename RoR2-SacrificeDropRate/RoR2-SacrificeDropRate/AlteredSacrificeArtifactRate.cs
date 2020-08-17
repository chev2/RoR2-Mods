using BepInEx;
using BepInEx.Configuration;
using RoR2;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using System.Reflection;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace ChevRoR
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.ChevRoR.AlteredSacrificeArtifactRate", "Sacrifice Artifact Drop Rate Changer", "1.1.0")]
    public class AlteredSacrificeArtifactRate : BaseUnityPlugin
    {
        public static ConfigEntry<float> DropRateModifier { get; set; }
        public static ConfigEntry<bool> CustomWeightsEnabled { get; set; }
        public static ConfigEntry<float> Tier1Weight { get; set; }
        public static ConfigEntry<float> Tier2Weight { get; set; }
        public static ConfigEntry<float> Tier3Weight { get; set; }
        public static ConfigEntry<float> EquipmentWeight { get; set; }
        public static ConfigEntry<float> LunarWeight { get; set; }
        public static ConfigEntry<float> BossWeight { get; set; }

        public void Awake()
        {
            DropRateModifier = Config.Bind("Main", "SacrificeArtifactDropRate", 5f, "Percent chance an item will drop from a monster on death. 1 is 1% drop rate, 100 is 100%, etc.");
            CustomWeightsEnabled = Config.Bind("Weights", "CustomWeightsEnabled", true, "If custom weights are enabled. Weights change how often some tiers of items will drop versus others.");

            IL.RoR2.Artifacts.SacrificeArtifactManager.OnServerCharacterDeath += (il) => // modification for overall drop % chance
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                    x => x.MatchLdloc(0),
                    x => x.MatchLdcR4(0),
                    x => x.MatchLdnull()
                ); // find location
                c.Next.OpCode = OpCodes.Ldc_R4; // change from local variable to float32
                c.Next.Operand = DropRateModifier.Value; // set to config value
            };

            if (CustomWeightsEnabled.Value)
            {
                On.RoR2.BasicPickupDropTable.GenerateWeightedSelection += (orig, self, run) => // modification to add boss items to the droptable
                {
                    orig(self, run); // call original code first
                    if (BossWeight.Value > 0f)
                    {
                        WeightedSelection<List<PickupIndex>> pi = self.GetFieldValue<WeightedSelection<List<PickupIndex>>>("selector"); // get BasicPickupDropTable.selector
                        print("Adding boss items to the drop table.");
                        pi.AddChoice(run.availableBossDropList, BossWeight.Value);
                    }
                };

                On.RoR2.Artifacts.SacrificeArtifactManager.Init += (orig) => // modification for individual item tier weights
                {
                    orig(); // call original code first

                    FieldInfo droptb = typeof(RoR2.Artifacts.SacrificeArtifactManager).GetField("dropTable", BindingFlags.NonPublic | BindingFlags.Static);
                    var dropTb = droptb.GetValue(null);

                    float getFieldVal(string name) => dropTb.GetFieldValue<float>(name);
                    void setFieldVal(string name, float val) { dropTb.SetFieldValue<float>(name, val); }

                    // config default values use the game's values in case the devs ever change the weights in the future
                    Tier1Weight = Config.Bind("Weights", "Tier1Weight", getFieldVal("tier1Weight"), "Weight of Tier 1 (white) items.");
                    Tier2Weight = Config.Bind("Weights", "Tier2Weight", getFieldVal("tier2Weight"), "Weight of Tier 2 (lime) items.");
                    Tier3Weight = Config.Bind("Weights", "Tier3Weight", getFieldVal("tier3Weight"), "Weight of Tier 3 (red) items.");
                    EquipmentWeight = Config.Bind("Weights", "EquipmentWeight", getFieldVal("equipmentWeight"), "Weight of Equipment (orange) items.");
                    LunarWeight = Config.Bind("Weights", "LunarWeight", getFieldVal("lunarWeight"), "Weight of Lunar (cyan) items.");
                    BossWeight = Config.Bind("Weights", "BossWeight", 0f, "Weight of Boss (yellow) items.");

                    setFieldVal("tier1Weight", Tier1Weight.Value);
                    setFieldVal("tier2Weight", Tier2Weight.Value);
                    setFieldVal("tier3Weight", Tier3Weight.Value);
                    setFieldVal("equipmentWeight", EquipmentWeight.Value);
                    setFieldVal("lunarWeight", LunarWeight.Value);
                };
            }
        }
    }
}
