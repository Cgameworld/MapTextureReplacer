import React from 'react';
import { useDataUpdate } from 'hookui-framework'
import $PanelMod from './panel_modified'
import $SliderMod from './slider_modified'

const TextureSelectUI = ({ label, textureType }) => (
    <div className="field_MBO" style={{minHeight: '52.5rem'}} >
        <div className="label_DGc label_ZLb">{label}</div>
        <button className="button_WWa button_SH8" onClick={() => engine.trigger(`map_texture.open_image_${textureType}`)}>Select Image</button>
        <button className="button_WWa button_SH8" onClick={() => engine.trigger(`map_texture.reset_texture_${textureType}`)}>Reset</button>
    </div>
);

const SliderComponent = ({ react, slider }) => {
    const [sliderValue, setSliderValue] = react.useState(0);
    const [sliderPos, setSliderPos] = react.useState();

    useDataUpdate(react, slider.pos, setSliderPos);

    const handleInputChange = (newValue) => {
        engine.trigger(slider.update, Number(newValue));
    };

    return sliderPos == null ? null :
        <$SliderMod react={react} title={slider.title} min={slider.min} max={slider.max} sliderPos={sliderPos} onInputChange={handleInputChange} />
}

const $Counter = ({ react }) => {

    const [texturePack, setTexturePack] = react.useState(0)
    const [openTextureZip, setOpenTextureZip] = react.useState(0)
    const [tileVal, setTileVal] = react.useState(0)

    const [count, setCount] = react.useState(0);

    useDataUpdate(react, 'map_texture.texture_pack', setTexturePack)
    useDataUpdate(react, 'map_texture.open_texture_zip', setOpenTextureZip)
    useDataUpdate(react, 'map_texture.tile_val', setTileVal)

    const sliders = [
        { title: 'Far Tiling', min: 1, max: 250, pos: 'map_texture.slider1_Pos', update: 'map_texture.slider1_UpdatedValue' },
        { title: 'Close Tiling', min: 10, max: 3000, pos: 'map_texture.slider2_Pos', update: 'map_texture.slider2_UpdatedValue' },
        { title: 'Close Dirt Tiling', min: 10, max: 4000, pos: 'map_texture.slider3_Pos', update: 'map_texture.slider3_UpdatedValue' },
        { title: 'Scale?', min: 1, max: 10, pos: 'map_texture.slider4_Pos', update: 'map_texture.slider4_UpdatedValue' },
    ];

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



        {sliders.map((slider, index) => <SliderComponent key={index} react={react} slider={slider} />)}

        <button className="button_WWa button_SH8" style={{ marginTop: '10rem', marginBottom: '10rem' }} onClick={() => engine.trigger(`map_texture.tile_val`)}>Set Tile</button>

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