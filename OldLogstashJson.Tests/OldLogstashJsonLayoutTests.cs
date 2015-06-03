using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using log4net.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OldLogstashJson.Tests
{
    [TestClass]
    public class OldLogstashJsonLayout
    {
        [TestMethod]
        public void AddsTimestamp()
        {
            var layout = new Log4Net.OldLogstashJson.Layout.OldLogstashJsonLayout();

            layout.ActivateOptions();

            var output = SendEvent(layout, new log4net.Core.LoggingEvent(typeof(OldLogstashJsonLayout), null, "mylogger", Level.Info, null, null));

            Assert.IsTrue(DateTime.Parse((string)output["@timestamp"]) <= DateTime.Now);
        }

        [TestMethod]
        public void AddsHostname()
        {
            var layout = new Log4Net.OldLogstashJson.Layout.OldLogstashJsonLayout();

            layout.ActivateOptions();

            var output = SendEvent(layout, new log4net.Core.LoggingEvent(typeof(OldLogstashJsonLayout), null, "mylogger", Level.Info, null, null));

            Assert.AreEqual((string)output["@source_host"], Environment.MachineName);
        }

        [TestMethod]
        public void AddsLoggerNameAsTag()
        {
            var layout = new Log4Net.OldLogstashJson.Layout.OldLogstashJsonLayout();

            layout.ActivateOptions();

            var output = SendEvent(layout, new log4net.Core.LoggingEvent(typeof(OldLogstashJsonLayout), null, "mylogger", Level.Info, null, null));

            Assert.AreEqual((string)output["@tags"][0], "mylogger");
        }

        [TestMethod]
        public void AddsTypeAsConfigured()
        {
            var layout = new Log4Net.OldLogstashJson.Layout.OldLogstashJsonLayout();

            layout.Type = "my_logging";

            layout.ActivateOptions();

            var output = SendEvent(layout, new log4net.Core.LoggingEvent(typeof(OldLogstashJsonLayout), null, "mylogger", Level.Info, null, null));

            Assert.AreEqual((string)output["@type"], "my_logging");
        }

        [TestMethod]
        public void AddsMessageIfString()
        {
            var layout = new Log4Net.OldLogstashJson.Layout.OldLogstashJsonLayout();

            layout.ActivateOptions();

            var output = SendEvent(layout, new log4net.Core.LoggingEvent(typeof(OldLogstashJsonLayout), null, "mylogger", Level.Info, "Banana", null));

            Assert.AreEqual((string)output["@message"], "Banana");
        }

        [TestMethod]
        public void AddsFeildsIfObject()
        {
            var layout = new Log4Net.OldLogstashJson.Layout.OldLogstashJsonLayout();

            layout.ActivateOptions();

            var obj = new { Hi = 2, Lo = "string", Complex = new { ComplicatedObject = true } };

            var output = SendEvent(layout, new log4net.Core.LoggingEvent(typeof(OldLogstashJsonLayout), null, "mylogger", Level.Info, obj, null));

            Assert.AreEqual((int)output["@fields"]["Hi"], obj.Hi);
            Assert.AreEqual((string)output["@fields"]["Lo"], obj.Lo);
            Assert.AreEqual((bool)output["@fields"]["Complex"]["ComplicatedObject"], obj.Complex.ComplicatedObject);
        }

        [TestMethod]
        public void AddsValueIfValueType()
        {
            var layout = new Log4Net.OldLogstashJson.Layout.OldLogstashJsonLayout();

            layout.ActivateOptions();

            var obj = 1;

            var output = SendEvent(layout, new log4net.Core.LoggingEvent(typeof(OldLogstashJsonLayout), null, "mylogger", Level.Info, obj, null));

            Assert.AreEqual((int)output["@fields"]["value"], obj);
        }

        private JObject SendEvent(log4net.Layout.ILayout layout, log4net.Core.LoggingEvent ev)
        {
            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder))
            {
                layout.Format(writer, ev);
            }

            return JObject.Parse(builder.ToString());
        }
    }
}


/*{
    "@timestamp": "1970-01-01T00:00:00.000Z",
    "@message": "<log message>",
    "@source_host": "<host>",
    "@tags": ["fudge","donkey","macarena"],
    "@fields": {
        "timestamp": 0000000000000,
        "level": "<level>",
        "file": "<file>",
        "exception": {
            "exception_class": "<e_class>",
            "exception_message": "<e_msg>",
            "stacktrace": "<trace>"
        },
    }
}*/