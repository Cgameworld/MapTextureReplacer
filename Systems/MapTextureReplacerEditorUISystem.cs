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
    public partial class MapTextureReplacerEditorUISystem : GameSystemBase
    {
        private View? m_UIView;

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        public void CreateAssetEditorButton()
        {
            UnityEngine.Debug.Log("CreateAssetEditorButton() Loaded!");
            m_UIView = GameManager.instance.userInterface.view.View;

            //cleanup injected react code if it exists
            m_UIView.ExecuteScript("document.querySelector(\".maptexturereplacer_custom_container\")?.parentNode?.removeChild(document.querySelector(\".maptexturereplacer_custom_container\"));");

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
