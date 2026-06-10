using GothicVampire.Game;
using UnityEngine;

namespace GothicVampire.Technologies
{
    [System.Serializable]
    public abstract class TechnologyEffect
    {
        [Tooltip("Tokenized description. Leave empty if description is already generated completely from code.")]
        [SerializeField, TextArea] private string _descriptionTemplate;
        
        public bool Active { get; private set; }
        public Faction Faction { get; private set; }
        public string Description => OnBuildDescription(_descriptionTemplate);
        
        protected virtual void OnActivate(Faction faction) { }
        protected virtual void OnDeactivate(Faction faction) { }
        protected virtual void OnUpdate(float dt) { }
        protected virtual string OnBuildDescription(string template) => _descriptionTemplate;
        
        public void Activate(Faction faction)
        {
            if (Active) return;
            Active = true;
            Faction = faction;
            
            OnActivate(faction);
        }

        public void Deactivate()
        {
            if (!Active) return;
            Active = false;
            
            OnDeactivate(Faction);
        }

        public void Update(float dt)
        {
            if (!Active) return;
            OnUpdate(dt);
        }
    }
}