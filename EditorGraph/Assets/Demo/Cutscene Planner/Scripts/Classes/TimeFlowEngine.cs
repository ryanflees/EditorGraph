using System;
using System.Collections.Generic;
using System.Linq;

namespace CutscenePlanner
{
    /// <summary> Represents the time line calculation engine. </summary>
    [Serializable]
    public class TimeFlowEngine 
    {
        private const string _READ_ONLY_EXCEPTION_MESSAGE = "Time flow engine is launched in read only mode!";

        /// <summary> On event delegate</summary>
        /// <param name="currentTimeOnEvent">Time when the event occurred.</param>
        public delegate void TimeDelegate(TimeSpan currentTimeOnEvent);
        /// <summary> On time changed event delegate.</summary>
        /// <param name="lastTime">Time befor change.</param>
        /// <param name="currentTime">Time after change.</param>
        public delegate void TimeChangedDelegate(TimeSpan lastTime, TimeSpan currentTime);
        /// <summary> Title click event. </summary>
        public event TimeChangedDelegate DurationChanged;
        /// <summary> Timeflow current time changed event.</summary>
        public event TimeChangedDelegate CurrentTimeChanged;
        /// <summary> On play event.</summary>
        public event TimeDelegate TimeFlowStarted;
        /// <summary> On stop event.</summary>
        public event TimeDelegate TimeFlowStopped;
        /// <summary> On pause event.</summary>
        public event TimeDelegate TimeFlowPaused;
        /// <summary> True, if time flow engine wont make any changes. False otherwise. </summary>
        public bool IsTimeflowReadOnly { get { return _readOnly; } }
        /// <summary> True if engine is started.</summary>
        public bool IsStarted { get { return _isPlaying; } }
        /// <summary> Current pixels per seconds value.</summary>
        public float PixelsPerSecond { get { return _pixelsPerSecond; } }
        /// <summary> Size of the timeline in pixels.</summary>
        public float DurationInPixels { get { return (float)(_duration * _pixelsPerSecond); } }

        /// <summary> Duration of the timeline. </summary>
        public TimeSpan Duration
        {
            get { return MakeTimeSpan(_duration); }
            set
            {
                if (_readOnly)
                    throw new InvalidOperationException(_READ_ONLY_EXCEPTION_MESSAGE + " Duration cannot be changed!");
                double old = _duration;
                _duration = value.TotalMilliseconds / 1000.0;
                if (_duration <= 1)
                    _duration = 2;
                if (old != _duration && DurationChanged != null)
                    DurationChanged.Invoke(MakeTimeSpan(old), MakeTimeSpan(_duration));
            }
        }
        /// <summary>Current time of the timeflow.</summary>
        public TimeSpan Time { get { return _time; } }

        private TimeSpan _time = TimeSpan.Zero;
        private bool _isPlaying;
        private float _pixelsPerSecond = -1;
        private double _duration = 60;
        private bool _readOnly;

        /// <summary> Creates new time flow engine.</summary>
        /// <param name="duration">Initial duration of the cutscene. Minimal is 2 sec.</param>
        /// <param name="readOnly">True if engine should be created in readonly mode. False otherwise. [defauld false]</param>
        public TimeFlowEngine(double duration, bool readOnly = false)
        {
            _duration = duration;
            if (_duration <= 1)
                _duration = 2;

            _readOnly = readOnly;
        }
        public TimeFlowEngine(TimeFlowEngine copy, TimeSpan currentTime, TimeSpan duration)
        {
            _time = currentTime;
            _duration = duration.TotalMilliseconds / 1000.0;

            _isPlaying = copy._isPlaying;
            _pixelsPerSecond = copy._pixelsPerSecond;
            
            _readOnly = copy._readOnly;
            CopyEventsSubscriptions(copy);
        }
        /// <summary> Adds amound of seconds to current time.</summary>
        /// <param name="seconds">Seconds to add.</param>
        /// <param name="invokeEvents"> True, if method should invoke TimeChange event</param>
        public void AddTime(double seconds)
        {
            SetTime(_time.TotalSeconds + seconds);
        }
        /// <summary> Set timeflow current time.</summary>
        /// <param name="time">Current time to be set.</param>
        public void SetTime(TimeSpan time, bool invokeEvents = true)
        {
            SetTime(time.TotalSeconds, invokeEvents);
        }
        /// <summary> Set timeflow current time.</summary>
        /// <param name="seconds">Current time in seconds to be set.</param>
        /// <param name="invokeEvents"> True, if method should invoke TimeChange event</param>
        public void SetTime(double seconds, bool invokeEvents = true)
        {
            TimeSpan oldTime = _time;

            _time = MakeTimeSpan(seconds);
            if (_time >= Duration)
            {
                StopTimeFlowEngine();
                _time = Duration;
            }
            if (invokeEvents && oldTime != _time && CurrentTimeChanged != null)
                CurrentTimeChanged.Invoke(oldTime, _time);
        }

