namespace AceSea
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class BurnTest : MathExpression
    {
        [Range(0, 100)]
        public float BurnTolerance = 15;

        public override float CalculateFunction(float x)
        {
            return Saturate(x * BurnTolerance);
        }
    }
}
