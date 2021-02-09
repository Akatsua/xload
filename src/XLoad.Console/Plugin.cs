namespace XLoad.Console
{
    using Newtonsoft.Json;
    using System;
    using System.Threading.Tasks;
    using XLoad.Console.Dto;
    using XLoad.Plugin;

    public class Plugin : IPlugin
    {
        private Config Config = null;

        public void Initialize(string configuration)
        {
            this.Config = JsonConvert.DeserializeObject<Config>(configuration);
        }
        
        public Task ExecuteAsync()
        {
            Console.WriteLine($"{this.Config.Label} | {this.Config.Body}");

            return Task.CompletedTask;
        }

        public void Clean()
        {
            this.Config = null;
        }

    }
}
