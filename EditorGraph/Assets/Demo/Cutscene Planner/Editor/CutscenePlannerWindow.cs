using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using CutscenePlanner.Editor.Resources;
using CutscenePlanner.Editor.Controls;
using CutscenePlanner.Editor.Controls.LinePanels;
using CutscenePlanner.Editor.Utils;
using System;

namespace CutscenePlanner.Editor
{
    /// <summary> Represents the cutscene planner window. </summary>
    public class CutscenePlannerWindow : EditorWindow
    {
        [SerializeField]
        private CutsceneAsset _baseData;
        private List<SideScenePanel> _scenePanels = new List<SideScenePanel>();
        [NonSerialized]  
        private bool initialized = false;

        /* GUI VARIABLES */
        /// <summary> Timeline control. </summary>
        private TimeLinePanel _timeLine
        {
            get {
                if (_timeLineVar == null)
                    _timeLineVar = new TimeLinePanel();

                return _timeLineVar;
            }
        }
        /// <summary> scenes time line control. </summary>
        private ScenesLinePanel _scenesLine {
            get
            {
                if (_scenesLineVar == null)
                {
                    _scenesLineVar = new ScenesLinePanel(_timeLine);
                    _scenesLineVar.SceneLabelSelected += OnSceneLabelSelected;
                    _scenesLineVar.SceneLabelTimeEnter += OnLabelTimeEnter;
                    _scenesLineVar.SceneLabelTimeExit += OnLabelTimeExit;
                }
                return _scenesLineVar; }
        }
        /// <summary> scenes time line control. </summary>
        private SoundLinePanel _soundLine
        {
            get
            {
                if (_soundLineVar == null)
                    _soundLineVar = new SoundLinePanel(_timeLine);
                return _soundLineVar;
            }
        }

        /// <summary> Time text field control. </summary>
        private TimeTextField _timeTextField { get { if (_timeTextFieldVar == null) _timeTextFieldVar = new TimeTextField(); return _timeTextFieldVar; } }

        /// <summary> Width of the right panel represented as percentage part of whole window [value between 0.0 and 1.0]</summary>
        private float _scenesPanelWidthP = 0.3f;
        /// <summary> Height of the bottom panel represented as percentage part of whole window [value between 0.0 and 1.0]</summary>
        private float _timelinePanelHeightP;
        /// <summary> Max height of the bottom panel represented as percentage part of whole window [value between 0.0 and 1.0]</summary>
        private float _timelinePanelHeightPMax;
        /// <summary> Min height of the bottom panel represented as percentage part of whole window [value between 0.0 and 1.0]</summary>
        private float _timelinePanelHeightPMin;
        /// <summary> Width of the bottom panel represented as percentage part of whole window [value between 0.0 and 1.0]</summary>
        private float _timelinePanelWidthP { get { return 1 - _scenesPanelWidthP; } }
        /// <summary>  Width of the main panel represented as percentage part of whole window [value between 0.0 and 1.0]</summary>
        private float _mainPanelWidthP { get { return 1 - _scenesPanelWidthP; } }
        /// <summary>  Height of the main panel represented as percentage part of whole window [value between 0.0 and 1.0]</summary>
        private float _mainPanelHeightP { get { return 1 - _timelinePanelHeightP; } }

        /// <summary> Helping field. Returns Rect of the right panel.</summary>
        private Rect _scenesPanelRect
        {
            get
            {
                return new Rect(position.width * (1 - _scenesPanelWidthP),    // X
                                0,                                            // Y
                                position.width * _scenesPanelWidthP,          // Width
                                position.height);                             // Height
            }
        }
        /// <summary> Helping field. Returns Rect of the bottom panel.</summary>
        private Rect _timelinePanelRect
        {
            get
            {
                return new Rect(0,                                              // X
                                position.height * (1 - _timelinePanelHeightP),  // Y
                                position.width * _timelinePanelWidthP - 1,      // Width
                                position.height * _timelinePanelHeightP);       // Height
            }
        }
        /// <summary> Helping field. Returns Rect of the main panel.</summary>
        private Rect _mainPanelRect
        {
            get
            {
                return new Rect(0,                                              // X
                                0,                                              // Y
                                position.width * _mainPanelWidthP - 1,      // Width
                                position.height * _mainPanelHeightP);       // Height
            }
        }

