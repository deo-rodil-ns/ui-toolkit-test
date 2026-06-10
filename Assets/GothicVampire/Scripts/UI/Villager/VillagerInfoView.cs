using System;
using GothicVampire.Villagers;
using TMPro;
using UnityEngine;

namespace GothicVampire
{
    public class VillagerInfoView : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _worriedText;
        [SerializeField] private TextMeshProUGUI _familyText;
        [SerializeField] private TextMeshProUGUI _activityText;

        public void Show(Villager villager)
        {
            _nameText.text = villager.Data.DisplayName;
            
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
