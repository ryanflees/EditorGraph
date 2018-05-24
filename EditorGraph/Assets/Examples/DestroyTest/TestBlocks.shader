// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "CR/Block"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

        [KeywordEnum(Both, X, Y, Blend)] _Channel ("Show Channel", Float) = 0
        _XScale ("X Scale", Range(-1, 1)) = 1
        _YScale ("Y Scale", Range(-1, 1)) = 1
        _Seed ("Seed", Range(0, 1)) = 0.1

        _WaveTotalFactor ("Total Factor", Float) = 43
        _WaveYFactor ("Y Factor", Range(-10, 10)) = 2
        _Distortion ("Distortion", Range(0,1)) = 0

        _XClip ("X Clip", Range(0, 50)) = 0
        _XOffset ("X Offset", Float) = 0

        _Strength ("Strength", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP
            #pragma multi_compile _CHANNEL_BOTH _CHANNEL_X _CHANNEL_Y _CHANNEL_BLEND

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _XScale;
            float _YScale;
            float _Seed;
            float _WaveTotalFactor;
            float _WaveYFactor;
            float _Distortion;
            float _XClip;
            float _XOffset;
            float _Strength;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = v.texcoord;

                OUT.color = v.color * _Color;
                return OUT;
            }

            sampler2D _MainTex;

            float r (float2 c)
            {
                float s = _WaveTotalFactor * sin(c.x + _WaveYFactor * c.y) * _Seed;
                s = frac(s);
                float xClip = _XClip - 0.0001;
                float lerpRate = smoothstep(xClip, _XClip, c.x);
                s = lerp(0, s, lerpRate);
                return s;
                //return frac(_WaveTotalFactor * sin(c.x + _WaveYFactor * c.y) * _Seed);
            }

            float2 n (float2 p)
            {
                float2 i = floor(p); //
                float2 w = p - i; //0~1
                float2 j = float2 (1.,0.);
                w = w * w * (3.-w-w);
                //return r(i + j.yx);
                //return r(i + float2(1, 1));
                //return lerp(r(i), r(i+j), w.x);
                return lerp(lerp(r(i), r(i+j), w.x), lerp(r(i+j.yx), r(i+1.), w.x), w.y);
                //return lerp(r(i+j.yx), r(i+1.), w.x);
                //w = (3.-w-w);
                //w = smoothstep(1, 3, w);
                //return lerp(lerp(r(i), r(i+j), w.x), lerp(r(i+j.yx), r(i+1.), w.x), w.y);
                //return i;
            }

            float a (float2 p)
            {
                float m = 0., f = 2.;

                //m = n(2 * p) /;
                for (int i = 0 ; i < 9 ; i++)
                { 
                     m += n(f*p)/f; 
                     f+=f; 
                }
                return m;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                //half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                // #ifdef UNITY_UI_CLIP_RECT
                // color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                // #endif

                // #ifdef UNITY_UI_ALPHACLIP
                // clip (color.a - 0.001);
                // #endif
                float2 uv = IN.texcoord.xy;
                //return color;
                half4 color = half4(1, 0, 0, 1);
                uv *= 3.5;
                //color.r = n(uv).y;
                float2 data = //a(uv);
                            n(uv);
                            //r(uv);

                // float t = frac(_Distortion * 0.9999);
                // data = smoothstep(t/1.2, t+.1, data.x);
                

                //data = r(data);
                //data = r(data + float2(1, 0));
                //data = r(data + float2(1, 1));
                color.rgb = 0;
                #if _CHANNEL_BOTH
                    color.rg = float2(data.x * _XScale, data.y * _YScale);
                    // /color.rgb = 1;
                #elif _CHANNEL_X
                    color.r = data.x * _XScale;
                #elif _CHANNEL_Y
                    color.g = data.y * _YScale;
                #elif _CHANNEL_BLEND
                    half4 albedo = tex2D(_MainTex, IN.texcoord);                    
                    color.rgb = albedo.rgb + data.x * _Strength;
                #endif
                return color;
            }
            ENDCG
        }
    }
}
