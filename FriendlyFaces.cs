using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
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

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        // loading assets
        string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        CustomAssets = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "friendlyfaces"));
        if (CustomAssets == null) {
            Logger.LogError("{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} failed loading assets!");
            return;
        }

        Logger.LogInfo("Awailable assets in FriendlyFaces:");
        foreach (string sAsset in CustomAssets.GetAllAssetNames())
            Logger.LogInfo($"{sAsset}");

        // Register our custom items
        RegisterItems();

        // More things expected by libs
        Patch();
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!\n\nKdybych měl lopatu, tak ti rozmlátím držku!\n");
    }

    internal static void RegisterItems() {
        int iRarity = 30;

        foreach (string name in new string[] { "Honza", "Jirka" }) {
            Instance.RegisterHead(name, iRarity);
        }
    }

    internal void RegisterHead(string name, int rarity) {
        Item head  = CustomAssets.LoadAsset<Item>($"assets/models/{name}.asset");
        LethalLib.Modules.Utilities.FixMixerGroups(head.spawnPrefab);
        LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(head.spawnPrefab);
        LethalLib.Modules.Items.RegisterScrap(head, rarity, LethalLib.Modules.Levels.LevelTypes.All);
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
