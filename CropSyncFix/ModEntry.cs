using Harmony;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace CropSyncFix
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            //Harmony patcher
            var harmony = HarmonyInstance.Create("com.github.kirbylink.cropsyncfix");
            var original = typeof().GetMethod("");
            var prefix = helper.Reflection.GetMethod(typeof(FixCropSync), "Prefix").MethodInfo;
            harmony.Patch(original, new HarmonyMethod(prefix), null);
        }
    }

    public static class FixCropSync
    {
        
        static bool Prefix()
        {

            

        }
    }
}
