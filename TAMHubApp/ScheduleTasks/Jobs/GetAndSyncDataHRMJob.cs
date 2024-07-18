using Microsoft.Extensions.DependencyInjection;
using Quartz;
using TAMHubApp.Data;
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
            var unitOfWork = ServiceLocator.Instance.GetRequiredService<IUnitOfWork>();
            await scheduletaskService.UpdateScheduleRuningStatus();
            var TAMService = new TAMCommonService();
            
            foreach (var device in TAMSInstance.TAMs)
            {
                await Task.Run(async () =>
                {
                    var datestart = DateTime.MinValue;

                    var lastrecord = (await unitOfWork.TAMScanLogRepository.FindListAsync(x => x.DeviceID == device.Id)).ToList().OrderByDescending(x => x.TimeLog).FirstOrDefault();

                    if (lastrecord != null)
                    {
                        datestart = lastrecord.TimeLog;
                    }

                    await TAMService.Action_GetDataByDate(device, datestart, DateTime.Now, string.Empty, "Schedule", true);
                });
            }
        }
    }
}
