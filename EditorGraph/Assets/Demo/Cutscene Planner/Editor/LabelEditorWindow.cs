using UnityEditor;
using UnityEngine;
using CutscenePlanner.Editor.Controls;
using CutscenePlanner.Editor.Controls.LinePanels;
using System;

namespace CutscenePlanner.Editor
{
    /// <summary> Represents the scene label editor window. </summary>
    public abstract class LabelEditorWindow : EditorWindow
    {
        /// <summary> True if the editor is now launched. False otherwise. </summary>
        public static bool IsShown { get { return _isShown; } }
        protected Label _owner;
        protected string _title;

        [SerializeField]
        private TimeTextField _startTimeField;
        [SerializeField]
        private TimeTextField _endTimeField;

        private static bool _isShown;
        private static bool _firstDrawAfterDataSet;

        protected LabelEditorWindow()  {  }

        /// <summary> Shows the scene label editor window. </summary>
        /// <param name="label">Data to fill.</param>
        public static void Show(Label label)
        {
            LabelEditorWindow window;
            if (label is SceneLabel)
            {
                window = GetWindow<SceneLabelEditorWindow>(true);
                window.titleContent = new GUIContent("Scene label editor");
            }
            else if (label is TimeLabel)
            {
                window = GetWindow<TimeLabelEditorWindow>(true);
                window.titleContent = new GUIContent("Time label editor");
            }
            else return;

            window.SetData(label);
            window.minSize = new Vector2(400, 75);
            window.maxSize = window.minSize;
            _isShown = true;
        }
        /// <summary> Method invoked at the begine of OnGUI.</summary>
        public abstract void OnGUIBegin();
        /// <summary> Method invoked at the end of OnGUI.</summary>
        public abstract void OnGUIEnd();
        /// <summary> Sets the new time label data for the editor.</summary>
        /// <param name="sceneLabel">Data to fill editor. </param>
        public void SetData(Label sceneLabel)
        {
            _owner = sceneLabel;
            _firstDrawAfterDataSet = true;
        }
        private void Update()
        {
            if (_owner == null || _owner.SourceData == null)
                Close(); 
        }
        void OnGUI()
        {
            if (_owner != null && _owner.SourceData != null)
            {
                if (_firstDrawAfterDataSet)
                {
                    GUIUtility.keyboardControl = 0;
                    _firstDrawAfterDataSet = false;
                }
                OnGUIBegin();

                _owner.Color = EditorGUILayout.ColorField(new GUIContent("Color: "), _owner.Color);

                if (_startTimeField == null)
                    _startTimeField = new TimeTextField();
                if (_endTimeField == null)
                    _endTimeField = new TimeTextField();

                Rect lastRect = GUILayoutUtility.GetLastRect();
                Rect startFieldRect = GUILayoutUtility.GetRect(lastRect.width, EditorGUIUtility.singleLineHeight);
                TimeSpan newStart = _startTimeField.Draw(startFieldRect, _owner.SourceData.Start, new GUIContent("Start time:"), ArrowsLayout.Horizontal);
                lastRect = GUILayoutUtility.GetLastRect();
                Rect endFieldRect = GUILayoutUtility.GetRect(lastRect.width, EditorGUIUtility.singleLineHeight);
                TimeSpan newEnd = _endTimeField.Draw(endFieldRect, _owner.SourceData.End, new GUIContent("End time:"), ArrowsLayout.Horizontal);

                _owner.SetStartAndEnd(newStart, newEnd, _owner.SourceData.Duration);

                OnGUIEnd();
            }
        }
        private void OnDestroy()
        {
            _isShown = false;
        }
        void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}