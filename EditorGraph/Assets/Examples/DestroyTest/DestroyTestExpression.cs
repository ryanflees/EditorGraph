namespace AceSea
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class DestroyTestExpression : MathExpression
    {
        public override float CalculateFunction(float x)
        {
            //return base.CalculateFunction(x);
            float t = Frac(x * 0.9999f);
            //return t / 1.2f;
            return t + 0.1f;
        }
    }
}
