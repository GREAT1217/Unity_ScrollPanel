/*************************************************************************
 *  Copyright © 2020 Great1217. All rights reserved.
 *------------------------------------------------------------------------
 *  File         :  GreatScrollEffectEditor.cs
 *  Description  :  Null.
 *------------------------------------------------------------------------
 *  Author       :  Great1217
 *  Version      :  0.1.0
 *  Date         :  8/10/2020
 *  Description  :  Initial development version.
 *************************************************************************/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GreatScrollEffect))]
public class GreatScrollEffectEditor : Editor
{
    GreatScrollEffect _greatScrollEffect;
    List<string> _effectTypes;

    int _effectTypeIndex;
    bool _effectSpace, _effectValue;
    float _vMin = 0, _vMax = 1, _sMin = -100, _sMax = 100;
    AnimationCurve _effectCurve = new AnimationCurve(new Keyframe(-100, 0), new Keyframe(0, 1), new Keyframe(100, 0));

    void Awake()
    {
        _greatScrollEffect = target as GreatScrollEffect;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        ShowScrollEffects();
        EditorGUILayout.Space();
        AddScrollEffect();

        serializedObject.ApplyModifiedProperties();
        PrefabUtility.RecordPrefabInstancePropertyModifications(_greatScrollEffect);
    }

    void ShowScrollEffects()
    {
        if (_greatScrollEffect._scrollEffects == null) return;
        for (int i = 0; i < _greatScrollEffect._scrollEffects.Count; i++)
        {
            _greatScrollEffect._scrollEffects[i].OnInspectorGUI();
        }
    }

    void AddScrollEffect()
    {
        _effectTypes = new List<string>();
        foreach (ScrollEffect.EffectType item in Enum.GetValues(typeof(ScrollEffect.EffectType)))
        {
            _effectTypes.Add(item.ToString());
        }

        if (_greatScrollEffect._scrollEffects != null)
        {
            foreach (ScrollEffect item in _greatScrollEffect._scrollEffects)
            {
                _effectTypes.Remove(item._effectType.ToString());
            }
        }
        if (_effectTypes.Count <= 0) return;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space();
        EditorGUI.indentLevel = 1;

        _effectTypeIndex = EditorGUILayout.Popup("EffectType", _effectTypeIndex, _effectTypes.ToArray());

        _effectSpace = EditorGUILayout.Foldout(_effectSpace, "EffectSpace", true);
        if (_effectSpace)
        {
            EditorGUI.indentLevel++;
            _sMin = EditorGUILayout.FloatField(new GUIContent("Min"), _sMin);
            _sMax = EditorGUILayout.FloatField(new GUIContent("Max"), _sMax);
            EditorGUI.indentLevel--;
        }

        _effectValue = EditorGUILayout.Foldout(_effectValue, "EffectValue", true);
        if (_effectValue)
        {
            EditorGUI.indentLevel++;
            _vMin = EditorGUILayout.FloatField(new GUIContent("Min"), _vMin);
            _vMax = EditorGUILayout.FloatField(new GUIContent("Max"), _vMax);
            EditorGUI.indentLevel--;
        }

        _effectCurve = EditorGUILayout.CurveField(new GUIContent("EffectCurve"), _effectCurve, Color.green, new Rect(_sMin, _vMin, _sMax - _sMin, _vMax - _vMin));

        EditorGUILayout.Space();
        if (GUILayout.Button("Add New ScrollEffect"))
        {
            ScrollEffect.EffectType effectType = (ScrollEffect.EffectType)Enum.Parse(typeof(ScrollEffect.EffectType), _effectTypes[_effectTypeIndex]);
            _greatScrollEffect._scrollEffects.Add(new ScrollEffect(effectType, _sMin, _sMax, _vMin, _vMax, new AnimationCurve(_effectCurve.keys), _greatScrollEffect));
        }
        EditorGUILayout.Space();

        EditorGUILayout.EndVertical();
    }
}