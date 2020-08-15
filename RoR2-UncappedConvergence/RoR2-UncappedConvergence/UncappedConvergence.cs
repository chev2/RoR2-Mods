using BepInEx;
using RoR2;
using MonoMod.Cil;

namespace ChevRoR
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.ChevRoR.UncappedConvergence", "Uncapped Convergence", "1.0.0")]
    public class UncappedConvergence : BaseUnityPlugin
    {
        public void Awake()
        {
            IL.RoR2.HoldoutZoneController.FocusConvergenceController.FixedUpdate += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                    x => x.MatchCall<UnityEngine.Mathf>("Min")
                );
                c.Index -= 3;
                c.RemoveRange(6); // remove Mathf.min code
            };

            Console.print("\'Uncapped Convergence\' mod loaded");
        }
    }
}