        private float _edgeThickness = 6;
        /// <summary> Helping field. Returns Rect of the vertical edge. </summary>
        private Rect _verticalEdgeRect
        {
            get
            {
                return new Rect(_scenesPanelRect.x - _edgeThickness / 2,
                                _scenesPanelRect.y,
                                _edgeThickness,
                                _scenesPanelRect.height);
            }
        }
        /// <summary> Helping field. Returns Rect of the Horizontal edge. </summary>
        private Rect _horizontalEdgeRect
        {
            get
            {
                return new Rect(_timelinePanelRect.x,
                                _timelinePanelRect.y - _edgeThickness / 2,
                                _timelinePanelRect.width,
                                _edgeThickness);
            }
        }
        /// <summary> Size of the every panel title header.</summary>
        private float _windowTitleHeight = 16;
        /// <summary> Normalized size of the every panel title header.</summary>
        private float _windowTitleHeightNormalized { get { return 16 / position.height; } }

        /// <summary> Heights of every field or button. </summary>
        private float _fieldsHeight = 15;
        /// <summary> Space between every field or buttons. </summary>
        private float _spaceBetweenFields = 5;

        private float _lastHeight;
        private float _scrollWidth;
        private float _scrollHeight;
        private Vector2 _scenePanelScrollPos;
        private Vector2 _timelinePanelScrollPos;
        private TimeLinePanel _timeLineVar;
        private ScenesLinePanel _scenesLineVar;
        private SoundLinePanel _soundLineVar;
        private TimeTextField _timeTextFieldVar;
        private SceneData _selectedScene;
        private bool _enableSave = true;
        private float _lastTime;
        /* MOUSE VARIABLES */

        private Vector2 _mousePos;
        /// <summary> Normalized mouse pos. </summary>
        private Vector2 _mousePosNormalized { get { return new Vector2(_mousePos.x / position.width, _mousePos.y / position.height); } }
        /// <summary> True, if mouse is on vertical edge. </summary>
        private bool _mouseOnVerticalEdge { get { return _verticalEdgeRect.Contains(_mousePos); } }
        /// <summary> True, if mouse is on horizontal edge. </summary>
        private bool _mouseOnHorizontalEdge { get { return _horizontalEdgeRect.Contains(_mousePos); } }

        private bool _mouseWasReleased;
        private bool _mousePressedOnHorizontal;
        private bool _mousePressedOnVertical;
        private bool _isGamePlaying = false;

        private void OnEnable()
        {
            if (!initialized) 
                LoadData(_baseData);
        }

        private void OnLostFocus()
        {
            SaveData();
        }
        private void OnDestroy()
        {
            SaveData();
        }
        private void OnProjectChange()
        {
            SaveData();
        }

