using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldLogstashJson.Console
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
                (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            log.Warn("Hi There");
            log.Warn(new { Hi = 2, Lo = "string", Complex = new { ComplicatedObject = true } });

            System.Console.ReadKey();
        }
    }
}
