namespace GothicVampire.Game
{
    /// <summary>
    /// Defines the object as a service/feature under a Faction
    /// </summary>
    public interface IFactionService
    {
        Faction Faction { get; set; }
        void OnFactionInitialize(Faction faction);
    }
}