        void OnGUI()
        {
            if (Event.current.rawType == EventType.KeyUp)
                SaveData();
            if (Event.current.rawType == EventType.MouseUp)
            {
                _mouseWasReleased = true;
                SaveData();
            }

            if (Event.current.type == EventType.MouseDrag) 
                Repaint();
            
            titleContent = new GUIContent("Cutscene", EditorResources.CutsceneIconSmall.Texture);
            // on height changed

            if (_lastHeight != position.height)
            {
                _timelinePanelHeightPMax = (_timeLine.PointerHeight + _windowTitleHeight + 45) / position.height;
                _timelinePanelHeightPMin = (_timeLine.PointerHeight + _windowTitleHeight + 17)/position.height;
                if (_timelinePanelHeightP < _timelinePanelHeightPMin)
                    _timelinePanelHeightP = _timelinePanelHeightPMin;
                if (_timelinePanelHeightP > _timelinePanelHeightPMax)
                    _timelinePanelHeightP = _timelinePanelHeightPMax;

                _lastHeight = position.height;
            }


            _scrollWidth = GUI.skin.verticalScrollbar.fixedWidth;
            _scrollHeight = GUI.skin.horizontalScrollbar.fixedHeight;
            _mousePos = Event.current.mousePosition;
            if (Event.current.type == EventType.MouseDown && _mouseOnHorizontalEdge)
                _mousePressedOnHorizontal = true;
            if (Event.current.type == EventType.MouseDown && _mouseOnVerticalEdge)
                _mousePressedOnVertical = true;
            if (Event.current.type == EventType.MouseUp || !position.Contains(new Vector2(Event.current.mousePosition.x + position.x, Event.current.mousePosition.y + position.y)))
            {
                _mousePressedOnHorizontal = false;
                _mousePressedOnVertical = false;
            }

            EditorGUIUtility.AddCursorRect(_verticalEdgeRect, MouseCursor.SplitResizeLeftRight);
            EditorGUIUtility.AddCursorRect(_horizontalEdgeRect, MouseCursor.SplitResizeUpDown);

            BeginWindows();

            GUI.Window(1, _scenesPanelRect, HandleScenesPanel, "Scenes");
            GUI.Window(2, _timelinePanelRect, HandleTimelinePanel, "Timeline");
            GUI.Window(3, _mainPanelRect, HandleMainPanel, "Concept art");

            EndWindows();
        }
        void OnInspectorUpdate()
        {
            SaveData();
            Repaint();
        }
        void Update()
        {
            if (_isGamePlaying && !EditorApplication.isPlaying)
            {
                _timeLineVar = null;
                _scenesLineVar = null;
                _soundLineVar = null;
                _timeTextFieldVar = null;

                LoadData(_baseData);
                _isGamePlaying = false;
            }
            if (!_isGamePlaying && EditorApplication.isPlaying)
            {
                if (_baseData != null)
                {
                    CutsceneEngine[] engines = FindObjectsOfType<CutsceneEngine>();
                    foreach (CutsceneEngine engine in engines)
                    {
                        if (engine.Cutscene.GetInstanceID() == _baseData.GetInstanceID())
                        {
                            _timeLine.AssignExternalTimeFlowEngine(engine.TimeEngine);
                            break;
                        }
                    }
                    _isGamePlaying = true;
                }
            }

            
            float currentTime = Time.realtimeSinceStartup;
            float deltaTime = currentTime - _lastTime;
            if (!_timeLine.TimeLineEngineIsExternal)
                _timeLine.TimeLineEngine.HandleTime(deltaTime);
            _lastTime = currentTime;

            if (_mousePressedOnHorizontal)
            {
                _timelinePanelHeightP = 1 - _mousePosNormalized.y;
                if (_timelinePanelHeightP < _timelinePanelHeightPMin)
                    _timelinePanelHeightP = _timelinePanelHeightPMin;
                if (_timelinePanelHeightP > _timelinePanelHeightPMax)
                    _timelinePanelHeightP = _timelinePanelHeightPMax;
            }
            if (_mousePressedOnVertical)
            {
                _scenesPanelWidthP = 1 - _mousePosNormalized.x;
                if (position.width * _scenesPanelWidthP < 83)
                    _scenesPanelWidthP = 83 / position.width;
            }
            bool allowDrag = (!_mousePressedOnHorizontal && !_mousePressedOnVertical) && !_mouseWasReleased;
            _timeLine.Update(allowDrag);
            _scenesLine.Update(allowDrag);
            _soundLine.Update(allowDrag);
            if (_mouseWasReleased)
                _mouseWasReleased = false;
        }
        /// <summary> Load data from given asset data.</summary>
        /// <param name="data">Data to be loaded.</param>
        public void LoadData(CutsceneAsset data)
        {
            if (data == null)
                return;
            _enableSave = false;


            _baseData = null;
            _selectedScene = null;
            _scenePanels.Clear(); 
            _scenesLine.ClearLabels();
            _timeLine.ClearLabels();
            _soundLine.AudioClip = null;
            _soundLine.AudioClipStartTime = TimeSpan.Zero;
            

            _baseData = data;
           
            _timeLine.Zoom = _baseData.Zoom;

            _soundLine.AudioClip = _baseData.AudioClip;
            _soundLine.AudioClipStartTime = new TimeSpan(_baseData.AudioClipStartTimeTicks);

            foreach (SceneData scene in _baseData.Scenes)
            {
                SideScenePanel panel = new SideScenePanel(scene);
                panel.Click += OnScenePanelClick;
                panel.Delete += OnScenePanelDelete;
                _scenePanels.Add(panel);
                _scenesLine.AddLabel(panel.Scene);
            }
            _timeLine.SourceListToTimeLabels(_baseData.TimeLabelsData); 

            _timelinePanelScrollPos = _baseData.ScrollPos;
            _enableSave = true;
            initialized = true;

            if (EditorApplication.isPlaying)
            {
                bool cutsceneEngineFound = false;
                CutsceneEngine[] engines = FindObjectsOfType<CutsceneEngine>();
                foreach (CutsceneEngine engine in engines)
                {
                    if (engine.Cutscene.GetInstanceID() == _baseData.GetInstanceID())
                    {
                        _timeLine.AssignExternalTimeFlowEngine(engine.TimeEngine);
                        cutsceneEngineFound = true;
                        break;
                    }
                }
                if (!cutsceneEngineFound)
                    _timeLine.CreateNewEngine(new TimeSpan(_baseData.TimeTicks), new TimeSpan(_baseData.DurationTicks));
            }
            else
            {
                _timeLine.TimeLineEngine.SetTime(new TimeSpan(_baseData.TimeTicks));
                _timeLine.TimeLineEngine.Duration = new TimeSpan(_baseData.DurationTicks);
            }

        }
        /// <summary> Saves data to asset file. </summary>
        public void SaveData()
        {
            if (_baseData == null || !_enableSave)
                return;
            List<TimeLabelData> timeLabelsData = _timeLine.TimeLabelsToSourceList();
            List<SceneData> scenes = new List<SceneData>();
            foreach (SideScenePanel scenePanel in _scenePanels)
                scenes.Add(scenePanel.Scene);

            _baseData.ScrollPos = _timelinePanelScrollPos;
            _baseData.Scenes = scenes;
            _baseData.TimeLabelsData = timeLabelsData;
            _baseData.DurationTicks = _timeLine.TimeLineEngine.Duration.Ticks;
            _baseData.AudioClip = _soundLine.AudioClip;
            _baseData.AudioClipStartTimeTicks = _soundLine.AudioClipStartTime.Ticks;
            _baseData.Zoom = _timeLine.Zoom;
            _baseData.TimeTicks = _timeLine.TimeLineEngine.Time.Ticks;
            EditorUtility.SetDirty(_baseData);
        }

