using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace CutscenePlanner.Editor
{
    /// <summary> Contains overridden methods for editor actions. </summary>
    public static class CutscenePlannerEditorActions
    {
        /// <summary> Shows cutscene planner windows.</summary>
        /// <param name="data"></param>
        public static void ShowWindow(CutsceneAsset data)
        {
            CutscenePlannerWindow window = EditorWindow.GetWindow<CutscenePlannerWindow>(); 
            if (data != null)
                window.LoadData(data);
            window.Focus();
        }
        /// <summary> Add option to Unity file inspector to create Cutscene file.</summary>
        [MenuItem("Assets/Create/Cutscene")]
        public static void AddCutscene()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
                path = "Assets";
            else if (System.IO.Path.GetExtension(path) != "")
                path = path.Replace(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");

            CutsceneAsset newFile = ScriptableObject.CreateInstance<CutsceneAsset>();
            newFile.DefaultInit();

            newFile.CreateAsset(path, "New Cutscene");
        }
        /// <summary> Handles file open in Unity file inspector to open cutscene planner if file is CutsceneAsset.  </summary>
        /// <param name="instanceID">Id of the instanced file.</param>
        /// <param name="line">??</param>
        /// <returns>Always false.</returns>
        [OnOpenAsset]
        public static bool OpenFile(int instanceID, int line)
        {
            CutsceneAsset data = EditorUtility.InstanceIDToObject(instanceID) as CutsceneAsset;
            if (data!=null)
                ShowWindow(data);
            return false;
        }
    }
}