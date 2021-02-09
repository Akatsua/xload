namespace XLoad.Kafka.Dto
{
    using Newtonsoft.Json.Converters;
    using System;
    using System.Text.Json.Serialization;

    public class Config
    {
        public Uri[] Nodes { get; set; }
        public string Topic { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        // Producer Config
        public int? MaxBatchBytes { get; set; }
        public int? MaxBatchMessageCount { get; set; }
        public int? BatchLingerMs { get; set; }

        public int? MaxMessageRetries { get; set; }
        public int? MaxMessageQueueCount { get; set; }
        public int? MessageTimeoutMs { get; set; }

        public Acks? AckType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Acks
        {
            All = -1,
            None = 0,
            Leader = 1
        }
    }
}
