//from hookui-framework

import React from 'react'
import * as styles from './styles'

const defaultStyle = {
    position: 'absolute',
    width: "400rem",
    height: "690rem"
}

const $CloseButton = ({ onClick }) => {
    return <button className="button_bvQ button_bvQ close-button_wKK" onClick={onClick}>
        <div className="tinted-icon_iKo icon_PhD" style={{ maskImage: 'url(Media/Glyphs/Close.svg)' }}></div>
    </button>
}

const $PanelMod = ({ title, children, react, style }) => {
    const [position, setPosition] = react.useState({ top: 100, left: 10 });
    const [dragging, setDragging] = react.useState(false);
    const [rel, setRel] = react.useState({ x: 0, y: 0 }); // Position relative to the cursor

    const onMouseDown = (e) => {
        if (e.button !== 0) return; // Only left mouse button
        const panelElement = e.target.closest('.panel_YqS');

        // Calculate the initial relative position
        const rect = panelElement.getBoundingClientRect();
        setRel({
            x: e.clientX - rect.left,
            y: e.clientY - rect.top,
        });

        setDragging(true);
        e.stopPropagation();
        e.preventDefault();
    }

    const onMouseUp = (e) => {
        setDragging(false);
        // Remove window event listeners when the mouse is released
        window.removeEventListener('mousemove', onMouseMove);
        window.removeEventListener('mouseup', onMouseUp);
    }

    const onMouseMove = (e) => {
        if (!dragging) return;

        setPosition({
            top: e.clientY - rel.y,
            left: e.clientX - rel.x,
        });
        e.stopPropagation();
        e.preventDefault();
    }

    const onClose = (e) => {
        const data = { type: "toggle_visibility", id: "example.map_texture" };
        const event = new CustomEvent('hookui', { detail: data });
        window.dispatchEvent(event);
        engine.trigger("audio.playSound", "select-item", 1);
    }

    const draggableStyle = {
        ...defaultStyle,
        top: position.top + 'px',
        left: position.left + 'px',
        ...style
    }

    react.useEffect(() => {
        if (dragging) {
            // Attach event listeners to window
            window.addEventListener('mousemove', onMouseMove);
            window.addEventListener('mouseup', onMouseUp);
        }

        return () => {
            // Clean up event listeners when the component unmounts or dragging is finished
            window.removeEventListener('mousemove', onMouseMove);
            window.removeEventListener('mouseup', onMouseUp);
        };
    }, [dragging]); // Only re-run the effect if dragging state changes

    return (
        <div className="panel_YqS" style={draggableStyle}>
            <div className="header_H_U header_Bpo child-opacity-transition_nkS"
                onMouseDown={onMouseDown}>
                <div className="title-bar_PF4">
                    <div className="icon-space_h_f"></div>
                    <div className="title_SVH title_zQN">{title}</div>
                    <$CloseButton onClick={onClose} />
                </div>
            </div>
            <div className="content_XD5 content_AD7 child-opacity-transition_nkS">
                {children}
            </div>
        </div>
    );
}

export default $PanelMod