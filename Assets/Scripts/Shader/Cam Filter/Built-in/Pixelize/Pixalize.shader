Shader "Hidden/Pixelize"
{
    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/Colors.hlsl"
        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/Sampling.hlsl"

    
        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float4 _MainTex_TexelSize;
        int _PixelSample;

        struct v2f
        {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
            float3 screen_pos : TEXCOORD2;
        };

        inline float4 ComputeScreenPos(float4 pos)
        {
            float4 o = pos * 0.5f;
            o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w;
            o.zw = pos.zw;
            return o;
        }

        v2f Vert (AttributesDefault v)
            {
                v2f o;
                o.vertex = float4(v.vertex.x, -v.vertex.y, 0.0, 1.0);
                o.uv = TransformTriangleVertexToUV(v.vertex.xy);
                o.screen_pos = ComputeScreenPos(o.vertex);
                return o;
            }

        float2 pixelArt(float2 uv, float pixelSample)
        {
            float2 quantizedUV = floor(uv * pixelSample) / pixelSample;
            return quantizedUV;
        }

        float4 Frag(v2f input) : SV_Target
        {
            float4 original = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
            
            float2 pixeledUV = pixelArt(input.uv, _PixelSample);
            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, pixeledUV);
            return color;
        }
    
    ENDHLSL
    
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

                #pragma vertex Vert
                #pragma fragment Frag

            ENDHLSL
        }
    }
}
