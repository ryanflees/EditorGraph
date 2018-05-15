using System.Collections.Generic;
using UnityEngine;
using System;
using CutscenePlanner.Editor.Utils;
using UnityEditor;
using System.Linq;

namespace CutscenePlanner.Editor.Controls.LinePanels
{
    /// <summary> Represents the time line panel component. </summary>
    public partial class TimeLinePanel : Panel, ILinePanel, ILineCopy
    {
        /// <summary> On label event delegate.</summary>
        /// <param name="timeLabelData">Source data that represents label. </param>
        public delegate void LabelDelegate(TimeLabelData timeLabelData);
        /// <summary> On zoom changed event delegate.</summary>
        /// <param name="zoom">New zoom value. </param>
        public delegate void ZoomChangedDelegate(float zoom);

        public event ZoomChangedDelegate ZoomChanged;
        /// <summary>  On time enter label event. </summary>
        public event LabelDelegate LabelTimeEnter;
        /// <summary>  On time exit label event. </summary>
        public event LabelDelegate LabelTimeExit;
        /// <summary>  On time exit label event. </summary>
        public event LabelDelegate LabelCreate;
        /// <summary>  On time exit label event. </summary>
        public event LabelDelegate LabelDelete;

        /// <summary> Panel to be copied.  </summary>
        public TimedData CopyData
        {
            get { return _copyData; }
            set { _copyData = value; }
        }
        /// <summary> True if data to be copied are available. False otherwise.  </summary>
        public bool CopyDataAvailable { get { return _copyData != null; } }

        /// <summary> Calculation engine of the timeline. </summary>
        public TimeFlowEngine TimeLineEngine { get { return _timeLineEngine; } }
        /// <summary> True if Timeline flow is externa. False otherwise. </summary>
        public bool TimeLineEngineIsExternal { get { return _timeLineEngineIsExternal; } }
        /// <summary>GUI content of the actual time of the timeline.</summary>
        public GUIContent GUITime { get { return new GUIContent(_timeLineEngine.Time.ToStringSpecial()); } }
        /// <summary> Normalized mouse X position.</summary>
        public float MouseNormalizedWidth { get { return Mathf.Clamp01(_mouseNormalizedWidth); } }
        /// <summary> Zoom value. </summary>
        public float Zoom
        {
            get { return _zoom; }
            set
            {
                if (value != _zoom)
                {
                    _zoom = value;
                    _zoomChanged = true;
                    if (ZoomChanged!=null)
                        ZoomChanged(_zoom);
                }
            }
        }
        /// <summary> Pointer height.</summary>
        public float PointerHeight { get { return 148; } }
        /// <summary> Height of the small stroke.</summary>
        public float SmallStroke { get { return _smallStroke; } }
        private TimedData _copyData;
        private TimeFlowEngine _timeLineEngine
        {
            get {
                if (_timeLineEngineVar == null)
                {
                    _timeLineEngineVar = new TimeFlowEngine(60);
                    _timeLineEngine.DurationChanged += (oldDuration, newDuration) =>
                    {
                        _durationWasChanged = true;
                        RecalcScale(_rect.width);
                    };
                    RecalcScale(_rect.width);
                    _timeLineEngineIsExternal = false;
                }
                return _timeLineEngineVar;
            }
        }
        private TimeFlowEngine _timeLineEngineVar;
        private bool _timeLineEngineIsExternal;
        private bool _zoomChanged;
        private int _smallStroke;
        private int _bigStroke;
        private float _mouseNormalizedWidth; 
        private bool _wasMouseDown;
        private float _zoom = 1;
        private float _zoomStep = 0.1f;

        private List<GUITimeLabelData> _GUILabelDatas;
        private Rect _controlRect;
        private int _labelStrokesWidth = 1;
        private Vector2 _timeLabelSize;

        private bool _rectWasChanged;
        private bool _durationWasChanged;
        [SerializeField]
        private float _lastXPos;
        private Texture2D _pointerTexture {
            get
            {
                if (_pointerTextureVar == null)
                {
                    _pointerTextureVar = new Texture2D(1, 1) { wrapMode = TextureWrapMode.Repeat };
                    _pointerTexture.SetPixel(0, 0, Color.red);
                    _pointerTexture.Apply();
                }
                return _pointerTextureVar;
            }
        }
        private Texture2D _strokeTexture;
        private Texture2D _strokeTextureOryginal;
        private GUIStyle _strokeTextureStyle;

