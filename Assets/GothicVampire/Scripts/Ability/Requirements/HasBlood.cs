using System.Collections.Generic;
using GothicVampire.Currencies;
using UnityEngine;

namespace GothicVampire.Abilities.Requirements
{
    [System.Serializable]
    public sealed class HasBlood : AbilityRequirement
    {
        [SerializeField] private float _value;
        
        private Wallet _wallet;
        private Currency _currency;

        public override IReadOnlyList<string> DescriptionList => new []{ _currency.ToFormattedString() };

        protected override void OnInitialize(Ability ability)
        {
            _wallet = ability.Actor.Faction.GetService<Wallet>();
            _currency = Ability.Settings.Blood.CreateCurrency(_value);
        }

        protected override bool OnCanUse(Ability ability)
        {
            return _wallet.HasEnough(_currency);
        }

        protected override void OnConsume(Ability ability)
        {
            _wallet.Deduct(_currency);
        }
    }
}