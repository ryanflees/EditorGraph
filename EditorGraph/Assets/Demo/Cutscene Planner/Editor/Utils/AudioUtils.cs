using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CutscenePlanner.Editor.Utils
{
    public static class AudioUtils
    {
        [SerializeField]
        private static AudioClip _clip;

        public static void PlayClip(AudioClip clip, float startTime = 0)
        {
            if (clip == null)
                return;


            if (IsClipPlaying())
                StopClip();
            _clip = clip;


            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "PlayClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new System.Type[] { typeof(AudioClip) },
                null
            );
            method.Invoke( null, new object[] { _clip } );
            if (startTime != 0)
                SetClipTimePosition(startTime);
        }
        public static void StopClip()
        {
            if (_clip == null)
                return;
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "StopClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new System.Type[] { typeof(AudioClip) },
                null
            );
            method.Invoke(null, new object[] { _clip });
        }
        public static void StopAllClips()
        {

            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "StopAllClips",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new System.Type[] { },
                null
            );
            method.Invoke(null, new object[] {  });
        }
        public static void SetClipTimePosition(float startTime)
        {
            int startSample = (int)((_clip.samples / _clip.length) * startTime);

            if (_clip == null)
                return;
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "SetClipSamplePosition",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new System.Type[] { typeof(AudioClip) , typeof (int) },
                null
            );
            method.Invoke(null, new object[] { _clip, startSample });
        }
        public static float GetClipTimePosition()
        {
            if (_clip == null)
                return -1;
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "GetClipSamplePosition",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new System.Type[] { typeof(AudioClip) },
                null
            );
            int actualSample = (int)method.Invoke(null, new object[] { _clip});
            return (_clip.length/_clip.samples) * actualSample;

        }
        public static bool IsClipPlaying()
        {
            if (_clip == null)
                return false;
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "IsClipPlaying",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new System.Type[] { typeof(AudioClip) },
                null
            );
            return (bool)method.Invoke(null, new object[] { _clip });
        }

        public static void DoRenderPreview(AudioClip clip, Texture2D texture, TimeFlowEngine timeLineEngine, float offsetX, double startTime)
        {

            int height = texture.height;
            int width = texture.width;


            AudioImporter audioImporter = GetImporterFromClip(clip);


            float[] minMaxData = (!(audioImporter == null)) ? GetMinMaxData(audioImporter) : null;
            int numChannels = clip.channels;
            int numSamples = (minMaxData != null) ? (minMaxData.Length / (2 * numChannels)) : 0;
            int channel;
            Color curveColor = new Color(1f, 0.549019635f, 0f, 1f);
            for (channel = 0; channel < numChannels; channel++)
            {
                for (int p = 0; p < width; p++)
                {
                    float x = (p+ offsetX - timeLineEngine.TimeToX(startTime)) / timeLineEngine.TimeToX(clip.length);
                    if (x>1)
                    {
                        texture.Apply();
                        break;
                    }
                    if (x<0)
                        continue;

                    float minValue = 0;
                    float maxValue = 0;
                    Color col = curveColor;
                    // converting x time to pos

                    if (numSamples <= 0)
                    {
                        minValue = 0f;
                        maxValue = 0f;
                    }
                    else
                    {
                        float f = Mathf.Clamp(x * (numSamples - 2), 0f, (numSamples - 2));
                        int num2 = (int)Mathf.Floor(f);
                        int num3 = (num2 * numChannels + channel) * 2;
                        int num4 = num3 + numChannels * 2;
                        minValue = Mathf.Min(minMaxData[num3 + 1], minMaxData[num4 + 1]) * 0.9f;
                        maxValue = Mathf.Max(minMaxData[num3], minMaxData[num4]) * 0.9f;
                        if (minValue > maxValue)
                        {
                            float num5 = minValue;
                            minValue = maxValue;
                            maxValue = num5;
                        }
                    }
                    

                    float minTextureValue = (minValue + 1) / 2;
                    float maxTextureValue = (maxValue + 1) / 2;

                    minTextureValue /= numChannels;
                    maxTextureValue /= numChannels;

                    minTextureValue += (1.0f / numChannels) * channel + 0.025f;
                    maxTextureValue += (1.0f / numChannels) * channel + 0.025f;
                    // until now normalized 

                    minTextureValue *= height;
                    maxTextureValue *= height;
                    float lineHeight = maxTextureValue - minTextureValue;
                    if (lineHeight < 1)
                        lineHeight = 1f;

                    float pixelScale = EditorGUIUtility.pixelsPerPoint;

                    int thicknes = 1;
                    Color[] _lineColors = new Color[(int)(thicknes * lineHeight)];
                    _lineColors.Fill(col);
                    for (int i = 0; i<7 && i<_lineColors.Length; i++)
                        _lineColors[i] = ExtensionMethods.ChangeColorBrightness(col, (i+10) / (float)(17));

                    for (int i = 0; i<7 && _lineColors.Length - i > 0; i++)
                        _lineColors[_lineColors.Length-1 - i] = ExtensionMethods.ChangeColorBrightness(col, (i + 10) / (float)(17));


                    texture.SetPixels(p, (int)minTextureValue, thicknes, (int)lineHeight, _lineColors);

                    if (channel == numChannels - 1 && p == width-1)
                        texture.Apply();
                }
            }
        }
        private static AudioImporter GetImporterFromClip(AudioClip clip)
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "GetImporterFromClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new System.Type[] { typeof(AudioClip) },
                null
            );
            return method.Invoke(null, new object[] { clip }) as AudioImporter;
        }
        private static float[] GetMinMaxData(AudioImporter importer)
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            MethodInfo method = audioUtilClass.GetMethod(
                "GetMinMaxData",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new System.Type[] { typeof(AudioImporter) },
                null
            );
            return method.Invoke(null, new object[] { importer })as float[];
        }
        
    }
}