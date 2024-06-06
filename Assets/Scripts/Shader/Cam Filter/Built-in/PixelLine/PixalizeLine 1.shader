Shader "Hidden/PixelizeLine1"
{
    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/Colors.hlsl"
        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/Sampling.hlsl"

    
        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float4 _MainTex_TexelSize;
        int _PixelSample;

        TEXTURE2D_SAMPLER2D(_CameraNormalsTexture, sampler_CameraNormalsTexture);
        TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
        float _Scale;
        float4 _Color;
        float4 _NormalColor;

        float _DepthThreshold;
		float _NormalThreshold;
		float _NormalEdgeBias;
		bool _Highlight;

		float3 normal_edge_bias = (float3(1., 1., 1.));

        // This matrix is populated in PostProcessOutline.cs.
		float4x4 _ClipToView;

        // Combines the top and bottom colors using normal blending.
        // https://en.wikipedia.org/wiki/Blend_modes#Normal_blend_mode
		// This performs the same operation as Blend SrcAlpha OneMinusSrcAlpha.
		float4 alphaBlend(float4 top, float4 bottom)
		{
			float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
			float alpha = top.a + bottom.a * (1 - top.a);

			return float4(color, alpha);
		}
        struct v2f
        {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
		    float2 texcoord : TEXCOORD0; //might remove
		    float3 viewSpaceDir : TEXCOORD1;
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
            //o.vertex = float4(v.vertex.xy, 0.0, 1.0);
            o.uv = TransformTriangleVertexToUV(v.vertex.xy);
            o.screen_pos = ComputeScreenPos(o.vertex);
            
            o.texcoord = TransformTriangleVertexToUV(v.vertex.xy);
            o.viewSpaceDir = mul(_ClipToView, o.vertex).xyz;
        	
            return o;
        }

        float2 pixelArt(float2 uv, float pixelSample)
        {
			float pixelScale = pixelSample * _Scale;
            float2 quantizedUV = floor(uv * pixelScale) / pixelScale;
            return quantizedUV;
        }
        float2 pixelArtSobel(float2 uv, float pixelSample, float pixelOffsetX, float pixelOffsetY)
        {
			float pixelScale = pixelSample * _Scale;
            float2 quantizedUV = floor(uv * pixelScale) / pixelScale;
            quantizedUV -= float2(pixelOffsetX / 2, pixelOffsetY) / pixelScale;
            return quantizedUV;
        }

		float3 GetDepth(float2 uv)
        {
	        return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv).r;
        }
		float3 GetNormal(float2 uv)
        {
	        return SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, uv).rgb;
        }
	    float normalEdgeIndicator(float3 baseNormal, float3 newNormal)
		{
		    float normalDiff = dot(baseNormal, newNormal);
		    float edgeIndicator = 1 - abs(normalDiff);
        		
		    return edgeIndicator;
		}

		float GetDepthStrength(float depthBiases[6])
		{
		    float difference = 0;
		    for (int i = 0; i < 4; i++)
		        difference += clamp(depthBiases[i], 0, 1);
		    return smoothstep(0.01f, 0.02f, difference);
		}

    
        float4 Frag(v2f input) : SV_Target
        {
        	#pragma region Pixelize UV
            //--------------Pixelize this-------------//
            float2 pixeledUV = pixelArt(input.uv, _PixelSample);
            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, pixeledUV);

            float2 pixeledUVOffset[6];
            
            pixeledUVOffset[0] = pixelArtSobel(input.uv, _PixelSample, 0, 1); //up +
            pixeledUVOffset[1] = pixelArtSobel(input.uv, _PixelSample, 0, -1); //down +
            pixeledUVOffset[2] = pixelArtSobel(input.uv, _PixelSample, -1, 0); //left +
            pixeledUVOffset[3] = pixelArtSobel(input.uv, _PixelSample, 1, 0); //right +
        	
            pixeledUVOffset[4] = pixelArtSobel(input.uv, _PixelSample, 0, 2); //up 2 + 
            pixeledUVOffset[5] = pixelArtSobel(input.uv, _PixelSample, 0, -2); //down 2 +
        	#pragma endregion

        	#pragma region Depth
        	//--------------Depth-------------//
            float depth = GetDepth(pixeledUV);
            float depths[6];
        	for(int j = 0; j < 6; j++)
        	{
				depths[j] = GetDepth(pixeledUVOffset[j]);
        	}
        	
            float depthThreshold = _DepthThreshold * depth;
            float depthFiniteDifference1 = depths[1] - depths[0];
            float depthFiniteDifference2 = depths[3] - depths[2];
        	
        	float edgeDepth = sqrt(pow(depthFiniteDifference1, 2) + pow(depthFiniteDifference2, 2)) * 100;
        	edgeDepth = edgeDepth > depthThreshold ? 1 : 0;
        	
        	//return edgeDepth; //test
        	//return removeArtifact; //test
        	
			#pragma endregion 
        	#pragma region Normal
        	
        	//---------------Normal------------//
        	
        	float3 normals[4];
        	float3 normal = GetNormal(pixeledUV) * 2.0 - 1.0;
        	
        	float normal_diff = 0.;
			for(int j = 0; j < 4; j++)
			{
				normals[j] = GetNormal(pixeledUVOffset[j]) * 2.0 - 1.0;
				//Highlights
				normal_diff += normalEdgeIndicator(normal, normals[j]);
			}
        	normal_diff = smoothstep(0.2f, 0.8f, normal_diff);

        	//return normal_diff + edgeDepth; //test
        	
        	//----------------------------------//
        	
        	#pragma endregion
        	float depthStrength = GetDepthStrength(depths);

			float4 edgeColor = float4(_Color.rgb, _Color.a * edgeDepth);
			float4 normalColor = float4(_NormalColor.rgb, _NormalColor.a * normal_diff * _Highlight);
        	
        	float4 finalNormalColor = alphaBlend(normalColor, color);
        	float4 finalColor = alphaBlend(edgeColor, finalNormalColor);
        	
        	//return  depthStrength;
        	return finalColor;
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
