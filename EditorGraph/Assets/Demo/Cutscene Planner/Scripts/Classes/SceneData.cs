using System;
using UnityEngine;

namespace CutscenePlanner
{
    /// <summary> Represents a scene data.</summary>
    [Serializable]
    public class SceneData : TimedData
    {
        /// <summary> Description of the scene. </summary>
        public string Desc;
        /// <summary> Concept art. </summary>
        public Texture2D ConceptArt;

        /// <summary> Creates empty scene. </summary>
        public SceneData() : base("New scene")
        {
            Desc = "Description of the scene...";
            ConceptArt = null;
        }
        /// <summary> Copy constructor. </summary>
        /// <param name="copy">Data to copy.</param>
        public SceneData(SceneData copy) : base(copy)
        {
            Desc = copy.Desc;
            ConceptArt = copy.ConceptArt;
        }
        /// <summary> Assign existing data without time.</summary>
        /// <param name="copy">Data to be assigned.</param>
        public override void AssignWithoutTime(TimedData copy)
        {
            SceneData sd = copy as SceneData;
            Name = sd.Name;
            Desc = sd.Desc;
            ConceptArt = sd.ConceptArt;
            Color = sd.Color;
        }
    }
}
