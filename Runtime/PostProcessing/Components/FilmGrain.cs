using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Heartfield.Rendering.HighDefinition
{
    using Random = System.Random;

    [Serializable]
    [VolumeComponentMenuForRenderPipeline("Post-processing/Heartfield Productions/Film Grain", typeof(HDRenderPipeline))]
    public sealed class FilmGrain : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        const string SHADER_NAME = "Hidden/Heartfield Productions/Post Process/Film Grain";

        const string CUSTOM_GRAIN = "_CUSTOM_GRAIN";

        readonly int INTENSITY = Shader.PropertyToID("_Intensity");
        readonly int RESPONSE = Shader.PropertyToID("_Response");
        readonly int GRAIN_TEXTURE = Shader.PropertyToID("_GrainTexture");
        readonly int OFFSET = Shader.PropertyToID("_Offset");

        [Tooltip("Use the slider to set the strength of the Film Grain effect.")]
        public ClampedFloatParameter intensity = new(0f, 0f, 1f);

        [Tooltip("Controls the noisiness response curve. The higher you set this value, the less noise there is in brighter areas.")]
        public ClampedFloatParameter response = new(0.8f, 0f, 1f);

        public ClampedIntParameter updateRate = new(24, 1, 60);
        [HideInInspector] public float updateRateFrac;

        [Tooltip("Specifies a tileable Texture to use for the grain. The neutral value for this Texture is 0.5 which means that HDRP does not apply grain at this value.")]
        public Texture2DParameter texture = new(null);

        [SerializeField, HideInInspector] Material _material;
        [HideInInspector] Vector2 _offset;
        readonly Random _rand = new();
        [HideInInspector] float _lastTime = 0f;

        public override void Setup()
        {
            var shader = Shader.Find(SHADER_NAME);

            if (shader != null)
            {
                _material = CoreUtils.CreateEngineMaterial(shader);
            }
            else
            {
                Debug.LogError($"Unable to find shader '{SHADER_NAME}'. Post Process Volume Film Grain is unable to load");
            }
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            //cmd.Clear();

#if HDRP_DEBUG_STATIC_POSTFX
            _offset.x = 0f;
            _offset.y = 0f;
#else
            if (Time.realtimeSinceStartup > _lastTime)
            {
                _lastTime = Time.realtimeSinceStartup + (1f / updateRate.value);
                _offset.x = (float)_rand.NextDouble();
                _offset.y = (float)_rand.NextDouble();
            }
#endif

            _material.SetTexture(GRAIN_TEXTURE, texture.value);
            _material.SetFloat(INTENSITY, intensity.value);
            _material.SetFloat(RESPONSE, response.value);
            _material.SetVector(OFFSET, _offset);

            cmd.Blit(source, destination, _material, 0, 0);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(_material);
        }

        public bool IsActive()
        {
            return _material != null && texture.value != null && intensity.value > 0f;
        }

        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;
    }
}