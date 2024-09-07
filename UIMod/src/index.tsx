import { ModRegistrar } from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import mod from "../mod.json";

const register: ModRegistrar = (moduleRegistry) => {

    const CustomMenuButton = () => {
        return <div>
            <button id="MapTextureReplacer-MainGameButton" className="button_ke4 button_ke4 button_h9N" onClick={() => trigger("map_texture", "MainWindowCreate")}>
                <div className="tinted-icon_iKo icon_be5" style={{ backgroundImage: 'url(Media/Game/Icons/MapTile.svg)', backgroundPositionY: '-1rem', backgroundColor: 'rgba(255,255,255,0)', backgroundSize: '40rem 40rem' }}>
                </div>
            </button>
        </div>;
    }

    //inject jsx window container
    trigger("map_texture", "MapButtonLoaded");

    moduleRegistry.append('GameTopLeft', CustomMenuButton);

    console.log(mod.id + " UI module registrations completed.");
}

export default register;