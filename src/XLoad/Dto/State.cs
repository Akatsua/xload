namespace XLoad.Dto
{
    using System.Threading;
    using External;

    public class State
    {
        // System Wide
        public bool Shutdown { get; set; }
        public CancellationTokenSource CancelSource { get; set; }
        
        // Data Generation
        public Noise Noise { get; set; }
        public int NumTicks { get; set; }

    }
}
