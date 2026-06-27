import { bindValue, trigger } from "cs2/api";

const GROUP = "map_texture";

// --- window visibility ---
export const WindowOpen$ = bindValue<boolean>(GROUP, "window_open", false);
export const SetWindowOpen = (open: boolean) => trigger(GROUP, "window_open", open);

// --- data getters (C# -> UI) ---
export const DetectedPacks$ = bindValue<string>(GROUP, "get_detected_packs", "{}");
export const TextureSelectData$ = bindValue<string>(GROUP, "get_texture_select_data", "[]");
export const TextureFloats$ = bindValue<string>(GROUP, "get_texture_floats", "[]");
export const ActivePackDropdown$ = bindValue<string>(GROUP, "get_active_pack_dropdown", "none");
export const TexturePack$ = bindValue<string>(GROUP, "texture_pack", "");

// --- triggers (UI -> C#) ---
export const ChangePack = (value: string) => trigger(GROUP, "change_pack", value);
export const SetActivePackDropdown = (value: string) => trigger(GROUP, "set_active_pack_dropdown", value);
export const ResetTextureSelectData = () => trigger(GROUP, "reset_texture_select_data");
export const OpenTextureZip = () => trigger(GROUP, "open_texture_zip");
export const OpenImage = (slot: string, path: string) => trigger(GROUP, `open_image_${slot}`, path);
export const ResetTexture = (slot: string) => trigger(GROUP, `reset_texture_${slot}`);
export const ChangeFloatField = (name: string, value: number) => trigger(GROUP, "change_float_field", name, value);
export const ResetTiling = () => trigger(GROUP, "reset_tiling");
