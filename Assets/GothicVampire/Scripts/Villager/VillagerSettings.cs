using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Unrest;
using Sylpheed.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GothicVampire.Villagers
{
    [CreateAssetMenu(menuName = "Villager/Settings", order = int.MaxValue)]
    public sealed class VillagerSettings : ScriptableObject
    {
        [SerializeField] private Villager _villagerPrefab;
        [SerializeField] private VillagerData[] _villagerTiers;

        [Header("Names")] 
        [SerializeField] private TextAsset _maleNames;
        [SerializeField] private TextAsset _femaleNames;
        [SerializeField] private TextAsset _surnames;
        
        [Header("Initial Villagers")]
        [SerializeField] private int _numInitialVillagers;
        [SerializeField] private VillagerData _initialVillager;

        [Header("Unrest")] 
        [SerializeField] private UnrestSource _paidUpkeepUnrest;
        [SerializeField] private UnrestSource _unpaidUpkeepUnrest;
        
        public Villager VillagerPrefab => _villagerPrefab;
        public IReadOnlyCollection<VillagerData> Villagers => _villagerTiers;
        public int NumInitialVillagers => _numInitialVillagers;
        public VillagerData InitialVillager => _initialVillager;
        
        public UnrestSource UnpaidUpkeepUnrest => _unpaidUpkeepUnrest;
        public UnrestSource PaidUpkeepUnrest => _paidUpkeepUnrest;
        
        public IReadOnlyCollection<string> MaleNames { get; private set; }
        public IReadOnlyCollection<string> FemaleNames { get; private set; }
        public IReadOnlyCollection<string> Surnames { get; private set; }

        private void OnEnable()
        {
            MaleNames = ParseNames(_maleNames.text);
            FemaleNames = ParseNames(_femaleNames.text);
            Surnames = ParseNames(_surnames.text);
        }

        #region Name Generation
        public VillagerIdentity GenerateIdentity()
        {
            var gender = Random.value < 0.5 ? Gender.Male : Gender.Female;
            return GenerateIdentity(gender);
        }

        public VillagerIdentity GenerateIdentity(Gender gender)
        {
            var firstName = (gender switch
            {
                Gender.Male => MaleNames,
                Gender.Female => FemaleNames,
                _ => throw new ArgumentOutOfRangeException(nameof(gender), gender, null)
            }).RandomOrDefault();
            
            return new VillagerIdentity(firstName, Surnames.RandomOrDefault(), gender);
        }
        
        private static IReadOnlyCollection<string> ParseNames(string text)
        {
            return text.Split('\n')
                .Select(n => n.Trim('\r').Trim('\n'))
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .ToList();
        }
        #endregion
    }
}