        private Texture2D _pointerTextureVar;
        Color[] _timelineBackgroundColor;

        /// <summary> Creates new Timeline panel.</summary>
        public TimeLinePanel () : base()
        {
            NormalColor = Color.white;
            _GUILabelDatas = new List<GUITimeLabelData>();
            BorderThickness = 1;
            _strokeTextureStyle = new GUIStyle();
        }
        /// <summary> Create new time flow engine.</summary>
        public void CreateNewEngine(TimeSpan currentTime, TimeSpan duration)
        {
            _timeLineEngineVar = new TimeFlowEngine(_timeLineEngineVar, currentTime, duration);

            RecalcScale(_rect.width);
        }
        /// <summary> Assigns external time flow engine.</summary>
        /// <param name="timeFlowEngine">Exteral timeflow to be assigned.</param>
        public void AssignExternalTimeFlowEngine(TimeFlowEngine timeFlowEngine)
        {
            _timeLineEngineVar.CopyEventsSubscriptions(timeFlowEngine);

            _timeLineEngineVar = timeFlowEngine;

            RecalcScale(_rect.width);
            _timeLineEngineIsExternal = true;
        }
        /// <summary> Do component rect recalcing, basing on the given one [have to be invoked in OnGUI].. </summary>
        /// <param name="rect">Window, in which should be shown timeline.</param>
        /// <returns>Real size of the component.</returns>
        public Rect GUICalcRect(Rect rect)
        {
            bool widthIsDiffrent = !Mathf.Approximately(rect.width, _rect.width);
            if (widthIsDiffrent)
            {
                RecalcScale(rect.width); 
                _rectWasChanged = true;
            }
            _mouseNormalizedWidth = Event.current.mousePosition.x/rect.width;
            _rect = rect;           
            _controlRect = new Rect(rect.x, rect.y, _timeLineEngine.DurationInPixels, rect.height);
            return _controlRect;
        }

        /// <summary> Draws the component [have to be invoked in OnGUI].</summary>
        /// <param name="rect">X position for the timeline.</param>
        /// <param name="scrollDelta">Scroll delta.</param>
        /// <returns>Real component size.</returns>
        public Rect GUIDraw(float xPos, float scrollDelta)
        {
            base.Draw(_rect, _rectWasChanged);

            if (_rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.ContextClick)
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Add"), false, HandleLabelAdd, null);
                    if (CopyDataAvailable)
                        menu.AddItem(new GUIContent("Paste"), false, HandleLabelPaste, null);
                    else
                        menu.AddItem(new GUIContent("Paste"), false, null, null);
                    menu.ShowAsContext();
                }

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    _wasMouseDown = true;
                if (Event.current.type == EventType.MouseDrag)
                {
                    foreach (Label label in _labels)
                        if (label.OnDrag)
                        {
                            _wasMouseDown = false;
                            break;
                        }
                }

                if (_wasMouseDown && (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseUp))
                    _timeLineEngine.SetTime(WorldXToTime(Event.current.mousePosition.x));

                if (Event.current.type == EventType.MouseUp)
                    _wasMouseDown = false;
            }

