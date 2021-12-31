using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace EcomServiceStatusMonitor.Jobs
{
    public class JobService
    {
        public JobService()
        {

        }
        public void Start()
        {
            // Your logic
            JobManager.Initialize(new ServiceMonitorScheduleRegistry());
            //list other job here e.g.
            //JobManager.Initialize(new SecondJobScheduleRegistry());
        }

        public void Stop()
        {
            // when service is stopped, Execution control comes here
            JobManager.Stop();
        }
    }
}
