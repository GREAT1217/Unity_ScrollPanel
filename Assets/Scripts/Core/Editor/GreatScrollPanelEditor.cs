/*************************************************************************
 *  Copyright © 2020 Great1217. All rights reserved.
 *------------------------------------------------------------------------
 *  File         :  GreatScrollPanelEditor.cs
 *  Description  :  Null.
 *------------------------------------------------------------------------
 *  Author       :  Great1217
 *  Version      :  0.1.0
 *  Date         :  8/8/2020
 *  Description  :  Initial development version.
 *************************************************************************/

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GreatScrollPanel))]
public class GreatScrollPanelEditor : Editor
{
    GreatScrollPanel _greatScrollPanel;

    SerializedProperty _scrollAxis,
        _itemInitType, _itemPrefab, _itemsCount, _itemAutoLayout, _itemSpacing, _itemSizeType, _itemSize, _infinite,
        _snapSpeed, _snapThreshold,
        _inertia, _startingIndex;

    void Awake()
    {
        _greatScrollPanel = target as GreatScrollPanel;

        _scrollAxis = serializedObject.FindProperty("_scrollAxis");

        _itemInitType = serializedObject.FindProperty("_itemInitType");
        _itemPrefab = serializedObject.FindProperty("_itemPrefab");
        _itemsCount = serializedObject.FindProperty("_itemsCount");
        _itemAutoLayout = serializedObject.FindProperty("_itemAutoLayout");
        _itemSizeType = serializedObject.FindProperty("_itemSizeType");
        _itemSize = serializedObject.FindProperty("_itemSize");
        _itemSpacing = serializedObject.FindProperty("_itemSpacing");

        _snapSpeed = serializedObject.FindProperty("_snapSpeed");
        _snapThreshold = serializedObject.FindProperty("_snapThreshold");

        _infinite = serializedObject.FindProperty("_infinite");
        _inertia = serializedObject.FindProperty("_inertia");
        _startingIndex = serializedObject.FindProperty("_startingIndex");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_scrollAxis, new GUIContent("Scroll Axis"));
        EditorGUILayout.Space();
        OnItemGUI();
        EditorGUILayout.Space();
        OnSnapGUI();

        serializedObject.ApplyModifiedProperties();
        PrefabUtility.RecordPrefabInstancePropertyModifications(_greatScrollPanel);
    }

    void OnItemGUI()
    {
        if (_greatScrollPanel._scrollAxis != GreatScrollPanel.ScrollAxis.Free)
        {
            EditorGUILayout.PropertyField(_itemInitType, new GUIContent("Item InitType"));
            if (_greatScrollPanel._itemInitType == GreatScrollPanel.ItemInitType.Dynamic)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_itemPrefab, new GUIContent("Item Prefab"));
                EditorGUILayout.PropertyField(_itemsCount, new GUIContent("Items Count"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(_itemAutoLayout, new GUIContent("Item AutoLayout"));
            if (_greatScrollPanel._itemAutoLayout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_itemSizeType, new GUIContent("Item SizeType"));
                if (_greatScrollPanel._itemSizeType == GreatScrollPanel.ItemSizeType.Custom)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_itemSize, new GUIContent("Item Size"));
                    EditorGUILayout.PropertyField(_itemSpacing, new GUIContent("Item Spacing"));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(_infinite, new GUIContent("Infinite"));
                EditorGUI.indentLevel--;
            }
            else
            {
                _greatScrollPanel._infinite = false;
            }
            EditorGUILayout.PropertyField(_startingIndex, new GUIContent("Starting Index"));
        }
    }

    void OnSnapGUI()
    {
        EditorGUILayout.Slider(_snapSpeed, 0f, 10f, new GUIContent("Snap Speed"));
        EditorGUILayout.Slider(_snapThreshold, 0.01f, 0.1f, new GUIContent("Snap Threshold"));
        EditorGUILayout.PropertyField(_inertia, new GUIContent("Inertia"));
    }

}