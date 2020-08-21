/*************************************************************************
 *  Copyright © 2020 Great1217. All rights reserved.
 *------------------------------------------------------------------------
 *  File         :  GreatScrollEffect.cs
 *  Description  :  Null.
 *------------------------------------------------------------------------
 *  Author       :  Great1217
 *  Version      :  0.1.0
 *  Date         :  8/8/2020
 *  Description  :  Initial development version.
 *************************************************************************/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ScrollEffect
{
    public enum EffectType
    {
        EularAnglesX,
        EularAnglesY,
        EularAnglesZ,
        ScaleX,
        ScaleY,
        PositionZ,
    }

    public EffectType _effectType;//效果类型
    public float _effectSpaceMin;//生效区间（与中心在滑动轴向的间距）
    public float _effectSpaceMax;
    public float _effectValueMin;//生效值（某一类效果的生效值区间）
    public float _effectValueMax;
    public AnimationCurve _effectCurve;//效果曲线：根据X坐标（与中心在滑动轴向的间距），求出Y坐标（某一类效果的生效值区间）

    public float GetEffectValue(float value)
    {
        return (_effectCurve != null) ? _effectCurve.Evaluate(value) : 0f;
    }

#if UNITY_EDITOR

    [SerializeField]
    GreatScrollEffect _scrollEffects;
    [SerializeField]
    bool _effectGUIBox, _effectSpace, _effectValue;

    public ScrollEffect(EffectType effectType, float effectSpaceMin, float effectSpaceMax, float effectValueMin, float effectValueMax, AnimationCurve effectCurve, GreatScrollEffect scrollEffect)
    {
        _effectType = effectType;
        _effectSpaceMin = effectSpaceMin;
        _effectSpaceMax = effectSpaceMax;
        _effectValueMin = effectValueMin;
        _effectValueMax = effectValueMax;
        _effectCurve = effectCurve;
        _scrollEffects = scrollEffect;
    }

    public void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUI.indentLevel = 1;

        _effectGUIBox = EditorGUILayout.Foldout(_effectGUIBox, _effectType.ToString(), true);
        if (_effectGUIBox)
        {
            _effectSpace = EditorGUILayout.Foldout(_effectSpace, "EffectSpace", true);
            if (_effectSpace)
            {
                EditorGUI.indentLevel++;
                _effectSpaceMin = EditorGUILayout.FloatField(new GUIContent("Min"), _effectSpaceMin);
                _effectSpaceMax = EditorGUILayout.FloatField(new GUIContent("Max"), _effectSpaceMax);
                EditorGUI.indentLevel--;
            }

            _effectValue = EditorGUILayout.Foldout(_effectValue, "EffectValue", true);
            if (_effectValue)
            {
                EditorGUI.indentLevel++;
                _effectValueMin = EditorGUILayout.FloatField(new GUIContent("Min"), _effectValueMin);
                _effectValueMax = EditorGUILayout.FloatField(new GUIContent("Max"), _effectValueMax);
                EditorGUI.indentLevel--;
            }

            _effectCurve = EditorGUILayout.CurveField("EffectCurve", _effectCurve, Color.white, new Rect(_effectSpaceMin, _effectValueMin, _effectSpaceMax - _effectSpaceMin, _effectValueMax - _effectValueMin));

            EditorGUILayout.Space();
            if (GUILayout.Button("Remove This ScrollEffect"))
            {
                _scrollEffects._scrollEffects.Remove(this);
            }
            EditorGUILayout.Space();

        }
        EditorGUILayout.EndVertical();
    }

#endif
}

[RequireComponent(typeof(GreatScrollPanel))]
public class GreatScrollEffect : MonoBehaviour
{
    public List<ScrollEffect> _scrollEffects = new List<ScrollEffect>();
    GreatScrollPanel _scrollPanel;

    public void AddScrollEffect(ScrollEffect effect)
    {
        if (_scrollEffects == null) _scrollEffects = new List<ScrollEffect>();
        _scrollEffects.Add(effect);
    }

    void Awake()
    {
        _scrollPanel = GetComponent<GreatScrollPanel>();
    }

    void Update()
    {
        OnScrollEffect();
    }

    /// <summary>
    /// 滚动效果
    /// </summary>
    void OnScrollEffect()
    {
        if (_scrollEffects == null || _scrollEffects.Count == 0) return;
        foreach (Transform item in _scrollPanel.ItemsRT)
        {
            float space = _scrollPanel.SpaceFromCenter(item.position);
            foreach (ScrollEffect effect in _scrollEffects)
            {
                switch (effect._effectType)
                {
                    case ScrollEffect.EffectType.EularAnglesX:
                        {
                            Vector3 eulerAngles = item.localEulerAngles;
                            eulerAngles.x = effect.GetEffectValue(space);
                            item.localRotation = Quaternion.Euler(eulerAngles);
                        }
                        break;
                    case ScrollEffect.EffectType.EularAnglesY:
                        {
                            Vector3 eulerAngles = item.localEulerAngles;
                            eulerAngles.y = effect.GetEffectValue(space);
                            item.localRotation = Quaternion.Euler(eulerAngles);
                        }
                        break;
                    case ScrollEffect.EffectType.EularAnglesZ:
                        {
                            Vector3 eulerAngles = item.localEulerAngles;
                            eulerAngles.z = effect.GetEffectValue(space);
                            item.localRotation = Quaternion.Euler(eulerAngles);
                        }
                        break;
                    case ScrollEffect.EffectType.ScaleX:
                        {
                            item.localScale = new Vector3(effect.GetEffectValue(space), item.localScale.y, 1);
                        }
                        break;
                    case ScrollEffect.EffectType.ScaleY:
                        {
                            item.localScale = new Vector3(item.localScale.x, effect.GetEffectValue(space), 1);
                        }
                        break;
                    case ScrollEffect.EffectType.PositionZ:
                        {
                            Vector3 position = item.localPosition;
                            position.z = effect.GetEffectValue(space);
                            item.localPosition = position;
                        }
                        break;
                }
            }
        }
    }

}
