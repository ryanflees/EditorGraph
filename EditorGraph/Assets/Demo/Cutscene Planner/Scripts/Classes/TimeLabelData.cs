using System;
using UnityEngine;
namespace CutscenePlanner
{
    /// <summary> Represents a time label data.</summary>
    [Serializable]
    public class TimeLabelData : TimedData, ISerializationCallbackReceiver
    {
        /// <summary> Creates empty time label. </summary>
        public TimeLabelData() : base("Time label...") { }

        /// <summary> Copy constructor </summary>
        /// <param name="copy">Data to copy.</param>
        public TimeLabelData(TimeLabelData copy) : base(copy) { }

        /// <summary> Assign existing data without time.</summary>
        /// <param name="copy">Data to be assigned.</param>
        public override void AssignWithoutTime(TimedData copy)
        {
            TimeLabelData sd = copy as TimeLabelData;
            Name = sd.Name;
            Color = sd.Color;
        }
    }
}