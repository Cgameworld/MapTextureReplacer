using cohtml.Net;
using Game;
using Game.SceneFlow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace MapTextureReplacer.Systems
{
    public class MapTextureReplacerEditorUISystem : GameSystemBase
    {
        private View m_UIView;

        protected override void OnCreate()
        {
            base.OnCreate();
            CreateAssetEditorButton();
        }

        public void CreateAssetEditorButton()
        {
            UnityEngine.Debug.Log("CreateAssetEditorButton() Loaded!");
            m_UIView = GameManager.instance.userInterface.view.View;

            //load custom react code
            using Stream embeddedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MapTextureReplacer.dist.game_editor.compiled.js");
            using System.IO.StreamReader reader = new(embeddedStream);
            {
                m_UIView.ExecuteScript(reader.ReadToEnd());
            }
        }

        protected override void OnUpdate() { }

    }
}
