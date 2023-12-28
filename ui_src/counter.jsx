import React from 'react'
import { useDataUpdate } from 'hookui-framework'
import $PanelMod from './panel_modified'

const TextureSelectUI = ({ label, textureType }) => (
    <div className="field_MBO">
        <div className="label_DGc label_ZLb">{label}</div>
        <button className="button_WWa button_SH8" onClick={() => engine.trigger(`map_texture.open_image_${textureType}`)}>Select Image</button>
        <button className="button_WWa button_SH8" onClick={() => engine.trigger(`map_texture.reset_texture_${textureType}`)}>Reset</button>
    </div>
);

const $Counter = ({ react }) => {

    const [texturePack, setTexturePack] = react.useState(0)
    const [openTextureZip, setOpenTextureZip] = react.useState(0)
    const [tileVal, setTileVal] = react.useState(0)

    const [count, setCount] = react.useState(0);

    useDataUpdate(react, 'map_texture.texture_pack', setTexturePack)
    useDataUpdate(react, 'map_texture.open_texture_zip', setOpenTextureZip)
    useDataUpdate(react, 'map_texture.tile_val', setTileVal)

    // Below, engine.trigger is responsible for triggering the TriggerBinding in the UI System
    return <$PanelMod react={react} title="Map Texture Replacer">
        <div className="field_MBO">
            <div className="label_DGc label_ZLb" style={{ textAlign: 'center' }}>{texturePack}</div>
        </div>
        <button className="button_WWa button_SH8" style={{ marginTop: '-10rem', marginBottom: '20rem' }} onClick={() => engine.trigger(`map_texture.open_texture_zip`)}>Load Texture Pack</button>


        <TextureSelectUI label="Grass Diffuse" textureType="gd" />
        <TextureSelectUI label="Grass Normal" textureType="gn" />
        <TextureSelectUI label="Dirt Diffuse" textureType="dd" />
        <TextureSelectUI label="Dirt Normal" textureType="dn" />
        <TextureSelectUI label="Cliff Diffuse" textureType="cd" />
        <TextureSelectUI label="Cliff Normal" textureType="cn" />

        <button className="button_WWa button_SH8" style={{ marginTop: '10rem', marginBottom: '10rem' }} onClick={() => engine.trigger(`map_texture.tile_val`)}>Set Tile</button>

        <div className="field_MBO">
            <div className="row_d2o">
                <div className="label_DGc label_ZLb">Tiling Test</div>
                <div className="control_Hds" style={{width:'67.5%',position:'relative',left:
            '20rem'} }>
                    <div className="slider-container_Q_K">
                        <div className="slider_KXG slider_pUS horizontal slider_ROT">
                            <div className="track-bounds_H8_">
                                <div className="range-bounds_lNt" style={{width:'30%'}}>
                                    <div className="range_nHO range_iUN"></div>
                                    <div className="thumb-container_aso">
                                        <div className="thumb_kkL"></div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <input className="slider-input_DXM input_Wfi" type="text" vk-title="" vk-description="" vk-type="text" rows="5" />
                </div>
            </div>
        </div>

        <button onClick={() => setCount(count + 1)}>
            {count}
        </button>


    </$PanelMod>
}

//Registering the panel with HookUI so it shows up in the menu
window._$hookui.registerPanel({
    id: "example.map_texture",
    name: "Map Texture Replacer",
    icon: "Media/Game/Icons/MapTile.svg",
    component: $Counter
})