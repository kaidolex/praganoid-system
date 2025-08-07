using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PraganoidSystems.Inventory
{
    [CustomEditor(typeof(ItemDatabase))]
    public class ItemDatabaseEditor : Editor
    {
        private SerializedProperty itemsProperty;

        private void OnEnable()
        {
            itemsProperty = serializedObject.FindProperty("items");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Item Database", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (itemsProperty != null)
            {
                EditorGUILayout.PropertyField(itemsProperty, new GUIContent("Items"), true);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
} 