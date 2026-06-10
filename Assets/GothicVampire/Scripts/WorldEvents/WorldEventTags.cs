using System;
using UnityEngine;

namespace GothicVampire
{
    [CreateAssetMenu(menuName = "World Events/Tag", order = 0)]
    public class WorldEventTags : ScriptableObject
    {
        [SerializeField] private string _tag;
        public string DisplayName => _tag;
        
        // We can add icons, etc here later on.
    }
}
