import React, { useMemo } from 'react';
import { useValue } from 'cs2/api';
import engine from 'cohtml/cohtml';
import Dropdown, { DropdownOption } from './Dropdown';
import { TextureSelectData$, OpenImage, ResetTexture } from '../bindings';

interface TextureSelectProps {
    index: number;  // stable slot index into textureSelectData (0..13)
    label: string;  // "Grass Diffuse" / "Extra 1 Normal" etc.
    packOptions: DropdownOption[];
}

const TextureSelect: React.FC<TextureSelectProps> = ({ index, label, packOptions }) => {
    const raw = useValue(TextureSelectData$);

    const entry = useMemo<{ Key: string; Value: string }>(() => {
        try {
            const data = JSON.parse(raw || '[]');
            return data[index] || { Key: 'Default', Value: 'none' };
        } catch {
            return { Key: 'Default', Value: 'none' };
        }
    }, [raw, index]);

    const currentValue = entry.Value;
    const isPack = packOptions.some(o => o.value === currentValue);

    // For the extra slots (index >= 6) only list packs that actually ship that texture; always keep the current pick.
    const visiblePacks = useMemo<DropdownOption[]>(() =>
        index >= 6
            ? packOptions.filter(o => (o.slots?.includes(index) ?? true) || o.value === currentValue)
            : packOptions,
        [packOptions, index, currentValue]);

    // Default + detected packs + Load Image..., plus a 'sel-' entry for a custom file selection.
    const options = useMemo<DropdownOption[]>(() => {
        const list: DropdownOption[] = [
            { value: 'none', label: 'Default' },
            ...visiblePacks,
            { value: 'loadimage', label: 'Load Image...' },
        ];
        if (currentValue !== 'none' && !isPack) {
            list.unshift({ value: 'sel-' + currentValue, label: entry.Key });
        }
        return list;
    }, [visiblePacks, currentValue, isPack, entry.Key]);

    const selected = currentValue === 'none' ? 'none' : (isPack ? currentValue : 'sel-' + currentValue);

    const onSelectionChanged = (value: string) => {
        if (value === 'loadimage') OpenImage(index, '');
        else if (value === 'none') ResetTexture(index);
        else if (value.startsWith('sel-')) OpenImage(index, value.replace('sel-', ''));
        else OpenImage(index, value);
    };

    return (
        <div className="field_MBO" style={{ minHeight: '52.5rem' }}>
            <div className="label_DGc label_ZLb" style={{ marginRight: '5rem' }}>{label}</div>
            <div style={{ width: '45%' }}>
                <Dropdown options={options} selected={selected} onSelectionChanged={onSelectionChanged} dropdownTextChar={11} />
            </div>
            <button className="button_WWa button_SH8" onClick={() => { ResetTexture(index); engine.trigger('audio.playSound', 'select-item', 1); }}>
                Reset
            </button>
        </div>
    );
};

export default TextureSelect;