        private void HandleMainPanel(int windowID)
        {
            Rect rect = _mainPanelRect;
            Rect textureRext = new Rect(rect.x + 2, rect.y + _windowTitleHeight + 2, rect.width - 4, rect.height - _windowTitleHeight - 4);
            EditorGUI.DrawRect(textureRext, Color.gray);

            if (_selectedScene!=null && _selectedScene.Duration > TimeSpan.Zero && _selectedScene.ConceptArt != null)
                EditorGUI.DrawPreviewTexture(textureRext, _selectedScene.ConceptArt, null, ScaleMode.ScaleToFit);
        }
        Rect _lastTimeLineRect;
        private float _LastPointerXPos;
        private void HandleTimelinePanel(int windowID)
        {
            Vector2 controlButtonsSize = new Vector2(27, 27);
            Rect skipPreviousRect = new Rect(new Vector2(1, _windowTitleHeight), controlButtonsSize);
            Rect playButtonRect = new Rect(new Vector2(skipPreviousRect.x + skipPreviousRect.width, _windowTitleHeight), controlButtonsSize);
            Rect stopButtonRect = new Rect(new Vector2(playButtonRect.x + playButtonRect.width, _windowTitleHeight), controlButtonsSize);
            Rect skipNextRect = new Rect(new Vector2(stopButtonRect.x + stopButtonRect.width, _windowTitleHeight), controlButtonsSize);
            Rect timeLabelRect = new Rect(new Vector2(skipNextRect.x + skipNextRect.width + _spaceBetweenFields, _windowTitleHeight + 6), GUI.skin.label.CalcSize(_timeLine.GUITime));

            Rect timeTextFieldRect = new Rect(new Vector2(_timelinePanelRect.width - 250, _windowTitleHeight), new Vector2(250, 24));


            if (GUI.Button(skipPreviousRect, EditorResources.AVPrevious.GUIContent))
                _timeLine.TimeLineEngine.BackwardTime();
            if (!_timeLine.TimeLineEngine.IsStarted)
            {
                if (GUI.Button(playButtonRect, EditorResources.AVPlay.GUIContent))
                    _timeLine.TimeLineEngine.StartTimeFlowEngine();
            }
            else
            {
                if (GUI.Button(playButtonRect, EditorResources.AVPause.GUIContent))
                    _timeLine.TimeLineEngine.PauseTimeFlowEngine();
            }
            if (GUI.Button(stopButtonRect, EditorResources.AVStop.GUIContent))
                _timeLine.TimeLineEngine.StopTimeFlowEngine();
            if (GUI.Button(skipNextRect, EditorResources.AVNext.GUIContent))
                _timeLine.TimeLineEngine.ForwardTime();

            GUI.Label(timeLabelRect, _timeLine.GUITime);

            float lastLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 65;
            _timeLine.TimeLineEngine.Duration = _timeTextField.Draw(timeTextFieldRect, _timeLine.TimeLineEngine.Duration, new GUIContent("Duration: "), ArrowsLayout.Vertical);
            float audioFieldWidth = 350;
            float diffAudioClip = _timelinePanelRect.width - timeLabelRect.x - timeLabelRect.width - _spaceBetweenFields - timeTextFieldRect.width;
            if (diffAudioClip < audioFieldWidth)
                audioFieldWidth = diffAudioClip;
            AudioClip audio = _soundLine.AudioClip;
            _soundLine.AudioClip = EditorGUI.ObjectField(new Rect(timeLabelRect.x + timeLabelRect.width + _spaceBetweenFields, _windowTitleHeight + 6, audioFieldWidth, 18), new GUIContent("Audio clip: "), _soundLine.AudioClip, typeof(AudioClip), false) as AudioClip;

            EditorGUIUtility.labelWidth = lastLabelWidth;

            Rect timeLineRect = _timeLine.GUICalcRect(new Rect(_timelinePanelRect.x, _windowTitleHeight + _timelinePanelRect.height - 50 - _scrollHeight - _windowTitleHeight, _timelinePanelRect.width, 50));
            Rect scenesLineRect = _scenesLine.GUICalcRect(new Rect(_timelinePanelRect.x, timeLineRect.y - 30, _timelinePanelRect.width, 30));
            _soundLine.GUICalcRect(new Rect(_timelinePanelRect.x, scenesLineRect.y - 70, _timelinePanelRect.width, 70));
            if (_lastTimeLineRect.width == 0)
                _lastTimeLineRect.width = timeLineRect.width;

            float deltaScroll = 0;
            if (Event.current.type == EventType.ScrollWheel)
            {
                deltaScroll = Event.current.delta.y;

                float sign = Math.Sign(deltaScroll);
                float modifier = _mousePos.x / _timeLine.Rect.width;
                if (sign > 0)
                    modifier = 1 - _mousePos.x / _timeLine.Rect.width;
                float diff = -sign * modifier * Math.Abs(_lastTimeLineRect.width - timeLineRect.width);

                _timelinePanelScrollPos.x += diff;
                _lastTimeLineRect = timeLineRect;
            }


            Vector2 oldScroll = _timelinePanelScrollPos;
            _timelinePanelScrollPos = GUI.BeginScrollView(new Rect(_timelinePanelRect.x + 1, _windowTitleHeight, _timelinePanelRect.width - 1, _timelinePanelRect.height - _windowTitleHeight - 1), _timelinePanelScrollPos, new Rect(_timelinePanelRect.x, _timelinePanelRect.y, timeLineRect.width, _timelinePanelRect.height - _scrollHeight - _windowTitleHeight - 1));          
            GUI.EndScrollView();
            if (deltaScroll < 0)
                _timeLine.ZoomIn();
            else if (deltaScroll > 0)
                _timeLine.ZoomOut();
            _timeLine.GUIDraw(_timelinePanelScrollPos.x, deltaScroll);
            _soundLine.GUIDraw(_timelinePanelScrollPos.x, deltaScroll);
            _scenesLine.GUIDraw(_timelinePanelScrollPos.x, deltaScroll);

            float pointerXPos = _timeLine.GUIDrawPointer();
            float timeLineX = _timeLine.Rect.x;
            float timeLineWidth = _timeLine.Rect.width;
            bool wasChange = timeLineX + timeLineWidth >= _LastPointerXPos && timeLineX + timeLineWidth < pointerXPos;
            if (_timeLine.TimeLineEngine.IsStarted && _timelinePanelScrollPos == oldScroll && wasChange)
                _timelinePanelScrollPos.x += _timeLine.Rect.width;


            Repaint();
            _LastPointerXPos = pointerXPos;
            
        }
        private void HandleScenesPanel(int windowID)
        {
            float addButtonWidth = 70f;
            float scenePanelHeight = 20;

            Rect addButtonRect = new Rect(_scenesPanelRect.width- addButtonWidth, _scenesPanelRect.height - _fieldsHeight, addButtonWidth, _fieldsHeight);
            if (GUI.Button(addButtonRect, new GUIContent("Add")))
                HandleSceneAddButton();

            float heightNeeded = _windowTitleHeight;
            for (int i = 0; i < _scenePanels.Count; i++)
                heightNeeded += _scenePanels[i].Rect.height;

            bool scrollNeeded = heightNeeded > _scenesPanelRect.height - _fieldsHeight + 3;
            Rect scenePanelRect = new Rect(-1, _windowTitleHeight, _scenesPanelRect.width - (scrollNeeded ? _scrollWidth : 0), scenePanelHeight);

            if (scrollNeeded)
                _scenePanelScrollPos = GUI.BeginScrollView(new Rect(0, _windowTitleHeight, _scenesPanelRect.width, _scenesPanelRect.height - _fieldsHeight - _windowTitleHeight), _scenePanelScrollPos, new Rect(0, _windowTitleHeight, _scenesPanelRect.width - (scrollNeeded ? _scrollWidth : 0), heightNeeded - _fieldsHeight + 3));

            for (int i = 0; i < _scenePanels.Count; i++)
            {
                float height = _scenePanels[i].Draw(scenePanelRect).height;
                scenePanelRect = new Rect(scenePanelRect.x, scenePanelRect.y + height, _scenesPanelRect.width - (scrollNeeded ? _scrollWidth : 0), scenePanelRect.height);
            }
            if (scrollNeeded)
                GUI.EndScrollView();
        }


