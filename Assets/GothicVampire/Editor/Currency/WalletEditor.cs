using System;
using System.Linq;
using GothicVampire.Currencies;
using UnityEditor;
using UnityEngine;

namespace GothicVampire.Editor.Currencies
{
    [CustomEditor(typeof(Wallet))]
    public class WalletEditor : UnityEditor.Editor
    {
        private Wallet _wallet;
        
        private void OnEnable()
        {
            _wallet = (Wallet)target;
            RequiresConstantRepaint();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI(); // Draw the default inspector
            
            if (!Application.isPlaying) return;
            
            // Sort currencies alphabetically
            var currencies = _wallet.Currencies.OrderBy(c => c.Type.DisplayName).ToList();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Currencies", EditorStyles.boldLabel);
            
            // Draw
            EditorGUI.indentLevel++;
            foreach (var currency in currencies)
            {
                var maxText = currency.HasMax ? currency.Max.ToString("N0") : "--";
                var text = $"[{currency.Type.DisplayName}]: {currency.Value:N0} / {maxText}";
                EditorGUILayout.LabelField(text);
            }
            EditorGUI.indentLevel--;
        }
    }
}