using Colossal.UI.Binding;
using Game.UI;

namespace MapTextureReplacer.Systems
{
    public partial class MapTextureReplacerUISystem : UISystemBase
    {
        public MapTextureReplacerSystem systemManaged;
        private ValueBinding<bool> m_WindowOpen;

        protected override void OnCreate()
        {
            this.systemManaged = this.World.GetExistingSystemManaged<MapTextureReplacerSystem>();

            base.OnCreate();

            this.AddUpdateBinding(new GetterValueBinding<bool>("map_texture", "in_univeral_mod_menu", () =>
            {
                return Mod.Options?.InUniversalModMenu ?? false;
            }));

            this.AddUpdateBinding(new GetterValueBinding<string>("map_texture", "texture_pack", () =>
            {
                return systemManaged.PackImportedText;
            }));

            this.AddUpdateBinding(new GetterValueBinding<string>("map_texture", "get_detected_packs", () =>
            {
                return systemManaged.importedPacksJsonString;
            }));

            this.AddUpdateBinding(new GetterValueBinding<string>("map_texture", "get_texture_select_data", () =>
            {
                return systemManaged.textureSelectDataJsonString;
            }));

            this.AddBinding(new TriggerBinding("map_texture", "reset_texture_select_data", this.systemManaged.ResetTextureSelectData));

            this.AddBinding(new TriggerBinding("map_texture", "open_texture_zip", this.systemManaged.GetTextureZip));

            this.AddBinding(new TriggerBinding<string>("map_texture", "change_pack", this.systemManaged.ChangePack));

            this.AddUpdateBinding(new GetterValueBinding<bool>("map_texture", "show_camera_height", () => Mod.Options?.ShowCameraHeight ?? false));

            this.AddUpdateBinding(new GetterValueBinding<float>("map_texture", "camera_height", () => systemManaged.CurrentCameraHeightAboveGround));

            this.AddUpdateBinding(new GetterValueBinding<string>("map_texture", "get_active_pack_dropdown", () => this.systemManaged.GetActivePackDropdown()));
            this.AddBinding(new TriggerBinding<string>("map_texture", "set_active_pack_dropdown", this.systemManaged.SetActivePackDropdown));

            //generic per-slot bindings keyed by slot index (scales to all texture slots incl. extras)
            this.AddBinding(new TriggerBinding<int, string>("map_texture", "open_image", this.systemManaged.OpenImage));
            this.AddBinding(new TriggerBinding<int>("map_texture", "reset_texture", this.systemManaged.ResetTexture));

            //tiling sliders (dynamically generated from the prefab's float fields)
            this.AddUpdateBinding(new GetterValueBinding<string>("map_texture", "get_texture_floats", () => systemManaged.textureFloatsJsonString));
            this.AddBinding(new TriggerBinding<string, float>("map_texture", "change_float_field", this.systemManaged.ChangeFloatField));
            this.AddBinding(new TriggerBinding<string>("map_texture", "reset_tiling", this.systemManaged.ResetTextureFloats));

            //window open/close state (read with useValue, toggled via trigger)
            m_WindowOpen = new ValueBinding<bool>("map_texture", "window_open", false);
            this.AddBinding(m_WindowOpen);
            this.AddBinding(new TriggerBinding<bool>("map_texture", "window_open", SetWindowOpen));
        }

        public void SetWindowOpen(bool open)
        {
            if (open)
            {
                //refresh slider values from the current map before the panel reads them
                systemManaged.PrepareTextureFloatSliders();
            }
            m_WindowOpen.Update(open);
        }
    }
}
