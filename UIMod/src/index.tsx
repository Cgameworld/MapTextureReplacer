import { ModRegistrar } from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import mod from "../mod.json";
import { VanillaComponentsResolver } from '../types/internal';
import engine from "cohtml/cohtml";

const inUniversalModMenu = bindValue<boolean>("map_texture", "in_univeral_mod_menu");

const register: ModRegistrar = (moduleRegistry) => {

    const { DescriptionTooltip } = VanillaComponentsResolver.instance;

    const isInUniversalModMenu = inUniversalModMenu.value; //static 

    const CustomMenuButton = () => {
        return <DescriptionTooltip title="Map Texture Replacer" description="Replace grass, dirt and cliff map textures. Click to open options">
            <button id="MapTextureReplacer-MainGameButton" className="button_ke4 button_ke4 button_h9N" onClick={() => {
                engine.trigger("audio.playSound", "select-item", 1); trigger("map_texture", "MainWindowCreate");
            }}>
                <div className="tinted-icon_iKo icon_be5" style={{ backgroundImage: 'url(Media/Game/Icons/MapTile.svg)', backgroundPositionY: isInUniversalModMenu ? '-2rem' : '-1rem', backgroundColor: 'rgba(255,255,255,0)', backgroundSize: isInUniversalModMenu ? '32rem 32rem' : '40rem 40rem' }}>
                </div>
            </button>
        </DescriptionTooltip>;
    }

    //inject jsx window container
    trigger("map_texture", "MapButtonLoaded");

    moduleRegistry.append(isInUniversalModMenu ? 'UniversalModMenu' : 'GameTopLeft', CustomMenuButton);

    console.log(mod.id + " UI module registrations completed.");
}

export default register;