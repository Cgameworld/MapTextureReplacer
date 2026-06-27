using System.Collections.Generic;

namespace MapTextureReplacer.Helpers
{
    // Manual slider ranges for TerrainRenderSettingsPrefab float fields.
    // Resolution priority used by MapTextureReplacerSystem.GrabTextureFloats():
    //   1. an entry in this dictionary (keyed by the field name, e.g. "m_TerrainFarTiling")
    //   2. the field's [Range(min, max)] attribute
    //   3. 0 - 100 when neither is found
    // Add or edit entries here to override the bounds shown by the sliders.
    public static class SliderRangeOverrides
    {
        public static readonly Dictionary<string, (float min, float max)> Ranges = new Dictionary<string, (float min, float max)>()
        {
            { "FarTiling", (1f, 250f) },
            { "MidTiling", (5f, 1000f) },
            { "NearTiling", (10f, 3000f) },
            { "BlurDepth", (1f, 20f) },
            { "DepthScale", (0.1f, 3.5f) }
        };
    }
}
