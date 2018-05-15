using CutscenePlanner.Editor.Utils;
using System;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace CutscenePlanner.Editor.Controls.LinePanels
{
    /// <summary> Represents the timeline bookmark label.</summary>
    public abstract class Label
    {
        /// <summary> On time exit event delegate.</summary>
        /// <param name="sourceData">Source data that represents this label. </param>
        public delegate void TimeDelegate(TimedData sourceData);
        /// <summary> Label selected event. </summary>
        public event TimeDelegate Selected;
        /// <summary>  On time enter event. </summary>
        public event TimeDelegate TimeEnter;
        /// <summary>  On time exit event. </summary>
        public event TimeDelegate TimeExit;

        /// <summary> Dimensions of the label on the texture. </summary>
        public abstract Rect TextureRect { get; }

        /// <summary> Index of the label in the list.</summary>
        public int Index { get { return _owner.Labels.IndexOf(this); } }
        /// <summary> Source data for this panel. </summary>
        public TimedData SourceData { get { return _sourceData; }  }
        /// <summary> Owning timeline.</summary>
        public Panel Owner { get { return _owner; } }
        /// <summary> Dimension of the label on the GUI.</summary>
        public Rect GUIRect { get { Rect rect = TextureRect; return new Rect(rect.x, Owner.Rect.y, rect.width, rect.height); } }
        /// <summary> Label color. </summary>
        public Color Color
        {
            get { return SourceData.Color; }
            set
            {
                bool refresh = SourceData.Color != value;
                SourceData.Color = value;
                if (refresh)
                    Refresh();
            }
        }
        /// <summary> True, if component was not redraw since Refresh. </summary>
        public bool DrawRequest { get { return _redrawOrder; } }
        /// <summary> True, if component is selected.</summary>
        public bool IsSelected { get { return _isSelected; } }
        /// <summary> True, if component is during drag.</summary>
        public bool OnDrag { get { return _onDrag; } }
        /// <summary> True if mouse is over this label.</summary>
        public bool MouseOver { get { return GUIRect.Contains(_mousePos); } }

        protected Panel _owner;
        protected TimedData _sourceData;
        protected TimeLinePanel _timeLinePanel;
        protected bool _isSelected;
        protected bool _redrawOrder;
        protected bool _removeRequest;
        protected bool _onDrag;

        private GUIContent _content;
        private Vector2 _contentSize;
        private TimeSpan _startOld = TimeSpan.Zero;
        private string   _startOldString;
        private TimeSpan _endOld = TimeSpan.Zero;
        private string   _endOldString;
        private string   _oldName;
        private bool     _rebuildConcent;
        private Rect     _oldGUIRect; 
        private bool _wasEnter;

        /// <summary> Helping field. Returns Rect of the left edge. </summary>
        protected Rect _leftEdgeRect
        {
            get
            {
                Rect rect = GUIRect;
                return new Rect(rect.x,
                                rect.y,
                                10,
                                rect.height);
            }
        }
        /// <summary> Helping field. Returns Rect of the left edge to closests seconds on the timeline. </summary>
        protected Rect _leftEdgeRectSecond
        {
            get
            {
                Rect rect = GUIRect;
                float secondDuration = _timeLinePanel.TimeLineEngine.TimeToX(new TimeSpan(0, 0, 1));
                return new Rect(rect.x - secondDuration,
                                rect.y,
                                secondDuration * 2,
                                rect.height);
            }
        }
        /// <summary> Helping field. Returns Rect of the right edge. </summary>
        protected Rect _rightEdgeRect
        {
            get
            {
                Rect rect = GUIRect;
                return new Rect(rect.x + rect.width - 10,
                                rect.y,
                                10,
                                rect.height);
            }
        }
        /// <summary> Helping field. Returns Rect of the right edge to closests seconds on the timeline. </summary>
        protected Rect _rightEdgeRectSecond
        {
            get
            {
                Rect rect = GUIRect;
                float secondDuration = _timeLinePanel.TimeLineEngine.TimeToX(new TimeSpan(0, 0, 1));
                return new Rect(rect.x + rect.width - secondDuration,
                                rect.y,
                                secondDuration * 2,
                                rect.height);
            }
        }
        /// <summary> Helping field. Returns Rect of the center without esges. </summary>
        protected Rect _centerRect
        {
            get
            {
                Rect rect = GUIRect;
                return new Rect(rect.x + 10,
                                rect.y,
                                rect.width - 20,
                                rect.height);
            }
        }

        protected Vector2 _mousePos;
        protected float _diffAtGrab;
        protected bool _mousePressedOnLeftEdge;
        protected bool _mousePressedOnRightEdge;
        protected bool _mousePressedOnCenter;

        protected GUIStyle _contentStyle;
        protected int _borderThickness;
        protected int _borderThicknessNormal = 2;
        protected int _borderThicknessSelected = 3;

        /// <summary> Draws the label on the texture, blending with already existing pixels. </summary>
        /// <param name="toDraw">Texture on which label should be printed.</param>
        /// <param name="oryginal">Oryginal source of the pixel colors.</param>
        /// <remarks>Invoke Refresh first to allow component to be redrawn.  </remarks>
        public abstract void Draw(Texture2D oryginal, Texture2D toDraw);

        /// <summary> Create new timeline bookmark label. </summary>
        /// <param name="owner"> Owning timeline.></param>
        /// <param name="sourceData">Source data</param>
        /// <param name="timeLinePanel">TimeLinePanel.</param>
        public Label(Panel owner, TimeLinePanel timeLinePanel, TimedData sourceData)
        {
            _owner = owner;
            _timeLinePanel = timeLinePanel;
            _sourceData = sourceData;
            SetStartAndEnd(SourceData.Start, SourceData.End, SourceData.Duration);

            _redrawOrder = true;
            _borderThickness = _borderThicknessNormal;

            _timeLinePanel.TimeLineEngine.CurrentTimeChanged += TimeLineEngine_TimeChanged;
        }

        /// <summary> Set start and end time for this label.</summary>
        /// <param name="start">Start time.</param>
        /// <param name="end">End time.</param>
        /// <param name="duration">Oryginal duration</param>
        public void SetStartAndEnd(TimeSpan start, TimeSpan end, TimeSpan duration)
        {
            bool bothChanged = start != SourceData.Start && end != SourceData.End;
            if (start == SourceData.Start && end == SourceData.End)
                return;
            if (start < TimeSpan.Zero)
            {
                start = TimeSpan.Zero;
                if (bothChanged)
                    end = start + duration;
                else
                    end = SourceData.End;
            }
            if (end > _timeLinePanel.TimeLineEngine.Duration)
            {
                end = _timeLinePanel.TimeLineEngine.Duration;
                if (bothChanged)
                    start = end - duration;
                else
                    start = SourceData.Start;
            }
            TimeSpan diff = end - start;
            if (diff > new TimeSpan(0,0,0,0,100))
            {
                Label errorLabel;
                int errorType;

                bool validationOk = _owner.IsTimelabelValid(start, end, Index, out errorType, out errorLabel);

                if (!validationOk)
                {
                    if (errorType == 3)
                    {
                        if (SourceData.Start > errorLabel.SourceData.End)
                            errorType = 1;
                        if (SourceData.Start < errorLabel.SourceData.Start)
                            errorType = 2;
                    }
                    if (errorType == 1)
                    {
                        start = errorLabel.SourceData.End;
                        if (bothChanged)
                            end = start + duration;
                        else
                            end = SourceData.End;
                        validationOk = true;
                    }
                    if (errorType == 2)
                    {
                        end = errorLabel.SourceData.Start;
                        if (bothChanged)
                            start = end - duration;
                        else
                            start = SourceData.Start;
                        validationOk = true;
                    }
                    validationOk = _owner.IsTimelabelValid(start, end, Index, out errorType, out errorLabel);
                }

                if (validationOk && start >= TimeSpan.Zero && end <= _timeLinePanel.TimeLineEngine.Duration)
                {
                    SourceData.End = end;
                    SourceData.Start = start;

                    Refresh();
                }
            }
        }
        /// <summary> Rise the flag that this component needs to be redrawn. </summary>
        public void Refresh()
        {
            _redrawOrder = true;
        }
       
        /// <summary>  Draws only a GUI elements.  </summary>
        /// <remarks> Needs to be invoked only in OnGUI</remarks>
        public virtual void DrawGUI()
        {
            if (_oldGUIRect != GUIRect)
            {
                _rebuildConcent = true;
                _oldGUIRect = GUIRect;
            }
            if (_contentStyle == null)
                _contentStyle = new GUIStyle(GUI.skin.label);

            if (_startOld != SourceData.Start)
            {
                _startOld = SourceData.Start;
                _startOldString = _startOld.ToStringSpecial();
                _rebuildConcent = true;
            }
            if (_endOld != SourceData.End)
            {
                _endOld = SourceData.End;
                _endOldString = _endOld.ToStringSpecial();
                _rebuildConcent = true;
            }
            if (_oldName != SourceData.Name)
            {
                _oldName = SourceData.Name;
                _rebuildConcent = true;
            }
            Rect rect = TextureRect;
            if (_rebuildConcent)
            {
                string fromTo = "From " + _startOldString + " to " + _endOldString;
                StringBuilder newText = new StringBuilder(SourceData.Name);
                _content = new GUIContent(SourceData.Name, fromTo);

                _contentSize = _contentStyle.CalcSize(_content);
                while (_contentSize.x > rect.width - _borderThickness)
                {
                    newText.Remove(newText.Length - 1, 1);
                    if (newText.Length > 0)
                    {
                        _content = new GUIContent(newText + "...", SourceData.Name + Environment.NewLine + fromTo);
                    }
                    else
                    {
                        _content = new GUIContent(string.Empty, SourceData.Name + Environment.NewLine + fromTo);
                        _contentSize = rect.size;
                        break;
                    }

                    _contentSize = _contentStyle.CalcSize(_content);
                    _rebuildConcent = false;
                }
            }
            
            
            

            // label         
            GUI.Label(new Rect(rect.x + rect.width / 2 - _contentSize.x / 2, Owner.Rect.y + rect.height / 2 - _contentSize.y / 2 + 1, _contentSize.x, _contentSize.y), _content, _contentStyle);

            // context menu

            _mousePos = Event.current.mousePosition;


            bool GUIContainsMouse = _oldGUIRect.Contains(_mousePos);
            if (Event.current.type == EventType.ContextClick && GUIContainsMouse)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Edit"), false, ContextEditCallback, null);
                menu.AddItem(new GUIContent("Remove"), false, ContextRemoveCallback, null);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Copy"), false, ContextCopyCallback, null);
                menu.AddItem(new GUIContent("Cut"), false, ContextCutCallback, null);
                if (Owner is ILineCopy && (Owner as ILineCopy).CopyDataAvailable)
                    menu.AddItem(new GUIContent("Paste"), false, ContextPasteCallback, null);
                else
                    menu.AddItem(new GUIContent("Paste"), false, null, null);
                // menu.AddItem(new GUIContent("SubMenu/MenuItem3"), false, Callback, "item 3");
                menu.ShowAsContext();
                Event.current.Use();
            }

            // single click handle
            if (Event.current.isMouse && Event.current.type == EventType.MouseDown && GUIContainsMouse)
            {
                Select();
                // double click handle
                if (Event.current.clickCount == 2)
                {
                    _mousePressedOnCenter = false;
                    _mousePressedOnLeftEdge = false;
                    _mousePressedOnRightEdge = false;
                    ContextEditCallback(null);
                    return;
                }
            }
            if (_isSelected)
            {
                EditorGUIUtility.AddCursorRect(_mousePressedOnLeftEdge ? _leftEdgeRectSecond : _leftEdgeRect, MouseCursor.SplitResizeLeftRight);
                EditorGUIUtility.AddCursorRect(_mousePressedOnRightEdge ? _rightEdgeRectSecond : _rightEdgeRect, MouseCursor.SplitResizeLeftRight);
                EditorGUIUtility.AddCursorRect(_centerRect, MouseCursor.Pan);

                bool mouseOnLeftEdge = _mousePressedOnLeftEdge ? _leftEdgeRectSecond.Contains(_mousePos) : _leftEdgeRect.Contains(_mousePos);
                bool mouseOnRightEdge = _mousePressedOnRightEdge ? _rightEdgeRectSecond.Contains(_mousePos) : _rightEdgeRect.Contains(_mousePos);
                bool mouseOnCenter = _centerRect.Contains(_mousePos);

                if (Event.current.type == EventType.MouseDown && mouseOnLeftEdge)
                {
                    _mousePressedOnLeftEdge = true;
                    _diffAtGrab = _mousePos.x - _timeLinePanel.TimeToWorldX(SourceData.Start);
                }
                else if (Event.current.type == EventType.MouseDown && mouseOnRightEdge)
                {
                    _mousePressedOnRightEdge = true;
                    _diffAtGrab = _timeLinePanel.TimeToWorldX(SourceData.End) - _mousePos.x;
                }
                else if (Event.current.type == EventType.MouseDown && mouseOnCenter)
                {
                    _mousePressedOnCenter = true;
                    _diffAtGrab = _mousePos.x - _timeLinePanel.TimeToWorldX(SourceData.Start);
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    _mousePressedOnLeftEdge = false;
                    _mousePressedOnRightEdge = false;
                    _mousePressedOnCenter = false;
                }
            }
            else
            {
                _mousePressedOnCenter = false;
                _mousePressedOnLeftEdge = false;
                _mousePressedOnRightEdge = false;
            }
        }

        /// <summary> Method that is invoked in Update signal. </summary>
        /// <param name="allowDrag">True if drag is allowed. False otherwise.</param>
        public void Update(bool allowDrag)
        {
            if (!allowDrag)
            {
                _onDrag = false;
                return;
            }
            if (_mousePressedOnRightEdge)
            {
                _onDrag = true;
                SetStartAndEnd(SourceData.Start, _timeLinePanel.WorldXToTime(_mousePos.x + _diffAtGrab), SourceData.Duration);
            }
            else if (_mousePressedOnLeftEdge)
            {
                _onDrag = true;
                SetStartAndEnd(_timeLinePanel.WorldXToTime(_mousePos.x - _diffAtGrab), SourceData.End, SourceData.Duration);
            }
            else if (_mousePressedOnCenter)
            {
                _onDrag = true;
                TimeSpan newStart = _timeLinePanel.WorldXToTime(_mousePos.x - _diffAtGrab);
                SetStartAndEnd(newStart, newStart + SourceData.Duration, SourceData.Duration);
            }
            else
                _onDrag = false;

        }
        /// <summary> Select this label. </summary>
        public void Select()
        {
            if (!_isSelected)
            {
                foreach (Label label in _owner.Labels)
                    label.Deselect();

                _isSelected = true;
                OnSelected(_sourceData);
                _borderThickness = _borderThicknessSelected;
                if (_contentStyle != null)
                    _contentStyle.fontStyle = FontStyle.Bold;
                if (LabelEditorWindow.IsShown)
                    LabelEditorWindow.Show(this);
                Refresh();
            }
        }
        /// <summary> Cabcel the drag. </summary>
        public void CancelDrag()
        {
            _onDrag = false;
            _mousePressedOnRightEdge = false;
            _mousePressedOnCenter = false;
            _mousePressedOnLeftEdge = false;
        }
        /// <summary> Deselect this label.</summary>
        public void Deselect()
        {
            if (_isSelected)
            {
                _isSelected = false;
                _borderThickness = _borderThicknessNormal;
                _contentStyle.fontStyle = FontStyle.Normal;
                Refresh();
            }
        }
        /// <summary> Checks if label time range contains given time. </summary>
        /// <param name="time">Time to test.</param>
        /// <returns>True if label time range contains given time. False otherwise.</returns>
        public bool ContainsTime(TimeSpan time)
        {
            return ContainsTime(time.TotalSeconds);
        }
        /// <summary> Checks if label time range contains given time. </summary>
        /// <param name="time">Time to test.</param>
        /// <returns>True if label time range contains given time. False otherwise.</returns>
        public bool ContainsTime(double time)
        {
            return time > SourceData.Start.TotalSeconds && time < SourceData.End.TotalSeconds;
        }
        /// <summary> Clears all events.</summary>
        public void ClearEvents()
        {
            Selected = null;
            TimeEnter = null;
            TimeExit = null;
        }
        protected void OnSelected(TimedData source)
        {
            if (Selected != null)
                Selected.Invoke(source);
        }
        protected void OnTimeEnter(TimedData source)
        {
            if (TimeEnter != null)
                TimeEnter.Invoke(source);
        }
        protected void OnTimeExit(TimedData source)
        {
            if (TimeExit != null)
                TimeExit.Invoke(source);
        }


        protected void ContextEditCallback(object obj)
        {
            LabelEditorWindow.Show(this);
        }
        protected void ContextRemoveCallback(object obj)
        {
            _sourceData.MarkToDelete();
        }
        protected void ContextCopyCallback(object obj)
        {
            ILineCopy panel = Owner as ILineCopy;
            if (panel != null)
                panel.CopySelectedPanel();
        }
        protected void ContextCutCallback(object obj)
        {
            ILineCopy panel = Owner as ILineCopy;
            if (panel != null)
                panel.CutSelecedPanel();
        }
        protected void ContextPasteCallback(object obj)
        {
            ILineCopy panel = Owner as ILineCopy;
            if (panel != null)
                panel.PasteToSelectedPanel();
        }
        
        protected static Color ChangeColorBrightness(Color color, float correctionFactor)
        {
            float red = color.r;
            float green = color.g;
            float blue = color.b;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (1 - red) * correctionFactor + red;
                green = (1 - green) * correctionFactor + green;
                blue = (1 - blue) * correctionFactor + blue;
            }

            return new Color(red, green, blue);
        }

        private void TimeLineEngine_TimeChanged(TimeSpan lastTime, TimeSpan actualTime)
        {
            if (_wasEnter && !ContainsTime(actualTime))
            {
                OnTimeExit(_sourceData);
                _wasEnter = false;
            }
            else if (!_wasEnter && ContainsTime(actualTime))
            {
                _wasEnter = true;
                OnTimeEnter(_sourceData);
            }

            SourceData.UpdateTime(actualTime);
        }
    }
}
