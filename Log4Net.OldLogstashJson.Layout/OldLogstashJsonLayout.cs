using System;
using log4net.Core;
using log4net.Layout;
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

        public override void Format(System.IO.TextWriter writer, LoggingEvent loggingEvent)
        {
            var message = new JObject();

            AddBasicInfo(message, loggingEvent);

            AddMessageAndFields(message, loggingEvent);

            writer.Write(JObject.FromObject(message));
        }

        private void AddMessageAndFields(JObject message, LoggingEvent loggingEvent)
        {
            var exception = GetException(loggingEvent);
            if (exception != null)
            {
                if (IsStringOrFormattedMessage(loggingEvent))
                    message["@message"] = loggingEvent.RenderedMessage;
                else
                    message["@message"] = exception.Message;
                message["@fields"] = JObject.FromObject(exception);
            }
            else if (IsStringOrFormattedMessage(loggingEvent))
                message["@message"] = loggingEvent.RenderedMessage;
            else if (MessageHasFields(loggingEvent))
                message["@fields"] = JObject.FromObject(loggingEvent.MessageObject);
            else if (IsValueObject(loggingEvent))
                message["@fields"] = JObject.FromObject(new { value = loggingEvent.MessageObject });
        }

        private bool IsValueObject(LoggingEvent loggingEvent)
        {
            return loggingEvent.MessageObject != null;
        }

        private bool MessageHasFields(LoggingEvent loggingEvent)
        {
            return loggingEvent.MessageObject != null && !loggingEvent.MessageObject.GetType().IsValueType;
        }

        private bool IsStringOrFormattedMessage(LoggingEvent loggingEvent)
        {
            return loggingEvent.MessageObject is string || loggingEvent.MessageObject is SystemStringFormat;
        }

        private void AddBasicInfo(JObject message, LoggingEvent loggingEvent)
        {
            message["@timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
            message["@source_host"] = Environment.MachineName;
            message["@type"] = Type;
            message["@tags"] = new JArray(loggingEvent.LoggerName, loggingEvent.Level.ToString().ToLower());
        }

        private Exception GetException(LoggingEvent loggingEvent)
        {
            if (loggingEvent.ExceptionObject is Exception)
                return loggingEvent.ExceptionObject;

            if (loggingEvent.MessageObject is Exception)
                return (Exception)loggingEvent.MessageObject;

            return null;
        }
    }

}
