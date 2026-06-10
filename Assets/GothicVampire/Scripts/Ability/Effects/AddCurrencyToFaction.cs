using System;
using GothicVampire.Currencies;
using GothicVampire.Game;
using UnityEngine;

namespace GothicVampire.Abilities.Effects
{
    [System.Serializable]
    public sealed class AddCurrencyToFaction : AbilityEffect
    {
        [SerializeField] private Currency[] _currencies;

        protected override void OnActivate(Ability ability, ITargetable target)
        {
            if (target is not Faction faction) throw new Exception("Invalid target");
            var wallet = faction.GetService<Wallet>() ?? throw new Exception("No Wallet");

            wallet.Add(_currencies);

            Log($"Added {_currencies.FormatToString()}");
        }
    }
}