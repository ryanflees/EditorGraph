namespace AceSea
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class BlackholeExpression : MathExpression
    {
        public const float Radius = 0.5f;

        //[Range(0, 0.7f)]
        //public float Dis = 0f;
        public float Distortion = 0f;

        public override float CalculateFunction(float x)
        {
            //float percent = (Radius - Dis) / Radius;
            //return percent;
            float dis = Mathf.SmoothStep(0, 0.7f, x);
            float percent = (Radius - dis) / Radius;

            float theta = percent * percent *(2.0f * Mathf.Sin(Distortion)) * 8.0f;
            return theta;
        }
    }
}