            if (_strokeTexture == null || _durationWasChanged || _rectWasChanged || _lastXPos != xPos || scrollDelta!=0 || _zoomChanged)
            {
                RecalcScale(_rect.width);
                _zoomChanged = false;

                int textureWidth = (int)_rect.width;
                int textureHeight = (int)_rect.height;
                if (_rectWasChanged)
                {
                    _timelineBackgroundColor = new Color[textureHeight * textureWidth];
                    _timelineBackgroundColor.Fill(new Color(1,1,1,0));
                }

                
                _strokeTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
                _strokeTexture.SetPixels(_timelineBackgroundColor);

                _strokeTextureOryginal = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
                

                _smallStroke = (int)GUI.skin.label.CalcSize(new GUIContent("text")).y + 5;
                _bigStroke = _smallStroke + 10;
                _lastXPos = xPos;
                _rectWasChanged = false;
                _durationWasChanged = false;

                _GUILabelDatas.Clear();
                float significantStepDuration = Mathf.Ceil((_timeLabelSize.x * 1.5f) / _timeLineEngine.PixelsPerSecond) * _timeLineEngine.PixelsPerSecond;
                float significantStrokeBuffor = (_timeLabelSize.x * 1.5f);
                Color[] strokeLong = new Color[_bigStroke * 2];

                strokeLong.Fill(Color.black);


                float offset = significantStepDuration - ((xPos / significantStepDuration) - (int)(xPos / significantStepDuration)) * significantStepDuration;
                int divider = Mathf.CeilToInt((_timeLabelSize.x * 1.5f)  / _timeLineEngine.PixelsPerSecond) - 1;
                if (divider < 2)
                    divider = 2;
                int minorStrokeCount = divider - ((divider - 1) / 5) * 5; // do not simplify! Int type is not by accident!
                if (minorStrokeCount < 2)
                    minorStrokeCount = 2;

                float step = (_timeLineEngine.PixelsPerSecond * (divider-1)) / (minorStrokeCount-1);
                for (float x = offset - significantStepDuration; x+ _labelStrokesWidth*2 < _rect.width; x += step)
                {
                    
                    if (significantStrokeBuffor >= (_timeLabelSize.x * 1.5f))
                    {
                        if (x >= 0)
                            _strokeTexture.SetPixels((int)x, Mathf.RoundToInt(_controlRect.height - _bigStroke), _labelStrokesWidth*2, _bigStroke, strokeLong);
                        TimeSpan strokeTime = new TimeSpan(0,0, Mathf.RoundToInt((x + xPos) / _timeLineEngine.PixelsPerSecond));
                        float labelX = TimeToWorldX(strokeTime) - _timeLabelSize.x / 2;
                        if (strokeTime == TimeSpan.Zero)
                            labelX = TimeToWorldX(strokeTime);
                        _GUILabelDatas.Add(new GUITimeLabelData(labelX, _controlRect.y + _smallStroke, _timeLabelSize, strokeTime.ToStringSpecial()));

                        significantStrokeBuffor = step;
                    } 
                    else
                    {
                        if (x >= 0)
                            _strokeTexture.SetPixels((int)x, Mathf.RoundToInt(_controlRect.height - _smallStroke), _labelStrokesWidth, _smallStroke, strokeLong);
                        significantStrokeBuffor += step;
                    }
                    
                }
                _GUILabelDatas.Add(new GUITimeLabelData(TimeToWorldX(_timeLineEngine.Duration) - _timeLabelSize.x, _controlRect.y + _smallStroke, _timeLabelSize, _timeLineEngine.Duration.ToStringSpecial()));
                if (_GUILabelDatas.Count >= 2 &&_GUILabelDatas[_GUILabelDatas.Count - 2].Rect.x + _GUILabelDatas[_GUILabelDatas.Count - 2].Rect.width > _GUILabelDatas[_GUILabelDatas.Count - 1].Rect.x)
                    _GUILabelDatas.RemoveAt(_GUILabelDatas.Count - 2);
                _strokeTextureStyle.normal.background = _strokeTexture;
                foreach (Label label in _labels)
                    label.Refresh();
                _strokeTexture.Apply();

                _strokeTextureOryginal.SetPixels(_strokeTexture.GetPixels());
                
            }
            bool redrawRequest = false;

            // checking if any label need redraw
            for (int i = _labels.Count-1; i>=0 ; i--)
            {
                if (_labels[i].DrawRequest || _labels[i].SourceData.RemoveRequest)
                {
                    // reset to oryginal
                    if (!redrawRequest)
                        _strokeTexture.SetPixels(_strokeTextureOryginal.GetPixels());
                    redrawRequest = true;

                    // removing requested label
                    if (_labels[i].SourceData.RemoveRequest)
                    {
                        TimeLabelData data = _labels[i].SourceData as TimeLabelData;
                        _labels.RemoveAt(i);
                        if (LabelDelete != null)
                            LabelDelete.Invoke(data);
                        
                    }
                }
            }
            if (redrawRequest)
            {
                foreach (Label label in _labels)
                    label.Draw(_strokeTextureOryginal, _strokeTexture);
                
                 _strokeTexture.Apply();

            }
            GUI.Box(_rect, GUIContent.none, _strokeTextureStyle);

            foreach (Label label in _labels)
                label.DrawGUI();
            foreach (GUITimeLabelData label in _GUILabelDatas) 
                label.Draw(_controlRect.y + _bigStroke);

