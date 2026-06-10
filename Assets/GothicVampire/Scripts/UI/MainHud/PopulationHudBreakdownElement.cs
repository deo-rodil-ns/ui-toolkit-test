using GothicVampire.Buildings;
using GothicVampire.Buildings.Effects;
using GothicVampire.Game;
using GothicVampire.Jobs;
using GothicVampire.Villagers;
using Sylpheed.Core;
using System.Linq;
using TMPro;
using UnityEngine;

namespace GothicVampire.UI.MainHud
{
    public class PopulationHudBreakdownElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text _villagersText;
        [SerializeField] private TMP_Text _housingText;
        [SerializeField] private TMP_Text _employedText;
        [SerializeField] private TMP_Text _unemployedText;

        private VillagerManager _villagerManager;

        public void Init(VillagerManager villagerManager)
        {
            _villagerManager = villagerManager;
        }

        public void Refresh(int totalHousingSlots)
        {
            _villagersText.text = _villagerManager.Villagers.Count.ToString();
            _housingText.text = totalHousingSlots.ToString();
            _employedText.text = _villagerManager.AssignedVillagers.Count.ToString();
            _unemployedText.text = _villagerManager.UnassignedVillagers.Count.ToString();
        }
    }
}
