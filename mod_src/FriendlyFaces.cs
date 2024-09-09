using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using LobbyCompatibility.Attributes;
using LobbyCompatibility.Enums;

using UnityEngine;

namespace FriendlyFaces;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(LethalLib.Plugin.ModGUID)] 
[LobbyCompatibility(CompatibilityLevel.ClientOnly, VersionStrictness.None)]
public class FriendlyFaces : BaseUnityPlugin
{
    public static FriendlyFaces Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }
    public static AssetBundle CustomAssets = null!;
    public static ConfigFile MyConfig = null!;
    private void Awake()
    {
        // Default rarity of custom items
        int item_rarity = 30;

        Logger = base.Logger;
        Instance = this;

        // Configs
        MyConfig = base.Config;
        
        // Loading assets
        var assembly = Assembly.GetExecutingAssembly();
        var asset_stream = assembly.GetManifestResourceStream("FriendlyFaces.assets.friendlyfaces");
        CustomAssets = AssetBundle.LoadFromStream(asset_stream);
        if (CustomAssets == null) {
            Logger.LogError($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} failed loading assets!");
            return;
        }

        // We want to disable saving our config file every time we bind a
        // setting as it's inefficient and slow
        // Configs arre accessed in register items
        MyConfig.SaveOnConfigSet = false;

        string[] names = new string[] { 
            "Honza", 
            "Jirka",
            "Mára", 
        };
        string[] paths = new string[] { 
            "assets/models/Honza.asset", 
            "assets/models/Jirka.asset",
            "assets/models/Mára.asset",
        };


        // Register our custom items
        RegisterItems(item_rarity, names, paths);

        // We need to manually save since we disabled `SaveOnConfigSet` earlier
        MyConfig.Save(); 
        // And finally, we re-enable `SaveOnConfigSet` so changes to our config
        // entries are written to the config file automatically from now on
        MyConfig.SaveOnConfigSet = true; 
        
        // More things expected by libs
        Patch();
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!\n\nKdybych měl lopatu, tak ti rozmlátím držku!\n");
    }

    internal static void RegisterItems(int rarity, string[] names, string[] paths) {
        for(int i = 0; i < names.Length; i++) {
            Instance.RegisterHead(names[i], paths[i], rarity);
        }
    }

    internal void RegisterHead(string name, string path, int rarity) {
        var rarity_config = Config.Bind<int>(
            "General", $"{name} Rarity", rarity, $"{name}'s item rarity. Default: {rarity}"
        );
        Item head = CustomAssets.LoadAsset<Item>(path);
        LethalLib.Modules.Utilities.FixMixerGroups(head.spawnPrefab);
        LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(head.spawnPrefab);
        LethalLib.Modules.Items.RegisterScrap(head, rarity_config.Value, LethalLib.Modules.Levels.LevelTypes.All);
    }

    internal static void Patch()
    {
        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);
        Logger.LogDebug("Patching...");
        Harmony.PatchAll();
        Logger.LogDebug("Finished patching!");
    }

    internal static void Unpatch()
    {
        Logger.LogDebug("Unpatching...");
        Harmony?.UnpatchSelf();
        Logger.LogDebug("Finished unpatching!");
    }
}
