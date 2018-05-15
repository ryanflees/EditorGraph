using System;
using UnityEngine;
namespace CutscenePlanner
{
    /// <summary> Represents data that can be put on the timeline.</summary>
    [Serializable]
    public abstract class TimedData : ISerializationCallbackReceiver
    {
        /// <summary> True if timed data is requested to be deleted. </summary>
        public bool RemoveRequest {  get { return _removeRequest; } }
        /// <summary> True if time is on data. False otherwise.</summary>
        public bool TimeOnData { get { return _lastTimeOnData > Start.TotalSeconds && _lastTimeOnData <= End.TotalSeconds; } }
        /// <summary> Description / Name of the data. </summary>
        public string Name;
        /// <summary> Data start time. </summary>
        public TimeSpan Start;
        /// <summary> Data stop time. </summary>
        public TimeSpan End;
        /// <summary> Color of the data </summary>
        public Color Color;
        /// <summary> Duration of the data. </summary>
        public TimeSpan Duration { get { return End - Start; } }

        [SerializeField]
        private long _startTicks;
        [SerializeField]
        private long _endTicks;

        private float _lastTimeOnData;
        private bool _removeRequest;

        /// <summary> Creates empty time label. </summary>
        public TimedData(string name)
        {
            Name = name;
            Start = TimeSpan.Zero;
            End = TimeSpan.Zero;
        }
        /// <summary> Copy constructor</summary>
        /// <param name="copy">Copy object</param>
        public TimedData(TimedData copy)
        {
            Start = copy.Start;
            End = copy.End;
            Name = copy.Name;
            Color = copy.Color;
        }
        /// <summary> Assign existing data without time.</summary>
        /// <param name="copy">Data to be assigned.</param>
        public abstract void AssignWithoutTime(TimedData copy);
        /// <summary> Updates information about current time in timeline for this data. </summary>
        /// <param name="time"> Time represented as TimeSpan.</param>
        public void UpdateTime(TimeSpan time)
        {
            UpdateTime((float)time.TotalSeconds);
        }
        /// <summary> Updates information about current time in timeline for this data. </summary>
        /// <param name="time"> Time represented in seconds.</param>
        public void UpdateTime(float time)
        {
            _lastTimeOnData = time;
        }
        /// <summary> Marks data to be deleted. </summary>
        public void MarkToDelete()
        {
            _removeRequest = true;
        }
        /// <summary> Method invoked befor serialization action. </summary>
        public virtual void OnBeforeSerialize()
        {
            _startTicks = Start.Ticks;
            _endTicks = End.Ticks;
        }
        /// <summary> Method invoked after serialization action. </summary>
        public virtual void OnAfterDeserialize()
        {
            Start = new TimeSpan(_startTicks);
            End = new TimeSpan(_endTicks);
        }
    }
}