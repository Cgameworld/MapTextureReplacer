using System;
using System.Collections.Generic;
using System.Text;

namespace MapTextureReplacer.Helpers
{
    public class MapTextureConfig
    {
        public string pack_name { get; set; }
        public string far_tiling { get; set; }
        public string close_tiling { get; set; }
        public string close_dirt_tiling { get; set; }
        public List<FarTilingBreakpoint> far_tiling_breakpoints { get; set; }
    }

    public class FarTilingBreakpoint
    {
        public float height { get; set; }
        public int far_tiling { get; set; }
    }
}
