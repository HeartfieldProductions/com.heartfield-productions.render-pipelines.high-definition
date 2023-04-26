using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Heartfield.Rendering.HighDefinition
{
    public enum ASCIILightMode
    {
        Luminance = 0,
        Average = 1,
        Red = 2,
        Green = 3,
        Blue = 4
    }

    public enum ASCIIColorMode
    {
        None = 0,
        BlackAndWhite = 1,
        //Gradient = 2,
        Flat = 3,
        Colored = 4,
    }

    [Serializable]
    [VolumeComponentMenuForRenderPipeline("Post-processing/Heartfield Productions/ASCII", typeof(HDRenderPipeline))]
    public sealed class ASCII : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        const string SHADER_NAME = "Hidden/Heartfield Productions/Post Process/ASCII";

        const string LIGHT_MODE_LUMINANCE = "_LIGHT_MODE_LUMINANCE";
        const string LIGHT_MODE_RED = "_LIGHT_MODE_RED";
        const string LIGHT_MODE_GREEN = "_LIGHT_MODE_GREEN";
        const string LIGHT_MODE_BLUE = "_LIGHT_MODE_BLUE";

        const string COLOR_MODE_BLACK_AND_WHITE = "_COLOR_MODE_BLACK_AND_WHITE";
        const string COLOR_MODE_FLAT = "_COLOR_MODE_FLAT";
        const string COLOR_MODE_GRADIENT = "_COLOR_MODE_GRADIENT";
        const string COLOR_MODE_COLORED = "_COLOR_MODE_COLORED";

        readonly int ASCII_MAP = Shader.PropertyToID("_AsciiMap");
        readonly int CHARS_COUNT = Shader.PropertyToID("_CharsCount");
        readonly int RESOLUTION = Shader.PropertyToID("_Resolution");
        readonly int FLAT_COLOR = Shader.PropertyToID("_FlatColor");
        readonly int GRADIENT_COLOR = Shader.PropertyToID("_GradientColor");
        readonly int COLORS_RANGE = Shader.PropertyToID("_ColorsRange");

        public Texture2DParameter asciiMap = new(null);

        public MinIntParameter charsCount = new(9, 1);

        public MinIntParameter resolution = new(128, 2);

        public ASCIILightModeParameter lightMode = new(ASCIILightMode.Luminance);

        public ASCIIColorModeParameter colorMode = new(ASCIIColorMode.None);

        [InspectorName("Color")]
        public ColorParameter flatColor = new(Color.green);

        [InspectorName("Gradient")]
        public GrandientParameter gradientColor = new(defaultGradient);

        [InspectorName("Range")]
        public MinIntParameter colorRange = new(8, 1);

        static readonly Gradient defaultGradient = new()
        {
            alphaKeys = new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f),
            },

            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(0.1f * Color.green, 0f),
                new GradientColorKey(Color.green, 1f)
            },

            mode = GradientMode.Blend
        };

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
                Debug.LogError($"Unable to find shader '{SHADER_NAME}'. Post Process Volume ASCII is unable to load");
            }
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            _material.enabledKeywords = null;

            _material.SetTexture(ASCII_MAP, asciiMap.value);
            _material.SetFloat(RESOLUTION, resolution.value);
            _material.SetFloat(CHARS_COUNT, charsCount.value);

            switch (lightMode.value)
            {
                case ASCIILightMode.Luminance:
                    _material.EnableKeyword(LIGHT_MODE_LUMINANCE);
                    break;

                case ASCIILightMode.Red:
                    _material.EnableKeyword(LIGHT_MODE_RED);
                    break;

                case ASCIILightMode.Green:
                    _material.EnableKeyword(LIGHT_MODE_GREEN);
                    break;

                case ASCIILightMode.Blue:
                    _material.EnableKeyword(LIGHT_MODE_BLUE);
                    break;

                default: break;
            }

            switch (colorMode.value)
            {
                case ASCIIColorMode.BlackAndWhite:
                    _material.EnableKeyword(COLOR_MODE_BLACK_AND_WHITE);
                    break;

                case ASCIIColorMode.Flat:
                    _material.EnableKeyword(COLOR_MODE_FLAT);
                    _material.SetColor(FLAT_COLOR, flatColor.value);
                    break;

                /*case ASCIIColorMode.Gradient:
                    _material.EnableKeyword(COLOR_MODE_GRADIENT);
                    _material.SetColor(GRADIENT_COLOR, gradientColor.value.Evaluate(1));
                    break;*/

                case ASCIIColorMode.Colored:
                    _material.EnableKeyword(COLOR_MODE_COLORED);
                    break;

                default: break;
            }

            _material.SetFloat(COLORS_RANGE, colorRange.value);            

            cmd.Blit(source, destination, _material, 0, 0);
        }

        public override void Cleanup()
        {
            CoreUtils.Destroy(_material);
        }

        public bool IsActive()
        {
            return _material != null && asciiMap.value != null;
        }

        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.BeforePostProcess;
    }

    [Serializable]
    public sealed class ASCIILightModeParameter : VolumeParameter<ASCIILightMode>
    {
        public ASCIILightModeParameter(ASCIILightMode value, bool overrideState = false) : base(value, overrideState) { }
    }

    [Serializable]
    public sealed class ASCIIColorModeParameter : VolumeParameter<ASCIIColorMode>
    {
        public ASCIIColorModeParameter(ASCIIColorMode value, bool overrideState = false) : base(value, overrideState) { }
    }

    [Serializable]
    public sealed class GrandientParameter : VolumeParameter<Gradient>
    {
        public GrandientParameter(Gradient value, bool overrideState = false) : base(value, overrideState) { }
    }
}