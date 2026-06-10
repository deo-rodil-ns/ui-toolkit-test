using GothicVampire.Currencies;
using GothicVampire.Game;
using UnityEngine;

namespace GothicVampire.Technologies.Prerequisites
{
    [System.Serializable]
    public class CurrencyReached : Prerequisite
    {
        [SerializeField] private CurrencyType _currencyType;
        
        private Wallet _wallet;

        protected override void OnInitialize(Faction faction)
        {
            _wallet = faction.GetService<Wallet>();
            
            _wallet.EvtUpdated.AddListener(OnWalletUpdated);
        }

        protected override void OnDestroy()
        {
            _wallet?.EvtUpdated.RemoveListener(OnWalletUpdated);
        }

        protected override string OnBuildDescription(string template)
        {
            return _currencyType.DisplayName;
        }

        protected override float OnResolveProgress()
        {
            return _wallet.GetValue(_currencyType);
        }

        private void OnWalletUpdated(Wallet wallet) => Resolve();
    }
}