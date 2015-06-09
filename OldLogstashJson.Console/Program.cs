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
            log.WarnFormat("Hi There {0}", "Jeff");
            log.Warn(new { Hi = 2, Lo = "string", Complex = new { ComplicatedObject = true } });

            log.Error("Boom", GenerateException());
            log.Error(GenerateAggregateException());

            System.Console.ReadKey();
        }

        private static Exception GenerateException()
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

        private static Exception GenerateAggregateException()
        {
            try
            {
                Enumerable.Range(1, 3).AsParallel().ForAll(n =>
                {
                    // ReSharper disable PossibleNullReferenceException
                    throw null;
                    // ReSharper restore PossibleNullReferenceException
                });
            }
            catch (Exception ex)
            {
                return ex;
            }
            return null;
        }
    }
}
