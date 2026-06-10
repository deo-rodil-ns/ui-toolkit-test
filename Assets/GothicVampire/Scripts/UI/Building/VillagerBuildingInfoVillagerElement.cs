using GothicVampire.Villagers;
using TMPro;
using UnityEngine;

namespace GothicVampire.UI.Buildings
{
    public class VillagerBuildingInfoVillagerElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;

        public void Show(Villager villager)
        {
            _nameText.text = villager.Identity.FullName;
        }
    }
}