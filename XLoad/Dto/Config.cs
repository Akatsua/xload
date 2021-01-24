namespace XLoad.Dto
{
    public class Config
    {
        // General
        public bool DryRun;

        // Noise
        public float Scale;
        public int Seed;
        public int Resolution;

        // Load
        public int Time;
        public int Requests;
        public bool Infinite;
        public int MaxTasks;
        public int MinTasks;

        // Image
        public string Image;

        // Computed Config
        public int NumTicks;
    }
}
