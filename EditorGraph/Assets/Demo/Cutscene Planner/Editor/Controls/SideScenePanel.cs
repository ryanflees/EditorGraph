using UnityEngine;
using UnityEditor;
using CutscenePlanner.Editor.Utils;

namespace CutscenePlanner.Editor.Controls
{
    /// <summary> Represents a side window scene panel. </summary>
    public class SideScenePanel : Panel
    {
        /// <summary> On click event delegate. </summary>
        /// <param name="scene">Scene source of the component.</param>
        /// <param name="selected">True if this click was selecting panel one. False otherwise.</param>
        public delegate void ClickDelegate(SceneData scene, bool selected);
        /// <summary> On delete event delegate</summary>
        /// <param name="scene">Scene source of the component.</param>
        public delegate void DeleteDelegate(SceneData scene);
        /// <summary> Title click event. </summary>
        public event ClickDelegate Click;
        /// <summary> Delete event. </summary>
        public event DeleteDelegate Delete;

        /// <summary> Source data for this panel. </summary>
        public SceneData Scene { get { return _scene; } }

        private SceneData _scene;
        private bool _isExpanded;
        private float _expandedHeight = 315;
        private bool _firstDrawAfterExpand;
        private GUIStyle _textAreaStyle;

        /// <summary> Create new scene Panel</summary>
        public SideScenePanel() : base()
        {
            _scene = new SceneData();
        }
        /// <summary> Create new scene Panel</summary>
        /// <param name="scene"> Source data.</param>
        public SideScenePanel(SceneData scene) : base()
        {
            _scene = scene;
        }
        /// <summary> Draws the component.</summary>
        /// <param name="rect">Size of the component header.</param>
        /// <param name="forceRedraw">Fore the texture to redraw, even if new and old rect is the same.</param>
        /// <returns>Real component size.</returns>
        public override Rect Draw(Rect rect, bool forceRedraw= false)
        {
            Rect originalRect = rect;
            GUIContent labelContent = new GUIContent(_scene.Name);
            float labelHeight = new GUIStyle(GUI.skin.label).CalcHeight(labelContent, rect.width);
            GUIContent arrowContent = new GUIContent(_isExpanded ? "▼" : "►");
            Vector2 arrowSize = new GUIStyle(GUI.skin.label).CalcSize(arrowContent);
            Rect arrowRect = new Rect(rect.width - arrowSize.x - 5, rect.y + rect.height / 2 - arrowSize.y / 2, rect.width, rect.height);
            if (_textAreaStyle== null)
            {
                _textAreaStyle = new GUIStyle(GUI.skin.textArea)
                {
                    wordWrap = true
                };
            }
            base.Draw(rect, forceRedraw);

            if (_isExpanded)
            {
                _rect = new Rect(rect.x, rect.y, rect.width, rect.height + _expandedHeight);
                for (int i = 0; i < _backgroundTexture.width; i++)
                    _backgroundTexture.SetPixel(i, (int)(_rect.height - originalRect.height), BorderColor);
                _backgroundTexture.Apply();
                if (_firstDrawAfterExpand)
                {
                    GUIUtility.keyboardControl = 0;
                    _firstDrawAfterExpand = false;
                }

                float fieldHeight = 15;
                float fieldSpace = 3;
                GUIContent noTextureText = new GUIContent("No concept art :(");
                Vector2 noTextureTextSize = new GUIStyle(GUI.skin.label).CalcSize(noTextureText);
                Rect textureRect = new Rect(_rect.x + 5, 2 + _rect.y + originalRect.height + 2 * fieldHeight + 2 * fieldSpace, _rect.width - 10, 150);

                _scene.Name = EditorGUI.TextField(new Rect(_rect.x, _rect.y + 2 + originalRect.height, _rect.width-5, fieldHeight), new GUIContent("Scene name: "), _scene.Name);
                _scene.ConceptArt = EditorGUI.ObjectField(new Rect(_rect.x, 2 + _rect.y + originalRect.height + fieldHeight + fieldSpace, _rect.width - 5, fieldHeight), new GUIContent("Concept art: "), _scene.ConceptArt, typeof(Texture2D), false) as Texture2D;


                EditorGUI.DrawRect(textureRect, Color.gray);
                
                if (_scene.ConceptArt != null)
                    EditorGUI.DrawPreviewTexture(textureRect, _scene.ConceptArt, null, ScaleMode.ScaleToFit);
                else
                    GUI.Label(new Rect(textureRect.width / 2 - noTextureTextSize.x / 2, textureRect.y + textureRect.height / 2 - noTextureTextSize.y / 2, noTextureTextSize.x, noTextureTextSize.y), noTextureText);

                Rect descRect = new Rect(_rect.x + 5, 2 + _rect.y + originalRect.height + 2 * fieldHeight + 150 + 3 * fieldSpace, _rect.width - 10, 100);
                _scene.Desc = EditorGUI.TextArea(descRect, _scene.Desc, _textAreaStyle);
                GUIContent timeContent = new GUIContent("Start: " + _scene.Start.ToStringSpecial() + " | End: " + _scene.End.ToStringSpecial() + " (" + _scene.Duration.TotalSeconds + "s) ");
                GUI.Label(new Rect(_rect.x, descRect.y + descRect.height + fieldSpace, _rect.width, fieldHeight), timeContent);

                if (_scene.RemoveRequest || GUI.Button(new Rect(_rect.width - 65, descRect.y + descRect.height + fieldSpace, 60, fieldHeight), new GUIContent("Delete")))
                {
                    _scene.MarkToDelete();
                    if (Delete != null)
                        Delete.Invoke(Scene);
                }
            }

            GUI.Label(new Rect(_rect.x, _rect.y + (_rect.height - (_isExpanded ? _expandedHeight : 0)) / 2 - labelHeight / 2 , _rect.width, _rect.height), labelContent);
            
            GUI.Label(arrowRect, arrowContent);
            if (originalRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseUp)
            {
                ToogleExpand();
                if (Click != null)
                    Click.Invoke(Scene, IsSelected);
            }
            return _rect;
        }
        /// <summary> Expands the panel. </summary>
        /// <param name="expand">True for panel expand. False for hide.</param>
        public void Expand(bool expand)
        {
            _firstDrawAfterExpand = expand;
            Select(expand);
            _isExpanded = expand;
        }
        private void ToogleExpand()
        {
            Expand(!_isExpanded);
        }
    }
}