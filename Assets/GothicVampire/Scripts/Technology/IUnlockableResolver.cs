namespace GothicVampire.Technologies
{
    public interface IUnlockableResolver
    {
        bool IsUnlocked(IUnlockable unlockable);
    }
}