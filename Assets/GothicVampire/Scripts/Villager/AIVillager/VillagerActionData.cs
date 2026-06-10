using System.Collections.Generic;
using UnityEngine;

namespace GothicVampire.Villagers.Actions
{
    [CreateAssetMenu(menuName = "Villager/ActionData", order = 0)]
    public class VillagerActionData : ScriptableObject
    {
        public List<VillagerAction> actions { 
            get 
            {
                var list = new List<VillagerAction>();

                list.Add(Rest);

                return list;
            } 
        }

        public VillagerRest Rest;
    }
}
