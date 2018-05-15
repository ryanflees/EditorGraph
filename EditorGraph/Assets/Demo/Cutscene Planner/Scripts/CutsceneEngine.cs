using UnityEngine;
using System;
#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace CutscenePlanner
{
    /// <summary> Represents ingame cutscene engine. </summary>
    [RequireComponent(typeof(AudioSource))]
    [AddComponentMenu("Cutscene/Engine")]
    public class CutsceneEngine : MonoBehaviour {

        /// <summary>Cutscene events delegate.</summary>
        /// <param name="sender">Sender of the event.</param>
        public delegate void CutsceneEventsDelegate(object sender);
        /// <summary> Time event for scene data delegate.</summary>
        /// <param name="scene">Scene data.</param>
        /// <param name="sender">Sender of the event.</param>
        public delegate void TimeEventSceneDeleagte(object sender, SceneData scene);
        /// <summary> Time event for scene time label data delegate.</summary>
        /// <param name="scene">Time label data.</param>
        /// <param name="sender">Sender of the event.</param>
        public delegate void TimeEventTimeLineLableDeleagte(object sender, TimeLabelData label);
        /// <summary> Time event for audio clip delegate.</summary>
        /// <param name="scene">Audio clip.</param>
        /// <param name="sender">Sender of the event.</param>
        public delegate void TimeEventAudioClipDeleagte(object sender, AudioClip audioClip);

        /// <summary> Event of the cutscene started.</summary>
        public event CutsceneEventsDelegate Started;
        /// <summary> Event of the cutscene ended.</summary>
        public event CutsceneEventsDelegate Ended;
        /// <summary> Event of the cutscene paused.</summary>
        public event CutsceneEventsDelegate Paused;
        /// <summary> Event of current time enter scene. </summary>
        public event TimeEventSceneDeleagte TimeSceneEnter;
        /// <summary> Event of current time exit scene. </summary>
        public event TimeEventSceneDeleagte TimeSceneExit;
        /// <summary> Event of current time enter time label. </summary>
        public event TimeEventTimeLineLableDeleagte TimeLabelDataEnter;
        /// <summary> Event of current time exit time label. </summary>
        public event TimeEventTimeLineLableDeleagte TimeLabelDataExit;
        /// <summary> Event of current time enter audio clip. </summary>
        public event TimeEventAudioClipDeleagte TimeAudioClipEnter;
        /// <summary> Event of current time exit audio clip. </summary>
        public event TimeEventAudioClipDeleagte TimeAudioClipExit;

        /// <summary> Cutscene asset file.</summary>
        public CutsceneAsset Cutscene;

        /// <summary> True if cutscene is playing. False otherwise.</summary>
        public bool IsPlaying { get { return _timeEngine != null ? _timeEngine.IsStarted : false; } }
        /// <summary> Duration of the cutscene.</summary>
        public float Duration { get { return (float)_timeEngine.Duration.TotalSeconds; } }
        /// <summary> Current time of the cutscene. </summary>
        public float Time {
            get { return (float)_timeEngine.Time.TotalSeconds; }
            set { _timeEngine.SetTime(value); }
        }
        /// <summary> Current normalized time of the cutscene. </summary>
        public float NormalizedTime {
            get { return (float) (_timeEngine.Time.TotalSeconds / _timeEngine.Duration.TotalSeconds); }
            set {
                float t = Mathf.Clamp01(value);
                _timeEngine.SetTime((float)_timeEngine.Duration.TotalSeconds * t);
            }
        }
        /// <summary> Time flow engine used for this cutscene. </summary>
        public TimeFlowEngine TimeEngine { get { return _timeEngine; } }
        private TimeFlowEngine _timeEngine
        {
            get
            {
                if (_timeEngineVar == null)
                    _timeEngineVar = new TimeFlowEngine(Cutscene != null ? (float) new TimeSpan(Cutscene.DurationTicks).TotalSeconds : 60);
                return _timeEngineVar;
            }
        }
        private bool _allowAudioPlaying;
        private AudioSource _audioSource { get { return GetComponent<AudioSource>(); } }
        private bool _timeOnAudioSource;
        private float _startTimer;
        private float _stopTimer;
        private TimeFlowEngine _timeEngineVar;
        private bool _paused = false;
        private bool _unpaused = true;
        #region MonoBehaviours

        private void Start()
        {
            if (Cutscene != null)
                _audioSource.clip = Cutscene.AudioClip;
        }
        private void OnEnable()
        {
            _timeEngine.CurrentTimeChanged += _timeEngine_TimeChanged;
            _timeEngine.TimeFlowStarted += _timeEngine_TimeFlowStarted;
            _timeEngine.TimeFlowStopped += _timeEngine_TimeFlowStopped;
            _timeEngine.TimeFlowPaused += _timeEngine_TimeFlowPaused;
        }
        private void OnDisable()
        {
            _timeEngine.CurrentTimeChanged -= _timeEngine_TimeChanged;
            _timeEngine.TimeFlowStarted -= _timeEngine_TimeFlowStarted;
            _timeEngine.TimeFlowStopped -= _timeEngine_TimeFlowStopped;
            _timeEngine.TimeFlowPaused -= _timeEngine_TimeFlowPaused;
        }
        private void OnApplicationPause(bool pause)
        {
            if (pause)
                _paused = true;
        }

        private void Update()
        {
            float deltaTime = UnityEngine.Time.unscaledDeltaTime;

            
            
            #if UNITY_EDITOR
                if (EditorApplication.isPaused)
                    deltaTime = 0;
            #endif
            if (_paused)
            {
                _paused = false;
                _unpaused = false;
                return;
            }
            if (!_unpaused) 
            {
                deltaTime = 0;
                _unpaused = true;
            }
            _timeEngine.HandleTime(deltaTime);

            // timers handling
            if (_startTimer>0)
            {
                _startTimer -= deltaTime;
                if (_startTimer<=0)
                {
                    _startTimer = 0;
                    PlayCutscene();
                }
            }
            if (_stopTimer > 0)
            {
                _stopTimer -= deltaTime;
                if (_stopTimer <= 0)
                {
                    _stopTimer = 0;
                    StopCutscene();
                }
            }
        }
        #endregion

        #region Events
        protected void OnStarted()
        {
            if (Started != null)
                Started.Invoke(this);
        }
        protected void OnEnded()
        {
            if (Ended != null)
                Ended.Invoke(this);
        }
        protected void OnPaused()
        {
            if (Paused != null)
                Paused.Invoke(this);
        }
        protected void OnTimeSceneEnter(SceneData scene)
        {
            if (TimeSceneEnter != null)
                TimeSceneEnter.Invoke(this, scene);
        }
        protected void OnTimeSceneExit(SceneData scene)
        {
            if (TimeSceneExit != null)
                TimeSceneExit.Invoke(this, scene);
        }
        protected void OnTimeLabelDataEnter(TimeLabelData label)
        {
            if (TimeLabelDataEnter != null)
                TimeLabelDataEnter.Invoke(this, label);
        }
        protected void OnTimeLabelDataExit(TimeLabelData label)
        {
            if (TimeLabelDataExit != null)
                TimeLabelDataExit.Invoke(this, label);
        }
        protected void OnTimeAudioClipEnter(AudioClip audioClip)
        {
            _timeOnAudioSource = true;
            if (TimeAudioClipEnter != null)
                TimeAudioClipEnter.Invoke(this, audioClip);
        }
        protected void OnTimeAudioClipExit(AudioClip audioClip)
        {
            _timeOnAudioSource = false;
            if (TimeAudioClipExit != null)
                TimeAudioClipExit.Invoke(this, audioClip);
        }

        #endregion

        /// <summary> Starts playing cutscene. </summary>
        public void PlayCutscene()
        {
            foreach (SceneData scene in Cutscene.Scenes)
                scene.UpdateTime(0);
            foreach (TimeLabelData label in Cutscene.TimeLabelsData)
                label.UpdateTime(0);
            _timeEngine.SetTime(TimeSpan.Zero);
            _timeEngine.StartTimeFlowEngine();
        }
        /// <summary> Starts playing cutscene. </summary>
        /// <param name="delay">Delay time [in sec] before cutscene start.</param>
        public void PlayCutscene(float delay)
        {
            if (Cutscene == null)
                return;
            if (delay <= 0)
                PlayCutscene();
            else
                _startTimer = delay;
        }
        /// <summary> Stops playing cutscene. </summary>
        public void StopCutscene()
        {
            _timeEngine.StopTimeFlowEngine();
            _timeOnAudioSource = false;
        }
        /// <summary> Stops playing cutscene. </summary>
        /// <param name="delay">Delay time [in sec]  before cutscene stop.</param>
        public void StopCutscene(float delay)
        {
            if (delay <= 0)
                StopCutscene();
            else
                _stopTimer = delay;
        }
        /// <summary> Pause cutscene. </summary>
        public void PauseCutscene()
        {
            _timeEngine.PauseTimeFlowEngine();
        }

        private void _timeEngine_TimeFlowPaused(TimeSpan actualTimeOnEvent)
        {
            OnPaused();
            _allowAudioPlaying = false;
            _audioSource.Pause();
        }
        private void _timeEngine_TimeFlowStopped(TimeSpan actualTimeOnEvent)
        {
            OnEnded();
            _allowAudioPlaying = false;
            _timeOnAudioSource = false;

            if (Cutscene == null)
                return;

            _audioSource.Stop();
        }
        private void _timeEngine_TimeFlowStarted(TimeSpan actualTimeOnEvent)
        {
            OnStarted();
            _allowAudioPlaying = true;
            _audioSource.UnPause();
        }
        private void _timeEngine_TimeChanged(TimeSpan lastTime, TimeSpan currentTime)
        {
            if (Cutscene == null)
                return;

            float timeDiff = Mathf.Abs((float)(currentTime.TotalSeconds - lastTime.TotalSeconds));
            // scenes handling
            foreach (SceneData scene in Cutscene.Scenes)
            {
                if (!scene.TimeOnData && TimeOnScene(currentTime, scene))
                    OnTimeSceneEnter(scene);
                else if (scene.TimeOnData && !TimeOnScene(currentTime, scene))
                    OnTimeSceneExit(scene);

                scene.UpdateTime(currentTime);
            }
            // time label handling
            foreach (TimeLabelData label in Cutscene.TimeLabelsData)
            {
                if (!label.TimeOnData && TimeOnTimeLabel(currentTime, label))
                    OnTimeLabelDataEnter(label);
                else if (label.TimeOnData && !TimeOnTimeLabel(currentTime, label))
                    OnTimeLabelDataExit(label);

                label.UpdateTime(currentTime);
            }
            // audio handling

            float audioClipStartTime = (float)new TimeSpan(Cutscene.AudioClipStartTimeTicks).TotalSeconds;
            float offsetBetweenTimeAndAudio = Mathf.Abs((float)(audioClipStartTime + _audioSource.time - currentTime.TotalSeconds));
            if (!_audioSource.isPlaying && Cutscene.AudioClip != null && TimeOnAudioClip(currentTime, Cutscene))
                _audioSource.time = (float)currentTime.TotalSeconds - audioClipStartTime;
            
            if (!_allowAudioPlaying)
                return;

            if (Cutscene.AudioClip != null && !TimeOnAudioClip(currentTime, Cutscene) && _timeOnAudioSource)
            {
                OnTimeAudioClipExit(Cutscene.AudioClip);
                _audioSource.Stop();
                
            }
            else if (Cutscene.AudioClip != null && TimeOnAudioClip(currentTime, Cutscene) && !_timeOnAudioSource)
            {
                _audioSource.Play();
                _audioSource.time = (float)currentTime.TotalSeconds - audioClipStartTime;
                OnTimeAudioClipEnter(Cutscene.AudioClip);
            }
            else if (_audioSource.isPlaying && timeDiff > UnityEngine.Time.unscaledDeltaTime * 2)
            {
                
                if (TimeOnAudioClip(currentTime, Cutscene))
                {
                    _audioSource.time = (float)currentTime.TotalSeconds - audioClipStartTime;
                }
                else
                {
                    _audioSource.Stop();
                    OnTimeAudioClipExit(Cutscene.AudioClip);
                }
            }
            else if (_audioSource.isPlaying && offsetBetweenTimeAndAudio  > 0.02)
                _timeEngine.SetTime(audioClipStartTime + _audioSource.time, false);
            
            
        }
        #region Utils
        private static bool TimeOnScene(TimeSpan t, SceneData scene)
        {
            return t >= scene.Start && t <= scene.End;
        }
        private static bool TimeOnTimeLabel (TimeSpan t, TimeLabelData label)
        {
            return t>= label.Start && t <= label.End;
        }
        private bool TimeOnAudioClip(TimeSpan t, CutsceneAsset asset)
        {
            float audioClipStartInSec = (float)new TimeSpan(asset.AudioClipStartTimeTicks).TotalSeconds;
            float audioClipEndInSec = audioClipStartInSec + asset.AudioClip.length;
            return t.TotalSeconds >= audioClipStartInSec && t.TotalSeconds <= audioClipEndInSec ;
        }
        #endregion
    }


}