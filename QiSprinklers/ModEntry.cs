using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using QiSprinklers.Framework;


namespace QiSprinklers
{
    public class ModEntry : Mod
    {
        public static IModHelper ModHelper;
        public static ModConfig Config;
        // public static Texture2D ToolsTexture;

        public override void Entry(IModHelper helper)
        {
            ModHelper = helper;

            Config = this.Helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Content.AssetEditors.Add(new AssetEditor());

            SprinklerInitializer.Init(helper.Events);
        }

        /// <summary>Raised after the player loads a save slot and the world is initialised.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // force add sprinkler recipe for people who were level 10 before installing mod
            if (Game1.player.FarmingLevel >= QiSprinklerItem.CRAFTING_LEVEL)
            {
                try
                {
                    Game1.player.craftingRecipes.Add("Qi Sprinkler", 0);
                }
                catch { }
            }

        }
    }

    public class ModConfig {
        public int SprinklerRange { get; set; } = 3;
    }

    public class QiSprinklerItem {
        public const int INDEX = 1113;
        public const int PRICE = 2000;
        public const int EDIBILITY = -300;
        public const string TYPE = "Crafting";
        public const int CATEGORY = Object.CraftingCategory;
        public const int CRAFTING_LEVEL = 9;
    }

    // searches map for any currently placed qi sprinklers and:
    //   - waters adjacent tiles
    public static class SprinklerInitializer
    {

        public static void Init(IModEvents events)
        {
            events.GameLoop.DayStarted += OnDayStarted;
            events.GameLoop.SaveLoaded += OnSaveLoaded;
            events.World.ObjectListChanged += OnObjectListChanged;
        }

        private static void OnSaveLoaded(object sender, System.EventArgs e)
        {
            Object sprinkler;
            foreach (GameLocation location in Game1.locations)
            {
                if (location is GameLocation)
                {
                    foreach (KeyValuePair<Vector2, Object> pair in location.objects.Pairs)
                    {
                        if (location.objects[pair.Key].ParentSheetIndex == QiSprinklerItem.INDEX)
                        {
                            sprinkler = location.objects[pair.Key];
                            int id = (int)sprinkler.TileLocation.X * 4000 + (int)sprinkler.TileLocation.Y;
                        }
                    }
                }
            }
        }

        /// <summary>Raised after objects are added or removed in a location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            WaterDirt();
        }

        /// <summary>Raised after the game begins a new day (including when the player loads a save).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnDayStarted(object sender, System.EventArgs e)
        {
            WaterDirt();
        }

        private static void WaterDirt()
        {
            foreach (GameLocation location in Game1.locations)
            {
                foreach (Object obj in location.Objects.Values)
                {
                    if (obj.ParentSheetIndex == QiSprinklerItem.INDEX)
                    {

                        // add water spray animation
                        location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 2176, 320, 320), 60f, 4, 100, obj.TileLocation * 64 + new Vector2(-192, -208), false, false)
                        {
                            color = Color.White * 0.4f,
                            scale = 7f / 5f,
                            delayBeforeAnimationStart = 0,
                            id = obj.TileLocation.X * 4000f + obj.TileLocation.Y
                        });

                        if (location is Farm || location.IsGreenhouse)
                        {
                            for (int index1 = (int)obj.TileLocation.X - ModEntry.Config.SprinklerRange; index1 <= obj.TileLocation.X + ModEntry.Config.SprinklerRange; ++index1)
                            {
                                for (int index2 = (int)obj.TileLocation.Y - ModEntry.Config.SprinklerRange; index2 <= obj.TileLocation.Y + ModEntry.Config.SprinklerRange; ++index2)
                                {
                                    Vector2 key = new Vector2(index1, index2);
                                    // water dirt
                                    if (location.terrainFeatures.ContainsKey(key) && location.terrainFeatures[key] is HoeDirt)
                                    {
                                        (location.terrainFeatures[key] as HoeDirt).state.Value = 1;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void FertilzeDirt()
        {
            // Do Stuff... and things
        }
    }
}