        /// <summary> Launch the timeflow.</summary>
        public void StartTimeFlowEngine()
        {
            if (!_isPlaying && _time.TotalSeconds == _duration)
                SetTime(0);
            _isPlaying = true;
            if (TimeFlowStarted != null)
                TimeFlowStarted.Invoke(_time);
        }
        /// <summary> Pause the timeflow. </summary>
        public void PauseTimeFlowEngine()
        {
            _isPlaying = false;
            if (TimeFlowPaused != null)
                TimeFlowPaused.Invoke(_time);
            

        }
        /// <summary>Pause the timeflow and set current time to 0.</summary>
        public void StopTimeFlowEngine()
        {
            _isPlaying = false;
            if (TimeFlowStopped != null)
                TimeFlowStopped.Invoke(_time);
            SetTime(0);

        }
        /// <summary> Forwards to the end.</summary>
        public void ForwardTime()
        {
            SetTime(Duration);
        }
        /// <summary> Backward to the begining.</summary>
        public void BackwardTime()
        {
            SetTime(0);
        }
        /// <summary> Handle timeflow engine. Have to be invoked in some kind of loop (update for example).</summary>
        /// <param name="deltaTime">Delta time </param>
        public void HandleTime(double deltaTime)
        {
            double _deltaTime = deltaTime;
            if (_isPlaying)
                AddTime(_deltaTime);
        }


