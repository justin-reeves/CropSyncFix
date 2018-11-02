using Harmony;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;
using System.Linq;
using StardewValley.Network;
using Netcode;
using System;
using StardewValley.Objects;
using StardewValley.Locations;
using xTile.ObjectModel;
using xTile.Tiles;
using StardewValley.Monsters;

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
            var original = typeof(GameLocation).GetMethod("DayUpdate");
            var prefix = helper.Reflection.GetMethod(typeof(FixCropSync), "Prefix").MethodInfo;
            harmony.Patch(original, new HarmonyMethod(prefix), null);
        }
    }

    public static class FixCropSync
    {

        static bool Prefix(GameLocation __instance, int dayOfMonth)
        {
            __instance.updateMap();
            __instance.temporarySprites.Clear();

            Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);

            //Update Terrain Features
            var tFeatures = __instance.terrainFeatures;
            foreach (KeyValuePair<Vector2, TerrainFeature> pair in tFeatures.Pairs)
            {
                if (!__instance.isTileOnMap(pair.Key) || (!__instance.IsFarm && (pair.Value is HoeDirt && ((pair.Value as HoeDirt).crop == null || (pair.Value as HoeDirt).crop.forageCrop.Value))))
                {
                    tFeatures.Remove(pair.Key);
                }
                else
                {
                    pair.Value.dayUpdate(__instance, pair.Key);
                }
            }

            //Update Large Terrain Features
            if (__instance.largeTerrainFeatures.Count > 0)
            {
                foreach (LargeTerrainFeature largeTerrainFeature in __instance.largeTerrainFeatures)
                    largeTerrainFeature.dayUpdate(__instance);
            }

            //Update Objects
            var objects = __instance.objects;
            foreach (KeyValuePair<Vector2, StardewValley.Object> pair in objects.Pairs) 
            {
                pair.Value.DayUpdate(__instance);

                if (__instance.IsOutdoors)
                {
                    if (Game1.dayOfMonth % 7 == 0 && !(__instance is Farm))
                    {
                        if (pair.Value.IsSpawnedObject)
                        {
                            objects.Remove(pair.Key);
                        }

                        __instance.numberOfSpawnedObjectsOnMap = 0;
                        __instance.spawnObjects();
                        __instance.spawnObjects();
                    }
                }
            }

            if (!(__instance is FarmHouse)) {
                __instance.debris.Filter(d => d.item != null);
            }
            
            if (__instance.IsOutdoors)
            {
                __instance.spawnObjects();

                if (Game1.dayOfMonth == 1)
                {
                    __instance.spawnObjects();
                }

                if (Game1.stats.DaysPlayed < 4U)
                {
                    __instance.spawnObjects();
                }

                bool flag = false;
                foreach (Component layer in __instance.map.Layers)
                {
                    if (layer.Id.Equals("Paths"))
                    {
                        flag = true;
                        break;
                    }
                }

                if (flag && !(__instance is Farm))
                {
                    for (int index1 = 0; index1 < __instance.map.Layers[0].LayerWidth; ++index1)
                    {
                        for (int index2 = 0; index2 < __instance.map.Layers[0].LayerHeight; ++index2)
                        {
                            if (__instance.map.GetLayer("Paths").Tiles[index1, index2] != null && Game1.random.NextDouble() < 0.5)
                            {
                                Vector2 key = new Vector2(index1, index2);
                                int which = -1;
                                switch (__instance.map.GetLayer("Paths").Tiles[index1, index2].TileIndex)
                                {
                                    case 9:
                                        which = 1;
                                        if (Game1.currentSeason.Equals("winter"))
                                        {
                                            which += 3;
                                            break;
                                        }
                                        break;
                                    case 10:
                                        which = 2;
                                        if (Game1.currentSeason.Equals("winter"))
                                        {
                                            which += 3;
                                            break;
                                        }
                                        break;
                                    case 11:
                                        which = 3;
                                        break;
                                    case 12:
                                        which = 6;
                                        break;
                                }

                                if (which != -1 && !tFeatures.ContainsKey(key) && !objects.ContainsKey(key))
                                {
                                    tFeatures.Add(key, new Tree(which, 2));
                                }
                            }
                        }
                    }
                }
            }

            __instance.LightLevel = 0.0f;

            if (__instance.Name.Equals("BugLand"))
            {
                for (int index1 = 0; index1 < __instance.map.Layers[0].LayerWidth; ++index1)
                {
                    for (int index2 = 0; index2 < __instance.map.Layers[0].LayerHeight; ++index2)
                    {
                        if (Game1.random.NextDouble() < 0.33)
                        {
                            Tile tile = __instance.map.GetLayer("Paths").Tiles[index1, index2];
                            if (tile != null)
                            {
                                Vector2 vector2 = new Vector2((float)index1, (float)index2);
                                switch (tile.TileIndex)
                                {
                                    case 13:
                                    case 14:
                                    case 15:
                                        if (!__instance.objects.ContainsKey(vector2))
                                        {
                                            __instance.objects.Add(vector2, new StardewValley.Object(vector2, GameLocation.getWeedForSeason(Game1.random, "spring"), 1));
                                            continue;
                                        }
                                        continue;
                                    case 16:
                                        if (!__instance.objects.ContainsKey(vector2))
                                        {
                                            __instance.objects.Add(vector2, new StardewValley.Object(vector2, Game1.random.NextDouble() < 0.5 ? 343 : 450, 1));
                                            continue;
                                        }
                                        continue;
                                    case 17:
                                        if (!__instance.objects.ContainsKey(vector2))
                                        {
                                            __instance.objects.Add(vector2, new StardewValley.Object(vector2, Game1.random.NextDouble() < 0.5 ? 343 : 450, 1));
                                            continue;
                                        }
                                        continue;
                                    case 18:
                                        if (!__instance.objects.ContainsKey(vector2))
                                        {
                                            __instance.objects.Add(vector2, new StardewValley.Object(vector2, Game1.random.NextDouble() < 0.5 ? 294 : 295, 1));
                                            continue;
                                        }
                                        continue;
                                    case 28:
                                        if (__instance.isTileLocationTotallyClearAndPlaceable(vector2) && __instance.characters.Count < 50)
                                        {
                                            __instance.characters.Add((NPC)new Grub(new Vector2(vector2.X * 64f, vector2.Y * 64f), true));
                                            continue;
                                        }
                                        continue;
                                    default:
                                        continue;
                                }
                            }
                        }
                    }
                }
            }

            __instance.addLightGlows();
            return false;
        }
    }
}
