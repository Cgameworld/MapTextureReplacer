import React, { useEffect, useRef, useState } from 'react';
import engine from 'cohtml/cohtml';

interface DraggablePanelProps {
    title: string;
    onClose: () => void;
    style?: React.CSSProperties;
    children?: React.ReactNode;
}

const defaultStyle: React.CSSProperties = {
    position: 'absolute',
    width: '400rem',
};

const CloseButton: React.FC<{ onClick: () => void }> = ({ onClick }) => (
    <button className="button_bvQ button_bvQ close-button_wKK" onClick={onClick}>
        <div className="tinted-icon_iKo icon_PhD" style={{ maskImage: 'url(Media/Glyphs/Close.svg)' }}></div>
    </button>
);

const DraggablePanel: React.FC<DraggablePanelProps> = ({ title, onClose, style, children }) => {
    const [position, setPosition] = useState({ top: 60, left: 10 });
    const [dragging, setDragging] = useState(false);
    const relRef = useRef({ x: 0, y: 0 }); // cursor offset within the panel

    const onMouseDown = (e: React.MouseEvent) => {
        if (e.button !== 0) return; // left button only
        const panelElement = (e.target as HTMLElement).closest('.panel_YqS');
        if (!panelElement) return;

        const rect = panelElement.getBoundingClientRect();
        relRef.current = { x: e.clientX - rect.left, y: e.clientY - rect.top };

        setDragging(true);
        e.stopPropagation();
        e.preventDefault();
    };

    useEffect(() => {
        if (!dragging) return;

        const onMouseMove = (e: MouseEvent) => {
            setPosition({ top: e.clientY - relRef.current.y, left: e.clientX - relRef.current.x });
            e.stopPropagation();
            e.preventDefault();
        };
        const onMouseUp = () => setDragging(false);

        window.addEventListener('mousemove', onMouseMove);
        window.addEventListener('mouseup', onMouseUp);

        return () => {
            window.removeEventListener('mousemove', onMouseMove);
            window.removeEventListener('mouseup', onMouseUp);
        };
    }, [dragging]);

    const handleClose = () => {
        engine.trigger('audio.playSound', 'select-item', 1);
        onClose();
    };

    const draggableStyle: React.CSSProperties = {
        ...defaultStyle,
        top: position.top + 'rem',
        left: position.left + 'rem',
        ...style,
    };

    return (
        <div className="panel_YqS" style={draggableStyle}>
            <div className="header_H_U header_Bpo child-opacity-transition_nkS" onMouseDown={onMouseDown}>
                <div className="title-bar_PF4">
                    <div className="icon-space_h_f"></div>
                    <div className="title_SVH title_zQN">{title}</div>
                    <CloseButton onClick={handleClose} />
                </div>
            </div>
            <div className="content_XD5 content_AD7 child-opacity-transition_nkS">
                {children}
            </div>
        </div>
    );
};

export default DraggablePanel;
