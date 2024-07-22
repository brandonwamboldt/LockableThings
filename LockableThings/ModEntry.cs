using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace LockableThings
{
    public class ModEntry : Mod
    {
        private static ModConfig? Config;
        private static IMonitor? sMonitor;
        private static Harmony? Harmony;
        private static IModHelper? Helper;
        private static bool ToggleUnlocked = false;

        public override void Entry(IModHelper helper)
        {
            sMonitor = Monitor;
            Helper = helper;
            Config = Helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            Harmony = new Harmony(ModManifest.UniqueID);

            Harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.checkForAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(Object_checkForAction_Prefix))
            );

            Harmony.Patch(
               original: AccessTools.Method(typeof(Sign), nameof(Sign.checkForAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(Sign_checkForAction_Prefix))
            );

            Harmony.Patch(
               original: AccessTools.Method(typeof(CrabPot), nameof(CrabPot.checkForAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(CrabPot_checkForAction_Prefix))
            );

            Harmony.Patch(
               original: AccessTools.Method(typeof(Flooring), nameof(Flooring.performToolAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(Flooring_performToolAction_Prefix))
            );

            Harmony.Patch(
               original: AccessTools.Method(typeof(Furniture), nameof(Furniture.clicked)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(Furniture_clicked_Prefix))
            );

            Harmony.Patch(
               original: AccessTools.Method(typeof(Furniture), nameof(Furniture.canBeRemoved)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(Furniture_canBeRemoved_Postfix))
            );

            Harmony.Patch(
               original: AccessTools.Method(typeof(BedFurniture), nameof(BedFurniture.canBeRemoved)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(Furniture_canBeRemoved_Postfix))
            );
        }

        private void Input_ButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady) return;

            if (Config.ToggleLock && e.Button == Config.UnlockKeybind)
            {
                ToggleUnlocked = !ToggleUnlocked;
                Game1.showGlobalMessage(Helper.Translation.Get($"Message." + (ToggleUnlocked ? "Unlocked" : "Locked")));
            }
        }

        private void GameLoop_GameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            // Get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.UnlockKeybind.Name"),
                tooltip: () => Helper.Translation.Get("Config.UnlockKeybind.Tooltip"),
                getValue: () => Config.UnlockKeybind,
                setValue: value => Config.UnlockKeybind = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.ToggleLock.Name"),
                tooltip: () => Helper.Translation.Get("Config.ToggleLock.Tooltip"),
                getValue: () => Config.ToggleLock,
                setValue: value => Config.ToggleLock = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.LockItemSigns.Name"),
                tooltip: () => Helper.Translation.Get("Config.LockItemSigns.Tooltip"),
                getValue: () => Config.LockItemSigns,
                setValue: value => Config.LockItemSigns = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.LockTextSigns.Name"),
                tooltip: () => Helper.Translation.Get("Config.LockTextSigns.Tooltip"),
                getValue: () => Config.LockTextSigns,
                setValue: value => Config.LockTextSigns = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.LockCrabPots.Name"),
                tooltip: () => Helper.Translation.Get("Config.LockCrabPots.Tooltip"),
                getValue: () => Config.LockCrabPots,
                setValue: value => Config.LockCrabPots = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.LockFloors.Name"),
                tooltip: () => Helper.Translation.Get("Config.LockFloors.Tooltip"),
                getValue: () => Config.LockFloors,
                setValue: value => Config.LockFloors = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.LockDecorations.Name"),
                tooltip: () => Helper.Translation.Get("Config.LockDecorations.Tooltip"),
                getValue: () => Config.LockDecorations,
                setValue: value => Config.LockDecorations = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.LockFurniture.Name"),
                tooltip: () => Helper.Translation.Get("Config.LockFurniture.Tooltip"),
                getValue: () => Config.LockFurniture,
                setValue: value => Config.LockFurniture = value
            );
        }

        public static void Object_checkForAction_Prefix(Sign __instance, Farmer who, ref bool justCheckingForActivity)
        {
            if (__instance.isTemporarilyInvisible || __instance.QualifiedItemId != "(BC)TextSign" || justCheckingForActivity || Game1.activeClickableMenu != null || __instance.signText.Value == null || __instance.signText.Value.Length == 0 || IsUnlocked(Config.LockTextSigns))
            {
                return;
            }

            justCheckingForActivity = true;
            Game1.showRedMessage(Helper.Translation.Get("Error.Locked"));
        }

        public static void CrabPot_checkForAction_Prefix(CrabPot __instance, Farmer who, bool justCheckingForActivity, ref int ___ignoreRemovalTimer)
        {
            if (IsUnlocked(Config.LockCrabPots)) return;

            ___ignoreRemovalTimer = 200;
            Game1.showRedMessage(Helper.Translation.Get("Error.Locked"));
        }

        public static void Sign_checkForAction_Prefix(Sign __instance, Farmer who, ref bool justCheckingForActivity)
        {
            if (justCheckingForActivity) return;
            if (IsUnlocked(Config.LockItemSigns)) return;
            if (who.CurrentItem == null) return;
            if (__instance.displayItem.Value == null) return;

            justCheckingForActivity = true;
            Game1.showRedMessage(Helper.Translation.Get("Error.Locked"));
        }

        public static bool Flooring_performToolAction_Prefix(Flooring __instance, Tool t)
        {
            if (IsUnlocked(Config.LockFloors)) return true;

            Game1.showRedMessage(Helper.Translation.Get("Error.Locked"));
            return false;
        }

        public static bool Furniture_clicked_Prefix(Furniture __instance, Farmer who)
        {
            if ((int)__instance.furniture_type == 11 && who.ActiveObject != null && __instance.heldObject.Value == null)
            {
                return true;
            }

            if (__instance.heldObject.Value != null && !IsUnlocked(Config.LockDecorations))
            {
                Game1.showRedMessage(Helper.Translation.Get("Error.Locked"));
                return false;
            }

            return true;
        }

        public static void Furniture_canBeRemoved_Postfix(Furniture __instance, Farmer who, ref bool __result)
        {
            if (!__result || IsUnlocked(Config.LockFurniture))
            {
                return;
            }

            Game1.showRedMessage(Helper.Translation.Get("Error.Locked"));
            __result = false;
        }

        private static bool IsUnlocked(bool config)
        {
            return (!config || ToggleUnlocked || (Helper.Input.IsDown(Config.UnlockKeybind) && !Config.ToggleLock));
        }
    }
}
