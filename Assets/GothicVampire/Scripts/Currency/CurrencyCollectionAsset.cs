using System.Collections.Generic;
using UnityEngine;

namespace GothicVampire.Currencies
{
    /// <summary>
    /// Use this for templating a list of Currencies (with values).
    /// </summary>
    [CreateAssetMenu(menuName = "Currency/Collection")]
    public sealed class CurrencyCollectionAsset : ScriptableObject
    {
        [SerializeField] private Currency[] _currencies;
        
        public IReadOnlyCollection<Currency> Currencies => _currencies;
    }
}