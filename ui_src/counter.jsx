import React from 'react'
import { useDataUpdate, $Panel } from 'hookui-framework'

const Field = ({ label, textureType }) => (
    <div className="field_MBO">
        <div className="label_DGc label_ZLb">{label}</div>
        <button className="button_WWa button_SH8" onClick={() => engine.trigger(`map_texture.open_image_${textureType}`)}>Select Image</button>
        <button className="button_WWa button_SH8" onClick={() => engine.trigger(`map_texture.reset_texture_${textureType}`)}>Reset</button>
    </div>
);

const $Counter = ({ react }) => {
    // This sets up the currentCount as local state
    // const [currentCount, setCurrentCount] = react.useState(0)

    // useDataUpdate binds the result of the GetterValueBinding to currentCount
    // useDataUpdate(react, 'map_texture.current_vehicle_count', setCurrentCount)

    // Below, engine.trigger is responsible for triggering the TriggerBinding in the UI System
    return <$Panel react={react} title="Map Texture Replacer">
        <Field label="Grass Diffuse" textureType="gd" />
        <Field label="Grass Normal" textureType="gn" />
        <Field label="Dirt Diffuse" textureType="dd" />
        <Field label="Dirt Normal" textureType="dn" />
        <Field label="Cliff Diffuse" textureType="cd" />
        <Field label="Cliff Normal" textureType="cn" />
    </$Panel>
}

//Registering the panel with HookUI so it shows up in the menu
window._$hookui.registerPanel({
    id: "example.map_texture",
    name: "Map Texture Replacer",
    icon: "Media/Game/Icons/MapTile.svg",
    component: $Counter
})