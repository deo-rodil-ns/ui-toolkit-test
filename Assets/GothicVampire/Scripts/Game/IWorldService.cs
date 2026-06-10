namespace GothicVampire.Game
{
    /// <summary>
    /// Defines the object as a service/feature under a World
    /// </summary>
    public interface IWorldService
    {
        World World { get; set; }
        void OnWorldInitialize(World world);
    }
}