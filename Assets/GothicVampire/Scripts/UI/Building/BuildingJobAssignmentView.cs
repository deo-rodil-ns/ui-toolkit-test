using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Buildings;
using GothicVampire.Jobs;
using GothicVampire.Villagers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GothicVampire.UI.Buildings
{
    public class BuildingJobAssignmentView : MonoBehaviour
    {
        [Header("Slots")] 
        [SerializeField] private TMP_Text _efficiencyText;
        [SerializeField] private Image _efficiencyFill;
        [SerializeField] private JobSlotView _slotTemplate;
        [SerializeField] private List<JobAssignmentState> _slotSortOrder = new();
        
        [Header("Buttons")]
        [SerializeField] private Button _incrementButton;
        [SerializeField] private Button _decrementButton;
        [SerializeField] private Button _assignAllButton;
        [SerializeField] private Button _unassignAllButton;
        
        private BuildingJobAssignment _assignment;
        private VillagerManager _villagerManager;
        private readonly List<JobSlotView> _slots = new();
        private bool _wasUpdated;

        private void Awake()
        {
            if (!_slotTemplate.gameObject.scene.IsValid()) throw new Exception("Slot template must not be a prefab.");
            _slotTemplate.gameObject.SetActive(false);
        }

        public void Show(BuildingJobAssignment assignment)
        {
            _assignment = assignment;
            _villagerManager = _assignment.Building.Faction.GetService<VillagerManager>();
            
            UpdateDisplay(assignment);

            _assignment.EvtJobUpdated?.AddListener(OnJobUpdated);
            _villagerManager.EvtUpdated?.AddListener(OnVillagerManagerUpdated);
            
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _assignment.EvtJobUpdated?.RemoveListener(OnJobUpdated);
            _villagerManager.EvtUpdated?.RemoveListener(OnVillagerManagerUpdated);
        }

        private void UpdateDisplay(BuildingJobAssignment assignment)
        {
            _efficiencyText.text = assignment.AssignmentRate.ToString("P0");
            _efficiencyFill.fillAmount = assignment.AssignmentRate;
            
            _incrementButton.interactable = assignment.CanAssign;
            _decrementButton.interactable = assignment.CanUnassign;
            if (_assignAllButton) _assignAllButton.interactable = _incrementButton.interactable;
            if (_unassignAllButton) _unassignAllButton.interactable = _decrementButton.interactable;
            
            UpdateSlots();
        }

        private void UpdateSlots()
        {
            // Sort jobs based on state
            var jobs = _assignment.Jobs
                .Select(j => (Job: j, State: j.State))
                .OrderBy(j => _slotSortOrder.IndexOf(j.State) < 0) // Deprioritize states that weren't included
                .ThenBy(j => _slotSortOrder.IndexOf(j.State)) // Sort by the set list
                .ThenByDescending(j => j.Job.Locked) // Show locked first
                .Select(j => j.Job)
                .ToList();

            // Update job slot view
            _slots.ForEach(s => s.gameObject.SetActive(false));
            foreach (var job in jobs)
            {
                // Get or instantiate a slot
                var slot = _slots.FirstOrDefault(s => !s.gameObject.activeSelf);
                if (!slot)
                {
                    slot = Instantiate(_slotTemplate, _slotTemplate.transform.parent);
                    _slots.Add(slot);
                }
                slot.gameObject.SetActive(true);
                
                // Update view
                slot.Show(job);
            }
        }

        private void LateUpdate()
        {
            if (!_wasUpdated) return;
            
            UpdateDisplay(_assignment);
            _wasUpdated = false;
        }

        private void OnVillagerManagerUpdated() => _wasUpdated = true;
        private void OnJobUpdated(Job job) => _wasUpdated = true;

        public void Evt_IncrementPressed() => _assignment.Assign();
        public void Evt_DecrementPressed() => _assignment.Unassign();
        public void Evt_AssignAllPressed() => _assignment.AssignAll();
        public void Evt_UnassignAllPressed() => _assignment.UnassignAll();
    }
}