//adapted from TreeController

namespace MapTextureReplacer.Locale
{
    using System.Collections.Generic;
    using Colossal;

    public class LocaleEN : IDictionarySource
    {
        private readonly MapTextureReplacerOptions m_Setting;
        public LocaleEN(MapTextureReplacerOptions options)
        {
            m_Setting = options;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "Map Texture Replacer" },

                { m_Setting.GetOptionGroupLocaleID(MapTextureReplacerOptions.DisplayGroup), "Display" },
                { m_Setting.GetOptionGroupLocaleID(MapTextureReplacerOptions.CreatorToolsGroup), "Creator Tools" },
                { m_Setting.GetOptionGroupLocaleID(MapTextureReplacerOptions.OtherGroup), "Other" },

                { m_Setting.GetOptionLabelLocaleID(nameof(MapTextureReplacerOptions.InUniversalModMenu)), "Show Button in Universal Mod Menu" },
                { m_Setting.GetOptionDescLocaleID(nameof(MapTextureReplacerOptions.InUniversalModMenu)), "When enabled, the mod button is placed inside the Universal Mod Menu Tray instead of the upper left hand corner." },
                { m_Setting.GetOptionLabelLocaleID(nameof(MapTextureReplacerOptions.ShowDownloadedPacks)), "Show Downloaded Packs" },
                { m_Setting.GetOptionDescLocaleID(nameof(MapTextureReplacerOptions.ShowDownloadedPacks)), "When enabled, packs from the Paradox Mods folder appear in the dropdown." },
                { m_Setting.GetOptionLabelLocaleID(nameof(MapTextureReplacerOptions.ShowLocalPacks)), "Show Local Packs" },
                { m_Setting.GetOptionDescLocaleID(nameof(MapTextureReplacerOptions.ShowLocalPacks)), "When enabled, packs from the local Mods folder appear in the dropdown." },

                { m_Setting.GetOptionLabelLocaleID(nameof(MapTextureReplacerOptions.ShowCameraHeight)), "Show Current Camera Height" },
                { m_Setting.GetOptionDescLocaleID(nameof(MapTextureReplacerOptions.ShowCameraHeight)), "When enabled, displays a small window in the upper right corner showing the camera's current height above the ground." },
                { m_Setting.GetOptionLabelLocaleID(nameof(MapTextureReplacerOptions.ModVersion)), "Mod Version"},
                { m_Setting.GetOptionDescLocaleID(nameof(MapTextureReplacerOptions.ModVersion)), "Installed version of mod"},
                { m_Setting.GetOptionLabelLocaleID(nameof(MapTextureReplacerOptions.ResetModSettings)), "Reset All Settings" },
                { m_Setting.GetOptionDescLocaleID(nameof(MapTextureReplacerOptions.ResetModSettings)), "Reset Mod Settings to Default Values"},
                { m_Setting.GetOptionWarningLocaleID(nameof(MapTextureReplacerOptions.ResetModSettings)), "Are you sure you want to reset all mod settings?"}
            };

        }
        public void Unload()
        {
        }
    }
}