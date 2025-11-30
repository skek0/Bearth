using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WeaponStat))]
public class WeaponStatEditor : Editor
{
    // 루트 경로(필드명이 다르면 여기만 바꾸면 됨)
    const string RootField = "stats";

    SerializedProperty settingsProp;
    SerializedProperty kindProp;
    SerializedProperty currentProp;

    // 타입 캐시
    static List<Type> meleeTypes;
    static List<Type> rangedTypes;
    static string[] meleeDisplay;
    static string[] rangedDisplay;

    void OnEnable()
    {
        settingsProp = serializedObject.FindProperty(RootField);
        if (settingsProp != null)
        {
            kindProp = settingsProp.FindPropertyRelative("kind");
            currentProp = settingsProp.FindPropertyRelative("stats");
        }
        EnsureTypeCaches();
    }

    public override void OnInspectorGUI()
    {
        if (settingsProp == null)
        {
            EditorGUILayout.HelpBox($"'{RootField}' 필드를 찾을 수 없습니다.", MessageType.Error);
            return;
        }

        serializedObject.Update();

        // 제목
        EditorGUILayout.LabelField("Weapon Config", EditorStyles.boldLabel);

        // 1) 상위 타입
        EditorGUILayout.PropertyField(kindProp);
        var kind = (CombatKind)kindProp.enumValueIndex;

        // 2) 하위 타입 목록 결정(리플렉션)
        var (list, names) = kind == CombatKind.Melee ? (meleeTypes, meleeDisplay) : (rangedTypes, rangedDisplay);

        if (list == null || list.Count == 0)
        {
            EditorGUILayout.HelpBox(
                $"No {kind} subtype found. Create a class inheriting from {(kind == CombatKind.Melee ? nameof(MeleeSettings) : nameof(RangedSettings))}.",
                MessageType.Warning);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        // 3) 현재 타입 인덱스
        int curIndex = IndexOfType(list, currentProp.managedReferenceFullTypename);

        // kind 변경으로 목록 불일치 시, 즉시 기본 타입으로 교체
        if (curIndex < 0)
        {
            currentProp.managedReferenceValue = Activator.CreateInstance(list[0]);
            curIndex = 0;
        }

        // 4) 하위 타입 드롭다운
        int nextIndex = EditorGUILayout.Popup("Subtype", curIndex, names);

        // 선택 변경 또는 최초 비어있음 → 교체
        if (nextIndex != curIndex || string.IsNullOrEmpty(currentProp.managedReferenceFullTypename))
        {
            var newType = list[Mathf.Clamp(nextIndex, 0, list.Count - 1)];
            currentProp.managedReferenceValue = Activator.CreateInstance(newType);
        }

        // 5) 선택된 하위 타입 필드 자동 표시 (항상 펼쳐짐)
        EditorGUILayout.PropertyField(currentProp, includeChildren: true);

        serializedObject.ApplyModifiedProperties();
    }

    static void EnsureTypeCaches()
    {
        if (meleeTypes == null)
        {
            meleeTypes = TypeCache.GetTypesDerivedFrom<MeleeSettings>()
                .Where(t => !t.IsAbstract && t.IsClass && t.IsSerializable).ToList();
            meleeDisplay = meleeTypes.Select(EditTypeName).ToArray();
        }
        if (rangedTypes == null)
        {
            rangedTypes = TypeCache.GetTypesDerivedFrom<RangedSettings>()
                .Where(t => !t.IsAbstract && t.IsClass && t.IsSerializable).ToList();
            rangedDisplay = rangedTypes.Select(EditTypeName).ToArray();
        }
    }

    static string EditTypeName(Type t)
    {
        // 네임스페이스 제거 + Settings 접미사 제거
        var n = t.Name;
        return n.EndsWith("Weapon", StringComparison.Ordinal) ? n : n.Replace("Settings", "");
    }

    static int IndexOfType(List<Type> list, string fullTypeName)
    {
        if (string.IsNullOrEmpty(fullTypeName)) return -1;
        for (int i = 0; i < list.Count; i++)
            if (fullTypeName.Contains(list[i].FullName))
                return i;
        return -1;
    }
}
