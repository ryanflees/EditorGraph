using CutscenePlanner;
using UnityEngine;

public class SceneManager : MonoBehaviour {

    /// <summary> Cutsccene project file. </summary>
    public CutsceneEngine cutsceneEngine;
    [Space]
    public int targetFPS = -1;
    [Range(0,2)]
    public float timeScale = 1;
    [Space]
    public SpotlightController spotlightController;
    public TitleController titleController;
    public CameraController cameraController;
    public CubeController cubeController;

    public CubeController[] sideCubes;
    [Space]
    public bool play;

    private int _vSync;
    private float _additionalTime;
    private bool _disableAnimator;

    private void OnEnable()
    {
        // attaching to CutsceneEngine events
        if (cutsceneEngine!=null)
        {
            cutsceneEngine.Started += CutsceneEngine_Started;
            cutsceneEngine.Ended += CutsceneEngine_Ended;
            cutsceneEngine.Paused += CutsceneEngine_Paused;

            cutsceneEngine.TimeAudioClipEnter += CutsceneEngine_TimeAudioClipEnter;
            cutsceneEngine.TimeAudioClipExit += CutsceneEngine_TimeAudioClipExit;
            cutsceneEngine.TimeLabelDataEnter += CutsceneEngine_TimeLabelDataEnter;
            cutsceneEngine.TimeLabelDataExit += CutsceneEngine_TimeLabelDataExit;
            cutsceneEngine.TimeSceneEnter += CutsceneEngine_TimeSceneEnter;
            cutsceneEngine.TimeSceneExit += CutsceneEngine_TimeSceneExit;
        }
    }

    private void OnDisable()
    {
        if (cutsceneEngine != null)
        {
            cutsceneEngine.Started -= CutsceneEngine_Started;
            cutsceneEngine.Ended -= CutsceneEngine_Ended;
            cutsceneEngine.Paused -= CutsceneEngine_Paused;

            cutsceneEngine.TimeAudioClipEnter -= CutsceneEngine_TimeAudioClipEnter;
            cutsceneEngine.TimeAudioClipExit -= CutsceneEngine_TimeAudioClipExit;
            cutsceneEngine.TimeLabelDataEnter -= CutsceneEngine_TimeLabelDataEnter;
            cutsceneEngine.TimeLabelDataExit -= CutsceneEngine_TimeLabelDataExit;
            cutsceneEngine.TimeSceneEnter -= CutsceneEngine_TimeSceneEnter;
            cutsceneEngine.TimeSceneExit -= CutsceneEngine_TimeSceneExit;
        }
    }
    // Use this for initialization
    private void Start()
    {
        _vSync = QualitySettings.vSyncCount;
        if (play)
            _additionalTime = 1.5f;
    }

