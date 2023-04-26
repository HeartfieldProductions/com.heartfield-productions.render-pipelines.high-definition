using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Heartfield.Rendering.HighDefinition
{
    public enum VignetteMode
    {
        Procedural = 0,
        Masked = 1
    }

    [Serializable]
    [VolumeComponentMenuForRenderPipeline("Post-processing/Heartfield Productions/Natural Vignette", typeof(HDRenderPipeline))]
    public sealed class NaturalVignette : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        const string SHADER_NAME = "Hidden/Heartfield Productions/Post Process/Natural Vignette";

        const string PROCEDURAL = "_PROCEDURAL";

        readonly int MASK = Shader.PropertyToID("_VignetteMask");
        readonly int FALLOFF = Shader.PropertyToID("_Falloff");
        readonly int OPACITY = Shader.PropertyToID("_Opacity");

        [Tooltip("Specifies the mode HDRP uses to display the vignette effect.")]
        public VignetteModeParameter mode = new(VignetteMode.Procedural);

        public ClampedFloatParameter falloff = new(0f, 0f, 1f);

        [Tooltip("Specifies a black and white mask Texture to use as a vignette.")]
        public Texture2DParameter mask = new(null);

        [Range(0f, 1f), Tooltip("Controls the opacity of the mask vignette. Lower values result in a more transparent vignette.")]
        public ClampedFloatParameter opacity = new(0f, 0f, 1f);

        [SerializeField, HideInInspector] Material _material;

        public override void Setup()
        {
            var shader = Shader.Find(SHADER_NAME);

            if (shader != null)
            {
                _material = CoreUtils.CreateEngineMaterial(shader);
            }
            else
            {
                Debug.LogError($"Unable to find shader '{SHADER_NAME}'. Post Process Volume Natural Vignette is unable to load");
            }
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            _material.enabledKeywords = null;

            if (mode.value == VignetteMode.Procedural)
            {
                _material.EnableKeyword(PROCEDURAL);
                _material.SetFloat(FALLOFF, falloff.value);
            }
            else
            {
                _material.SetTexture(MASK, mask.value);
                _material.SetFloat(OPACITY, opacity.value);
            }

            cmd.Blit(source, destination, _material, 0, 0);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(_material);
        }

        public bool IsActive()
        {
            return _material != null && ((mode.value == VignetteMode.Procedural && falloff.value > 0f)
                                     || (mode.value == VignetteMode.Masked && opacity.value > 0f && mask.value != null));
        }

        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;
    }

    /// <summary>
    /// A <see cref="VolumeParameter"/> that holds a <see cref="VignetteMode"/> value.
    /// </summary>
    [Serializable]
    public sealed class VignetteModeParameter : VolumeParameter<VignetteMode>
    {
        /// <summary>
        /// Creates a new <see cref="VignetteModeParameter"/> instance.
        /// </summary>
        /// <param name="value">The initial value to store in the parameter.</param>
        /// <param name="overrideState">The initial override state for the parameter.</param>
        public VignetteModeParameter(VignetteMode value, bool overrideState = false) : base(value, overrideState) { }
    }
}