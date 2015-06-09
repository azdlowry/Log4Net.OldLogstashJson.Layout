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
using log4net.Util;

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
            message["@tags"] = new JArray(loggingEvent.LoggerName, loggingEvent.Level.ToString().ToLower());

            var exception = GetException(loggingEvent);
            if (exception != null)
            {
                if (loggingEvent.MessageObject is string || loggingEvent.MessageObject is SystemStringFormat)
                {
                    message["@message"] = loggingEvent.RenderedMessage;
                }
                else
                {
                    message["@message"] = exception.Message;
                }
                message["@fields"] = JObject.FromObject(exception);
            }
            else if (loggingEvent.MessageObject is string || loggingEvent.MessageObject is SystemStringFormat)
            {
                message["@message"] = loggingEvent.RenderedMessage;
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

        private Exception GetException(log4net.Core.LoggingEvent loggingEvent)
        {
            if (loggingEvent.ExceptionObject is Exception)
                return loggingEvent.ExceptionObject;

            if (loggingEvent.MessageObject is Exception)
                return (Exception)loggingEvent.MessageObject;

            return null;
        }
    }
}
