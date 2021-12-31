using EcomServiceStatusMonitor.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace EcomServiceStatusMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            var hostFactory = HostFactory.Run(hostConfigurator =>
            {
                hostConfigurator.Service<JobService>(serviceConfigurator =>
                {
                    serviceConfigurator.ConstructUsing(() => new JobService());

                    serviceConfigurator.WhenStarted(service => service.Start());
                    serviceConfigurator.WhenStopped(service => service.Stop());
                });

                hostConfigurator.RunAsLocalSystem();

                hostConfigurator.SetServiceName("DemoWindowsService");
                hostConfigurator.SetDisplayName("Demo Windows Service");
                hostConfigurator.SetDescription("This is demo windows service");
            });

            Environment.ExitCode = (int)Convert.ChangeType(hostFactory, hostFactory.GetTypeCode());
        }
    }
}
