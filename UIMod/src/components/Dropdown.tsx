import React, { useEffect, useRef, useState } from 'react';
import ReactDOM from 'react-dom';
import engine from 'cohtml/cohtml';

export interface DropdownOption {
    value: string;
    label: string;
    source?: string;
}

interface ScrollableDropdownProps {
    children?: React.ReactNode;
    maxHeight: number;
    itemCount: number;
}

// Scrollable container with a custom scrollbar matching the game UI.
const ScrollableDropdown: React.FC<ScrollableDropdownProps> = ({ children, maxHeight, itemCount }) => {
    const containerRef = useRef<HTMLDivElement | null>(null);
    const innerRef = useRef<HTMLDivElement | null>(null);
    const scrollAreaRef = useRef<HTMLDivElement | null>(null);
    const scrollTopRef = useRef(0);
    const [scrollTop, setScrollTop] = useState(0);
    const [measuredHeight, setMeasuredHeight] = useState(0);
    const [thumbHover, setThumbHover] = useState(false);
    const [thumbActive, setThumbActive] = useState(false);

    // Estimate ensures scrollbar appears immediately; measurement refines scroll range.
    const estimatedHeight = 35 * itemCount;

    useEffect(() => {
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

    useEffect(() => {
        if (!needsScroll) return;

        const onWheel = (e: any) => {
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

    // Native hover + mousedown on the entire track area.
    useEffect(() => {
        const el = scrollAreaRef.current;
        if (!el) return;

        const onEnter = () => setThumbHover(true);
        const onLeave = () => { if (!thumbActive) setThumbHover(false); };

        const onDown = (e: MouseEvent) => {
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

            const onMove = (me: MouseEvent) => {
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
        <div ref={containerRef} style={{ maxHeight: maxHeight + 'rem', overflow: 'hidden', position: 'relative' }}>
            <div ref={innerRef}
                style={{
                    position: needsScroll ? 'relative' : undefined,
                    top: needsScroll ? (-clamped + 'rem') : undefined,
                    paddingRight: needsScroll ? '16rem' : undefined,
                }}>
                {children}
            </div>
            {needsScroll && (
                <div ref={scrollAreaRef} style={{ position: 'absolute', top: '6rem', bottom: '6rem', right: '6rem', width: '4rem', zIndex: 1 }}>
                    {/* Track */}
                    <div style={{ position: 'absolute', top: 0, left: 0, right: 0, bottom: 0, backgroundColor: 'rgba(255,255,255, 0.15)', borderRadius: '2rem' }} />
                    {/* Thumb */}
                    <div style={{
                        position: 'absolute',
                        left: 0, right: 0,
                        top: `${topPct}%`,
                        height: `${thumbPct}%`,
                        backgroundColor: `rgba(255,255,255, ${thumbActive ? 1 : thumbHover ? 0.8 : 0.6})`,
                        borderRadius: '2rem',
                        pointerEvents: 'none',
                    }} />
                </div>
            )}
        </div>
    );
};

interface DropdownProps {
    options: DropdownOption[];
    selected: string;
    onSelectionChanged: (value: string) => void;
    style?: React.CSSProperties;
    dropdownTextChar?: number | null;
}

const Dropdown: React.FC<DropdownProps> = ({ options, selected, onSelectionChanged, style, dropdownTextChar = null }) => {
    const [active, setActive] = useState(false);
    const [portalContainer, setPortalContainer] = useState<HTMLElement | null>(null);
    const pickerRef = useRef<HTMLDivElement | null>(null);
    const dropdownRef = useRef<HTMLDivElement | null>(null);

    const handleClickOutside = (event: MouseEvent) => {
        if (pickerRef.current && !pickerRef.current.contains(event.target as Node) &&
            dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
            setActive(false);
        }
    };

    useEffect(() => {
        let container = document.getElementById('select-portal');
        if (!container) {
            container = document.createElement('div');
            container.id = 'select-portal';
            document.body.appendChild(container);
        }
        setPortalContainer(container);

        document.addEventListener('click', handleClickOutside, true);
        return () => {
            document.removeEventListener('click', handleClickOutside, true);
        };
    }, []);

    useEffect(() => {
        if (active) {
            document.addEventListener('click', handleClickOutside, true);
        } else {
            document.removeEventListener('click', handleClickOutside, true);
        }
    }, [active]);

    const getDropdownPosition = (): React.CSSProperties => {
        if (pickerRef.current) {
            const rect = pickerRef.current.getBoundingClientRect();
            return { top: rect.bottom + window.scrollY, left: rect.left + window.scrollX, width: rect.width };
        }
        return {};
    };

    const getDropdownMaxHeight = () => {
        if (pickerRef.current) {
            const rect = pickerRef.current.getBoundingClientRect();
            return Math.max(100, Math.min(500, window.innerHeight - rect.bottom - 200));
        }
        return 300;
    };

    const onToggle = () => {
        setActive(!active);
        engine.trigger('audio.playSound', 'select-dropdown', 1);
    };

    const changeSelection = (value: string) => {
        setActive(false);
        onSelectionChanged(value);
    };

    let selectedIndex = options.findIndex(o => o.value === selected);
    if (selectedIndex === -1) {
        selectedIndex = 0;
    }
    const selectedOption = options[selectedIndex];

    const dropdownContent = active ? (
        <div ref={dropdownRef} style={{ display: 'flex', position: 'absolute', ...getDropdownPosition(), zIndex: 9999 }}>
            <div className="dropdown-popup_mMv" style={{ maxWidth: 'inherit', width: '100%' }}>
                <div className="dropdown-menu_jf2 dropdown-menu_Swd">
                    <ScrollableDropdown maxHeight={getDropdownMaxHeight()} itemCount={options.length}>
                        {options.map((option) => (
                            <button key={option.value} className="dropdown-item_sZT" style={{ padding: '5rem', height: 'auto', whiteSpace: 'pre-wrap', display: 'flex', alignItems: 'center', gap: '4rem' }} onClick={() => changeSelection(option.value)}>
                                {option.source === 'local' && (
                                    <div className="tinted-icon_iKo" style={{ maskImage: 'url(Media/Glyphs/Save.svg)', width: '16rem', height: '16rem', flexShrink: 0 }} />
                                )}
                                <span>{option.label}</span>
                            </button>
                        ))}
                    </ScrollableDropdown>
                </div>
            </div>
        </div>
    ) : null;

    const displayLabel = selected === 'Loading...'
        ? selected
        : (dropdownTextChar != null && selectedOption && selectedOption.label.length >= dropdownTextChar
            ? selectedOption.label.substring(0, dropdownTextChar) + '...'
            : selectedOption?.label);

    return (
        <div>
            <div ref={pickerRef} className="dropdown-toggle_V9z dropdown-toggle_prl value-field_yJi value_PW_ dropdown_pJu item-states_QjV" onClick={onToggle} style={{ padding: '5rem', height: 'auto', backgroundColor: dropdownTextChar != null ? 'rgba(0, 0, 0, 0.15)' : 'rgba(0, 0, 0, 0.6)', ...style }}>
                {selected !== 'Loading...' && selectedOption && selectedOption.source === 'local' && (
                    <div className="tinted-icon_iKo" style={{ maskImage: 'url(Media/Glyphs/Save.svg)', width: '16rem', height: '16rem', flexShrink: 0, marginRight: '4rem' }} />
                )}
                <div className="label_l_4" style={{ position: 'relative', top: '1rem' }}>{displayLabel}</div>
                <div className="tinted-icon_iKo indicator_Xmj" style={{ marginLeft: '0rem', maskImage: 'url(Media/Glyphs/StrokeArrowDown.svg)' }}></div>
                {portalContainer && dropdownContent && ReactDOM.createPortal(dropdownContent, portalContainer)}
            </div>
        </div>
    );
};

export default Dropdown;
