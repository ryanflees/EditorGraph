namespace AceSea
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class LinearExpression : MathExpression
    {
        public float ConstValue = 1;

        public override float CalculateFunction(float x)
        {
            return ConstValue;
        }
    }
}
