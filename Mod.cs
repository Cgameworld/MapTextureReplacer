using Game.Modding;
using Game;
using Colossal.IO.AssetDatabase;
using Game.SceneFlow;
using MapTextureReplacer.Locale;
using HarmonyLib;
using Colossal.Logging;
using System.IO;

namespace MapTextureReplacer
{
    public class Mod : IMod
    {
        private Harmony? _harmony;
        public static MapTextureReplacerOptions? Options { get; set; }

        public static ILog log = LogManager.GetLogger($"{nameof(MapTextureReplacer)}.{nameof(Mod)}").SetShowsErrorsInUI(false);

        public void OnLoad(UpdateSystem updateSystem)
        {

            _harmony = new($"{nameof(MapTextureReplacer)}.{nameof(Mod)}");

            _harmony.PatchAll(typeof(Mod).Assembly);

            Options = new(this);
            Options.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(Options));

            AssetDatabase.global.LoadSettings(nameof(MapTextureReplacer), Options, new MapTextureReplacerOptions(this));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            {
                log.Info("DLLPATH!" + Path.GetDirectoryName(asset.path));
            }
        }

        public void OnDispose()
        {
            _harmony?.UnpatchAll($"{nameof(MapTextureReplacer)}.{nameof(Mod)}");
        }
    }
}
