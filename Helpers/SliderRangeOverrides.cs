using System.Collections.Generic;

namespace MapTextureReplacer.Helpers
{
    // Manual slider ranges for TerrainRenderSettingsPrefab float fields.
    // Resolution priority used by MapTextureReplacerSystem.PrepareTextureFloatSliders():
    //   1. an entry in this dictionary (matched when the field path contains the key, e.g. "FarTiling")
    //   2. the field's [Range(min, max)] attribute
    //   3. 0 - 100 when neither is found
    // Keys are matched by substring, so keep them specific enough not to collide (e.g. "SplatMultiplier" not "Splat").
    public static class SliderRangeOverrides
    {
        public static readonly Dictionary<string, (float min, float max)> Ranges = new Dictionary<string, (float min, float max)>()
        {
            { "FarTiling", (1f, 250f) },
            { "MidTiling", (5f, 1000f) },
            { "NearTiling", (10f, 3000f) },
            { "DepthScale", (0.1f, 3.5f) },
            { "Triplanar", (0f, 1f) },
            { "SplatMultiplier", (0f, 10f) },
            { "SplatPower", (0f, 5f) }
        };
    }
}
