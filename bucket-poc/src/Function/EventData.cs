using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Function
{
    public class EventData
    {
        public string EventType { get; }
        public string Source { get; }
        public string BucketName { get; }

        public string RawData {get;}
        public EventData(string data)
        {
            RawData = data;
            try
            {
                var json= JsonDocument.Parse(data);
                EventType=json.RootElement.GetProperty("eventType").GetString();
                Source= json.RootElement.GetProperty("source").GetString();
                BucketName = json.RootElement.GetProperty("data")
                    .GetProperty("additionalDetails").GetProperty("bucketName").GetString();
            }
            catch
            {
            }
        }
    }
}
