using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Config;

namespace CodedUITestProject1
{
    public static class Logger
    {
        private static readonly log4net.ILog logger;

        static Logger()
        {
            XmlConfigurator.Configure();
            logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        public static void log(string message)
        {
            logger.Debug(message);
        }
    }
}
