import React from 'react';
import { useDataUpdate } from 'hookui-framework'
import $PanelMod from './panel_modified'
import $SliderMod from './slider_modified'
import $DropdownMod from './dropdown_modified'

const TextureSelectUI = ({ label, textureType, selectImageText, filePath }) => (
    <div className="field_MBO" style={{ minHeight: '52.5rem' }} >
        <div className="label_DGc label_ZLb">{label}</div>
        <button className="button_WWa button_SH8" style={{ width: '37.5%'}} onClick={() => engine.trigger(`map_texture.open_image_${textureType}`)}>{selectImageText}</button>
        <button className="button_WWa button_SH8" onClick={() => engine.trigger(`map_texture.reset_texture_${textureType}`)}>Reset {filePath}</button>
    </div>
);

const TextureSelectUIs = ({ react }) => {

    const [textureTypes, setTextureTypes] = react.useState([
        { label: "Grass Diffuse", type: "gd", selectImageText: "Select Image", filepath: "" },
        { label: "Grass Normal", type: "gn", selectImageText: "Select Image", filepath: "" },
        { label: "Dirt Diffuse", type: "dd", selectImageText: "1Select Image", filepath: "" },
        { label: "Dirt Normal", type: "dn", selectImageText: "Select Image", filepath: "" },
        { label: "Cliff Diffuse", type: "cd", selectImageText: "Select Image", filepath: "" },
        { label: "Cliff Normal", type: "cn", selectImageText: "Select Image", filepath: "" },
    ]);

    return textureTypes.map(texture =>
        <TextureSelectUI
            key={texture.type}
            label={texture.label}
            textureType={texture.type}
            selectImageText={texture.selectImageText}
            filePath={texture.filePath}
        />
    );
};

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

    const [options, setOptions] = react.useState([
        { value: 'none', label: 'Default' },
        { value: 'loadfile', label: 'Load from file...'},
    ]);

    const [getDetectedPacks, setGetDetectedPacks] = react.useState()
    useDataUpdate(react, 'map_texture.get_detected_packs', setGetDetectedPacks)

    //load in packs dectected from the mod folder
    react.useEffect(() => {
        if (getDetectedPacks) {
            var newitems = JSON.parse(getDetectedPacks);
            for (let key in newitems) {
                let newItem = {
                    "value": key,
                    "label": newitems[key]
                };
                options.splice(options.length - 1, 0, newItem);
            }
        }
    }, [getDetectedPacks]);

    

    const [onSelectedPackDropdown, setOnSelectedPackDropdown] = react.useState(options[0].value)

    const onSelectionChanged1 = (value) => {
        setOnSelectedPackDropdown(value);
        if (value == "loadfile") {
            engine.trigger(`map_texture.open_texture_zip`);
        }
        else {
            console.log("dropdownval: " + value);
            engine.trigger('map_texture.change_pack', value);
            //hack to reset slider position
            setSlidersRendered(false);
            requestAnimationFrame(() => {
                setSlidersRendered(true);
            });
        }
    };

    //in general have variable to get filepath when adding files manually
    //then find way to feed all three paramters on load to options const

    react.useEffect(() => {
        if (texturePack && texturePack != ",") {
            setOptions(prevOptions => {               
                if (!prevOptions.some(option => option.value === texturePack)) {
                    let newOptions = [...prevOptions];
                    newOptions.splice(newOptions.length - 1, 0, { value: texturePack, label: texturePack.split(",")[0]});
                    return newOptions;
                }
                return prevOptions;
            });
            console.log(options);

            onSelectionChanged1(texturePack);
        }
    }, [texturePack]);



    return <$PanelMod react={react} title="Map Texture Replacer">
        <div className="field_MBO">
            <div className="label_DGc label_ZLb">Pack Loaded:</div>
            <$DropdownMod react={react} onSelectionChanged={onSelectionChanged1} selected={onSelectedPackDropdown} options={options} />
        </div>

        <TextureSelectUIs react={react} />

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