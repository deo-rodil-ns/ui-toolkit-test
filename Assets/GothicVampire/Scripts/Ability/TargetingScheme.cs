using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GothicVampire.Game;
using Sylpheed.Core;

namespace GothicVampire.Abilities
{
    [System.Serializable]
    public abstract class TargetingScheme
    {
        public Ability Ability { get; private set; }
        public World World { get; private set; }
        
        public bool Active { get; private set; }
        
        protected abstract void OnBegin(Ability ability);
        protected virtual void OnEnd(Ability ability) { }
        protected virtual IReadOnlyList<ITargetable> OnPeekTargets() => Array.Empty<ITargetable>();

        private bool _initialized;
        
        public void Initialize(Ability ability)
        {
            if (_initialized) throw new Exception("Already initialized");
            _initialized = true;
            
            Ability = ability;
            World = ServiceLocator.Get<World>() ?? throw new Exception("World not found.");
        }

        public void Begin()
        {
            if (!_initialized) throw new Exception("Not initialized");
            if (Active) throw new Exception("Already active");
            
            Active = true;
            OnBegin(Ability);
        }

        public void End()
        {
            if (!_initialized) throw new Exception("Not initialized");
            
            Active = false;
            OnEnd(Ability);
        }
        
        public ITargetable PeekTarget()
        {
            return PeekTargets().FirstOrDefault();
        }

        /// <summary>
        /// Check for available targets without execution. This is not applicable to targeting schemes that require user input (telegraphed aoe, UI selection).
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<ITargetable> PeekTargets()
        {
            return OnPeekTargets();
        }

        /// <summary>
        /// Activates the ability with the selected target. This ends the targeting.
        /// </summary>
        /// <param name="target"></param>
        protected void Activate(ITargetable target)
        {
            // Handle requirement consumption if delegated to targeting
            if (!HandleRequirementConsumption()) return;
            
            Ability.Activate(target);
            End();
        }

        /// <summary>
        /// Activates the ability against selected targets. This ends the targeting.
        /// </summary>
        /// <param name="targets"></param>
        protected void Activate(IReadOnlyList<ITargetable> targets)
        {
            // Handle requirement consumption if delegated to targeting
            if (!HandleRequirementConsumption()) return;
            
            Ability.Activate(targets);
            End();
        }

        private bool HandleRequirementConsumption()
        {
            // Skip if it's handled  by the ability
            if (!Ability.Data.ConsumeRequirementsOnTargeting) return true;

            return Ability.ConsumeRequirements();
        }
    }

    public interface ITargetable
    {
        
    }
}