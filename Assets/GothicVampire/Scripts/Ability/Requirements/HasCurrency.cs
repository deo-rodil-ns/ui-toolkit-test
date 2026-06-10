using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Currencies;
using UnityEngine;

namespace GothicVampire.Abilities.Requirements
{
    [System.Serializable]
    public sealed class HasCurrency : AbilityRequirement
    {
        [SerializeField] private Currency[] _currencies;
        
        public override IReadOnlyList<string> DescriptionList => _currencies.Select(c => c.ToFormattedString()).ToArray();

        protected override bool OnCanUse(Ability ability)
        {
            var wallet = ability.Actor.Faction?.GetService<Wallet>() ?? throw new Exception($"Requires {nameof(Wallet)}");
            return wallet.HasEnough(_currencies);
        }

        protected override void OnConsume(Ability ability)
        {
            var wallet = ability.Actor.Faction?.GetService<Wallet>() ?? throw new Exception($"Requires {nameof(Wallet)}");
            wallet.Deduct(_currencies);
        }
    }
}