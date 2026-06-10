namespace GothicVampire.Unrest
{
    /// <summary>
    /// Predict unrest sources that are yet to be resolved (not applied immediately).
    /// </summary>
    public interface IUnrestPredictor
    {
        void Predict(UnrestSnapshot snapshot);
    }
}