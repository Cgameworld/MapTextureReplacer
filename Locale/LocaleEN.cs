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
            // \n for making new lines doesn't work - hacky way to make new lines
            string persistentSettingsDescription = "persistentSettingsDescription" + new string('\r', 210) + "Note: persistentSettingsDescription";

            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "Map Texture Replacer" },
                { m_Setting.GetOptionLabelLocaleID(nameof(MapTextureReplacerOptions.ResetModSettings)), "Reset Settings" },
                { m_Setting.GetOptionDescLocaleID(nameof(MapTextureReplacerOptions.ResetModSettings)), persistentSettingsDescription}
            };

        }
        public void Unload()
        {
        }
    }
}