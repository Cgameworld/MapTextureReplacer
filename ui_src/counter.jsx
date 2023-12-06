import React from 'react'
import {useDataUpdate, $Panel} from 'hookui-framework'

const $Counter = ({react}) => {
    // This sets up the currentCount as local state
    const [currentCount, setCurrentCount] = react.useState(0)

    // useDataUpdate binds the result of the GetterValueBinding to currentCount
    useDataUpdate(react, 'vehicle_counter.current_vehicle_count', setCurrentCount)

    // Below, engine.trigger is responsible for triggering the TriggerBinding in the UI System
    return <$Panel react={react} title="Map Texture">
        <div className="field_MBO">
            <div className="label_DGc label_ZLb">Active vehicles</div>
            <div>{currentCount}</div>
        </div>
        <div>
            <button className="button_WWa button_SH8" onClick={() => engine.trigger('vehicle_counter.remove_vehicles')}>Remove vehicles</button>
        </div>
    </$Panel>
}

/ Registering the panel with HookUI so it shows up in the menu
window._$hookui.registerPanel({
    id: "example.map_texture",
    name: "Map Texture",
    icon: "Media/Game/Icons/MapTile.svg",
    component: $Counter
})