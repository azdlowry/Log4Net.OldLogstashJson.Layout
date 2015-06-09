using System;
using System.Linq;
using System.IO;
using System.Text;
using log4net.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Log4Net.OldLogstashJson.Layout;
using NUnit.Framework;
using log4net.Layout;

namespace OldLogstashJson.Tests
{
    [TestFixture]
    public class OldLogstashJsonLayoutTests
    {
        [Test]
        public void AddsTimestamp()
        {
            var layout = new OldLogstashJsonLayout();

            var output = SendEvent(layout, CreateLoggingEvent(null, null));

            Assert.IsTrue(DateTime.ParseExact((string)output["@timestamp"], "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture) <= DateTime.Now);
        }

        [Test]
        public void AddsHostname()
        {
            var layout = new OldLogstashJsonLayout();

            var output = SendEvent(layout, CreateLoggingEvent(null, null));

            Assert.AreEqual((string)output["@source_host"], Environment.MachineName);
        }

        [Test]
        public void AddsLoggerNameAsTag()
        {
            var layout = new OldLogstashJsonLayout();

            var output = SendEvent(layout, CreateLoggingEvent(null, null));

            var tags = output["@tags"].Select(obj => (string)obj);
            Assert.That(tags, Has.Member("mylogger"));
        }

        [Test]
        public void AddsTypeAsConfigured()
        {
            var layout = new OldLogstashJsonLayout { Type = "my_logging" };

            var output = SendEvent(layout, CreateLoggingEvent(null, null));

            Assert.AreEqual((string)output["@type"], "my_logging");
        }

        [Test]
        public void AddsMessageIfString()
        {
            var layout = new OldLogstashJsonLayout();

            var output = SendEvent(layout, CreateLoggingEvent("Banana", null));

            Assert.AreEqual((string)output["@message"], "Banana");
        }

        [Test]
        public void AddsFormattedMessageIfString()
        {
            var layout = new OldLogstashJsonLayout();

            var message = new log4net.Util.SystemStringFormat(CultureInfo.CurrentCulture, "Blah {0}", "Jeff");

            var output = SendEvent(layout, CreateLoggingEvent(message, null));

            Assert.AreEqual((string)output["@message"], "Blah Jeff");
        }

        [Test]
        public void AddsFeildsIfObject()
        {
            var layout = new OldLogstashJsonLayout();

            var obj = new { Hi = 2, Lo = "string", Complex = new { ComplicatedObject = true } };

            var output = SendEvent(layout, CreateLoggingEvent(obj, null));

            Assert.AreEqual((int)output["@fields"]["Hi"], obj.Hi);
            Assert.AreEqual((string)output["@fields"]["Lo"], obj.Lo);
            Assert.AreEqual((bool)output["@fields"]["Complex"]["ComplicatedObject"], obj.Complex.ComplicatedObject);
        }

        [Test]
        public void AddsValueIfValueType()
        {
            var layout = new OldLogstashJsonLayout();

            const int obj = 1;
            var output = SendEvent(layout, CreateLoggingEvent(obj, null));

            Assert.AreEqual((int)output["@fields"]["value"], obj);
        }

        [Test]
        public void AddsExceptionIfSpecified()
        {
            var layout = new OldLogstashJsonLayout();

            var exception = GenerateException();

            var output = SendEvent(layout, CreateLoggingEvent("error", exception));

            Assert.AreEqual((string)output["@fields"]["Message"], exception.Message);
            Assert.AreEqual((string)output["@fields"]["StackTraceString"], exception.StackTrace);
        }

        [Test]
        public void AddsMessageIfExceptionIsSpecified()
        {
            var layout = new OldLogstashJsonLayout();

            var exception = GenerateException();

            var output = SendEvent(layout, CreateLoggingEvent("mymessage", exception));

            Assert.AreEqual((string)output["@message"], "mymessage");
            Assert.AreEqual((string)output["@fields"]["StackTraceString"], exception.StackTrace);
        }

        [Test]
        public void AddsExceptionMessageIfExceptionIsSpecifiedButMessageIsNot()
        {
            var layout = new OldLogstashJsonLayout();

            var exception = GenerateException();

            var output = SendEvent(layout, CreateLoggingEvent(exception, null));

            Assert.AreEqual((string)output["@message"], exception.Message);
            Assert.AreEqual((string)output["@fields"]["StackTraceString"], exception.StackTrace);
        }

        [Test]
        public void AddsExceptionDataIfSpecified()
        {
            var layout = new OldLogstashJsonLayout();

            var exception = GenerateException();

            exception.Data.Add("blah", "hello");

            var output = SendEvent(layout, CreateLoggingEvent("error", exception));

            Assert.AreEqual((string)output["@fields"]["Data"]["blah"], "hello");
        }

        [Test]
        public void AddsExceptionType()
        {
            var layout = new OldLogstashJsonLayout();

            var exception = GenerateException();

            exception.Data.Add("blah", "hello");

            var output = SendEvent(layout, CreateLoggingEvent("error", exception));

            Assert.AreEqual((string)output["@fields"]["ClassName"], exception.GetType().ToString());
        }

        [Test]
        public void AddsLogLevelAsTag()
        {
            var layout = new OldLogstashJsonLayout();

            var output = SendEvent(layout, CreateLoggingEvent("some info", null));

            var tags = output["@tags"].Select(obj => (string)obj);
            Assert.That(tags, Has.Member("info"));
        }

        private static LoggingEvent CreateLoggingEvent(object message, Exception exception)
        {
            return new LoggingEvent(typeof(OldLogstashJsonLayout), null, "mylogger", Level.Info, message, exception);
        }


        private JObject SendEvent(ILayout layout, LoggingEvent ev)
        {
            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder))
            {
                layout.Format(writer, ev);
            }
            JsonReader reader = new JsonTextReader(new StringReader(builder.ToString()));
            reader.DateParseHandling = DateParseHandling.None;
            return JObject.Load(reader);
        }

        private Exception GenerateException()
        {
            try
            {
                // ReSharper disable PossibleNullReferenceException
                throw null;
                // ReSharper restore PossibleNullReferenceException
            }
            catch (Exception ex)
            {
                return ex;
            }
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