    // Update is called once per frame
    void Update()
    {
        if (_disableAnimator)
        {
            cameraController.animator.enabled = false;
            _disableAnimator = false;
        }

        
        if (play)
        {
            cutsceneEngine.PlayCutscene(_additionalTime);
            play = false;
        }
        if (targetFPS < -1)
            targetFPS = -1;

        if (targetFPS == -1)
            QualitySettings.vSyncCount = _vSync;
        else
            QualitySettings.vSyncCount = 0;

        Application.targetFrameRate = targetFPS;
        Time.timeScale = timeScale;
    }
    private void CutsceneEngine_TimeSceneEnter(object sender, SceneData scene)
    {
        CutsceneEngine ce = sender as CutsceneEngine;
        float currentTime = ce.Time;
        Debug.Log(ce.Time + ": Scene data entered - " + scene.Name);

        if (scene.Name == "Light Spot")
        {
            titleController.Show(4, "Dragon's Diamond presets...");
            spotlightController.lightComponent.enabled = true;
        }
        if (scene.Name == "Falling cube")
        {
            titleController.Show(0, " Dancing cube");
            cameraController.animator.enabled = false;
            cameraController.SmoothLookAt(cubeController.transform, 720);
        }
        if (scene.Name == "Jumping cube")
        {
            cubeController.enableForce = true;
            spotlightController.SmoothLookAt(cubeController.transform, 720);
        }
        if (scene.Name == "Jumping cube 2")
        {
            titleController.Hide(5);

            cameraController.animator.enabled = true;
            cameraController.animator.SetTrigger(scene.Name);
            _disableAnimator = true;      
        }
        if (scene.Name == "Cube cross")
        {
            cubeController.enableForce = false;
            cubeController.animator.enabled = true;
            cameraController.animator.enabled = true;
            cameraController.animator.SetTrigger(scene.Name);
            _disableAnimator = true;                 
        }
        if (scene.Name == "Falling lot of cubes")
        {           
            cameraController.animator.enabled = true;
            cameraController.animator.SetTrigger(scene.Name);
            _disableAnimator = true;
            foreach (CubeController cube in sideCubes)
            {
                cube.gameObject.SetActive(true);
                cube.animator.SetTrigger(scene.Name);
            }
        }
        if (scene.Name == "Lot of cubes")
        {
            cubeController.enableForce = true;
            foreach (CubeController cube in sideCubes)
                cube.enableForce = true;
        }
        if (scene.Name == "The End")
        {
            titleController.Show(0.5f, "The End");
            spotlightController.lightComponent.enabled = false;
        }
        if (HasParameter(scene.Name, cameraController.animator))
            cameraController.animator.SetTrigger(scene.Name);

        if (HasParameter(scene.Name, cubeController.animator))
            cubeController.animator.SetTrigger(scene.Name);
    }

    private void CutsceneEngine_TimeSceneExit(object sender, SceneData scene)
    {
        CutsceneEngine ce = sender as CutsceneEngine;
        float currentTime = ce.Time;
        Debug.Log(ce.Time + ": Scene data exit - " + scene.Name);

        if (scene.Name == "Lot of cubes")
        {
            cubeController.enableForce = false;
            foreach (CubeController cube in sideCubes)
                cube.enableForce = false;
        }
    }



    private void CutsceneEngine_TimeLabelDataExit(object sender, TimeLabelData label)
    {
        CutsceneEngine ce = sender as CutsceneEngine;
        float currentTime = ce.Time;
        Debug.Log(ce.Time + ": Time label data exit - " + label.Name);
    }

    private void CutsceneEngine_TimeLabelDataEnter(object sender, TimeLabelData label)
    {
        CutsceneEngine ce = sender as CutsceneEngine;
        float currentTime = ce.Time;
        Debug.Log(ce.Time + ": Time label data entered - " + label.Name);
    }

    private void CutsceneEngine_TimeAudioClipExit(object sender, AudioClip audioClip)
    {
        CutsceneEngine ce = sender as CutsceneEngine;
        float currentTime = ce.Time;
        Debug.Log(ce.Time + ": Audio clip exit - " + audioClip.name);
    }

    private void CutsceneEngine_TimeAudioClipEnter(object sender, AudioClip audioClip)
    {
        CutsceneEngine ce = sender as CutsceneEngine;
        float currentTime = ce.Time;
        Debug.Log(ce.Time + ": Audio clip entered - " + audioClip.name);
    }
    private void CutsceneEngine_Started(object sender)
    {
        CutsceneEngine ce = sender as CutsceneEngine;
        float currentTime = ce.Time;

        Debug.Log(ce.Time + ": Cutscene started");
    }
    private void CutsceneEngine_Paused(object sender)
    {
        CutsceneEngine ce = sender as CutsceneEngine;
        float currentTime = ce.Time;
        Debug.Log(ce.Time + ": Cutscene paused");
    }
    private void CutsceneEngine_Ended(object sender)
    {
        CutsceneEngine ce = sender as CutsceneEngine;
        float currentTime = ce.Time;
        Debug.Log(ce.Time + ": Cutscene ended");

        titleController.Hide(5f);
    }

    private static bool HasParameter(string paramName, Animator animator)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
}
