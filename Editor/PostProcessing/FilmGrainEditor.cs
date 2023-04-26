using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;
using Heartfield.Rendering.HighDefinition;

namespace HeartfieldEditor.Rendering.HighDefinition
{
    [VolumeComponentEditor(typeof(FilmGrain))]
    internal sealed class FilmGrainEditor : VolumeComponentEditor
    {
        SerializedDataParameter _intensity;
        SerializedDataParameter _response;
        SerializedDataParameter _updateRate;
        SerializedDataParameter _texture;

        [SerializeField] FilmGrain _target;

        public override void OnEnable()
        {
            _target = (FilmGrain)target;

            var o = new PropertyFetcher<FilmGrain>(serializedObject);

            _intensity = Unpack(o.Find(x => x.intensity));
            _response = Unpack(o.Find(x => x.response));
            _updateRate = Unpack(o.Find(x => x.updateRate));
            _texture = Unpack(o.Find(x => x.texture));
        }

        public override void OnInspectorGUI()
        {
            using (new IndentLevelScope())
                PropertyField(_texture);

            var texture = _target.texture.value;

            if (texture != null)
            {
                var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;

                // Fails when using an internal texture as you can't change import settings on
                // builtin resources, thus the check for null
                if (importer != null)
                {
                    bool valid = importer.mipmapEnabled == false
                        && importer.alphaSource == TextureImporterAlphaSource.FromGrayScale
                        && importer.filterMode == FilterMode.Point
                        && importer.textureCompression == TextureImporterCompression.Uncompressed
                        && importer.textureType == TextureImporterType.SingleChannel;

                    if (!valid)
                    {
                        CoreEditorUtils.DrawFixMeBox("Invalid texture import settings.", () => SetTextureImportSettings(importer));
                    }
                }
            }

            PropertyField(_intensity);
            PropertyField(_response);
            PropertyField(_updateRate);
        }

        void SetTextureImportSettings(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.SingleChannel;
            importer.alphaSource = TextureImporterAlphaSource.FromGrayScale;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            AssetDatabase.Refresh();
        }
    }
}