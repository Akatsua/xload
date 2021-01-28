namespace XLoad.Plugin
{
    using Microsoft.Extensions.Configuration;
    using System.Threading.Tasks;

    public interface IPlugin
    {
        public void Initialize(IConfiguration configuration);

        public Task ExecuteAsync();

        public void Clean();
    }
}
