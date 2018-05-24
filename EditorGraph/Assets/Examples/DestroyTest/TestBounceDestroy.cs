namespace AceSea
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class TestBounceDestroy : MonoBehaviour
    {

        public float SeedSpeed = 0.1f;

        public float StartDistortion = 0f;
        public float EndDistortion = 1f;
        public float DistortionSpeed = 0.5f;
        public float StartClip = 0f;
        public float EndClip = 1;
        public float ClipSpeed = 0.5f;

        private const string PROP_XCLIP = "_XClip";
        private const string PROP_SEED = "_Seed";
        private const string PROP_DISTORTION = "_Distortion";

        private float mClip = 0;
        private float mDistortion = 0f;

        private bool mForward = true;

        private Image mImage;

        //_Seed("Seed", Range(0, 1)) = 0.1

        //_WaveTotalFactor("Total Factor", Float) = 43
        //_WaveYFactor("Y Factor", Range(-10, 10)) = 2
        //_Distortion("Distortion", Range(0,1)) = 0

        //_XClip("X Clip", Range(0, 50)) = 0
        //_XOffset("X Offset", Float) = 0

        //_Strength("Strength", Range(0, 1)) = 1

        private void Awake()
        {
            mImage = transform.GetComponent<Image>();
            if (mImage)
            {
                mClip = StartClip;
                mDistortion = StartDistortion;
                mImage.material = new Material(mImage.material);
                mImage.material.SetFloat(PROP_XCLIP, mClip);
                mImage.material.SetFloat(PROP_DISTORTION, mDistortion);
            }
        }

        private void Start()
        {
            Forward();
        }

        private void Revert()
        {
            mForward = false;
            Invoke("Forward", 3);
        }

        private void Forward()
        {
            mForward = true;
            Invoke("Revert", 3);
        }

        private void Update()
        {
            if (mImage)
            {
                if (mForward)
                {
                    mClip += Time.deltaTime * ClipSpeed;
                    mClip = Mathf.Min(EndClip, mClip);
                    mImage.material.SetFloat(PROP_XCLIP, mClip);


                    mDistortion += Time.deltaTime * DistortionSpeed;
                    mDistortion = Mathf.Min(EndDistortion, mDistortion);
                 
                    mImage.material.SetFloat(PROP_XCLIP, mClip);
                    mImage.material.SetFloat(PROP_DISTORTION, mDistortion);
                }
                else
                {
                    mClip -= Time.deltaTime * ClipSpeed;
                    mClip = Mathf.Max(StartClip, mClip);
               
                    mDistortion -= Time.deltaTime * DistortionSpeed;
                    mDistortion = Mathf.Max(StartDistortion, mDistortion);

                    mImage.material.SetFloat(PROP_XCLIP, mClip);
                    mImage.material.SetFloat(PROP_DISTORTION, mDistortion);
                }
            }
        }
    }
}
