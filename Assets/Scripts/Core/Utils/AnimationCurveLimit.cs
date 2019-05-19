using System;
using UnityEditor;
using UnityEngine;

namespace Wheeled.Sound
{

    public class AnimationCurveLimitAttribute : PropertyAttribute
    {
        public Rect Range { get; }

        public AnimationCurveLimitAttribute(float _minX = 0.0f, float _maxX = 1.0f, float _minY = 0.0f, float _maxY = 1.0f)
        {
            Range = new Rect(_minX, _minY, _maxX - _minX, _maxY - _minY);
        }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(AnimationCurveLimitAttribute))]
    public class AnimationCurveLimitDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            AnimationCurveLimitAttribute limitAttribute = (AnimationCurveLimitAttribute) attribute;
            EditorGUI.CurveField(_position, _property, Color.white, limitAttribute.Range);
        }
    }

#endif

}
