using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;
using NeedyCats;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace CatLaser.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
public class PlayerControllerBPatch
{
    private const float maxDistance = 10.0f;
    
    [HarmonyPatch(typeof(PlayerControllerB), "Update")]
    [HarmonyPrefix]
    static void PatchUpdate(PlayerControllerB __instance)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (__instance == null) return;
        
        GrabbableObject laserSource = __instance.currentlyHeldObjectServer;
        if (laserSource == null) return;

        bool isLauncher = LethalThingsCompatibility.enabled && LethalThingsCompatibility.IsLauncher(laserSource);
        if (!(laserSource is FlashlightItem || isLauncher)) return;
        if (laserSource is FlashlightItem flashlightItem && (flashlightItem.flashlightTypeID != 2 || !flashlightItem.isBeingUsed)) return;
        if (isLauncher && LethalThingsCompatibility.IsLauncherActive(laserSource)) return;
        
        Vector3 startPos = __instance.gameplayCamera.transform.position;
        Vector3 forward = __instance.gameplayCamera.transform.forward;
        
        RaycastHit hit;
        if (!Physics.Raycast(startPos, forward, out hit, Mathf.Infinity, 605030721))
        {
            return;
        }

        NeedyCatProp[] cats = GameObject.FindObjectsByType<NeedyCatProp>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (NeedyCatProp cat in cats)
        {
            if (Vector3.Distance(cat.transform.position, hit.point) >= maxDistance) continue;
            
            NavMeshAgent catAgent = typeof(NeedyCatProp).GetField("agent", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(cat) as NavMeshAgent;
            if (catAgent != null)
            {
                catAgent.speed = cat.RunningSpeed;
            }
            
            typeof(NeedyCatProp).GetField("timeBeforeNextMove", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(cat, cat.IntervalMove.y);
            
            cat.SetDestinationToPosition(hit.point);
        }
    }
}