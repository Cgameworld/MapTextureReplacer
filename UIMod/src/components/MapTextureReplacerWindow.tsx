import React, { useEffect, useMemo, useRef, useState } from 'react';
import { useValue } from 'cs2/api';
import { Scrollable } from 'cs2/ui';
import engine from 'cohtml/cohtml';
import { VanillaComponentsResolver } from '../../types/internal';
import styles from './MapTextureReplacerWindow.module.scss';
import DraggablePanel from './DraggablePanel';
import Dropdown, { DropdownOption } from './Dropdown';
import Slider from './Slider';
import TextureSelect from './TextureSelect';
import {
    WindowOpen$, SetWindowOpen,
    DetectedPacks$, ActivePackDropdown$, TexturePack$, TextureFloats$,
    ChangePack, SetActivePackDropdown, ResetTextureSelectData, OpenTextureZip,
    ChangeFloatField, ResetTiling,
} from '../bindings';

interface FloatEntry {
    name: string;
    label: string;
    value: number;
    min: number;
    max: number;
    group: string;
}

type TilingSlidersProps = {
    group: string;
    resetLabel: string;
};

const TilingSliders: React.FC<TilingSlidersProps> = ({ group, resetLabel }) => {
    const raw = useValue(TextureFloats$);
    let floats: FloatEntry[] = [];
    try { floats = JSON.parse(raw || '[]'); } catch { floats = []; }

    const matching = floats.filter(f => f.group === group);

    if (matching.length === 0) return null;

    return (
        <div>
            {matching.map(f => (
                <Slider key={f.name} title={f.label} min={f.min} max={f.max} value={f.value} onChange={(v) => ChangeFloatField(f.name, v)}
                />
            ))}
            <div className="field_MBO" style={{ minHeight: '52.5rem' }}>
                <button className="button_WWa button_SH8" onClick={() => { ResetTiling(group); engine.trigger('audio.playSound', 'select-item', 1); }}>
                    Reset {resetLabel}
                </button>
            </div>
        </div>
    );
};

