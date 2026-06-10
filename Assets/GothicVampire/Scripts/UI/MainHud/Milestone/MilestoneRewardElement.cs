using GothicVampire.Technologies;
using TMPro;
using UnityEngine;

namespace GothicVampire.UI.MainHud
{
    public class MilestoneRewardElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text _descriptionText;

        public TechnologyEffect Effect { get; private set; }
        
        public void Initialize(TechnologyEffect effect)
        {
            Effect = effect;
            _descriptionText.text = Effect.Description;
        }
    }
}