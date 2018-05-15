using CutscenePlanner.Editor.Utils;
using System;
using UnityEditor;
using UnityEngine;

namespace CutscenePlanner.Editor.Controls.LinePanels
{
    /// <summary> Represents the sound line panel component. </summary>
    public partial class SoundLinePanel : Panel, ILinePanel
    {
        /// <summary> Audio Clip start time changed event delegate. </summary>
        /// <param name="audioClipNewStartTime">New Audio Clip start time.</param>
        public delegate void AudioClipStartTimeChangedDelegate(TimeSpan audioClipNewStartTime);
        /// <summary> Audio Clip start time changed event. </summary>
        public event AudioClipStartTimeChangedDelegate AudioClipStartTimeChanged;

        /// <summary> start time of the audioclip.</summary>
        public TimeSpan AudioClipStartTime
        {
            get { return ExtensionMethods.MakeTimeSpan(_audioClipStartTime); }
            set
            {
                double newTime = (float)value.TotalSeconds;
                double duration = _audioClip != null ? _audioClip.length : 0;

                if (duration > _timeLinePanel.TimeLineEngine.Duration.TotalSeconds)
                {
                    if (newTime < 0 && newTime + duration < _timeLinePanel.TimeLineEngine.Duration.TotalSeconds)
                        newTime = _timeLinePanel.TimeLineEngine.Duration.TotalSeconds - duration;

                    if (newTime > 0 && newTime + duration > _timeLinePanel.TimeLineEngine.Duration.TotalSeconds)
                        newTime = 0;
                }
                else
                {
                    if (newTime < 0)
                        newTime = 0;

                    if (newTime + duration > _timeLinePanel.TimeLineEngine.Duration.TotalSeconds)
                        newTime = _timeLinePanel.TimeLineEngine.Duration.TotalSeconds - duration;
                }
                if (_audioClipStartTime != newTime)
                {
                    if (AudioClipStartTimeChanged != null)
                        AudioClipStartTimeChanged.Invoke(ExtensionMethods.MakeTimeSpan(newTime));
                    _durationWasChanged = true;
                }
                _audioClipStartTime = newTime;
            }

        }
        /// <summary> Rect of the sound texture on whole panel texture rect.</summary>
        public Rect TextureSoundRect
        {
            get
            {
                int textureWidth = (int)_rect.width;
                int textureHeight = (int)_rect.height;

                int soundWavesTextureWidth = textureWidth - BorderThickness * 2;
                int soundWavesTextureHeight = textureHeight - BorderThickness * 2;

                float x = 0;
                float width = 0;

                if (_audioClip != null)
                {
                    x = _timeLinePanel.TimeLineEngine.TimeToX(_audioClipStartTime) + BorderThickness - _lastXPos;
                    width = _timeLinePanel.TimeLineEngine.TimeToX(_audioClip.length) - BorderThickness * 2;
                    if (width < 0)
                        width = 0;
                    if (x + width < 0)
                    {
                        x = 0;
                        width = 0;
                    }
                    else if (x < 0)
                    {
                        width += x;
                        x = BorderThickness;

                    }
                    if (x > soundWavesTextureWidth)
                    {
                        x = 0;
                        width = 0;
                    }


                    if (x + width > soundWavesTextureWidth)
                        width = soundWavesTextureWidth - x;

                    return new Rect(x, BorderThickness, width, soundWavesTextureHeight - BorderThickness * 2);
                }
                return new Rect();
            }
        }
        /// <summary> Rect of the sound texture on relative to GUI. </summary>
        public Rect GUITextureSoundRect
        {
            get
            {
                Rect rect = TextureSoundRect;
                return new Rect(rect.x, Rect.y, rect.width, rect.height);
            }
        }
        /// <summary> SoundLine audioclip.</summary>
        public AudioClip AudioClip
        {
            get { return _audioClip; }
            set {
                if (_audioClip != value)
                    _durationWasChanged = true;
                _audioClip = value;
            }
        }

        private TimeLinePanel _timeLinePanel;

        private bool _rectWasChanged;
        private bool _durationWasChanged;
        private float _lastXPos;
        private Rect _controlRect;
        private Texture2D _soundWavesTexture;
        private GUIStyle _soundWavesTextureStyle;
        private AudioClip _audioClip;
        private double _audioClipStartTime;
        private bool _allowDrag;
        private bool _wasMouseDown;
        private bool _dragOnAudioClip;
        private bool _dragOutsideAudioClip;

