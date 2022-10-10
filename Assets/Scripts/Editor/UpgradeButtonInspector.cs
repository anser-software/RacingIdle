using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using System.Linq;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(UpgradeButtonUI)), ExecuteInEditMode]
public class UpgradeButtonInspector : Editor
{

    [SerializeField]
    private string[] upgrades;

    [SerializeField]
    SerializedProperty upgradeIndex;

    [SerializeField]
    private int selectedIndex; 

    private void OnEnable()
    {
        var currentUpgrades = Resources.FindObjectsOfTypeAll<Upgrade>().Select(u => u.GetType().Name).ToArray();

        if (upgrades == null || upgrades != currentUpgrades)
        {
            upgrades = currentUpgrades;
        }

        upgradeIndex = serializedObject.FindProperty("upgradeIndex");

        selectedIndex = upgradeIndex.intValue;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        selectedIndex = EditorGUILayout.Popup(new GUIContent("Upgrade: "), upgradeIndex.intValue, upgrades);

        upgradeIndex.intValue = selectedIndex;

        serializedObject.ApplyModifiedProperties();

        DrawDefaultInspector();
    }

}

