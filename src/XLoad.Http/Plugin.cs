namespace XLoad.Http
{
    using Newtonsoft.Json;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Dto;
    using XLoad.Plugin;

    public class Plugin : IPlugin
    {
        private Config       Config       = null;
        private HttpClient   HttpClient   = null;
        private ServicePoint ServicePoint = null;

        public void Initialize(string configuration)
        {
            this.Config = JsonConvert.DeserializeObject<Config>(configuration);
            this.ServicePoint = ServicePointManager.FindServicePoint(Config.Uri);
            this.ServicePoint.ConnectionLimit = Config.Concurrency;
            this.HttpClient = new HttpClient();
        }

        public async Task ExecuteAsync()
        {
            if (this.Config.Method == Config.MethodEnum.GET)
            {
                var test = await this.HttpClient.GetAsync(this.Config.Uri);
            }
            else if (this.Config.Method == Config.MethodEnum.POST)
            {
                await this.HttpClient.PostAsync(this.Config.Uri, new StringContent(this.Config.Body));
            }
        }

        public void Clean()
        {
            this.HttpClient.Dispose();
            this.ServicePoint = null;
            this.Config = null;
        }
    }
}
