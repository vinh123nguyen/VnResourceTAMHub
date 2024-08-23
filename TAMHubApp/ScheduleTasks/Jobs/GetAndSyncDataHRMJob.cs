using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System.Globalization;
using TAMHubApp.Data;
using TAMHubApp.Infrastructures.Enums;
using TAMHubApp.Services;
using TAMHubApp.Services.TAMServices;

namespace TAMHubApp.ScheduleTasks.Jobs
{
    public class GetAndSyncDataHRMJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var schedulekey = context.JobDetail.Key.Name;  // job__idjob
            Log.Information($"[Start Schedule] + {DateTime.Now} + JobKey: {schedulekey}");
            var scheduletaskService = ServiceLocator.Instance.GetRequiredService<ScheduleTaskService>();
            var TAMSInstance = ServiceLocator.Instance.GetRequiredService<SingletonTAMInstance>();
            var unitOfWork = ServiceLocator.Instance.GetRequiredService<IUnitOfWork>();
            var scheduletasksevice = ServiceLocator.Instance.GetRequiredService<ScheduleTaskService>();
            await scheduletaskService.UpdateScheduleRuningStatus();

            var idstring = schedulekey.Split("__")[1];
            var scheduleDB = await unitOfWork.ScheduleTaskRepository.FindAsync( x => x.Id.ToString() == idstring);
            if(scheduleDB != null)
            {
                scheduleDB.LastTimeRun = DateTime.Now;
                await unitOfWork.SaveChanageAsync();
            }
            var scheduletask = scheduletasksevice.ScheduleTasks.FirstOrDefault(x => x.Id.ToString() == idstring);
            if(scheduletask != null)
            {
                scheduletask.LastTimeRun = DateTime.Now;
            }
            var lstSDK = TAMSInstance.TAMs.Select(x => x.DeviceSDKType).ToList();
            if (lstSDK.Contains(TAMSDKTypes.E_ZKTECO))
            {
                await GlobalEnumInOutConverter.InitUpdateEnumConverter("ZKTeco");
            }
            if (lstSDK.Contains(TAMSDKTypes.E_REALAND))
            {
                await GlobalEnumInOutConverter.InitUpdateEnumConverter("Realand");
            }
            foreach (var device in TAMSInstance.TAMs)
            {
                await Task.Run(async () =>
                {
                    var TAMService = new TAMCommonService();
                    var datestart = DateTime.MinValue;
                    var configmindate = await unitOfWork.AppSettingRepository.FindAsync(x => x.Name == ConfigEnumConstant.SYNC_MIN_SCHEDULE_DATESTART.ToString());
                    if(configmindate != null && !string.IsNullOrEmpty(configmindate.Value))
                    {
                        DateTime? dateconfig = DateTime.TryParseExact(configmindate.Value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateout) ? dateout : null;
                        if(dateconfig != null)
                            datestart = dateconfig.Value;
                    }
                    var listrecord = await unitOfWork.TAMScanLogHistoryRepository.GetManyAsync(x => x.DeviceID == device.Id);
                    var lastrecord = listrecord.OrderByDescending(x => x.TimeLog).FirstOrDefault();

                    if (lastrecord != null)
                    {
                        datestart = lastrecord.TimeLog;
                    }

                    await TAMService.Action_GetDataByDate(TAMService, device, datestart, DateTime.Now, string.Empty, "Schedule", true);
                });
            }
        }
    }
}
