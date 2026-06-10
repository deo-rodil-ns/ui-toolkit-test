using UnityEngine;

namespace GothicVampire.Unrest
{
    [CreateAssetMenu(menuName = "Unrest/Category", order = 1)]
    public class UnrestCategory : ScriptableObject
    {
        [SerializeField] [TextArea] private string _shortDescription;
        
        public string Id => name;
        public string ShortDescription => _shortDescription;
    }
}