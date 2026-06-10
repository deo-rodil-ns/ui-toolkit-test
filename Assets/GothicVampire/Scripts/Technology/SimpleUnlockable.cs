using System;
using GothicVampire.Game;
using UnityEngine;

namespace GothicVampire.Technologies
{
    [CreateAssetMenu(menuName = "Technology/Simple Unlockable", order = 1)]
    public sealed class SimpleUnlockable : ScriptableObject, IUnlockable
    {
        [SerializeField] private string _displayName;
        [SerializeField, TextArea] private string _description;

        public string Name => _displayName;
        public string Description => _description;
        
        public bool IsUnlocked(IUnlockableResolver resolver) => resolver.IsUnlocked(this);
        public bool IsUnlocked(Faction faction) => faction.IsUnlocked(this);
    }
}