        private Vector2 _mousePos;
        private float _diffAtGrab;
        private bool _allowPlaying;

        /// <summary> Creates new sound line panel component.</summary>
        /// <param name="timeLinePanel">TimeLinePanel that this panel is related to.</param>
        public SoundLinePanel(TimeLinePanel timeLinePanel)
        {
            _timeLinePanel = timeLinePanel;
            _timeLinePanel.TimeLineEngine.DurationChanged += (oldDuration, newDuration) =>
            {
                _durationWasChanged = true;
            };
            _soundWavesTextureStyle = new GUIStyle();

            timeLinePanel.TimeLineEngine.CurrentTimeChanged += TimeLineEngine_TimeChanged;
            timeLinePanel.TimeLineEngine.TimeFlowStopped += TimeLineEngine_TimeStopped;
            timeLinePanel.TimeLineEngine.TimeFlowPaused += TimeLineEngine_TimeStopped;
            timeLinePanel.TimeLineEngine.TimeFlowStarted += TimeLineEngine_TimePlaying;
        }

        private void TimeLineEngine_TimePlaying(TimeSpan timeOnEvent)
        {
            if (_timeLinePanel.TimeLineEngineIsExternal)
                return;
            _allowPlaying = true;
        }

        private void TimeLineEngine_TimeChanged(TimeSpan lastTime, TimeSpan currentTime)
        {
            if (!_allowPlaying || _timeLinePanel.TimeLineEngineIsExternal)
                return;
            if (_audioClip != null && !AudioUtils.IsClipPlaying() && currentTime > AudioClipStartTime && currentTime.TotalSeconds < _audioClipStartTime + _audioClip.length)
                AudioUtils.PlayClip(_audioClip, (float)(currentTime.TotalSeconds - _audioClipStartTime));
            else if (AudioUtils.IsClipPlaying() && Mathf.Abs((float)currentTime.TotalMilliseconds - (float)lastTime.TotalMilliseconds) > 200)
            {
                if (currentTime >= AudioClipStartTime && currentTime.TotalSeconds < _audioClipStartTime + _audioClip.length)
                    AudioUtils.SetClipTimePosition((float)(currentTime.TotalSeconds - _audioClipStartTime));
                else
                    AudioUtils.StopClip();
            }
            else if (AudioUtils.IsClipPlaying() && Mathf.Abs((float)(_audioClipStartTime + AudioUtils.GetClipTimePosition() - currentTime.TotalSeconds)) > 0.01)
                _timeLinePanel.TimeLineEngine.SetTime(_audioClipStartTime + AudioUtils.GetClipTimePosition(), false);
        }

        private void TimeLineEngine_TimeStopped(TimeSpan timeOnEvent)
        {
            if (_timeLinePanel.TimeLineEngineIsExternal)
                return;
            _allowPlaying = false;
            AudioUtils.StopClip();
        }

        /// <summary> Do component rect recalcing, basing on the given one [have to be invoked in OnGUI].. </summary>
        /// <param name="rect">Window, in which should be shown timeline.</param>
        /// <returns>Real size of the component.</returns>
        public Rect GUICalcRect(Rect rect)
        {          
            bool widthIsDiffrent = !Mathf.Approximately(rect.width, _rect.width);
            if (widthIsDiffrent)
                _rectWasChanged = true;

            _rect = rect;
            _controlRect = new Rect(rect.x, rect.y, _timeLinePanel.TimeLineEngine.DurationInPixels, rect.height);

            return _controlRect;
        }

