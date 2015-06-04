using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Layout;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Log4Net.OldLogstashJson.Layout
{
    public class OldLogstashJsonLayout : LayoutSkeleton
    {
        public string Type { get; set; }

        public OldLogstashJsonLayout()
        {
            IgnoresException = false;
        }

        public override void ActivateOptions()
        {
        }

        public override void Format(System.IO.TextWriter writer, log4net.Core.LoggingEvent loggingEvent)
        {
            var message = new JObject();
            message["@timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
            message["@source_host"] = Environment.MachineName;
            message["@type"] = this.Type;
            message["@tags"] = new JArray(loggingEvent.LoggerName);
            if (loggingEvent.ExceptionObject != null)
            {
                message["@message"] = loggingEvent.ExceptionObject.Message;
                message["@fields"] = JObject.FromObject(new { stack = loggingEvent.ExceptionObject.StackTrace });
            }
            else if (loggingEvent.MessageObject is string)
            {
                message["@message"] = (string)loggingEvent.MessageObject;
            }
            else if (loggingEvent.MessageObject != null && !loggingEvent.MessageObject.GetType().IsValueType)
            {
                message["@fields"] = JObject.FromObject(loggingEvent.MessageObject);
            }
            else if (loggingEvent.MessageObject != null)
            {
                message["@fields"] = JObject.FromObject(new { value = loggingEvent.MessageObject });
            }

            writer.Write(JObject.FromObject(message));
        }
    }
}
