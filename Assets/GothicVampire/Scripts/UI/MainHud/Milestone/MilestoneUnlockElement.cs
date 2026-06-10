using GothicVampire.Technologies;
using TMPro;
using UnityEngine;

namespace GothicVampire.UI.MainHud
{
    public class MilestoneUnlockElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text _descriptionText;

        public IUnlockable Unlockable { get; private set; }
        
        public void Initialize(IUnlockable unlockable)
        {
            Unlockable = unlockable;
            _descriptionText.text = Unlockable.Description;
        }
    }
}