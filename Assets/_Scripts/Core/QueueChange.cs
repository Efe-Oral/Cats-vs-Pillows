namespace CatsVsPillows.Core
{
    /// <summary>Why the pillow queue changed, so views can animate each case differently.</summary>
    public enum QueueChange
    {
        /// <summary>The queue was (re)filled from scratch.</summary>
        Filled,

        /// <summary>The current pillow was used; every pillow moved one step forward.</summary>
        Advanced,

        /// <summary>The current pillow was swapped because it no longer fit anywhere.</summary>
        Rerolled
    }
}
