import React from 'react';
import { createRoot } from 'react-dom/client';
import { createPortal } from 'react-dom';


import $Main from './main'

const Render = ({ react }) => {
    const [showCounter, setShowCounter] = react.useState(false);

    const handleClose = () => {
        engine.trigger("audio.playSound", "select-item", 1);
        setShowCounter(false);
    };

    return (
        <div>
            <button id="MapTextureReplacer-EditorButton" className="button_M6C button_M6C button_wKY" onClick={() => setShowCounter(prevState => !prevState)}>
                <img className="icon_PhD" src="Media/Game/Icons/MapTile.svg" />
            </button>
            {showCounter && <$Main react={react} onClose={handleClose}/>}
        </div>
    );
};

// Injection Script
const injectionPoint = document.getElementsByClassName('inspector-modes_ur5')[0];

// Create a new div element
const newDiv = document.createElement('div');
newDiv.className = 'maptexturereplacer_custom_container';
injectionPoint.insertBefore(newDiv, injectionPoint.firstChild);

const root = createRoot(newDiv);
root.render(createPortal(<Render react={React} />, newDiv));
