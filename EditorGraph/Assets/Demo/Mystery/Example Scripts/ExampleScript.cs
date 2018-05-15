using UnityEngine;
using System.Collections;

public class ExampleScript : MonoBehaviour
{
    public bool BooleanValue;
    public float FloatValue;
    public int IntValue;
    public Vector2 Vector2Value;
    public Vector3 Vector3Value;
    public Color ColorValue;
    public Color32 Color32Value;
    public string StringValue;
    public ExampleEnum EnumValue;

    public enum ExampleEnum
    {
        Walk,
        Run,
        Sprint
    }

    public enum ExampleEnum2
    {
        Crouch,
        Stand,
        Jump
    }

    void Update()
	{
        BooleanValue = Mathf.Repeat(Time.time, 1.0f) > 0.5f;

        FloatValue = Mathf.PingPong(Time.realtimeSinceStartup, 1.0f);

        IntValue = Mathf.FloorToInt(FloatValue * 10.0f);

        Vector2Value = GetComponent<Rigidbody2D>().velocity;

        Vector3Value = transform.position;

        ColorValue = Color.Lerp(DebugGraph.DefaultBlue, new Color(1.0f, 0.75f, 0.25f), Mathf.PingPong(Time.realtimeSinceStartup, 1.0f));

        Color32Value = ColorValue;

        StringValue = "Hello World! The Current Frame Number Is: " + Time.frameCount;

        EnumValue = (ExampleEnum)(Time.frameCount % 3);

        float sin = Mathf.Sin(Mathf.Repeat(Time.time, 6.28f));
        float cos = Mathf.Cos(Mathf.Repeat(Time.time, 6.28f));

        DebugGraph.Log("Color Gradient", ColorValue);

        DebugGraph.Write("String", StringValue);

        DebugGraph.Log("Vector3", Input.mousePosition);

        DebugGraph.Log("Vector4", new Vector4(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)));

        transform.Rotate(Vector3.up, Time.deltaTime * 90.0f);
        DebugGraph.Log("Quaternion", transform.rotation);

        DebugGraph.Log("Rect", new Rect(0, 0, 100, 100));

        DebugGraph.Draw(new Vector2(sin * Time.time, cos * Time.time));

        DebugGraph.MultiLog("Related Variables", DebugGraph.DefaultRed, sin, "Sin");
        DebugGraph.MultiLog("Related Variables", DebugGraph.DefaultGreen, cos, "Cos");

        DebugGraph.Log(FloatValue); //Anonymous Float

        //Anonymous Multi Float
        DebugGraph.MultiLog(cos * 1.1f); 
        DebugGraph.MultiLog(cos * 1.2f);
        DebugGraph.MultiLog(cos * 1.3f, "C"); //with Value names
        DebugGraph.MultiLog(cos * 1.4f, "D");
        DebugGraph.MultiLog(cos * 1.5f, "E");

        //Anonymous Values in a Loop
        for (int i = 0; i < 10; i++)
            DebugGraph.MultiLog(DebugGraph.GetUniqueColor(i), sin * (1.0f + i * 0.1f), i.ToString());

        DebugGraph.Log(Mathf.FloorToInt(sin * 10)); //Anonymous Integer

        DebugGraph.Log(Mathf.RoundToInt(Mathf.PerlinNoise(Time.time, Time.time) * 1) > 0); //Anonymous Boolean

        //Anonymous Multi Enum
        DebugGraph.MultiLog(EnumValue);
        DebugGraph.MultiLog((ExampleEnum2)(Mathf.PerlinNoise(Time.time, Time.time) * 3));
    }
}
