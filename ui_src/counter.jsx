import React from 'react'
import {useDataUpdate, $Panel} from 'hookui-framework'

const $Counter = ({react}) => {
    // This sets up the currentCount as local state
    const [currentCount, setCurrentCount] = react.useState(0)

    // useDataUpdate binds the result of the GetterValueBinding to currentCount
    useDataUpdate(react, 'vehicle_counter.current_vehicle_count', setCurrentCount)

    // Below, engine.trigger is responsible for triggering the TriggerBinding in the UI System
    return <$Panel react={react} title="Map Texture Replacer" style="height:430rem">
        <div className="field_MBO">
            <div className="label_DGc label_ZLb">Grass Diffuse</div>
            <button className="button_WWa button_SH8" onClick={() => engine.trigger('vehicle_counter.remove_vehicles')}>Select Image</button>
        </div>
        <div className="field_MBO">
            <div className="label_DGc label_ZLb">Grass Normal</div>
            <button className="button_WWa button_SH8" onClick={() => engine.trigger('vehicle_counter.remove_vehicles')}>Select Image</button>
        </div>
        <div className="field_MBO">
            <div className="label_DGc label_ZLb">Dirt Diffuse</div>
            <button className="button_WWa button_SH8" onClick={() => engine.trigger('vehicle_counter.remove_vehicles')}>Select Image</button>
        </div>
        <div className="field_MBO">
            <div className="label_DGc label_ZLb">Dirt Normal</div>
            <button className="button_WWa button_SH8" onClick={() => engine.trigger('vehicle_counter.remove_vehicles')}>Select Image</button>
        </div>
        <div className="field_MBO">
            <div className="label_DGc label_ZLb">Cliff Diffuse</div>
            <button className="button_WWa button_SH8" onClick={() => engine.trigger('vehicle_counter.remove_vehicles')}>Select Image</button>
        </div>
        <div className="field_MBO">
            <div className="label_DGc label_ZLb">Cliff Normal</div>
            <button className="button_WWa button_SH8" onClick={() => engine.trigger('vehicle_counter.remove_vehicles')}>Select Image</button>
        </div>
    </$Panel>
}

//Registering the panel with HookUI so it shows up in the menu
window._$hookui.registerPanel({
    id: "example.map_texture",
    name: "Map Texture Replacer",
    icon: "Media/Game/Icons/MapTile.svg",
    component: $Counter
})