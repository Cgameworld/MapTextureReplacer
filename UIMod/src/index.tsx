import { ModRegistrar } from "cs2/modding";
import { bindValue, useValue } from "cs2/api";
import mod from "../mod.json";
import { VanillaComponentsResolver } from '../types/internal';
import engine from "cohtml/cohtml";
import CreatorToolsWindow from './components/CreatorToolsWindow';
import MapTextureReplacerWindow from './components/MapTextureReplacerWindow';
import { WindowOpen$, SetWindowOpen } from './bindings';

const inUniversalModMenu = bindValue<boolean>("map_texture", "in_univeral_mod_menu");

const register: ModRegistrar = (moduleRegistry) => {

    const { DescriptionTooltip } = VanillaComponentsResolver.instance;

    const isInUniversalModMenu = inUniversalModMenu.value; //static

    const CustomMenuButton = () => {
        const open = useValue(WindowOpen$);
        const universalSize = `${32 * (isInUniversalModMenu ? parseFloat(getComputedStyle(document.documentElement).getPropertyValue('--toolbarScale')) || 1 : 1)}rem`;
        return <DescriptionTooltip title="Map Texture Replacer" description="Replace grass, dirt and cliff map textures. Click to open options">
            <button id="MapTextureReplacer-MainGameButton" className="button_ke4 button_ke4 button_h9N" onClick={() => {
                engine.trigger("audio.playSound", "select-item", 1); SetWindowOpen(!open);
            }}>
                <div className="tinted-icon_iKo icon_be5" style={{ backgroundImage: 'url(Media/Game/Icons/MapTile.svg)', backgroundPositionY: isInUniversalModMenu ? '-2rem' : '-1rem', backgroundColor: 'rgba(255,255,255,0)', backgroundSize: isInUniversalModMenu ? `${universalSize} ${universalSize}` : '40rem 40rem' }}>
                </div>
            </button>
        </DescriptionTooltip>;
    }

    moduleRegistry.append(isInUniversalModMenu ? 'UniversalModMenu' : 'GameTopLeft', CustomMenuButton);
    moduleRegistry.append('GameTopRight', CreatorToolsWindow);
    moduleRegistry.append('Game', MapTextureReplacerWindow);

    console.log(mod.id + " UI module registrations completed.");
}

export default register;
