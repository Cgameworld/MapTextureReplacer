import React from 'react'
import ReactDOM from 'react-dom';

// Scrollable container with custom scrollbar matching game UI
const ScrollableDropdown = ({ react, children, maxHeight, itemCount }) => {
    const containerRef = react.useRef(null);
    const innerRef = react.useRef(null);
    const scrollAreaRef = react.useRef(null);
    const scrollTopRef = react.useRef(0);
    const [scrollTop, setScrollTop] = react.useState(0);
    const [measuredHeight, setMeasuredHeight] = react.useState(0);
    const [thumbHover, setThumbHover] = react.useState(false);
    const [thumbActive, setThumbActive] = react.useState(false);

    // Estimate ensures scrollbar appears immediately; measurement refines scroll range
    const estimatedHeight = 35 * itemCount;

    // Measure actual content height via getBoundingClientRect after render
    react.useEffect(() => {
        if (!innerRef.current) return;
        const h = innerRef.current.getBoundingClientRect().height;
        if (h > measuredHeight + 1) {
            setMeasuredHeight(h);
        }
    });

    const contentHeight = measuredHeight > 0 ? Math.max(estimatedHeight, measuredHeight) : estimatedHeight;
    const needsScroll = contentHeight > maxHeight;
    const maxScroll = needsScroll ? contentHeight - maxHeight : 0;
    const clamped = Math.max(0, Math.min(scrollTop, maxScroll));

    scrollTopRef.current = clamped;
    const thumbPct = needsScroll ? Math.max(10, (maxHeight / contentHeight) * 100) : 0;
    const topPct = maxScroll > 0 ? (clamped / maxScroll) * (100 - thumbPct) : 0;

    
    react.useEffect(() => {
        if (!needsScroll) return;

        const onWheel = (e) => {
            if (!containerRef.current) return;

            
            const rect = containerRef.current.getBoundingClientRect();
            if (e.clientX < rect.left || e.clientX > rect.right ||
                e.clientY < rect.top || e.clientY > rect.bottom) return;

            
            let delta = 0;
            if (e.deltaY !== undefined && e.deltaY !== 0) {
                delta = e.deltaY;
            } else if (e.wheelDelta !== undefined && e.wheelDelta !== 0) {
                delta = -e.wheelDelta / 3;
            } else if (e.detail !== undefined && e.detail !== 0) {
                delta = e.detail * 10;
            }

            if (delta === 0) return;

            const newScroll = Math.max(0, Math.min(maxScroll, scrollTopRef.current + delta));
            setScrollTop(newScroll);

            try { e.preventDefault(); } catch (ex) { }
            try { e.stopPropagation(); } catch (ex) { }
        };

        document.addEventListener('wheel', onWheel, true);
        document.addEventListener('mousewheel', onWheel, true);
        document.addEventListener('DOMMouseScroll', onWheel, true);

        return () => {
            document.removeEventListener('wheel', onWheel, true);
            document.removeEventListener('mousewheel', onWheel, true);
            document.removeEventListener('DOMMouseScroll', onWheel, true);
        };
    }, [needsScroll, maxScroll]);

    // Native hover + mousedown on the entire track area
    react.useEffect(() => {
        const el = scrollAreaRef.current;
        if (!el) return;

        const onEnter = () => setThumbHover(true);
        const onLeave = () => { if (!thumbActive) setThumbHover(false); };

        const onDown = (e) => {
            e.preventDefault();
            e.stopPropagation();
            setThumbActive(true);

            const areaRect = el.getBoundingClientRect();
            const areaH = areaRect.height;
            const clickRatio = (e.clientY - areaRect.top) / areaH;
            const jumpScroll = Math.max(0, Math.min(maxScroll, clickRatio * maxScroll));
            setScrollTop(jumpScroll);

            const startY = e.clientY;
            const startScroll = jumpScroll;
            const thumbH = areaH * thumbPct / 100;
            const range = areaH - thumbH;

            const onMove = (me) => {
                if (range <= 0) return;
                const dy = me.clientY - startY;
                setScrollTop(Math.max(0, Math.min(maxScroll, startScroll + (dy / range) * maxScroll)));
            };

            const onUp = () => {
                setThumbActive(false);
                setThumbHover(false);
                document.removeEventListener('mousemove', onMove);
                document.removeEventListener('mouseup', onUp);
            };

            document.addEventListener('mousemove', onMove);
            document.addEventListener('mouseup', onUp);
        };

        el.addEventListener('mouseenter', onEnter);
        el.addEventListener('mouseleave', onLeave);
        el.addEventListener('mousedown', onDown);

        return () => {
            el.removeEventListener('mouseenter', onEnter);
            el.removeEventListener('mouseleave', onLeave);
            el.removeEventListener('mousedown', onDown);
        };
    }, [needsScroll, thumbActive, maxScroll, thumbPct, clamped]);

    return (
        <div ref={containerRef}
            style={{ maxHeight: maxHeight + 'rem', overflow: 'hidden', position: 'relative' }}>
            <div ref={innerRef}
                style={{
                    position: needsScroll ? 'relative' : undefined,
                    top: needsScroll ? (-clamped + 'rem') : undefined,
                    paddingRight: needsScroll ? '16rem' : undefined,
                }}>
                {children}
            </div>
            {needsScroll && (
                <div ref={scrollAreaRef} style={{
                    position: 'absolute',
                    top: '6rem',
                    bottom: '6rem',
                    right: '6rem',
                    width: '4rem',
                    zIndex: 1,
                }}>
                    {/* Track */}
                    <div style={{
                        position: 'absolute',
                        top: 0, left: 0, right: 0, bottom: 0,
                        backgroundColor: 'rgba(255,255,255, 0.15)',
                        borderRadius: '2rem',
                    }} />
                    {/* Thumb */}
                    <div
                        style={{
                            position: 'absolute',
                            left: 0, right: 0,
                            top: `${topPct}%`,
                            height: `${thumbPct}%`,
                            backgroundColor: `rgba(255,255,255, ${thumbActive ? 1 : thumbHover ? 0.8 : 0.6})`,
                            borderRadius: '2rem',
                            pointerEvents: 'none',
                        }}
                    />
                </div>
            )}
        </div>
    );
};


