import React, { useEffect } from 'react';
import ReactDOM from 'react-dom';
import { bindValue, useValue } from 'cs2/api';
import { WindowOpen$ } from '../bindings';

const ShowCameraHeight$ = bindValue<boolean>('map_texture', 'show_camera_height');
const CameraHeight$ = bindValue<number>('map_texture', 'camera_height');

const CreatorToolsPanel: React.FC = () => {
    const cameraHeight = useValue(CameraHeight$);
    return (
        <div className="panel_YqS expanded collapsible advisor-panel_dXi advisor-panel_mrr top-right-panel_A2r" style={{
            position: 'absolute',
            top: '60rem',
            right: '0rem',
            display: 'flex',
            width: '260rem'
        }}>
            <div className="header_H_U header_Bpo child-opacity-transition_nkS">
                <div className="title-bar_PF4">
                    <div className="icon-space_h_f"></div>
                    <div className="title_SVH title_zQN">Creator Tools</div>
                </div>
            </div>
            <div className="content_XD5 content_AD7 child-opacity-transition_nkS">
                <div className="content_gqa">
                    <div className="infoview-panel-section_RXJ">
                        <div className="content_1xS focusable_GEc item-focused_FuT">
                            <div className="row_S2v" style={{ paddingTop: '10rem', paddingBottom: '10rem' }}>
                                <div className="left_Lgw row_S2v" style={{ fontSize: '18rem', alignItems: 'center' }}>Camera Height</div>
                                <div className="right_k3O row_S2v" style={{ fontSize: '18rem', alignItems: 'center' }}>{cameraHeight.toFixed(1)} m</div>
                            </div>
                            <div className="bottom-padding_JS3"></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

const CreatorToolsWindow: React.FC = () => {
    const showCameraHeight = useValue(ShowCameraHeight$);
    const mainWindowOpen = useValue(WindowOpen$);

    useEffect(() => {
        const parentElement = document.querySelector('.main-container__E2');
        if (showCameraHeight && mainWindowOpen && parentElement) {
            const root = document.createElement('div');
            root.id = 'maptexturereplacer-creator-tools-root';
            root.style.width = '100%';
            parentElement.appendChild(root);
            ReactDOM.render(<CreatorToolsPanel />, root);
            return () => {
                ReactDOM.unmountComponentAtNode(root);
                parentElement.removeChild(root);
            };
        }
    }, [showCameraHeight, mainWindowOpen]);

    return null;
};

export default CreatorToolsWindow;
