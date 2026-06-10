using System;
using System.Collections.Generic;
using UnityEngine;

namespace GothicVampire.Villagers
{
    // TODO: Convert to ScriptableObject 
    [CreateAssetMenu(menuName = "Villager/Needs", order = 0)]
    [Serializable]
    public class VillagerNeedData : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField][TextArea] private string _description;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Color _backgroundColor;
        [SerializeField] private List<VillagerNeedTag> _tags;

        public string Id => _id;
        public string DisplayName => _displayName;
        public string Description => _description;

        public Sprite Icon => _icon;
        public Color BackgroundColor => _backgroundColor;


        public List<VillagerNeedTag> Tags => _tags;
    }

}
