namespace AceSea
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class MathExpression : MonoBehaviour
    {
        public enum AxisAlignType
        {
            LeftOrTop,
            Middle,
            RightOrBottom,
            Custom
        }

        public AxisAlignType HorAlignType = AxisAlignType.Middle;
        public AxisAlignType VerAlignType = AxisAlignType.Middle;

        public float YScale = 1;
        public float XScale = 1;

        public float XAxisOffset = 0;
        public float YAxisOffset = 0;

        #region Processing
        public virtual float CalculateFunction(float x)
        {
            return 0;
        }

        public virtual float CalculateDelta(float deltaX, float X)
        {
            return 0;
        }

        public virtual void ResetValue()
        {

        }
        #endregion

        #region Utils
        public Vector4 Frac(Vector4 v)
        {
            return new Vector4(v.x % 1.0f, v.y % 1.0f, v.z % 1.0f, v.w % 1.0f);
        }

        public Vector3 Frac(Vector3 v)
        {
            return new Vector3(v.x % 1.0f, v.y % 1.0f, v.z % 1.0f);
        }

        public Vector2 Frac(Vector2 v)
        {
            return new Vector2(v.x % 1.0f, v.y % 1.0f);
        }

        public float Frac(float v)
        {
            return v % 1.0f;
        }

        public Vector4 Abs(Vector4 v)
        {
            return new Vector4(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z), Mathf.Abs(v.w));
        }

        public Vector3 Abs(Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        public Vector2 Abs(Vector2 v)
        {
            return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
        }
        
        public float Abs(float v)
        {
            return Mathf.Abs(v);
        }

        public Vector4 Add(Vector4 v, float y)
        {
            return v + Vector4.one * y;
        }
        
        public Vector3 Add(Vector3 v, float y)
        {
            return v + Vector3.one * y;
        }

        public Vector2 Add(Vector2 v, float y)
        {
            return v + Vector2.one * y;
        }
        
        public float Saturate(float v)
        {
            return Mathf.Clamp(v, 0f, 1f);
        }
        #endregion
    }
}
