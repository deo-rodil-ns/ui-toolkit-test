using System;
using Random = UnityEngine.Random;

namespace GothicVampire.Villagers
{
    public struct VillagerIdentity : IEquatable<VillagerIdentity>
    {
        public string FirstName { get; private set; }
        public string Surname { get; private set; }
        public Gender Gender { get; private set; }
        public string Guid { get; private set; }
        
        public string FullName => $"{FirstName} {Surname}";

        public VillagerIdentity(string firstName, string surname, Gender gender)
        {
            Guid = System.Guid.NewGuid().ToString();
            FirstName = firstName;
            Surname = surname;
            Gender = gender;
        }

        public bool Equals(VillagerIdentity other)
        {
            return Guid == other.Guid;
        }

        public override bool Equals(object obj)
        {
            return obj is VillagerIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Guid != null ? Guid.GetHashCode() : 0);
        }
    }
    
    public enum Gender { Male, Female }
}