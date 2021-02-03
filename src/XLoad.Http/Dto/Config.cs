namespace XLoad.Http.Dto
{
    using Newtonsoft.Json.Converters;
    using System;
    using System.Text.Json.Serialization;


    public class Config
    {
        public Uri Uri { get; set; }
        public MethodEnum Method { get; set; }
        public string Body { get; set; }
        public int Concurrency { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum MethodEnum
        {
            GET,
            POST
        }
    }
}
