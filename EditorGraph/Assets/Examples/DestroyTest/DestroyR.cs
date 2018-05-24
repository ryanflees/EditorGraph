namespace AceSea
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class DestroyR : MathExpression
    {        
        [Range(0, 1)]
        public float Y = 0;

        //return frac(43.*sin(c.x+7.*c.y)*_Size);
        public override float CalculateFunction(float x)
        {
            //return Frac(43 * Mathf.Sin(x + 0.7f * Y));
            return (43 * Mathf.Sin(x + 0.7f * Y));
        }
    }
}
