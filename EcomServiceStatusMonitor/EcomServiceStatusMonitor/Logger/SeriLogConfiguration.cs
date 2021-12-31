using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcomServiceStatusMonitor.Logger
{
    public static class SeriLogConfiguration
    {
        private static readonly ILogger _logger;
        static SeriLogConfiguration()
        {
            _logger = new LoggerConfiguration()
                      .WriteTo.File(
                                  path:Utility.Helper.FilePathSetUp()
                                  //retainedFileTimeLimit: TimeSpan.FromDays(7), // clear all log after 7 days
                                    )
                      .CreateLogger();
        }
        public static void LogInfo(string error)
        {
            _logger.Information(error);
        }
        public static void LogError(string error)
        {
            _logger.Error(error);
        }
        public static void LogWarning(string error)
        {
            _logger.Warning(error);
        }
    }
}
