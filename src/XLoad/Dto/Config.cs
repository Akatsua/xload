namespace XLoad.Dto
{
    public class Config
    {
        public SystemConfig System { get; set; }
        public NoiseConfig Noise { get; set; }
        public LoadConfig Load { get; set; }
        public DiagnosticConfig Diagnostic { get; set; }

        public Config()
        {
            this.System     = new SystemConfig();
            this.Noise      = new NoiseConfig();
            this.Load       = new LoadConfig();
            this.Diagnostic = new DiagnosticConfig();
        }

        public class SystemConfig
        {
            public bool? DryRun { get; set; }
            public bool? NoStats { get; set; }
            public int? MaxTasks { get; set; }
            public int? MinTasks { get; set; }
            public PluginConfig[] Plugins { get; set; }
        }

        public class PluginConfig
        {
            public string Name { get; set; }

            public string Config { get; set; }
        }

        public class NoiseConfig
        {
            public float? Scale { get; set; }
            public int? Seed { get; set; }
            public int? Resolution { get; set; }
        }

        public class LoadConfig
        {
            public int? Time { get; set; }
            public int? Requests { get; set; }
            public bool? Infinite { get; set; }
        }

        public class DiagnosticConfig
        {
            public string Image { get; set; }
        }
    }
}
