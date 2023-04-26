using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;
using Heartfield.Rendering.HighDefinition;

namespace HeartfieldEditor.Rendering.HighDefinition
{
    [VolumeComponentEditor(typeof(ASCII))]
    internal sealed class ASCIIEditor : VolumeComponentEditor
    {
        SerializedDataParameter _asciiMap;
        SerializedDataParameter _charsCount;
        SerializedDataParameter _resolution;
        SerializedDataParameter _lightMode;
        SerializedDataParameter _colorMode;
        SerializedDataParameter _flatColor;
        SerializedDataParameter _gradientColor;
        SerializedDataParameter _colorRange;

        [SerializeField] ASCII _target;

        public override void OnEnable()
        {
            _target = (ASCII)target;

            var o = new PropertyFetcher<ASCII>(serializedObject);

            _asciiMap = Unpack(o.Find(x => x.asciiMap));
            _charsCount = Unpack(o.Find(x => x.charsCount));
            _resolution = Unpack(o.Find(x => x.resolution));
            _lightMode = Unpack(o.Find(x => x.lightMode));
            _colorMode = Unpack(o.Find(x => x.colorMode));
            _flatColor = Unpack(o.Find(x => x.flatColor));
            _gradientColor = Unpack(o.Find(x => x.gradientColor));
            _colorRange = Unpack(o.Find(x => x.colorRange));
        }

        public override void OnInspectorGUI()
        {
            PropertyField(_asciiMap);

            var texture = _target.asciiMap.value;

            EditorGUI.BeginDisabledGroup(texture == null);
            {
                using (new IndentLevelScope())
                {
                    PropertyField(_charsCount);
                    PropertyField(_resolution);
                }

                PropertyField(_lightMode);
                PropertyField(_colorMode);                

                int colorIndex = _colorMode.value.enumValueIndex;

                if (colorIndex > 0)
                {
                    bool useGradientColor = colorIndex == 2;
                    bool useFlatColor = colorIndex == 3;

                    using (new IndentLevelScope())
                    {
                        if (useFlatColor)
                        {
                            PropertyField(_flatColor);
                        }
                        else if (useGradientColor)
                        {
                            PropertyField(_gradientColor);
                        }

                        PropertyField(_colorRange);
                    }
                }
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}