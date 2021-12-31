using EcomServiceStatusMonitor.Logger;
using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcomServiceStatusMonitor.Jobs
{
    public class ServiceMonitorScheduleRegistry : Registry
    {
        public ServiceMonitorScheduleRegistry()
        {
            
            Schedule(() =>
            {
                new ServiceMonitor().GetCurrentProcessedQueue();
            })
            .NonReentrant() //doesn’t allows a schedule to run in parallel with a previously triggered execution of the same schedule
            .ToRunNow()    //to run the job without any delay
            .AndEvery(30)
            .Minutes();
        }
    }
}
