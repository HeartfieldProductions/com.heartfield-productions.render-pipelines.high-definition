using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;
using Heartfield.Rendering.HighDefinition;

namespace HeartfieldEditor.Rendering.HighDefinition
{
    [VolumeComponentEditor(typeof(NaturalVignette))]
    internal sealed class NaturalVignetteEditor : VolumeComponentEditor
    {
        SerializedDataParameter _mode;
        SerializedDataParameter _mask;
        SerializedDataParameter _falloff;
        SerializedDataParameter _opacity;

        [SerializeField] NaturalVignette _target;

        public override void OnEnable()
        {
            _target = (NaturalVignette)target;

            var o = new PropertyFetcher<NaturalVignette>(serializedObject);

            _mode = Unpack(o.Find(x => x.mode));
            _mask = Unpack(o.Find(x => x.mask));
            _falloff = Unpack(o.Find(x => x.falloff));
            _opacity = Unpack(o.Find(x => x.opacity));
        }

        public override void OnInspectorGUI()
        {
            PropertyField(_mode);

            if (_mode.value.enumValueIndex == 0)
            {
                PropertyField(_falloff);
            }
            else
            {
                using (new IndentLevelScope())
                    PropertyField(_mask);

                var mask = _target.mask.value;

                if (mask != null)
                {
                    var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(mask)) as TextureImporter;

                    // Fails when using an internal texture as you can't change import settings on
                    // builtin resources, thus the check for null
                    if (importer != null)
                    {
                        bool valid = importer.mipmapEnabled == false
                            && importer.alphaSource == TextureImporterAlphaSource.FromGrayScale
                            && importer.filterMode == FilterMode.Bilinear
                            && importer.textureCompression == TextureImporterCompression.Uncompressed
                            && importer.textureType == TextureImporterType.SingleChannel;

                        if (!valid)
                        {
                            CoreEditorUtils.DrawFixMeBox("Invalid texture import settings.", () => SetTextureImportSettings(importer));
                        }
                    }
                }

                PropertyField(_opacity);
            }
        }

        void SetTextureImportSettings(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.SingleChannel;
            importer.alphaSource = TextureImporterAlphaSource.FromGrayScale;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            AssetDatabase.Refresh();
        }
    }
}