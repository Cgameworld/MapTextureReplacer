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