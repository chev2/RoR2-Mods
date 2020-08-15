using BepInEx;
using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Runtime.CompilerServices;

namespace ChevRoR
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.ChevRoR.AlteredSacrificeArtifactRate", "Sacrifice Artifact Drop Rate Changer", "1.0.0")]
    public class AlteredSacrificeArtifactRate : BaseUnityPlugin
    {
        private static ConfigFile ASARConfig = new ConfigFile(Paths.ConfigPath + "\\SacrificeArtifactRate.cfg", true);
        public static ConfigEntry<bool> ModEnabled { get; set; }
        public static ConfigEntry<float> DropRateModifier { get; set; }

        public void Awake()
        {
            ModEnabled = Config.Bind<bool>("Main", "ModEnabled", true, "Changes whether or not the mod is enabled.");
            DropRateModifier = Config.Bind<float>("Main", "SacrificeArtifactDropRate", 5f, "The modifier used for drop rate. 1 is 1% drop rate, 100 is 100%, etc.");

            if (!ModEnabled.Value)
                return;

            IL.RoR2.Artifacts.SacrificeArtifactManager.OnServerCharacterDeath += (il) =>
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

            Console.print("\'Sacrifice Artifact Drop Rate Changer\' mod loaded");
        }
    }
}
