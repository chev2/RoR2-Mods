using BepInEx;
using MonoMod.Cil;
using System.Security.Permissions;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace Chev
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class PluginUncappedConvergence : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Chev";
        public const string PluginName = "UncappedConvergence";
        public const string PluginVersion = "2.0.0";

        public void Awake()
        {
            Logger.LogInfo("Hooking into HoldoutZoneController.FocusConvergenceController.FixedUpdate...");

            IL.RoR2.HoldoutZoneController.FocusConvergenceController.FixedUpdate += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                    x => x.MatchCall<UnityEngine.Mathf>("Min")
                );
                c.Index -= 3;
                c.RemoveRange(6); // remove Mathf.min code
            };

            Logger.LogInfo("Hook finished, mod loaded");
        }
    }
}
