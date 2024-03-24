import React from 'react';
import { createRoot } from 'react-dom/client';
import { createPortal } from 'react-dom';

import $Main from './main'


const Render = ({ react }) => {
    const [showCounter, setShowCounter] = react.useState(false);

    react.useEffect(() => {
        const handleCustomEvent = () => {
            setShowCounter(window.mapTextureReplacerShowWindow);
            console.log("window.maptexturereplacershowwindow updated!!");
        };

        window.addEventListener('mapTextureReplacerShowWindowChanged', handleCustomEvent);

        // Cleanup on unmount
        return () => {
            window.removeEventListener('mapTextureReplacerShowWindowChanged', handleCustomEvent);
        };
    }, []);

    const handleClose = () => {
        setShowCounter(false);
    };

    return (
        <div>
            {showCounter && <$Main react={react} onClose={handleClose} />}
        </div>
    );
};


// Injection Script
const injectionPoint = document.body;

// Create a new div element
const newDiv = document.createElement('div');
newDiv.className = 'maptexturereplacer_custom_container';
injectionPoint.insertBefore(newDiv, injectionPoint.firstChild);

const root = createRoot(newDiv);
root.render(createPortal(<Render react={React} />, newDiv));

