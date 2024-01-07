import React from 'react';
import { useDataUpdate } from 'hookui-framework'
import $PanelMod from './panel_modified'
import $SliderMod from './slider_modified'
import $DropdownMod from './dropdown_modified'

var packsrefreshed = false;
var currentpackdropdown = "none";

const TextureSelectUI = ({ react, options, label, textureType, selectedImage, filePath }) => {
    const [selectedDefault, setSelectedDefault] = react.useState("none");
    const [localOptions, setLocalOptions] = react.useState([...options]);

    ///
    react.useEffect(() => {
        //console.log("packsrefreshedran!");
        let newItem = {
            "value": "loadimage",
            "label": "Load Image..."
        };
        let filteredOptions = options.filter(option => option.value !== 'loadzipfile');

        setLocalOptions([...filteredOptions, newItem]);

        //doesn't work immediately
        setTimeout(() => {
            setSelectedDefault(currentpackdropdown);
        }, 250);

        packsrefreshed = false;
    }, [packsrefreshed]);


    const onSelectionChanged = (selection) => {
        if (selection == "loadzipfile") {
            engine.trigger(`map_texture.open_image_${textureType}`);
        }
        else {
            console.log("dropdownval: " + selection);
            console.log(localOptions);
            console.log(selectedDefault);
        }
    };

    return (
        <div className="field_MBO" style={{ minHeight: '52.5rem' }} >
            <div className="label_DGc label_ZLb">{label}</div>
            <$DropdownMod react={react} style={{ width: '40%' }} onSelectionChanged={onSelectionChanged} selected={selectedDefault} options={localOptions} />
            <button className="button_WWa button_SH8" onClick={() => engine.trigger(`map_texture.reset_texture_${textureType}`)}>Reset {filePath}</button>
        </div>
    );
};



const TextureSelectUIs = ({ react, options }) => {

    const labels = ["Grass Diffuse", "Grass Normal", "Dirt Diffuse", "Dirt Normal", "Cliff Diffuse", "Cliff Normal"];
    const types = ["gd", "gn", "dd", "dn", "cd", "cn"];

    const textureTypesData = labels.map((label, index) => {
        return { label: label, type: types[index], selectedImage: "Select Image", filepath: "" };
    });

    const [textureTypes, setTextureTypes] = react.useState(textureTypesData);


    const [getTextureSelectData, setGetTextureSelectData] = react.useState();
    useDataUpdate(react, 'map_texture.get_texture_select_data', setGetTextureSelectData)

    //load in new data when packs change 
    react.useEffect(() => {
        if (getTextureSelectData) {
            var newitems = JSON.parse(getTextureSelectData);
            console.log("getTextureSelectData updated!")
            console.log(newitems);

            const newTextureTypes = textureTypes.map((textureType, index) => {
                return {
                    ...textureType,
                    selectedImage: newitems[index]?.Key || textureType.selectedImage,
                    filepath: newitems[index]?.Value || textureType.filepath
                };
            });
            setTextureTypes(newTextureTypes);
        }
    }, [getTextureSelectData]);

    return textureTypes.map(texture =>
        <TextureSelectUI
            key={texture.type}
            label={texture.label}
            textureType={texture.type}
            selectedImage={texture.selectedImage}
            filePath={texture.filePath}
            react={react}
            options={options}
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
        { value: 'loadzipfile', label: 'Load from zip...' },
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
            packsrefreshed = true;
        }
    }, [getDetectedPacks]);



    const [onSelectedPackDropdown, setOnSelectedPackDropdown] = react.useState(options[0].value)

    const onSelectionChanged1 = (value) => {
        setOnSelectedPackDropdown(value);
        currentpackdropdown = value;
        if (value == "loadzipfile") {
            engine.trigger(`map_texture.open_texture_zip`);
        }
        else {
            console.log("dropdownval: " + value);
            engine.trigger('map_texture.change_pack', value);
            packsrefreshed = true;
            //hack to reset slider position
            setSlidersRendered(false);
            requestAnimationFrame(() => {
                setSlidersRendered(true);
            });
        }
    };

    //add zip file to general dropdown options

    react.useEffect(() => {
        if (texturePack && texturePack != ",") {
            setOptions(prevOptions => {
                if (!prevOptions.some(option => option.value === texturePack)) {
                    let newOptions = [...prevOptions];
                    newOptions.splice(newOptions.length - 1, 0, { value: texturePack, label: texturePack.split(",")[0] });
                    return newOptions;
                }
                return prevOptions;
            });
            console.log(options);

            onSelectionChanged1(texturePack);
            packsrefreshed = true;
        }
    }, [texturePack]);



    return <$PanelMod react={react} title="Map Texture Replacer">
        <div className="field_MBO">
            <div className="label_DGc label_ZLb">Pack Loaded:</div>
            <div style={{ width: '68%' }}>
                <$DropdownMod react={react} onSelectionChanged={onSelectionChanged1} selected={onSelectedPackDropdown} options={options} />
            </div>
        </div>

        <TextureSelectUIs react={react} options={options} />

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