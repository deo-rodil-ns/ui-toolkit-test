// using System;
// using GothicVampire.Currencies;
// using UnityEditor;
// using UnityEngine;
//
// namespace GothicVampire.Editor.Currencies
// {
//     [CustomPropertyDrawer(typeof(Currency))]
//     public class CurrencyPropertyDrawer : PropertyDrawer
//     {
//         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//         {
//             var originalLabelWidth = EditorGUIUtility.labelWidth;
//             
//             GUILayout.Space(-EditorGUIUtility.singleLineHeight - 2f);
//             EditorGUILayout.BeginHorizontal();
//
//             var obj = (Currency)property.boxedValue;
//
//             // Type
//             var type = EditorGUILayout.ObjectField(obj.Type, typeof(CurrencyType), false, GUILayout.ExpandWidth(true));
//
//             // Value
//             EditorGUIUtility.labelWidth = 40f;
//             var value = EditorGUILayout.FloatField("Value", obj.Value, GUILayout.MaxWidth(150f));
//             
//             // Max toggle
//             var max = obj.Max;
//             var maxToggled = EditorGUILayout.Toggle(obj.HasMax, GUILayout.MaxWidth(15f));
//             if (maxToggled && !obj.HasMax) max = 100f;
//             else if (!maxToggled && obj.HasMax) max = float.PositiveInfinity;
//             
//             // Disable max field if untoggled
//             if (!maxToggled) GUI.enabled = false;
//             EditorGUIUtility.labelWidth = 30f;
//             max = EditorGUILayout.FloatField("Max", max, GUILayout.MaxWidth(150f));
//             GUI.enabled = true;
//             
//             // Update obj
//             obj = new Currency(type as CurrencyType, value, max);
//             property.boxedValue = obj;
//             
//             EditorGUILayout.Space(10f);
//
//             EditorGUILayout.EndHorizontal();
//             property.serializedObject.ApplyModifiedProperties();
//             
//             // Add vertical padding
//             GUILayout.Space(EditorGUIUtility.singleLineHeight / 4f);
//
//             EditorGUIUtility.labelWidth = originalLabelWidth;
//         }
//     }
// }