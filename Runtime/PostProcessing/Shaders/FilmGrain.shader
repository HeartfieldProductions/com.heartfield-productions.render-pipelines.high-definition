Shader "Hidden/Heartfield Productions/Post Process/Film Grain"
{
    Properties
    {
        _MainTex("Main Texture", 2DArray) = "grey" {}
    }

    HLSLINCLUDE

        #pragma target 4.5
        #pragma editor_sync_compilation
        #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"    

        TEXTURE2D_X(_MainTex);
        TEXTURE2D(_GrainTexture);

        uniform float4 _GrainTexture_TexelSize;
        float _Intensity;
        float _Response;
        float2 _Offset;

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

        float4 Frag(Varyings input) : SV_Target0
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            float2 uv = input.texcoord;
            float3 sourceColor = SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, uv).rgb;

            float2 pos = (uv * (_ScreenParams.xy / _GrainTexture_TexelSize.zw)) + _Offset;
            float grain = SAMPLE_TEXTURE2D(_GrainTexture, s_linear_repeat_sampler, pos).w;

            // Remap [-1;1]
            grain = (grain - 0.5) * 2.0;

            // Noisiness response curve based on scene luminance
            float lum = 1.0 - sqrt(Luminance(sourceColor));
            lum = lerp(1.0, lum, _Response);

            sourceColor += sourceColor * grain * _Intensity * 4.0 * lum;

            return float4(sourceColor, 1);
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