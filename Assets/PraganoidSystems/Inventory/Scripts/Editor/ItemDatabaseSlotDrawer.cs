using UnityEngine;
using UnityEditor;

namespace PraganoidSystems.Inventory
{
    [CustomPropertyDrawer(typeof(ItemDatabase.ItemDatabaseSlot))]
    public class ItemDatabaseSlotDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get the id and item properties
            SerializedProperty idProperty = property.FindPropertyRelative("id");
            SerializedProperty itemProperty = property.FindPropertyRelative("item");

            // Create the display label
            string displayText = $"{idProperty.intValue} - {(itemProperty.objectReferenceValue != null ? ((Item)itemProperty.objectReferenceValue).Name : "None")}";
            
            // Draw the foldout with our custom label
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, displayText);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                
                // Draw the ID field
                Rect idRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(idRect, idProperty, new GUIContent("ID"));

                // Draw the Item field
                Rect itemRect = new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + 2) * 2, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(itemRect, itemProperty, new GUIContent("Item"));

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight * 3 + 4; // 3 lines + spacing
            }
            return EditorGUIUtility.singleLineHeight;
        }
    }
} 