using System.Runtime.CompilerServices;
using LethalThings;

namespace CatLaser;

public static class LethalThingsCompatibility
{
    private static bool? _enabled;

    public static bool enabled
    {
        get
        {
            if (_enabled == null)
            {
                _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("evaisa.lethalthings");
            }
            
            return (bool)_enabled;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool IsLauncherButDisabled(GrabbableObject laserSource)
    {
        return laserSource is RocketLauncher launcher && !launcher.laserPointer.enabled;
    }
}