import React from 'react';
import { useDataUpdate } from 'hookui-framework'
import $PanelMod from './panel_modified'
import $SliderMod from './slider_modified'
import $DropdownMod from './dropdown_modified'

const TextureSelectUI = ({ label, textureType }) => (
    <div className="field_MBO" style={{minHeight: '52.5rem'}} >
        <div className="label_DGc label_ZLb">{label}</div>
        <button className="button_WWa button_SH8" onClick={() => engine.trigger(`map_texture.open_image_${textureType}`)}>Select Image</button>
        <button className="button_WWa button_SH8" onClick={() => engine.trigger(`map_texture.reset_texture_${textureType}`)}>Reset</button>
    </div>
);

const SliderComponent = ({ react, slider, isRendered }) => {
    const [sliderValue, setSliderValue] = react.useState(0);
    const [sliderPos, setSliderPos] = react.useState();

    useDataUpdate(react, slider.pos, setSliderPos);

    const handleInputChange = (newValue) => {
        engine.trigger(slider.update, Number(newValue));
    };

    return (!isRendered || sliderPos == null) ? null :
        <$SliderMod react={react} title={slider.title} min={slider.min} max={slider.max} sliderPos={sliderPos} onInputChange={handleInputChange} />
}


const $Counter = ({ react }) => {

    const [texturePack, setTexturePack] = react.useState(0)
    const [openTextureZip, setOpenTextureZip] = react.useState(0)
    const [resetTiling, setResetTiling] = react.useState(0)

    const [count, setCount] = react.useState(0);

    useDataUpdate(react, 'map_texture.texture_pack', setTexturePack)
    useDataUpdate(react, 'map_texture.open_texture_zip', setOpenTextureZip)
    useDataUpdate(react, 'map_texture.reset_tiling', setResetTiling)

    const sliders = [
        { title: 'Far Tiling', min: 1, max: 250, pos: 'map_texture.slider1_Pos', update: 'map_texture.slider1_UpdatedValue' },
        { title: 'Close Tiling', min: 10, max: 3000, pos: 'map_texture.slider2_Pos', update: 'map_texture.slider2_UpdatedValue' },
        { title: 'Close Dirt Tiling', min: 10, max: 4000, pos: 'map_texture.slider3_Pos', update: 'map_texture.slider3_UpdatedValue' },
    ];

    const [slidersRendered, setSlidersRendered] = react.useState(true);

    const handleButtonClick = () => {
        engine.trigger('map_texture.reset_tiling');
        //hack to reset slider position
        setSlidersRendered(false);
        requestAnimationFrame(() => {
            setSlidersRendered(true);
        });
    }

    const options = [
        { value: 'option1', label: texturePack },
        { value: 'option2', label: 'Tropical Map Theme' },
        { value: 'option3', label: '[CS1] Cleyra' },
        { value: 'option4', label: '[CS1] Seychelles' },
        { value: 'option5', label: 'Load from file...' },
    ];

    const onSelectionChanged1 = (value) => {
        console.log(`Selected value: ${value}`);
    };

    return <$PanelMod react={react} title="Map Texture Replacer">
        <div className="field_MBO">
            <div className="label_DGc label_ZLb">Pack Loaded:</div>
            <$DropdownMod react={react} onSelectionChanged={onSelectionChanged1} selected={options[0].value} options={options} />
        </div>
        <button className="button_WWa button_SH8" style={{ marginTop: '-10rem', marginBottom: '20rem' }} onClick={() => engine.trigger(`map_texture.open_texture_zip`)}>Load Texture Pack</button>

        <TextureSelectUI label="Grass Diffuse" textureType="gd" />
        <TextureSelectUI label="Grass Normal" textureType="gn" />
        <TextureSelectUI label="Dirt Diffuse" textureType="dd" />
        <TextureSelectUI label="Dirt Normal" textureType="dn" />
        <TextureSelectUI label="Cliff Diffuse" textureType="cd" />
        <TextureSelectUI label="Cliff Normal" textureType="cn" />

        {sliders.map((slider, index) => <SliderComponent key={index} react={react} slider={slider} isRendered={slidersRendered} />)}

        <div className="field_MBO" style={{ minHeight: '52.5rem' }} >
            <button className="button_WWa button_SH8" onClick={handleButtonClick}>Reset Tiling</button>
        </div>

    </$PanelMod>
}     

//Registering the panel with HookUI so it shows up in the menu
window._$hookui.registerPanel({
    id: "example.map_texture",
    name: "Map Texture Replacer",
    icon: "Media/Game/Icons/MapTile.svg",
    component: $Counter
})