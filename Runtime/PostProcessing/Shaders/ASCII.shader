Shader "Hidden/Heartfield Productions/Post Process/ASCII"
{
    Properties
    {
        _MainTex("Main Texture", 2DArray) = "grey" {}
    }

    HLSLINCLUDE

        #pragma target 4.5
        #pragma editor_sync_compilation
        #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

        #pragma multi_compile_local_fragment _ _LIGHT_MODE_LUMINANCE _LIGHT_MODE_RED _LIGHT_MODE_GREEN _LIGHT_MODE_BLUE
        #pragma multi_compile_local_fragment _ _COLOR_MODE_BLACK_AND_WHITE _COLOR_MODE_FLAT _COLOR_MODE_GRADIENT _COLOR_MODE_COLORED 

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"    

        TEXTURE2D_X(_MainTex);
        TEXTURE2D(_AsciiMap);

        float _Resolution;
        float _CharsCount;
        float _ColorsRange;
        float3 _FlatColor;

        struct Attributes
        {
            uint vertexID : SV_VertexID;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 texcoord   : TEXCOORD0;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        Varyings Vert(Attributes input)
        {
            Varyings output;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
            output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
            output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
            return output;
        }    

        float snap(float value, float increment)
        {
            return increment * floor(value / increment);
        }

        float getLight(float3 color)
        {
        #if _LIGHT_MODE_LUMINANCE
            return Luminance(color);
        #else            
            #if _LIGHT_MODE_RED
                return color.r;
            #elif _LIGHT_MODE_GREEN
                return color.g;
            #elif _LIGHT_MODE_BLUE
                return color.b;
            #else            
                return (color.r + color.g + color.b) / 3.0;
            #endif
        #endif
        }

        float4 Frag(Varyings input) : SV_Target0
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            float2 uv = input.texcoord;

            float resFract = 1.0 / _Resolution;
            float aspectRatio = _ScreenSize.x / _ScreenSize.y;
            float2 pixelatedUV = float2(snap(uv.x, resFract / aspectRatio), snap(uv.y, resFract));

            float3 pixelatedImage = SAMPLE_TEXTURE2D_X(_MainTex, s_point_clamp_sampler, pixelatedUV).rgb;
            pixelatedImage = round(pixelatedImage * _ColorsRange) / _ColorsRange;

            float light = getLight(pixelatedImage);
            float lightRemap = round(saturate(light) * (_CharsCount - 1.0));

            float2 asciiUV;
            asciiUV.x = (frac(uv.x * aspectRatio * _Resolution) + lightRemap) / _CharsCount;
            asciiUV.y = frac(uv.y * _Resolution);

            float ascii = 1.0 - step(SAMPLE_TEXTURE2D(_AsciiMap, s_point_clamp_sampler, asciiUV).r, 0.0);
            float3 result = ascii.rrr;

        #if _COLOR_MODE_COLORED
            result *= pixelatedImage;
        #else          
            #if _COLOR_MODE_BLACK_AND_WHITE
                result *= light;
            #else
                #if _COLOR_MODE_FLAT
                    result *= _FlatColor;
                #endif
                
                result *= max(light, 1.0);
             #endif
        #endif

            return float4(result, 1);
        }

    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }

        Pass
        {
            Cull Off ZWrite Off ZTest Always

            HLSLPROGRAM

                #pragma vertex Vert
                #pragma fragment Frag

            ENDHLSL
        }
    }
}