const MapTextureReplacerWindow: React.FC = () => {
    const open = useValue(WindowOpen$);
    const persistedPack = useValue(ActivePackDropdown$);
    const detected = useValue(DetectedPacks$);
    const texturePack = useValue(TexturePack$);

    const [activeTab, setActiveTab] = useState<string>('GRASS');
    const [selectedPack, setSelectedPack] = useState<string>(persistedPack);
    const [zipOptions, setZipOptions] = useState<DropdownOption[]>([]);
    const lastTexturePackRef = useRef<string>(texturePack); // baseline so we ignore the value present at mount

    // mirror the persisted (C#) base-pack selection
    useEffect(() => { setSelectedPack(persistedPack); }, [persistedPack]);

    // a zip chosen via the file dialog arrives as "name,path" on the texture_pack binding.
    // Only react to a genuine change (not the stale value already present at mount).
    useEffect(() => {
        if (texturePack === lastTexturePackRef.current) return;
        lastTexturePackRef.current = texturePack;
        if (texturePack && texturePack !== ',' && texturePack.includes(',')) {
            setZipOptions(prev => prev.some(o => o.value === texturePack) ? prev : [...prev, { value: texturePack, label: texturePack.split(',')[0] }]);
            setSelectedPack(texturePack);
            ChangePack(texturePack);
            SetActivePackDropdown(texturePack);
        }
    }, [texturePack]);

    const packOptions = useMemo<DropdownOption[]>(() => {
        try {
            const obj = JSON.parse(detected || '{}');
            return Object.keys(obj).map(key => ({ value: key, label: obj[key].name, source: obj[key].source }));
        } catch {
            return [];
        }
    }, [detected]);

    const { TabBar, Tab } = VanillaComponentsResolver.instance;

    const onSelectTab = (id: unknown) => {
        engine.trigger('audio.playSound', 'select-item', 1);
        setActiveTab(id as string);
    };

    const onBasePackChange = (value: string) => {
        ResetTextureSelectData();
        if (value === 'loadzipfile') {
            OpenTextureZip();
            return;
        }

        setSelectedPack('Loading...');
        setTimeout(() => {
            ChangePack(value);
            SetActivePackDropdown(value);
            setSelectedPack(value);
        }, 100);
    };

    const basePackOptions: DropdownOption[] = [
        { value: 'none', label: 'Default' },
        ...packOptions,
        ...zipOptions,
        { value: 'loadzipfile', label: 'Load from zip...' },
    ];

    const TABS: { id: string; label: string; content: React.ReactNode }[] = [
        {
            id: 'GRASS', label: 'Grass', content: (
                <>
                    <TextureSelect index={0} label="Grass Diffuse" packOptions={packOptions} />
                    <TextureSelect index={1} label="Grass Normal" packOptions={packOptions} />
                    <TilingSliders group="grass" resetLabel="Grass" />
                </>
            )
        },
        {
            id: 'DIRT', label: 'Dirt', content: (
                <>
                    <TextureSelect index={2} label="Dirt Diffuse" packOptions={packOptions} />
                    <TextureSelect index={3} label="Dirt Normal" packOptions={packOptions} />
                    <TilingSliders group="dirt" resetLabel="Dirt" />
                </>
            )
        },
        {
            id: 'ROCK', label: 'Rock', content: (
                <>
                    <TextureSelect index={4} label="Rock Diffuse" packOptions={packOptions} />
                    <TextureSelect index={5} label="Rock Normal" packOptions={packOptions} />
                    <TilingSliders group="rock" resetLabel="Rock" />
                </>
            )
        },
        {
            id: 'PAINTED', label: 'Painted', content: (
                <>
                    <TextureSelect index={6} label="Extra 1 Diffuse" packOptions={packOptions} />
                    <TextureSelect index={7} label="Extra 1 Normal" packOptions={packOptions} />
                    <TextureSelect index={8} label="Extra 2 Diffuse" packOptions={packOptions} />
                    <TextureSelect index={9} label="Extra 2 Normal" packOptions={packOptions} />
                    <TextureSelect index={10} label="Extra 3 Diffuse" packOptions={packOptions} />
                    <TextureSelect index={11} label="Extra 3 Normal" packOptions={packOptions} />
                    <TextureSelect index={12} label="Extra 4 Diffuse" packOptions={packOptions} />
                    <TextureSelect index={13} label="Extra 4 Normal" packOptions={packOptions} />
                    <TilingSliders group="extra" resetLabel="Painted" />
                </>
            )
        },
        {
            id: 'COMMON', label: 'Common', content: <TilingSliders group="common" resetLabel="Common" />
        },
    ];

    if (!open) return null;

    const activeContent = (TABS.find(t => t.id === activeTab) ?? TABS[0]).content;

    return (
        <DraggablePanel title="Map Texture Replacer" onClose={() => SetWindowOpen(false)} style={{ width: '450rem' }}>
            <div style={{ padding: '8rem' }}>
                <div className="field_MBO" style={{ minHeight: '52.5rem', marginTop: '4rem' }}>
                    <div className="label_DGc label_ZLb">Base Pack</div>
                    <div style={{ width: '67%' }}>
                        <Dropdown options={basePackOptions} selected={selectedPack} onSelectionChanged={onBasePackChange} />
                    </div>
                </div>

                <TabBar className={styles.tabBar}>
                    {TABS.map(tab => (
                        <Tab key={tab.id} id={tab.id} selectedId={activeTab} onSelect={onSelectTab} className={styles.tab}>
                            {tab.label}
                        </Tab>
                    ))}
                </TabBar>

                <Scrollable vertical trackVisibility="scrollable" style={{ maxHeight: '570rem', marginTop: '8rem' }}>
                    {activeContent}
                </Scrollable>

            </div>
        </DraggablePanel>
    );
};

export default MapTextureReplacerWindow;