        private void DeexpandAllScenePanelsWithout(SceneData scene)
        {
            foreach (SideScenePanel panel in _scenePanels)
            {
                if (!panel.Scene.Equals(scene))
                    panel.Expand(false);
            }
        }
        private void DeselectAllScenePanelsWithout(long ID)
        {
            foreach (SideScenePanel panel in _scenePanels)
            {
                if (panel.ID != ID)
                    panel.Select(false);
            }
        }
        public void HandleSceneAddButton(SceneData sceneData = null)
        {
            SideScenePanel panel = new SideScenePanel();
            panel.Click += OnScenePanelClick;
            panel.Delete += OnScenePanelDelete;

            TimeSpan coursorTime = _timeLine.TimeLineEngine.Time;
            TimeSpan duration = new TimeSpan(0, 0, 10);
            if (sceneData != null)
                duration = sceneData.Duration;
            foreach (SideScenePanel scenePanel in _scenePanels)
            {
                if (coursorTime >= scenePanel.Scene.Start && coursorTime < scenePanel.Scene.End)
                {
                    EditorUtility.DisplayDialog("Error!", "You cannot add scene on the occupied time.", "Ok"); 
                    return;
                }
                if (coursorTime < scenePanel.Scene.Start && coursorTime + duration > scenePanel.Scene.Start)
                {
                    TimeSpan gap = scenePanel.Scene.Start - coursorTime;
                    if (gap < duration)
                        duration = gap;
                }
            }
            if (coursorTime + duration > _timeLine.TimeLineEngine.Duration)
                duration = _timeLine.TimeLineEngine.Duration - coursorTime;
            if (sceneData == null)
                panel.Scene.Color = ExtensionMethods.ChangeColorBrightness(Color.gray, 0.7f);
            else
                panel.Scene.AssignWithoutTime(sceneData);

            panel.Scene.Start = coursorTime;
            panel.Scene.End = coursorTime + duration;
            _scenePanels.Add(panel);
            _scenesLine.AddLabel(panel.Scene);
        }

        private void OnScenePanelClick(SceneData scene, bool selected)
        {
            if (selected)
            {
                DeexpandAllScenePanelsWithout(scene);
                foreach (SideScenePanel panel in _scenePanels)
                {
                    if (panel.Scene.Equals(scene))
                        panel.Expand(true);
                }
                _scenesLine.SelectLabel(scene);
            }
            else
                _scenesLine.DeselectLabel(scene);
        }
        private void OnSceneLabelSelected(SceneData scene)
        {
            DeexpandAllScenePanelsWithout(scene);
            foreach (SideScenePanel panel in _scenePanels)
            {
                if (panel.Scene.Equals(scene))
                    panel.Expand(true);
            }
        }
        private void OnLabelTimeEnter(SceneData scene)
        {
            _selectedScene = scene;
            OnScenePanelClick(scene, true);
        }
        private void OnLabelTimeExit(SceneData scene)
        {
            if (_selectedScene!=null && _selectedScene.Equals(scene))
                _selectedScene = null;
        }
        private void OnScenePanelDelete(SceneData scene)
        {
            _scenePanels.RemoveAll(sp => sp.Scene.Equals(scene) && scene.RemoveRequest);
        }
    }
}