namespace AceSea
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class ModExpression : MathExpression
    {
        public float Modu = 1.0f;

        public override float CalculateFunction(float x)
        {
            x = Frac((x + 5) / 5);
            return x;
            //x = (x + 5) / 5;
            //float floored = x - Mathf.Floor(x * (1 / Modu)) * Modu;
            //return floored;
        }
    }
}
