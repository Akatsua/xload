namespace XLoad.Dto
{
    using System.Collections.Generic;
    using System.Threading;
    using External;
    using Plugin;

    public class State
    {
        // System Wide
        public bool Shutdown { get; set; }
        public CancellationTokenSource CancelSource { get; set; }
        
        // Data Generation
        public Noise Noise { get; set; }
        public int NumTicks { get; set; }

        // Plugins
        public List<IPlugin> Plugins { get; set; }

    }
}
