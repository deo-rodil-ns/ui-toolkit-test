using GothicVampire.Buildings;
using GothicVampire.Technologies;
using UnityEngine;
using UnityEngine.UI;

namespace GothicVampire.UI.BuildingProgression
{
    public class UnlockableBuildingElement : MonoBehaviour
    {
        [SerializeField] private Image _icon;

        public void Show(BuildingTier tier)
        {
            _icon.sprite = tier.Building.InfoIcon;
        }
    }
}