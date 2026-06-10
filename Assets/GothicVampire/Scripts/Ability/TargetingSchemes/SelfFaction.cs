using System;
using System.Collections.Generic;
using UnityEngine;

namespace GothicVampire.Abilities.TargetingSchemes
{
    [System.Serializable]
    public sealed class SelfFaction : TargetingScheme
    {
        protected override void OnBegin(Ability ability)
        {
            var target = Ability.Actor.Faction ?? throw new Exception($"Requires {nameof(AbilityActor)}.{nameof(AbilityActor.Faction)}");
            Activate(target);
        }

        protected override IReadOnlyList<ITargetable> OnPeekTargets()
        {
            return new[] { Ability.Actor.Faction ?? throw new Exception($"Requires {nameof(AbilityActor)}.{nameof(AbilityActor.Faction)}") };
        }
    }
}