//from legacyflavor

const $DropdownMod = ({ react, style, onSelectionChanged, selected, options, dropdownTextChar = null }) => {
    const [active, setActive] = react.useState(false);
    const [internalValue, setInternalValue] = react.useState(selected);
    const [portalContainer, setPortalContainer] = react.useState(null);
    const pickerRef = react.useRef(null); // Ref to attach to the select field
    const dropdownRef = react.useRef(null); // Ref for the dropdown content

    // Function to check if the click is outside the dropdown
    const handleClickOutside = (event) => {
        if (pickerRef.current && !pickerRef.current.contains(event.target) &&
            dropdownRef.current && !dropdownRef.current.contains(event.target)) {
            setActive(false);
        }
    };

    react.useEffect(() => {
        // Create a single container for the portal if not already created
        if (!document.getElementById('select-portal')) {
            const container = document.createElement('div');
            container.id = 'select-portal';
            document.body.appendChild(container);
            setPortalContainer(container);
        } else {
            setPortalContainer(document.getElementById('select-portal'));
        }

        // Add event listener to close the dropdown when clicking outside
        document.addEventListener('click', handleClickOutside, true);

        // Cleanup the event listener
        return () => {
            document.removeEventListener('click', handleClickOutside, true);
        };
    }, []);

    react.useEffect(() => {
        setInternalValue(selected);
    }, [selected]);

    react.useEffect(() => {
        // Toggle the click listener based on dropdown state
        if (active) {
            document.addEventListener('click', handleClickOutside, true);
        } else {
            document.removeEventListener('click', handleClickOutside, true);
        }
    }, [active]);

    const getDropdownPosition = () => {
        if (pickerRef.current) {
            const rect = pickerRef.current.getBoundingClientRect();
            return {
                top: rect.bottom + window.scrollY,
                left: rect.left + window.scrollX,
                width: rect.width
            };
        }
        return {};
    };

    const getDropdownMaxHeight = () => {
        if (pickerRef.current) {
            const rect = pickerRef.current.getBoundingClientRect();
            // Cap at 500rem height
            return Math.max(100, Math.min(500, window.innerHeight - rect.bottom - 200));
        }
        return 300;
    };

    const onToggle = () => {
        setActive(!active);
        engine.trigger("audio.playSound", "select-dropdown", 1);
    };

    const changeSelection = (value) => {
        setInternalValue(value);
        onSelectionChanged(value);
    };

    let selectedIndex = options.findIndex(o => o.value === internalValue);

    if (selectedIndex === -1) {
        // Handle error here
        //console.log('MapTextureReplacer - No matching option found for the given internal value.');
        selectedIndex = 0;
    }


    // Define the dropdown content
    const dropdownContent = active ? (
        <div ref={dropdownRef} style={{
            display: 'flex',
            position: 'absolute',
            ...getDropdownPosition(),
            zIndex: 9999
        }}>
            <div className="dropdown-popup_mMv" style={{ maxWidth: 'inherit', 'width': '100%' }}>
                <div className="dropdown-menu_jf2 dropdown-menu_Swd">
                    <ScrollableDropdown react={react} maxHeight={getDropdownMaxHeight()} itemCount={options.length}>
                        {
                            options.map((option) => (
                                <button key={option.value} className="dropdown-item_sZT" style={{ padding: '5rem', height: 'auto', whiteSpace: 'pre-wrap', display: 'flex', alignItems: 'center', gap: '4rem' }} onClick={() => changeSelection(option.value)}>
                                    {option.source === "local" && (
                                        <div className="tinted-icon_iKo" style={{ maskImage: 'url(Media/Glyphs/Save.svg)', width: '16rem', height: '16rem', flexShrink: 0 }} />
                                    )}
                                    <span>{option.label}</span>
                                </button>
                            ))
                        }
                    </ScrollableDropdown>
                </div>
            </div>
        </div>
    ) : null;

    return (
        <div>
            <div ref={pickerRef} className="dropdown-toggle_V9z dropdown-toggle_prl value-field_yJi value_PW_ dropdown_pJu item-states_QjV" onClick={onToggle} style={{ padding: '5rem', height: 'auto', backgroundColor: dropdownTextChar != null ? 'rgba(0, 0, 0, 0.15)' : 'rgba(0, 0, 0, 0.6)', ...style }}>
                {selected !== "Loading..." && options[selectedIndex] && options[selectedIndex].source === "local" && (
                    <div className="tinted-icon_iKo" style={{ maskImage: 'url(Media/Glyphs/Save.svg)', width: '16rem', height: '16rem', flexShrink: 0, marginRight: '4rem' }} />
                )}
                <div className="label_l_4" style={{ position: 'relative', top: '1rem' }}>
                    {selected === "Loading..."
                        ? selected
                        : dropdownTextChar != null && options[selectedIndex].label.length >= dropdownTextChar
                            ? options[selectedIndex].label.substring(0, dropdownTextChar) + "..."
                            : options[selectedIndex].label}
                </div>
                <div className="tinted-icon_iKo indicator_Xmj" style={{ marginLeft: '0rem', maskImage: 'url(Media/Glyphs/StrokeArrowDown.svg)' }}></div>
                {portalContainer && dropdownContent && ReactDOM.createPortal(dropdownContent, portalContainer)}
            </div>
        </div>
    );

}

export default $DropdownMod