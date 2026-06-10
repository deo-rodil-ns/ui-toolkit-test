using System;
using GothicVampire.Jobs;
using UnityEngine;
using UnityEngine.UI;

namespace GothicVampire.UI.Buildings
{
    public class JobSlotView : MonoBehaviour
    {
        [SerializeField] private Sprite _assignedSprite;
        [SerializeField] private Sprite _reservedSprite;
        [SerializeField] private Sprite _unavailableSprite;
        [SerializeField] private Sprite _lockedSprite;

        [Header("UI References")]
        [SerializeField] private Image _backgroundImage;
        
        public void Show(Job job)
        {
            if (job.Locked)
            {
                _backgroundImage.overrideSprite = _lockedSprite;
                return;
            }

            _backgroundImage.overrideSprite = job.State switch
            {
                JobAssignmentState.Assigned => _assignedSprite,
                JobAssignmentState.Reserved => _reservedSprite,
                JobAssignmentState.Unassigned => _unavailableSprite,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}