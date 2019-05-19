using System;
using UnityEditor;
using UnityEngine;

namespace Wheeled.Sound
{

    public class AnimationCurveLimitAttribute : PropertyAttribute
    {
        public Rect Range { get; }

        public AnimationCurveLimitAttribute(float _minX = float.NegativeInfinity, float _maxX = float.PositiveInfinity, float _minY = float.NegativeInfinity, float _maxY = float.PositiveInfinity)
        {
            Range = new Rect(_minX, _minY, _maxX - _minX, _maxY - _minY);
        }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(AnimationCurveLimitAttribute))]
    public class AnimationCurveLimitDrawer : PropertyDrawer
    {

        private Color? m_color;

        private void ChooseColor()
        {
            // TODO
            m_color = Color.red;
        }

        private void CacheColor()
        {
            if (m_color != null)
            {
                ChooseColor();
            }
        }

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            CacheColor();
            AnimationCurveLimitAttribute limitAttribute = (AnimationCurveLimitAttribute) attribute;
            EditorGUI.CurveField(_position, _property, m_color.Value, limitAttribute.Range);
        }
    }

#endif

}