        /// <summary> Converts time on the time lint to component relative x position. </summary>
        /// <param name="time">Time on the timeflow.</param>
        /// <returns>Converted time to component relative x position.</returns>
        public float TimeToX(TimeSpan time)
        {
            return (float)(time.TotalSeconds * _pixelsPerSecond);
        }
        /// <summary> Converts time on the time lint to component relative x position. </summary>
        /// <param name="seconds">Time on the timeflow in seconds.</param>
        /// <returns>Converted time to component relative x position.</returns>
        public float TimeToX(double seconds)
        {
            return (float)(seconds * _pixelsPerSecond);
        }
        /// <summary> Convert the component relative x position to time on the timeflow. </summary>
        /// <param name="x">X position on the component.</param>
        /// <returns>Time on the timeflow.</returns>
        public TimeSpan XToTime(float x)
        {
            int totalMiliseconds = (int)((x / _pixelsPerSecond) * 1000);
            return RoundToMiliseconds(new TimeSpan(0, 0, 0, 0, totalMiliseconds)); ;
        }
        /// <summary> Recalcs the scale of the time, that depends on the viewport width and zoom value.</summary>
        /// <param name="width">Timeline viewpor width.</param>
        /// <param name="zoom">Zoom value. Zoom is clamped if its value cause _pixelsPerSecond being bigger than int max value.</param>
        public void RecalcScale(float width, ref float zoom)
        {
            if (_readOnly)
                throw new InvalidOperationException(_READ_ONLY_EXCEPTION_MESSAGE + " Zoom cannot be changed!");
            _pixelsPerSecond = (float)(width / _duration) * zoom;
            if (_pixelsPerSecond * _duration > int.MaxValue)
            {
                _pixelsPerSecond = (float)(int.MaxValue / _duration);
                zoom = (float)(_pixelsPerSecond / (width / _duration));
            }
        }
        /// <summary> Copy all event subscriptions from this TimeFlowEngine to given one </summary>
        /// <param name="copyTo">TimeFlowEngine to which events should be copied.</param>
        public void CopyEventsSubscriptions(TimeFlowEngine copyTo)
        {
            List<TimeChangedDelegate> durationChangedSubscriptions;
            List<TimeChangedDelegate> currentTimeChangedSubscriptions;
            List<TimeDelegate> timeFlowStartedSubscriptions;
            List<TimeDelegate> timeFlowStoppedSubscriptions;
            List<TimeDelegate> timeFlowPausedSubscription;

            List<TimeChangedDelegate> durationChangedSubscriptionsOld;
            List<TimeChangedDelegate> currentTimeChangedSubscriptionsOld;
            List<TimeDelegate> timeFlowStartedSubscriptionsOld;
            List<TimeDelegate> timeFlowStoppedSubscriptionsOld;
            List<TimeDelegate> timeFlowPausedSubscriptionOld;

            CopyEventsSubscriptionsHandler(out durationChangedSubscriptionsOld, out currentTimeChangedSubscriptionsOld,
                                          out timeFlowStartedSubscriptionsOld, out timeFlowStoppedSubscriptionsOld, out timeFlowPausedSubscriptionOld);
            copyTo.CopyEventsSubscriptionsHandler(out durationChangedSubscriptions, out currentTimeChangedSubscriptions,
                                         out timeFlowStartedSubscriptions, out timeFlowStoppedSubscriptions, out timeFlowPausedSubscription);

            foreach (TimeChangedDelegate subscriber in durationChangedSubscriptionsOld)
            {
                if (!durationChangedSubscriptions.Contains(subscriber))
                    copyTo.DurationChanged += subscriber;
            }
            foreach (TimeChangedDelegate subscriber in currentTimeChangedSubscriptionsOld)
            {
                if (!currentTimeChangedSubscriptions.Contains(subscriber))
                    copyTo.CurrentTimeChanged += subscriber;
            }
            foreach (TimeDelegate subscriber in timeFlowStartedSubscriptionsOld)
            {
                if (!timeFlowStartedSubscriptions.Contains(subscriber))
                    copyTo.TimeFlowStarted += subscriber;
            }
            foreach (TimeDelegate subscriber in timeFlowStoppedSubscriptionsOld)
            {
                if (!timeFlowStoppedSubscriptions.Contains(subscriber))
                    copyTo.TimeFlowStopped += subscriber;
            }
            foreach (TimeDelegate subscriber in timeFlowPausedSubscriptionOld)
            {
                if (!timeFlowPausedSubscription.Contains(subscriber))
                    copyTo.TimeFlowPaused += subscriber;
            }
        }
        private void CopyEventsSubscriptionsHandler(out List<TimeChangedDelegate> durationChangedSubscriptions, 
                               out List<TimeChangedDelegate> currentTimeChangedSubscriptions, 
                               out List<TimeDelegate> timeFlowStartedSubscriptions,
                               out List<TimeDelegate> timeFlowStoppedSubscriptions,
                               out List<TimeDelegate> timeFlowPausedSubscriptions)
        {
            durationChangedSubscriptions = new List<TimeChangedDelegate>();
            currentTimeChangedSubscriptions = new List<TimeChangedDelegate>();
            timeFlowStartedSubscriptions = new List<TimeDelegate>();
            timeFlowStoppedSubscriptions = new List<TimeDelegate>();
            timeFlowPausedSubscriptions = new List<TimeDelegate>();

            if (DurationChanged!=null)
                durationChangedSubscriptions = Array.ConvertAll(DurationChanged.GetInvocationList(), item => (TimeChangedDelegate)item).ToList();
            if (CurrentTimeChanged != null)
                currentTimeChangedSubscriptions = Array.ConvertAll(CurrentTimeChanged.GetInvocationList(), item => (TimeChangedDelegate)item).ToList();
            if (TimeFlowStarted != null)
                timeFlowStartedSubscriptions = Array.ConvertAll(TimeFlowStarted.GetInvocationList(), item => (TimeDelegate)item).ToList();
            if (TimeFlowStopped != null)
                timeFlowStoppedSubscriptions = Array.ConvertAll(TimeFlowStopped.GetInvocationList(), item => (TimeDelegate)item).ToList();
            if (TimeFlowPaused != null)
                timeFlowPausedSubscriptions = Array.ConvertAll(TimeFlowPaused.GetInvocationList(), item => (TimeDelegate)item).ToList();
        }
        /// <summary> Rounds the timespan to miliseconds. </summary>
        /// <param name="t">Time span that should be rounded.</param>
        /// <returns>Timespan rounded to miliseconds.</returns>
        private static TimeSpan RoundToMiliseconds(TimeSpan t)
        {
            int precision = 3;
            const int TIMESPAN_SIZE = 7;
            int factor = (int)Math.Pow(10, (TIMESPAN_SIZE - precision));

            return new TimeSpan(((long)Math.Round((1.0 * t.Ticks / factor)) * factor));
        }
        /// <summary> Make TimeSpan object based on given time in seconds.</summary>
        /// <param name="time">Time in seconds.</param>
        /// <returns>TimeSpan.</returns>
        private static TimeSpan MakeTimeSpan(double time)
        {
            long ticks = (long)time * TimeSpan.TicksPerSecond + (long)(Math.Round((time - (long)time) * 1000)) * TimeSpan.TicksPerMillisecond;
            TimeSpan result = new TimeSpan(ticks);
            return result;
        }
    }
}