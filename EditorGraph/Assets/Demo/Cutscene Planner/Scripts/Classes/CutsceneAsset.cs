using UnityEngine;
using System.IO;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CutscenePlanner
{
    /// <summary> Represents the cutscene plan save data.</summary>
    public class CutsceneAsset : ScriptableObject
    {
        /// <summary> Extension for CutsceneAsset data asset.</summary>
        public const string EXT = ".asset";

        /// <summary> Cutscene actual time saved in ticks. </summary>
        [HideInInspector]
        public long TimeTicks;
        /// <summary> Audio clip used in cutscene. </summary>
        [HideInInspector]
        public AudioClip AudioClip;
        /// <summary> Audio clip start time.</summary>
        [HideInInspector]
        public long AudioClipStartTimeTicks;
        /// <summary> Duration of the cutscene.</summary>
        [HideInInspector]
        public long DurationTicks;
        /// <summary> List of the scenes data in cutscene.</summary>
        [HideInInspector]
        public List<SceneData> Scenes;
        /// <summary> List of the time labels data in cutscene.</summary>
        [HideInInspector]
        public List<TimeLabelData> TimeLabelsData;

        //Editor data!
        /// <summary> [Editor data] Scroll position.</summary>
        [HideInInspector]
        public Vector2 ScrollPos;
        /// <summary> [Editor data] Zoom value.</summary>
        [HideInInspector]
        public float Zoom;


        /// <summary> Initialize default data. </summary>
        public void DefaultInit()
        {
            TimeTicks = 0;
            ScrollPos = new Vector2();

            Scenes = new List<SceneData>();
            TimeLabelsData = new List<TimeLabelData>();
            DurationTicks = new System.TimeSpan(0, 0, 60).Ticks;
            
            Zoom = 1;
            AudioClip = null;
            AudioClipStartTimeTicks = 0;
        }
        /// <summary>  Copy another CutsceneAsset values to this one. </summary>
        /// <param name="toCopy">CutsceneAsset to copy.</param>
        public void Assign(CutsceneAsset toCopy)
        {
            TimeTicks = toCopy.TimeTicks;
            ScrollPos = toCopy.ScrollPos;
            Scenes = toCopy.Scenes;
            DurationTicks = toCopy.DurationTicks;
            Zoom = toCopy.Zoom;
            AudioClip = toCopy.AudioClip;
            AudioClipStartTimeTicks = toCopy.AudioClipStartTimeTicks;
            TimeLabelsData = toCopy.TimeLabelsData;
        }
        /// <summary>
        /// Save data to asset. Works only when in edit mode.
        /// </summary>
        public void SaveAsset()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }
        /// <summary> Create and save data to asset. Works only when in edit mode.  </summary>
        /// <param name="path">Path of saving data collection.</param>
        /// <param name="name">Name of saving data collection.</param>
        public void CreateAsset(string path, string name)
        {
#if UNITY_EDITOR
            string newPath = Path.Combine(path, name);
            CutsceneAsset asset = CreateInstance<CutsceneAsset>();
            asset.Assign(this);
            int count = 0;
            object testObject = AssetDatabase.LoadAssetAtPath<CutsceneAsset>(newPath + EXT);
            while (testObject != null)
            {
                count++;
                testObject = AssetDatabase.LoadAssetAtPath<CutsceneAsset>(newPath + " " + (count+1) + EXT);
            }
            if (count > 0)
                newPath += " " + (count + 1);
            AssetDatabase.CreateAsset(asset, newPath + EXT);
            AssetDatabase.SaveAssets();
#endif
        }

        /// <summary>  Load asset from file.  </summary>
        /// <param name="path">Path of saving data collection.</param>
        /// <param name="name">Name of saving data collection.</param>
        /// <returns>True, if save successful, false otherwise. </returns>
        public bool LoadAsset(string path, string name)
        {
            CutsceneAsset temp = Resources.Load(Path.Combine(path, name)) as CutsceneAsset;
            if (temp == null)
                return false;
            else
            {
                Assign(temp);
                return true;
            }
        }
    }
}