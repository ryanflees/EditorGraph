using UnityEditor;
using UnityEngine;

namespace CutscenePlanner.Editor.Controls.LinePanels
{
    /// <summary> Represents the scene line panel component. </summary>
    public partial class ScenesLinePanel : Panel, ILinePanel, ILineCopy
    {
        /// <summary> On time exit label event delegate.</summary>
        /// <param name="labelText">Text that describe this label. </param>
        public delegate void LabelTimeExitDelegate(SceneData sourceData);
        /// <summary> On time enter label event delegate.</summary>
        /// <param name="sourceData">Source data that belong to label. </param>
        public delegate void LabelTimeEnterDelegate(SceneData sourceData);
        /// <summary> Label selected event delegate. </summary>
        /// <param name="source">Scene source of the panel.</param>
        public delegate void LabelSelectedDelegate(SceneData source);
        /// <summary> Label selected event. </summary>
        public event LabelSelectedDelegate SceneLabelSelected;
        /// <summary>  On time enter label event. </summary>
        public event LabelTimeEnterDelegate SceneLabelTimeEnter;
        /// <summary>  On time exit label event. </summary>
        public event LabelTimeEnterDelegate SceneLabelTimeExit;

        /// <summary> Panel to be copied.  </summary>
        public TimedData CopyData
        {
            get { return _copyData; }
            set { _copyData = value; }
        }
        /// <summary> True if data to be copied are available. False otherwise.  </summary>
        public bool CopyDataAvailable { get { return _copyData != null; } }

        private bool _wasMouseDown;
        private TimedData _copyData;
        private TimeLinePanel _timeLinePanel;

        private bool _rectWasChanged;
        private bool _durationWasChanged;
        private float _lastXPos;

        private Rect _controlRect;

        private Texture2D _backgroundTextureOryginal;

        /// <summary> Creates new scenes line panel component.</summary>
        /// <param name="timeLinePanel">TimeLinePanel that this panel is related to.</param>
        public ScenesLinePanel(TimeLinePanel timeLinePanel)
        {
            _timeLinePanel = timeLinePanel;
            _timeLinePanel.TimeLineEngine.DurationChanged += (oldDuration, newDuration) =>
            {
                _durationWasChanged = true;
            };
        }
        /// <summary> Clears labels.</summary>
        public void ClearLabels()
        {
            foreach (Label l in _labels)
                l.ClearEvents();
            _labels.Clear();
            _rectWasChanged = true;
        }
        /// <summary> Add new scene label.</summary>
        /// <param name="sourceData">Source data for this panel.</param>
        public void AddLabel(SceneData sourceData)
        {
            _labels.Add(new SceneLabel(this, _timeLinePanel, sourceData));
            _labels[_labels.Count - 1].Selected += ((TimedData source) =>
              {
                  if (SceneLabelSelected != null)
                      SceneLabelSelected.Invoke(source as SceneData);
              });
            _labels[_labels.Count - 1].TimeEnter += ((TimedData source) =>
            {
                if (SceneLabelTimeEnter != null)
                    SceneLabelTimeEnter.Invoke(source as SceneData);
            });
            _labels[_labels.Count - 1].TimeExit += ((TimedData source) =>
            {
                if (SceneLabelTimeExit != null)
                    SceneLabelTimeExit.Invoke(source as SceneData);
            });
        }
        /// <summary> Select scene label by given scene source </summary>
        /// <param name="sourceData">Scene source, that owning label should be selected.</param>
        public void SelectLabel(SceneData sourceData)
        {
            foreach (Label label in _labels)
                if (label.SourceData.Equals(sourceData))
                    label.Select();
                else
                    label.Deselect();
        }
        /// <summary> Deselect scene label by given scene source </summary>
        /// <param name="sourceData">Scene source, that owning label should be deselected.</param>
        public void DeselectLabel(SceneData sourceData)
        {
            foreach (Label label in _labels)
                if (label.SourceData.Equals(sourceData))
                    label.Deselect();
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
            if (_rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.ContextClick)
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Add"), false, HandleLabelAdd, null);
                    if (CopyDataAvailable)
                        menu.AddItem(new GUIContent("Paste"), false, HandleLabelAdd, _copyData);
                    else
                        menu.AddItem(new GUIContent("Paste"), false, null, null);
                    menu.ShowAsContext();
                }

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    _wasMouseDown = true;

                if (_wasMouseDown && Event.current.type == EventType.MouseDrag)
                {
                    foreach (Label label in _labels)
                        if (label.OnDrag)
                        {
                            _wasMouseDown = false;
                            break;
                        }
                }

                if (_wasMouseDown && (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseUp))
                    _timeLinePanel.TimeLineEngine.SetTime(_timeLinePanel.WorldXToTime(Event.current.mousePosition.x));
                if (Event.current.type == EventType.MouseUp)
                    _wasMouseDown = false;
            } else if (Event.current.mousePosition.x < _controlRect.x || Event.current.mousePosition.x > _controlRect.x + _controlRect.width)
            {
                foreach (Label label in _labels)
                    label.CancelDrag();
            }

            if (_backgroundTextureOryginal == null || _durationWasChanged || _rectWasChanged || _lastXPos != xPos || scrollDelta != 0)
            {
                int textureWidth = (int)_rect.width;
                int textureHeight = (int)_rect.height;

                if (_rectWasChanged || _backgroundTextureOryginal==null)
                {
                    _backgroundTextureOryginal = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
                    _backgroundTextureOryginal.SetPixels(_backgroundTexture.GetPixels());
                    _backgroundTextureOryginal.Apply();
                }
                _lastXPos = xPos;
                _rectWasChanged = false;
                _durationWasChanged = false;
                foreach (Label label in _labels)
                    label.Refresh();
            }

            bool redrawRequest = false;

            // checking if any label need redraw
            for (int i = _labels.Count - 1; i >= 0; i--)
            {
                if (_labels[i].DrawRequest || _labels[i].SourceData.RemoveRequest)
                {
                    // reset to oryginal
                    if (!redrawRequest)
                        _backgroundTexture.SetPixels(_backgroundTextureOryginal.GetPixels());
                    redrawRequest = true;

                    // removing requested label
                    if (_labels[i].SourceData.RemoveRequest)
                        _labels.RemoveAll(l => l.SourceData.Equals(_labels[i].SourceData));

                }
            }
            if (redrawRequest)
            {
                foreach (Label label in _labels)
                    label.Draw(_backgroundTextureOryginal, _backgroundTexture);

                _backgroundTexture.Apply();
            }
            foreach (Label label in _labels)
                label.DrawGUI();

            return _controlRect;
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

        /// <summary> Method that copy selected panel.</summary>
        public void CopySelectedPanel()
        {
            Label selectedSceneLable = _labels.Find(sl => sl.IsSelected);
            _copyData = selectedSceneLable.SourceData;
        }
        /// <summary> Method that copy and remove selected panel.</summary>
        public void CutSelecedPanel()
        {
            Label selectedSceneLable = _labels.Find(sl => sl.IsSelected);
            _copyData = selectedSceneLable.SourceData;
            RemoveLabel(selectedSceneLable.SourceData as SceneData);
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
            AddLabel(_copyData as SceneData);
        }

        private void HandleLabelAdd(object obj)
        {
            CutscenePlannerWindow window = EditorWindow.GetWindow<CutscenePlannerWindow>();
            window.HandleSceneAddButton(obj as SceneData);
        }
    }
}