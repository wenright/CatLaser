using GameNetcodeStuff;
using HarmonyLib;
using LethalThings;
using NeedyCats;
using Unity.Netcode;
using UnityEngine;

namespace CatLaser.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
public class PlayerControllerBPatch
{
    private static PlayerControllerBPatch Instance;
    private const float maxDistance = 10.0f;

    void Awake()
    {
        Instance = this;
    }
    
    [HarmonyPatch(typeof(PlayerControllerB), "Update")]
    [HarmonyPrefix]
    static void pathUpdate(PlayerControllerB __instance)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (__instance == null) return;
        
        GrabbableObject laserSource = __instance.currentlyHeldObjectServer;
        if (laserSource == null) return;
        if (laserSource is not (RocketLauncher or FlashlightItem)) return;
        if (laserSource is RocketLauncher launcher && !launcher.laserPointer.enabled) return;
        if (laserSource is FlashlightItem item &&
            (item.flashlightTypeID != 2 || !item.isBeingUsed)) return;

        Vector3 startPos, forward;
        if (laserSource is RocketLauncher rocketLauncher)
        {
            startPos = rocketLauncher.laserLine.transform.parent.position;
            forward = rocketLauncher.aimDirection.forward;
        }
        else
        {
            startPos = laserSource.transform.position;
            forward = laserSource.transform.forward;
        }
        var catTargetPosition = GetRaycastHitPoint(startPos, forward);

        if (catTargetPosition == Vector3.zero) return;

        float closestDist = maxDistance;
        NeedyCatProp closestCat = null;
        NeedyCatProp[] cats = GameObject.FindObjectsByType<NeedyCatProp>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (NeedyCatProp cat in cats)
        {
            cat.WalkingSpeed = 1.0f;
            float dist = Vector3.Distance(__instance.transform.position, cat.transform.position);
            if (dist < closestDist)
            {
                closestCat = cat;
                closestDist = dist;
            }
        }

        if (closestCat != null)
        {
            closestCat.WalkingSpeed = 8.0f;
            
            // cat agent is private, so call this method to set agent.speed, even though we'll overwrite pos later
            closestCat.SetRandomDestination();
            
            closestCat.SetDestinationToPosition(catTargetPosition);
        }
    }

    private static Vector3 GetRaycastHitPoint(Vector3 startingPos, Vector3 aimDirection)
    {
        RaycastHit hit;
        if (Physics.Raycast(startingPos, aimDirection, out hit, maxDistance, 605030721))
        {
            return hit.point;
        }
        else
        {
            return startingPos + (aimDirection * maxDistance);
        }
    }
}