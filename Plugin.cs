using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using CatLaser.Patches;
using HarmonyLib;
using UnityEngine;

namespace CatLaser;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("evaisa.lethalthings", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        
    private void Awake()
    {
        // Plugin startup logic
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        Debug.Log(BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("evaisa.lethalthings"));
            
        harmony.PatchAll(typeof(PlayerControllerBPatch));
    }
}