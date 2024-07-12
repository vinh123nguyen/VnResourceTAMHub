using Microsoft.Extensions.DependencyInjection;
using Quartz;
using TAMHubApp.Services;
using TAMHubApp.Services.TAMServices;

namespace TAMHubApp.ScheduleTasks.Jobs
{
    public class GetAndSyncDataHRMJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var dw = context.JobDetail.Key.Name;  // job__idjob
            var scheduletaskService = ServiceLocator.Instance.GetRequiredService<ScheduleTaskService>();
            var TAMSInstance = ServiceLocator.Instance.GetRequiredService<SingletonTAMInstance>();
            await scheduletaskService.UpdateScheduleRuningStatus();

            //TAMSInstance.TAMs



        }
    }
}
