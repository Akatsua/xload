namespace XLoad.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Plugin;

    public class RequestTask
    {
        public Task Task { get; set; }
        public CancellationTokenSource Token { get; set; }

        public RequestTask(Task task, CancellationTokenSource token)
        {
            this.Task = task;
            this.Token = token;
        }
    }

    public class Request
    {
        public DateTime RunAt { get; set; }

        public List<IPlugin> Plugins { get; set; }

        public Request(DateTime runAt, List<IPlugin> plugins)
        {
            this.RunAt = runAt;
            this.Plugins = plugins;
        }
    }
}
