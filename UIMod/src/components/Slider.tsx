import React, { useEffect, useLayoutEffect, useRef, useState } from 'react';
import engine from 'cohtml/cohtml';
import { VanillaComponentsResolver } from '../../types/internal';

interface SliderProps {
    title: string;
    min: number;
    max: number;
    value: number;
    step?: number;
    alert?: string | null;  // when set, shows a yellow warning triangle with this text in a tooltip
    onChange: (value: number) => void;
}

const Slider: React.FC<SliderProps> = ({ title, min, max, value, step = 0.01, alert, onChange }) => {
    const { DescriptionTooltip } = VanillaComponentsResolver.instance;
    // snap to the step and clean up floating-point drift (e.g. 1.2000000000000002 -> 1.2)
    const snap = (v: number) => Math.round(Math.round(v / step) * step * 1e6) / 1e6;
    const [sliderWidth, setSliderWidth] = useState(0);
    const [inputValue, setInputValue] = useState<number | string>(value);
    const sliderRef = useRef<HTMLDivElement | null>(null);
    const scaleRef = useRef(1);
    const draggingRef = useRef(false);

    // 155.90866 is the track width (rem) at the default UI scale; refined on drag from the live rect.
    const syncFromValue = (v: number) => {
        const scale = (max - min) / 155.90866;
        scaleRef.current = scale;
        const width = (v - min) / scale;
        setSliderWidth(width > max / scale ? max / scale : width);
        setInputValue(v);
    };

    useLayoutEffect(() => {
        syncFromValue(value);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    // keep the slider in sync when the value changes externally (e.g. Reset Tiling)
    useEffect(() => {
        if (!draggingRef.current) syncFromValue(value);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [value]);

    const handleMouseDown = (e: React.MouseEvent) => {
        e.preventDefault();
        draggingRef.current = true;

        const handleMouseMove = (event: MouseEvent) => {
            if (!sliderRef.current) return;
            const rect = sliderRef.current.getBoundingClientRect();
            const scale = (max - min) / rect.width;
            scaleRef.current = scale;
            const newWidth = Math.min(Math.max(event.clientX - rect.left, 0), rect.width);
            setSliderWidth(newWidth);
            const newValue = snap(min + newWidth * scale);
            setInputValue(newValue);
            onChange(newValue);
            engine.trigger('audio.playSound', 'drag-slider', 1);
        };

        const handleMouseUp = () => {
            draggingRef.current = false;
            document.removeEventListener('mousemove', handleMouseMove);
            document.removeEventListener('mouseup', handleMouseUp);
        };

        document.addEventListener('mousemove', handleMouseMove);
        document.addEventListener('mouseup', handleMouseUp);
    };

    const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        const raw = event.target.value;
        setInputValue(raw);
        const num = Number(raw);
        if (!isNaN(num)) {
            if (num >= min && num <= max) setSliderWidth((num - min) / scaleRef.current);
            else if (num > max) setSliderWidth(max / scaleRef.current);
            onChange(num);
        }
    };

    return (
        <div className="field_MBO" style={{ minHeight: '52.5rem' }}>
            <div className="row_d2o">
                <div className="label_ZLb label_test2" style={{ width: '140rem', marginRight: '12rem', display: 'flex', alignItems: 'center', overflow: 'visible' }}>
                    {title}
                    {alert ? (
                        <DescriptionTooltip title="Tiling Changed" description={alert}>
                            <div className="tinted-icon_iKo" style={{ maskImage: 'url(Media/Glyphs/Warning.svg)', backgroundColor: 'var(--warningColor)', width: '16rem', height: '16rem', flexShrink: 0, marginLeft: '4rem' }} />
                        </DescriptionTooltip>
                    ) : null}
                </div>
                <div style={{ flexGrow: 1, flexShrink: 1, display: 'flex', flexDirection: 'row', width: '55%', position: 'relative', left: '20rem', top: '5rem' }}>
                    <div className="slider-container_Q_K" style={{ height: '10rem' }}>
                        <div className="slider_KXG slider_pUS horizontal slider_ROT">
                            <div className="track-bounds_H8_" ref={sliderRef}>
                                <div className="range-bounds_lNt" style={{ width: `${(sliderWidth / (window.innerWidth / 1920))}rem` }} onMouseDown={handleMouseDown}>
                                    <div className="range_nHO range_iUN"></div>
                                    <div className="thumb-container_aso">
                                        <div className="thumb_kkL"></div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <input className="slider-input_DXM input_Wfi" style={{ marginTop: '-10rem' }} type="text" value={inputValue} onChange={handleInputChange} />
                </div>
            </div>
        </div>
    );
};

export default Slider;
