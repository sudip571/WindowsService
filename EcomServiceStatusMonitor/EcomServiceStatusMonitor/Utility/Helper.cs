using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcomServiceStatusMonitor.Utility
{
    public static class Helper
    {
        public static  string FilePathSetUp(string fullPath = "")
        {
            if(string.IsNullOrWhiteSpace(fullPath))
                fullPath = @"C:\FlightdeckLog\ServiceMonitorLog\Log.txt";
            try
            {
                var fileInfo = new FileInfo(fullPath);
                if (!fileInfo.Directory.Exists)
                    fileInfo.Directory.Create();
                if (!File.Exists(fullPath))
                    File.Create(fullPath).Close();

            }
            catch (Exception ex)
            { 
                
            }
            return fullPath;
        }
    }
}
