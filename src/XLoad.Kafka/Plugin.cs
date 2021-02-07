namespace XLoad.Kafka
{
    using Newtonsoft.Json;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Dto;
    using XLoad.Plugin;
    using Confluent.Kafka;
    using System.Linq;
    using System.Reflection;
    using System.IO;
    using System;

    public class Plugin : IPlugin
    {
        private Dto.Config Config = null;
        private IProducer<Null, string> Producer;

        public void Initialize(string configuration)
        {
            this.Config = JsonConvert.DeserializeObject<Dto.Config>(configuration);

            var producerConfig = new ProducerConfig()
            {
                BootstrapServers            = string.Join(',', this.Config.Nodes.Select(n => n.AbsoluteUri)),
                SaslMechanism               = SaslMechanism.ScramSha512,
                SecurityProtocol            = SecurityProtocol.SaslSsl,
                SaslUsername                = this.Config.Username,
                SaslPassword                = this.Config.Password,

                BatchSize                   = this.Config.MaxBatchBytes,
                BatchNumMessages            = this.Config.MaxBatchMessageCount,
                Acks                        = (Acks?)this.Config.AckType,
                LingerMs                    = this.Config.BatchLingerMs,
                MessageTimeoutMs            = this.Config.MessageTimeoutMs,
                MessageSendMaxRetries       = this.Config.MaxMessageRetries,
                QueueBufferingMaxMessages   = this.Config.MaxMessageQueueCount
            };

            var dllPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "librdkafka", 
                Environment.Is64BitProcess ? "x64" : "x86", 
                "librdkafka.dll"
            );
            
            Library.Load(dllPath);

            this.Producer = new ProducerBuilder<Null, string>(producerConfig).Build();

        }

        public Task ExecuteAsync()
        {
            this.Producer.Produce(this.Config.Topic, new Message<Null, string>() { Value = "test" });

            return Task.CompletedTask;
        }

        public void Clean()
        {
            this.Producer.Dispose();
            this.Config = null;
        }
    }
}
