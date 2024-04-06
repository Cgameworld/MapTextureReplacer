import React from 'react'

const $SliderMod = ({ react, title, min, max, sliderPos, onInputChange }) => {
    const [sliderWidth, setSliderWidth] = react.useState(0);
    const [inputValue, setInputValue] = react.useState(sliderPos);
    const sliderRef = react.useRef();
    const [scale, setScale] = react.useState(1);


    react.useLayoutEffect(() => {
        //find better solution than hardcoding - this doesn't resolve immediately
        //const scale = (max - min) / sliderRef.current.getBoundingClientRect().width;
        const scale = (max - min) / 155.90866;

        setScale(scale);

        const sliderWidth = (sliderPos - min) / scale;
        if (sliderWidth > max) {
            setSliderWidth(max / scale);
        }

        else {
            setSliderWidth(sliderWidth);
        }

    }, []);

    const handleMouseDown = (e) => {
        e.preventDefault();

        const handleMouseMove = (event) => {
            const scale = (max - min) / sliderRef.current.getBoundingClientRect().width;
            setScale(scale);
            console.log(sliderRef.current.getBoundingClientRect().width);
            const newWidth = Math.min(Math.max(event.clientX - sliderRef.current.getBoundingClientRect().left, 0), sliderRef.current.getBoundingClientRect().width);
            setSliderWidth(newWidth);
            const newInputValue = Math.round(min + newWidth * scale);
            setInputValue(newInputValue);
            onInputChange(newInputValue);

            engine.trigger("audio.playSound", "drag-slider", 1);
        };

        const handleMouseUp = () => {
            document.removeEventListener('mousemove', handleMouseMove);
            document.removeEventListener('mouseup', handleMouseUp);
        };

        document.addEventListener('mousemove', handleMouseMove);
        document.addEventListener('mouseup', handleMouseUp);
    };

    const handleInputChange = (event) => {
        const newValue = event.target.value;
        setInputValue(newValue);

        if (newValue >= min && newValue <= max) {
            setSliderWidth((newValue - min) / scale);
        }
        else if (newValue > max) {
            setSliderWidth(max / scale);
        }

        onInputChange(newValue);
    };

    return (
        <div className="field_MBO" style={{ minHeight: '52.5rem' }}>
            <div className="row_d2o">
                <div className="label_ZLb label_test2" style={{ width: '100rem', marginRight: '5rem' }} >{title}</div>
                <div className="control_Hds" style={{ width: '67.5%', position: 'relative', left: '20rem' }}>
                    <div className="slider-container_Q_K">
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
                    <input className="slider-input_DXM input_Wfi" type="text" value={inputValue} onChange={handleInputChange} />
                </div>
            </div>
        </div>
    );
};

export default $SliderMod