using System;
using GothicVampire.Unrest;
using Sylpheed.Extensions;
using TMPro;
using UnityEngine;

namespace GothicVampire.UI.MainHud
{
    
    public class UnrestSnapshotCategoryElement : MonoBehaviour
    {
        [SerializeField] private ValueType _valueType;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _valueText;

        [SerializeField] private Color _defaultValueColor;
        [SerializeField] private Color _increaseValueColor;
        [SerializeField] private Color _decreaseValueColor;

        public void Initialize(UnrestCategorySnapshot snapshot)
        {
            var value = _valueType switch
            {
                ValueType.Projected => snapshot.ProjectedValue,
                ValueType.Applied => snapshot.AppliedValue,
                ValueType.AppliedImmediate => snapshot.AppliedImmediateValue,
                ValueType.AppliedResolved => snapshot.AppliedResolvedValue,
                _ => throw new ArgumentOutOfRangeException()
            };

            _descriptionText.text = snapshot.Category.ShortDescription;
            _valueText.text = value.ToStringWithPrefix("N0");
            
            _valueText.color = value switch
            {
                > 0 => _increaseValueColor,
                < 0 => _decreaseValueColor,
                _ => _defaultValueColor
            };
        }
        
        private enum ValueType { Projected, Applied, AppliedImmediate, AppliedResolved }
    }
}