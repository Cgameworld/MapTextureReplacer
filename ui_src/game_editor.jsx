import React from 'react';
import { createRoot } from 'react-dom/client';
import { createPortal } from 'react-dom';
import { useDataUpdate, $Panel } from 'hookui-framework'

const Render = ({ react }) => {
    const [showCounter, setShowCounter] = react.useState(false);

    return (
        <div>
            <button id="MapTextureReplacer-EditorButton" className="button_M6C button_M6C button_wKY" onClick={() => setShowCounter(prevState => !prevState)}>
                <img className="icon_PhD" src="Media/Game/Icons/MapTile.svg" />
            </button>
            {showCounter && <$Counter react={react} />}
        </div>
    );
};

const $Counter = ({ react }) => {
    // This sets up the currentCount as local state
    const [currentCount, setCurrentCount] = react.useState(0)

    // useDataUpdate binds the result of the GetterValueBinding to currentCount
    useDataUpdate(react, 'vehicle_counter.current_vehicle_count', setCurrentCount)

    // Below, engine.trigger is responsible for triggering the TriggerBinding in the UI System
    return (
        <$Panel react={react} title="OK">
           
        </$Panel>
    );
}

// Injection Script
const injectionPoint = document.getElementsByClassName('inspector-modes_ur5')[0];

// Create a new div element
const newDiv = document.createElement('div');
newDiv.className = 'eeee';
injectionPoint.appendChild(newDiv);

const root = createRoot(newDiv);
root.render(createPortal(<Render react={React} />, newDiv));
