namespace XLoad.Dto
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

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

        public Request(DateTime runAt)
        {
            this.RunAt = runAt;
        }
    }
}
