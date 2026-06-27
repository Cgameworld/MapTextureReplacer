using System.Collections.Generic;

namespace MapTextureReplacer.Helpers
{
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
