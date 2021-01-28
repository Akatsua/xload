namespace XLoad.Http
{
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using XLoad.Http.Dto;
    using XLoad.Plugin;

    public class Plugin : IPlugin
    {
        private Config       Config       = null;
        private HttpClient   HttpClient   = null;
        private ServicePoint ServicePoint = null;

        public void Initialize(IConfiguration configuration)
        {
            this.Config = configuration.Get<Config>();
            this.ServicePoint = ServicePointManager.FindServicePoint(Config.Uri);
            this.ServicePoint.ConnectionLimit = Config.Concurrency;
            this.HttpClient = new HttpClient();
        }

        public async Task ExecuteAsync()
        {
            if (this.Config.Method == Config.MethodEnum.GET)
            {
                await this.HttpClient.GetAsync(this.Config.Uri);
            }
            else if (this.Config.Method == Config.MethodEnum.POST)
            {
                await this.HttpClient.PostAsync(this.Config.Uri, new StringContent(this.Config.Body));
            }

            Console.WriteLine("I run");
        }

        public void Clean()
        {
            this.HttpClient.Dispose();
            this.ServicePoint = null;
            this.Config = null;
        }
    }
}
