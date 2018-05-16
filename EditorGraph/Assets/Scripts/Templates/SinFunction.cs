namespace AceSea
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class SinFunction : MathExpression
    {
        [Header("y = a * sin(x*b) + c")]
        public float A = 1;
        public float B = 1;
        public float C = 0;

        public override float CalculateFunction(float x)
        {
            return A * Mathf.Sin(x * B) + C;
        }
    }
}
