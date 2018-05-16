namespace AceSea
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class BinaryFunction : MathExpression
    {
        [Header("y = ax*x + bx + c")]
        public float A = 1;
        public float B = 2;
        public float C = 1;

        public override float CalculateFunction(float x)
        {
            return x * x * A + B * x + C;
        }
    }
}
