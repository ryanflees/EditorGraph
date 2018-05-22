namespace AceSea
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PlasmaExpression : MathExpression
    {
        public float Offset = 4f;

        public override float CalculateFunction(float x)
        {
            //return base.CalculateFunction(x);

            float a = 1.1f + x * 2.25f + Offset;
            return a;
        }
    }
}
