namespace AceSea
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    //float n(float2 p)
    //{
    //    float2 i = floor(p), w = p - i, j = float2(1., 0.);
    //    w = w * w * (3.- w - w);
    //    return lerp(lerp(r(i), r(i + j), w.x), lerp(r(i + j.yx), r(i + 1.), w.x), w.y);
    //}

    public class DestroyF : MathExpression
    {        
        [Range(0, 1)]
        public float Y = 0;

        //return frac(43.*sin(c.x+7.*c.y)*_Size);
        public override float CalculateFunction(float x)
        {
            return 0;
        }
    }
}
