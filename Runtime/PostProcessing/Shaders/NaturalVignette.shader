Shader "Hidden/Heartfield Productions/Post Process/Natural Vignette"
{
    Properties
    {
        _MainTex("Main Texture", 2DArray) = "grey" {}
    }

    HLSLINCLUDE

        #pragma target 4.5
        #pragma editor_sync_compilation
        #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

        #pragma multi_compile_local_fragment _ _PROCEDURAL

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"    

        TEXTURE2D_X(_MainTex);
        TEXTURE2D(_VignetteMask);

        float _Falloff;
        float _Opacity;

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

        float4 Frag(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            float2 uv = input.texcoord;
            float3 sourceColor = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, uv).rgb;
            
        #if _PROCEDURAL
            float2 coord = (uv - 0.5) * 2.0;
            //coord.x *= (_ScreenSize.z * _ScreenSize.w) + 1.0;
            float rf = sqrt(dot(coord, coord)) * _Falloff;
            float rf2_1 = rf * rf + 1.0;
            float vignette = 1.0 / (rf2_1 * rf2_1);
        #else
            float vignette = SAMPLE_TEXTURE2D(_VignetteMask, s_linear_clamp_sampler, uv).w;
            vignette = lerp(1.0, FastSRGBToLinear(vignette), _Opacity);
        #endif

            return float4(sourceColor * vignette, 1);
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