using GothicVampire.Technologies;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.UI.Technologies
{
    public abstract class PrerequisiteElementData
    {
        public PrerequisiteGroup Group { get; internal set; }
        public string Description { get; internal set; }
        public bool Satisfied { get; internal set; }
        public bool HasProgression { get; internal set; }
        public float CurrentProgress { get; internal set; }
        public float MaxProgress { get; internal set; }
        public float ProgressRate => Mathf.Clamp01(CurrentProgress / Mathf.Max(MaxProgress, 1));

        public UnityEvent<PrerequisiteElementData> EvtUpdated { get; } = new();
        
        protected virtual void OnRefresh() { }
    
        public static PrerequisiteElementData Create(IUnlockable unlockable, PrerequisiteGroup group) => new UnlockableElementData(unlockable, group);
        public static PrerequisiteElementData Create(TechnologyData technology, PrerequisiteGroup group) => new TechnologyElementData(technology, group);
        public static PrerequisiteElementData Create(Prerequisite prerequisite, PrerequisiteGroup group) => new CustomElementData(prerequisite, group);

        public PrerequisiteElementData(PrerequisiteGroup group)
        {
            Group = group;
            group.EvtProgressUpdated?.AddListener(OnUpdated);
        }
        
        ~PrerequisiteElementData()
        {
            Group?.EvtProgressUpdated?.RemoveListener(OnUpdated);
        }

        public void Refresh()
        {
            OnRefresh();
            EvtUpdated?.Invoke(this);
        }

        private void OnUpdated(PrerequisiteGroup arg0) => Refresh();
    }

    internal class UnlockableElementData : PrerequisiteElementData
    {
        public IUnlockable Unlockable { get; }
        
        public UnlockableElementData(IUnlockable unlockable, PrerequisiteGroup group) 
            : base(group)
        {
            Unlockable = unlockable;
            Refresh();
        }

        protected override void OnRefresh()
        {
            Description = Unlockable.Name;
            HasProgression = false;
            CurrentProgress = 0f;
            MaxProgress = 0f;
            Satisfied = Group.Faction.IsUnlocked(Unlockable);
        }
    }
    
    internal class TechnologyElementData : PrerequisiteElementData
    {
        public TechnologyData Technology { get; }
        
        public TechnologyElementData(TechnologyData technology, PrerequisiteGroup group)
            : base(group)
        {
            Technology = technology;
            Refresh();
        }

        protected override void OnRefresh()
        {
            Description = Technology.DisplayName;
            HasProgression = false;
            CurrentProgress = 0f;
            MaxProgress = 0f;
            Satisfied = Group.Faction.IsUnlocked(Technology);
        }
    }
    
    internal class CustomElementData : PrerequisiteElementData
    {
        public Prerequisite Prerequisite { get; }
        
        public CustomElementData(Prerequisite prerequisite, PrerequisiteGroup group)
            : base(group)
        {
            Prerequisite = prerequisite;
            Refresh();
        }

        protected override void OnRefresh()
        {
            Description = Prerequisite.Description;
            HasProgression = Prerequisite.HasProgression;
            CurrentProgress = Prerequisite.CurrentProgress;
            MaxProgress = Prerequisite.MaxProgress;
            Satisfied = Prerequisite.Satisfied;
        }
    }
}