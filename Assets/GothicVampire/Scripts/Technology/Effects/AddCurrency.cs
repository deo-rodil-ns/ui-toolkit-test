using GothicVampire.Currencies;
using GothicVampire.Game;
using UnityEngine;

namespace GothicVampire.Technologies.Effects
{
    [System.Serializable]
    public class AddCurrency : TechnologyEffect
    {
        [SerializeField] private Currency _currency;

        protected override void OnActivate(Faction faction)
        {
            var wallet = faction.GetService<Wallet>();
            if (!wallet) return;
            
            wallet.Add(_currency);
        }

        protected override string OnBuildDescription(string template)
        {
            return _currency.ToFormattedString();
        }
    }
}