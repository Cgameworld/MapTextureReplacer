using BepInEx;
using HarmonyLib;
using HookUILib.Core;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

#if BEPINEX_V6
    using BepInEx.Unity.Mono;
#endif

namespace MapTextureReplacer
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            var harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MyPluginInfo.PLUGIN_GUID + "_Cities2Harmony");
            var patchedMethods = harmony.GetPatchedMethods().ToArray();

            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} made patches! Patched methods: " + patchedMethods.Length);

            foreach (var patchedMethod in patchedMethods)
            {
                Logger.LogInfo($"Patched method: {patchedMethod.Module.Name}:{patchedMethod.Name}");
            }
        }
    }
    public class VehicleCounterUI : UIExtension
    {
        public new readonly string extensionID = "example.map_texture";
        public new readonly string extensionContent;
        public new readonly ExtensionType extensionType = ExtensionType.Panel;

        public VehicleCounterUI()
        {
            this.extensionContent = this.LoadEmbeddedResource("MapTextureReplacer.dist.bundle.js");
        }
    }
}
