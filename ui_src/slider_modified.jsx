import React from 'react'

const $SliderMod = ({ react, min, max, sliderPos }) => {
    const [sliderWidth, setSliderWidth] = react.useState(0);
    const [inputValue, setInputValue] = react.useState(sliderPos);
    const sliderRef = react.useRef();
    const [scale, setScale] = react.useState(1);

    //find better solution later - sliderRef.current.getBoundingClientRect().width doesn't resolve immediately
    react.useLayoutEffect(() => {
        setTimeout(() => {
            const scale = (max - min) / sliderRef.current.getBoundingClientRect().width;
            setScale(scale);
            setSliderWidth((sliderPos - min) / scale);
        }, 250);
    }, []);

    const handleMouseDown = (e) => {
        e.preventDefault();

        const handleMouseMove = (event) => {
            const scale = (max - min) / sliderRef.current.getBoundingClientRect().width;
            setScale(scale);

            const newWidth = Math.min(Math.max(event.clientX - sliderRef.current.getBoundingClientRect().left, 0), sliderRef.current.getBoundingClientRect().width);
            //console.log("newWidth" + newWidth);
            setSliderWidth(newWidth);
            setInputValue(Math.round(min + newWidth * scale));
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
    };

    return (
        <div className="field_MBO">
            <div className="row_d2o">
                <div className="label_DGc label_ZLb">Tiling Test</div>
                <div className="control_Hds" style={{ width: '67.5%', position: 'relative', left: '20rem' }}>
                    <div className="slider-container_Q_K">
                        <div className="slider_KXG slider_pUS horizontal slider_ROT">
                            <div className="track-bounds_H8_" ref={sliderRef}>
                                <div className="range-bounds_lNt" style={{ width: `${sliderWidth}rem` }} onMouseDown={handleMouseDown}>
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