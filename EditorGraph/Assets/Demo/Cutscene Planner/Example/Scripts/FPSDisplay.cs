using UnityEngine;
using System.Collections;

public class FPSDisplay : MonoBehaviour
{
    [Range(0,5)]
    public float sizeScale = 1;
    float deltaTime = 0.0f;

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;

        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w * sizeScale, (h * 2 / 100) * sizeScale);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = (int)((h * 2 / 100) * sizeScale);
        if (fps >= 30)
            style.normal.textColor = Color.green;
        else if (fps<30 && fps >=10)
            style.normal.textColor = Color.yellow;
        else if (fps <10)
            style.normal.textColor = Color.red;

        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text, style);
    }
}