namespace XLoad.Plugin
{
    using System.Threading.Tasks;

    public interface IPlugin
    {
        public void Initialize(string configuration);

        public Task ExecuteAsync();

        public void Clean();
    }
}
