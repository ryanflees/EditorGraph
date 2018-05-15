Cutscene planner is simple and easy to use editor extension, that allows to project and implement real time cutscenes. With that extension any developer can create very own event scene with no time.

Cutscene is based on event system. Extension have implemented real time player system (like a Unity Animation module). If current cutscene time hit one of event generator's: Time label, Scene label or Audio clip label, cutscene engine fires event, that can be used to launch animations, scripts, components, etc. 

Basic concept is:
1. Create cutscene project file
- Right mouse click in file browser
- Select Create > Cutscene
2. Use editor to create cutscene project
3. In Unity scene create Game Object and add Cutscene Engine. Select created file in Cutscene field.
4. Make your own script that require CutsceneEngine component and in it, attach method event handlers to CutsceneEngine event's. Those are:
CutsceneEngine.Started - Event fired when cutscene started,
CutsceneEngine.Ended   - Event fired when cutscene ended
CtsceneEngine.Paused   - Event fired when cutscene paused
CutsceneEngine.TimeAudioClipEnter - Event fired when cutscene current time enter audio clip
CutsceneEngine.TimeAudioClipExit  - Event fired when cutscene current time exit audio clip
CutsceneEngine.TimeLabelDataEnter - Event fired when cutscene current time enter time label
CutsceneEngine.TimeLabelDataExit  - Event fired when cutscene current time exit time label
CutsceneEngine.TimeSceneEnter     - Event fired when cutscene current time enter scene
CutsceneEngine.TimeSceneExit      - Event fired when cutscene current time exit scene

Mentioned Scene, Time Label and Audio clip are basic elements, from which you can build your own cutscene. 
- Scenes are used to represent cutscene scene or single clips
- Time labels are used to represent time range on the timeline.
- Audio clip is used as soundtrack in cutscene.

See Example scene to learn how you can implement project to game. 

Cutscene planner editor is split in three modules: 
- Timeline panel
- Scenes panel
- Concept art window

Heart of the cutscene planner is timeline, placed on bottom, on which you can place you scenes and time labels. Every label alike audio clip can be drag on the time axis to fit desired time and length. Time labels are added to timeline panel. Scenes are added in middle timeline. Audio clip is drawn on upper timeline.

To the right is Scenes panel. There scenes can be inspected. Every scene have it's own Name, Description And Concept Art.

When actual cutscene time enter specified scene label, its concept are displayed in Concept art module.

In case of any questions or for your feedback, write to:
contact@dragons-diamond.com