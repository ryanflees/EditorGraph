using UnityEditor;
using UnityEngine;

namespace CutscenePlanner.Editor
{
    /// <summary> Represents the time label editor window. </summary>
    public class TimeLabelEditorWindow : LabelEditorWindow
    {
        /// <summary> Method invoked at the begin of OnGUI.</summary>
        public override void OnGUIBegin()
        {
            _owner.SourceData.Name = EditorGUILayout.TextField(new GUIContent("Text: "), _owner.SourceData.Name);
        }
        /// <summary> Method invoked at the end of OnGUI.</summary>
        public override void OnGUIEnd() { }
        
    }
}