            return _controlRect;
        }
        /// <summary> Draws the red line. </summary>
        /// <returns> Pointer x position.</returns>
        public float GUIDrawPointer()
        {
            Rect pointerRect = new Rect(TimeToWorldX(_timeLineEngine.Time), _rect.y - 99, 2, PointerHeight);
            GUI.DrawTextureWithTexCoords(pointerRect, _pointerTexture, new Rect(0, 0, pointerRect.width, pointerRect.height));
            return pointerRect.x;
        }
        /// <summary> Method that is invoked in Update signal. </summary>
        /// <param name="allowDrag">True if drag is allowed. False otherwise.</param>
        public void Update(bool allowDrag)
        {
            if (!allowDrag)
            {
                _wasMouseDown = false;
                foreach (Label label in _labels)
                    label.CancelDrag();
            }
            for (int i = 0; i < _labels.Count; i++)
                _labels[i].Update(allowDrag);
        }
        /// <summary> Zoom in the timeline.  </summary>
        public void ZoomIn()
        {
            Zoom += _zoomStep;
            RecalcScale(_rect.width);
        }
        /// <summary> Zoom out the timeline.  </summary>
        public void ZoomOut()
        {
            float newZoom = _zoom;
            newZoom -= _zoomStep;
            if (newZoom < 1)
                newZoom = 1;
            Zoom = newZoom;
            RecalcScale(_rect.width);
        }
        /// <summary> Clears labels.</summary>
        public void ClearLabels()
        {
            foreach (Label l in _labels)
                l.ClearEvents();
            _labels.Clear();
            _rectWasChanged = true;
        }
        /// <summary> Converts time on the time lint to world (visible viewport) relative x position. </summary>
        /// <param name="time">Time on the timeline.</param>
        /// <returns>Converted time to world relative x position.</returns>
        public float TimeToWorldX(TimeSpan time)
        {
            return _timeLineEngine.TimeToX(time) - _lastXPos; 
        }
        /// <summary> Converts time on the time lint to world (visible viewport) relative x position. </summary>
        /// <param name="seconds">Time on the timeline in seconds.</param>
        /// <returns>Converted time to world relative x position.</returns>
        public float TimeToWorldX(float seconds)
        {
            return _timeLineEngine.TimeToX(seconds) - _lastXPos;
        }
        /// <summary> Convert the world relative x position to time on the timeline. </summary>
        /// <param name="x">X position on the screen.</param>
        /// <returns>Time on the timeline.</returns>
        public TimeSpan WorldXToTime(float x)
        {
            int totalMiliseconds = (int)(((x + _lastXPos) / _timeLineEngine.PixelsPerSecond) * 1000);
            return new TimeSpan(0, 0, 0, 0, totalMiliseconds).RoundToMiliseconds();
        }

        public List<TimeLabelData> TimeLabelsToSourceList()
        {
            List<TimeLabelData> result = new List<TimeLabelData>();
            foreach (Label timeLabel in _labels)
                result.Add(timeLabel.SourceData as TimeLabelData);
            return result;
        }
        public void SourceListToTimeLabels(List<TimeLabelData> sourcesList)
        {
            _labels.Clear();
            _rectWasChanged = true;
            foreach (TimeLabelData data in sourcesList)
            {
                TimeLabel newTimeLabel = new TimeLabel(this, data);
                newTimeLabel.TimeEnter += ((TimedData timeLabelData) =>
                {
                    if (LabelTimeEnter != null)
                        LabelTimeEnter.Invoke(timeLabelData as TimeLabelData);
                });
                newTimeLabel.TimeExit += ((timeLabelData) =>
                {
                    if (LabelTimeExit != null)
                        LabelTimeExit.Invoke(timeLabelData as TimeLabelData);
                });
                _labels.Add(newTimeLabel);
            }
        }
        /// <summary> Method that copy selected panel.</summary>
        public void CopySelectedPanel()
        {
            Label selectedSceneLable = _labels.Find(sl => sl.IsSelected);
            _copyData = new TimeLabelData(selectedSceneLable.SourceData as TimeLabelData);
        }
        /// <summary> Method that copy and remove selected panel.</summary>
        public void CutSelecedPanel()
        {
            Label selectedSceneLable = _labels.Find(sl => sl.IsSelected);
            _copyData = new TimeLabelData(selectedSceneLable.SourceData as TimeLabelData);
            RemoveLabel(selectedSceneLable.SourceData);
        }
        /// <summary> Method that assign copied panel data to selected one.</summary>
        public void PasteToSelectedPanel()
        {
            if (_copyData == null)
                return;
            Label selectedSceneLable = _labels.Find(sl => sl.IsSelected);
            if (selectedSceneLable != null)
            {
                selectedSceneLable.SourceData.AssignWithoutTime(_copyData);
                selectedSceneLable.Refresh();
            }
        }
        /// <summary> Method that assign copied panel data to new one.</summary>
        public void PasteToNewPanel()
        {
            if (_copyData == null)
                return;
            HandleLabelAdd(_copyData as TimeLabelData);
        }
        /// <summary> Recalcs scale when window is resized.</summary>
        /// <param name="windowWidth"></param>
        private void RecalcScale(float windowWidth)
        {
            _timeLineEngine.RecalcScale(windowWidth, ref _zoom);
            GUIContent labelContent = new GUIContent(_timeLineEngine.Duration.ToStringSpecial());
            Vector2 o = Vector2.zero;
            try
            {
                o = new GUIStyle(GUI.skin.label).CalcSize(labelContent);
            }
            catch
            {

            }
            _timeLabelSize = new Vector2(o.x, o.y);
        }
        private void HandleLabelPaste(object obj)
        {
            HandleLabelAdd(_copyData);
        }

        private void HandleLabelAdd(object obj)
        {
            TimeSpan coursorTime = _timeLineEngine.Time;
            TimeSpan duration = new TimeSpan(0, 0, 10);
            if (obj != null && obj is TimeLabelData)
                duration = (obj as TimeLabelData).Duration;
            foreach (TimeLabel timeLabel in _labels)
            {
                if (coursorTime >= timeLabel.SourceData.Start && coursorTime < timeLabel.SourceData.End)
                {
                    EditorUtility.DisplayDialog("Error!", "You cannot add time label on the occupied time.", "Ok");
                    return;
                }
                if (coursorTime < timeLabel.SourceData.Start && coursorTime + duration > timeLabel.SourceData.Start)
                {
                    TimeSpan gap = timeLabel.SourceData.Start - coursorTime;
                    if (gap < duration) 
                        duration = gap;
                }
            }
            if (coursorTime + duration > _timeLineEngine.Duration)
                duration = _timeLineEngine.Duration - coursorTime;
            TimeLabelData data;
            if (obj != null && obj is TimeLabelData)
            {
                data = new TimeLabelData(obj as TimeLabelData);
            }
            else
            {
                data = new TimeLabelData()
                {
                    Name = "Time label",
                    Color = Color.cyan
                };
            }
            data.Start = coursorTime;
            data.End = coursorTime + duration;
            TimeLabel newTimeLabel = new TimeLabel(this, data);
            newTimeLabel.TimeEnter += ((TimedData timeLabelData) =>
            {
                if (LabelTimeEnter != null)
                    LabelTimeEnter.Invoke(timeLabelData as TimeLabelData);
            });
            newTimeLabel.TimeExit += ((TimedData timeLabelData) =>
            {
                if (LabelTimeExit != null)
                    LabelTimeExit.Invoke(timeLabelData as TimeLabelData);
            });
            _labels.Add(newTimeLabel);
            if (LabelCreate != null)
                LabelCreate.Invoke(data);
        }

       
        /// <summary> Represents the data required by GUI Label</summary>
        private class GUITimeLabelData
        {
            /// <summary> Rect of the gui label.</summary>
            public readonly Rect Rect;
            /// <summary> Content of the gui label. </summary>
            public readonly GUIContent Content;

            /// <summary> Creates new gui label data.</summary>
            /// <param name="x">X position of label's rect.</param>
            /// <param name="y">Y position of label's rect.</param>
            /// <param name="size">Size of label's rect.</param>
            /// <param name="content">Label text.</param>
            public GUITimeLabelData(float x, float y, Vector2 size, string content)
            {
                Rect = new Rect(new Vector2(x,y), size);
                Content = new GUIContent(content);
            }
            /// <summary> Draws the label. Need to be invoked in OnGUI </summary>
            /// <param name="Y">Y of the label, since window of the timeline can change its height.</param>
            public void Draw(float Y)
            {
                GUI.Label(new Rect(new Vector2(Rect.x, Y), Rect.size), Content);
            }
        }
    }
}