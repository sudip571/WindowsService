using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.EntityClient;
using EcomServiceStatusMonitor.Logger;
using System.Data.SqlClient;
using Dapper;
using EcomServiceStatusMonitor.Utility;
using System.IO;
using System.ServiceProcess;
using System.Diagnostics;
using EcomServiceStatusMonitor.Email;

namespace EcomServiceStatusMonitor
{
     public class ServiceMonitor
    {
        int previousId = 0;
        int currentId = 0;        
        string queueIdPath = @"C:\FlightdeckLog\ServiceMonitorLog\QueueId.txt";
        string serviceName = ConfigurationManager.AppSettings["ServiceName"].ToString();
        string stop = "";
        string kill = "";
        string start = "";
        public void GetCurrentProcessedQueue()
        {
            try
            {
                var connectionString = GetConnectionString();
                using (var con = new SqlConnection(connectionString))
                {
                    var sqlQuery = @"SELECT ClientReportsId FROM web.ECommClientReportQueue where Status = 1";
                    con.Open();
                    // var queueIds = con.Query<int>(sqlQuery).ToList();
                    // currentId = 0 means there is no queue in 1 state
                    currentId = con.QueryFirstOrDefault<int>(sqlQuery);
                }

                if (currentId > 0)
                {
                    ReadFromFile();
                    // service is stuck
                    if(currentId == previousId)
                    {
                        StopService();
                        KillAllChrome();
                        StartService();
                        var sendTo = ConfigurationManager.AppSettings["EmailToSent"].Split(';').ToList();
                        new EmailService().SendEmail(sendTo, "Ecom Service Status", GetEmailTemplate());
                    }

                }
                WriteToFile();

            }
            catch (Exception ex)
            {
                SeriLogConfiguration.LogError(ex.Message);
            }
        }


        public void StopService()
        {
            try
            {
                ServiceController service = new ServiceController(serviceName);
                TimeSpan timeout = TimeSpan.FromSeconds(3);
                // check if service is running or about to run
                // if yes, stop the service
                if (service.Status.Equals(ServiceControllerStatus.Running) || service.Status.Equals(ServiceControllerStatus.StartPending))
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                }
                stop = @"service stop succeed <span style=""color:green"">&#x2714;</span>";
            }
            catch (Exception ex)
            {
                SeriLogConfiguration.LogError($"Error occured while Stopping service. Exception message is {ex.Message}");
                stop = @"service stop failed  <span style=""color:red"">&#x2718;</span>";
            }
           
        }
        public void StartService()
        {
            try
            {
                ServiceController service = new ServiceController(serviceName);
                TimeSpan timeout = TimeSpan.FromSeconds(3);
                // check if service is stopped or about to stop
                // if yes, start the service
                if (service.Status.Equals(ServiceControllerStatus.Stopped) || service.Status.Equals(ServiceControllerStatus.StopPending))
                {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                }
                start = @"service start succeed <span style=""color:green"">&#x2714;</span>";
            }
            catch (Exception ex)
            {
                SeriLogConfiguration.LogError($"Error occured while Starting service. Exception message is {ex.Message}");
                start = @"service start failed  <span style=""color:red"">&#x2718;</span>";
            }
        }
        public void KillAllChrome()
        {
            try
            {
                var processName = "chrome"; //msedge
                var allProcess = Process.GetProcesses();
                Process[] pr = Process.GetProcessesByName(processName);
                try
                {
                    foreach (var item in pr)
                    {
                        item.Kill();
                    }
                }
                catch (Exception ex)
                {
                    SeriLogConfiguration.LogError($"Error occured while Killing  Chromium SubProcess. Exception message is {ex.Message}");
                }
                Process[] pr1 = Process.GetProcessesByName(processName);
                if(pr.Length == pr1.Length)
                    kill = @"Killing Chromium in Task Manager failed  <span style=""color:red"">&#x2718;</span>";
                if (pr.Length > pr1.Length)
                    kill = @"Chromium in Task Manager could not be killed <span style=""color:yellow"">&#x26A0;</span>";
                if (pr1.Length == 0)
                    kill = @"All Chromium close succeed <span style=""color:green"">&#x2714;</span>";
            }
            catch (Exception ex)
            {
                SeriLogConfiguration.LogError($"Error occured while Killing  Chromium. Exception message is {ex.Message}");
            }
        }
        public void WriteToFile()
        {
            var fullPath = Helper.FilePathSetUp(queueIdPath);
            // save in key=value format
            using (var writer = new StreamWriter(fullPath, false))// false to overwrite data
            {
                var builder = new StringBuilder();
                builder.AppendLine(string.Format("QueueId={0}", currentId));
                writer.WriteLine(builder.ToString());
                writer.Close();
            }

        }

        public void ReadFromFile()
        {
            // data in file will be in  key=value format
            var fullPath = Helper.FilePathSetUp(queueIdPath);
            var queueIdString = "";
            var data = File.ReadAllLines(fullPath);
            if(data.Length > 0)
            {
                queueIdString = (data[0].Split('='))[1].Trim();
                int.TryParse(queueIdString, out previousId);
            }
        }

        //public Dictionary<string,string> ReadFromFile()
        //{
        //    // data in file will be in  key=value format
        //    var fullPath = Helper.FilePathSetUp(queueIdPath);
        //    var data = File.ReadAllLines(fullPath);
        //    var dict = data.Select(d => d.Split('=')).ToDictionary(a => a[0], a => a[1]);
        //    return dict;
        //}


        public string GetEmailTemplate()
        {
            var emailTemplate = @"<!DOCTYPE html>
                                        <html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
                                        <head>
                                        <meta charset = ""utf-8""/> 
                                        <title></title>
                                         </head>
                                         <body>
                                        <p>It seems the queue with <strong> ClientReportsId  {queueid} in table ECommClientReportQueue </strong> was stuck.</p>
                                        <p>Restart Attempt was made for the service named <strong>{serviceName}</strong> and details are given below.</p>
                                          <ul>
                                                <li>{stop}</li>
                                                <li>{kill}</li>
                                                <li>{start}</li>
                                           </ul>
                                            </body>
                                        </html>";
            emailTemplate = emailTemplate.Replace("{queueid}", currentId.ToString()).Replace("{serviceName}", serviceName).Replace("{start}", start).Replace("{kill}", kill).Replace("{stop}", stop);
            return emailTemplate;
        }
        public string GetConnectionString()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DBContext"].ConnectionString; ;
            //EF connectionString starts with 'metadata='
            if (connectionString.ToLower().StartsWith("metadata="))
            {
                EntityConnectionStringBuilder efBuilder = new EntityConnectionStringBuilder(connectionString);
                connectionString = efBuilder.ProviderConnectionString;
            }
            return connectionString;
        }
    }
}
