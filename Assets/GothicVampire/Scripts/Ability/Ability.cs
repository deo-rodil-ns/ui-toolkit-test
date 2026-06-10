using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Technologies;
using Sylpheed.Extensions;
using Sylpheed.Timers;

namespace GothicVampire.Abilities
{
    public sealed class Ability
    {
        public AbilityData Data { get; }
        public AbilityActor Actor { get; }
        public AbilityEffect Effect { get; }
        public IReadOnlyList<AbilityRequirement> Requirements { get; }
        public CooldownTimer Cooldown { get; }
        private TargetingScheme TargetingScheme { get; }
        public PrerequisiteGroup Prerequisites { get; }
        public AbilitySettings Settings => Actor.Settings;

        public bool CanUse
        {
            get
            {
                if (!IsUnlocked) return false;
                if (!Cooldown.Ready) return false;
                if (!Requirements.All(requirement => requirement.CanUse)) return false;
                
                return true;
            }
        }

        public bool IsUnlocked => Prerequisites?.Satisfied ?? true; // Always unlocked if there are no prerequisites
        
        public Ability(AbilityData data, AbilityActor actor)
        {
            Data = data;
            Actor = actor;
            
            // Create copy of requirements for run time use
            Requirements = data.Requirements.Select(template =>
            {
                var requirement = template.Clone();
                requirement.Initialize(this);
                return requirement;
            }).ToList();
            
            // Targeting scheme
            TargetingScheme = data.TargetingScheme.Clone();
            TargetingScheme.Initialize(this);
            
            // Create a copy of the effect for runtime use
            Effect = Data.Effect.Clone();
            Effect.Initialize(this);
            
            // Create copy of prerequisite group for runtime use. Only check if set.
            if (data.Prerequisites.Any())
            {
                if (!actor.Faction) throw new Exception("PrerequisiteGroup requires a Faction");
                var technologyManager = actor.Faction.GetService<TechnologyManager>();
                Prerequisites = technologyManager.RegisterPrerequisiteSource(data);
            }
            
            // Cooldown
            Cooldown = Data.Cooldown.Clone();
            Cooldown.Start();
        }
        
        /// <summary>
        /// Immediately activate the skill given a target. BeginTargeting() will call this once a target is selected.
        /// </summary>
        /// <param name="target"></param>
        public void Activate(ITargetable target)
        {
            // Try to consume requirements
            if (!HandleConsumeRequirements()) return;
            
            // Activate effect
            Effect.Activate(this, target);
        }
        
        /// <summary>
        /// Immediately activate the skill given multiple targets. BeginTargeting() will call this once a target is selected.
        /// </summary>
        /// <param name="targets"></param>
        public void Activate(IReadOnlyCollection<ITargetable> targets)
        {
            // Try to consume requirements
            if (!HandleConsumeRequirements()) return;
            
            // Activate effect
            Effect.Activate(this, targets);
        }

        /// <summary>
        /// Begin a TargetingScheme. Once a target is successfully selected, it will Activate() this ability.
        /// </summary>
        public void BeginTargeting()
        {
            if (!CanUse) return;
            TargetingScheme.Begin();
        }

        /// <summary>
        /// Consume all requirements (ability requirements, cooldown, etc.) needed to activate the ability.
        /// When consumption is set to targeting, TargetingScheme will call this internally.
        /// </summary>
        /// <returns></returns>
        public bool ConsumeRequirements()
        {
            if (!CanUse) return false;
            
            Cooldown.Start();
            Requirements.ForEach(r => r.Consume());
            
            return true;
        }

        private bool HandleConsumeRequirements()
        {
            // Do not consume if handled by targeting
            if (Data.ConsumeRequirementsOnTargeting) return true;
            
            // Try to consume requirements
            return ConsumeRequirements();
        }
    }
}