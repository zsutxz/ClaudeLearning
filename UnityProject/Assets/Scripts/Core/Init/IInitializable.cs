namespace GomokuGame.Core.Init
{
    /// <summary>
    /// Implement on MonoBehaviours which require controlled initialization order.
    /// </summary>
    public interface IInitializable
    {
        /// <summary>
        /// Called by <see cref="InitManager"/> during startup in a deterministic order.
        /// </summary>
        void Initialize();
    }
}