        /// <summary> Draws the component [have to be invoked in OnGUI]..</summary>
        /// <param name="rect">X position for the timeline.</param>
        /// <param name="scrollDelta">Scroll delta.</param>
        /// <returns>Real component size.</returns>
        public Rect GUIDraw(float xPos, float scrollDelta)
        {
            base.Draw(_rect, _rectWasChanged);

            _mousePos = Event.current.mousePosition;
            Rect GUITextureSoundRectTemp = GUITextureSoundRect;
            if (_allowDrag && _rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    _wasMouseDown = true;
                    if (GUITextureSoundRectTemp.Contains(_mousePos))
                        _diffAtGrab = _mousePos.x - _timeLinePanel.TimeToWorldX((float)_audioClipStartTime);
                }
                if (_wasMouseDown && Event.current.type == EventType.MouseDrag)
                {
                    _wasMouseDown = false;
                    if (!_dragOutsideAudioClip && GUITextureSoundRectTemp.Contains(_mousePos))
                        _dragOnAudioClip = true;
                    else
                        _dragOutsideAudioClip = true;
                }

                if (((_wasMouseDown && Event.current.type == EventType.MouseUp) || _dragOutsideAudioClip))
                    _timeLinePanel.TimeLineEngine.SetTime(_timeLinePanel.WorldXToTime(Event.current.mousePosition.x));
            }
            if (Event.current.type == EventType.MouseUp)
            {
                _dragOnAudioClip = false;
                _dragOutsideAudioClip = false;
                _wasMouseDown = false;
            }


            if (_soundWavesTexture == null || _durationWasChanged || _rectWasChanged || _lastXPos != xPos || scrollDelta != 0)
            {
                _lastXPos = xPos;
                _rectWasChanged = false;
                _durationWasChanged = false;

                int textureWidth = (int)_rect.width;
                int textureHeight = (int)_rect.height;

                int soundWavesTextureWidth = textureWidth - BorderThickness * 2;
                int soundWavesTextureHeight = textureHeight - BorderThickness * 2;
                _soundWavesTexture = new Texture2D(soundWavesTextureWidth, soundWavesTextureHeight, TextureFormat.ARGB32, false);
                bool valid = true;

                Rect textureSoundRect = TextureSoundRect;
                if (_audioClip == null || textureSoundRect.width == 0)
                    valid = false;

                Color[] soundWavesBackgroundColors;
                soundWavesBackgroundColors = new Color[soundWavesTextureWidth * soundWavesTextureHeight];
                soundWavesBackgroundColors.Fill(NormalColor);
                _soundWavesTexture.SetPixels(0, 0, soundWavesTextureWidth, soundWavesTextureHeight, soundWavesBackgroundColors);

                if (valid)
                {
                    soundWavesBackgroundColors = new Color[(int)(textureSoundRect.width * textureSoundRect.height)];
                    soundWavesBackgroundColors.Fill(new Color32(211, 211, 211, 255));

                    _soundWavesTexture.SetPixels((int)textureSoundRect.x, (int)textureSoundRect.y, (int)textureSoundRect.width, (int)textureSoundRect.height, soundWavesBackgroundColors);
                    if (_audioClip != null)
                        AudioUtils.DoRenderPreview(_audioClip, _soundWavesTexture, _timeLinePanel.TimeLineEngine, _lastXPos, _audioClipStartTime);
                    else
                        _soundWavesTexture.Apply();
                }
                else
                    _soundWavesTexture.Apply();

                _soundWavesTextureStyle.normal.background = _soundWavesTexture;
                // refresh
                SetTimeFromSec((float)AudioClipStartTime.TotalSeconds);
            }

            GUI.Box(new Rect(_rect.x + BorderThickness, _rect.y + BorderThickness, _rect.width - BorderThickness * 2, _rect.height - BorderThickness * 2), GUIContent.none, _soundWavesTextureStyle);
            if (_dragOnAudioClip)
                EditorGUIUtility.AddCursorRect(GUITextureSoundRectTemp, MouseCursor.Pan);
            return _controlRect;
        }
        /// <summary> Method that is invoked in Update signal. </summary>
        /// <param name="allowDrag">True if drag is allowed. False otherwise.</param>
        public void Update(bool allowDrag)
        {
            _allowDrag = allowDrag;
            if (!_allowDrag)
            {
                _wasMouseDown = false;
                _dragOnAudioClip = false;
                _dragOutsideAudioClip = false;
            }
            if (_dragOnAudioClip)
            {
                TimeSpan newStart = _timeLinePanel.WorldXToTime(_mousePos.x - _diffAtGrab);
                SetTimeFromSec(newStart.TotalSeconds);
            }
        }

        /// <summary> Sets the audio clip start time by seconds.</summary>
        /// <param name="seconds">Start time.</param>
        public void SetTimeFromSec(double seconds)
        {
            if (seconds != AudioClipStartTime.TotalSeconds)
                AudioClipStartTime = ExtensionMethods.MakeTimeSpan(seconds);
        }
    }
}