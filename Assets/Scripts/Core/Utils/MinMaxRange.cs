using UnityEditor;
using UnityEngine;

namespace Wheeled.Core.Utils
{

    [System.Serializable]
    public struct MinMaxRange
    {
        public float max;
        public float min;

        public MinMaxRange(float _min, float _max)
        {
            this.min = _min;
            this.max = _max;
        }

        public float Random => UnityEngine.Random.Range(min, max);

        public float Mid => (max + min) / 2.0f;

        public float Extent => max - min;

        public float Lerp(float _alpha)
        {
            return Mathf.Lerp(min, max, _alpha);
        }

        public float LerpUnclamped(float _alpha)
        {
            return Mathf.LerpUnclamped(min, max, _alpha);
        }

        public float GetProgress(float _value)
        {
            if (min >= max)
            {
                return 0.0f;
            }
            return (_value - min) / Extent;
        }

        public float GetClampedProgress(float _value)
        {
            return Mathf.Clamp01(GetProgress(_value));
        }

        public float Clamp(float _value)
        {
            return Mathf.Clamp(_value, min, max);
        }

    }

    public sealed class MinMaxRangeLimitAttribute : PropertyAttribute
    {
        public float Min { get; }
        public float Max { get; }

        public MinMaxRangeLimitAttribute(float _min, float _max)
        {
            Min = _min;
            Max = _max;
        }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(MinMaxRange))]
    public sealed class MinMaxIntRangeDrawer : PropertyDrawer
    {

        private static readonly GUIContent[] s_labels = new GUIContent[] { new GUIContent("L"), new GUIContent("U") };

        private float[] m_values = new float[2];

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            EditorGUI.BeginProperty(_position, _label, _property);
            var minProperty = _property.FindPropertyRelative(nameof(MinMaxRange.min));
            var maxProperty = _property.FindPropertyRelative(nameof(MinMaxRange.max));
            m_values[0] = minProperty.floatValue;
            m_values[1] = maxProperty.floatValue;
            EditorGUI.MultiFloatField(_position, _label, s_labels, m_values);
            m_values[1] = Mathf.Max(m_values);
            minProperty.floatValue = m_values[0];
            maxProperty.floatValue = m_values[1];
            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(MinMaxRangeLimitAttribute))]
    public sealed class MinMaxRangeLimitDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            MinMaxRangeLimitAttribute limitAttribute = (MinMaxRangeLimitAttribute) attribute;
            var minProperty = _property.FindPropertyRelative(nameof(MinMaxRange.min));
            var maxProperty = _property.FindPropertyRelative(nameof(MinMaxRange.max));
            float min = minProperty.floatValue, max = maxProperty.floatValue;
            string label = string.Format("{0} [{1:F2};{2:F2}]", _label.text, min, max);
            EditorGUI.MinMaxSlider(_position, label, ref min, ref max, limitAttribute.Min, limitAttribute.Max);
            minProperty.floatValue = min;
            maxProperty.floatValue = max;
        }
    }

#endif

}
