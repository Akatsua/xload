namespace XLoad.Dto
{
    using global::XLoad.Helpers;
    using System.Threading;

    public class State
    {
        // System Wide
        public bool Shutdown { get; set; }
        public CancellationTokenSource CancelSource { get; set; }
        
        // Data Generation
        public Noise Noise { get; set; }

    }
}
