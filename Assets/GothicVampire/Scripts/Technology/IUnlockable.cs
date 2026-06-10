using GothicVampire.Game;

namespace GothicVampire.Technologies
{
    public interface IUnlockable
    {
        string Name { get; }
        string Description { get; }
        
        public bool IsUnlocked(IUnlockableResolver resolver) => resolver.IsUnlocked(this);
        public bool IsUnlocked(Faction faction) => faction.IsUnlocked(this);
    }
}