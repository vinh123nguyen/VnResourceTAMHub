using Quartz;
using Quartz.Impl.Calendar;
using Quartz.Impl.Matchers;
using System.Collections.ObjectModel;
using TAMHubApp.Data;
using TAMHubApp.Infrastructures.Enums;
using TAMHubApp.Models;
using TAMHubApp.ScheduleTasks.Jobs;
using TAMHubApp.ViewModels;

namespace TAMHubApp.Services
{
    public class ScheduleTaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISchedulerFactory _schedulerFactory;

        public ObservableCollection<ScheduleTaskModel> ScheduleTasks { get; set; }

        public ScheduleTaskService(ISchedulerFactory schedulerFactory
            , IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _schedulerFactory = schedulerFactory;
            InitScheduleTask().Wait();
        }

        public async Task StopSchedule(ScheduleTaskModel model)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobKey = new JobKey($"job__{model.Id}");
            await scheduler.DeleteJob(jobKey);
        }

        public async Task New_AddJob(ScheduleTaskModel model)
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            var jobKey = new JobKey($"job__{model.Id}");
            var triggerKey = new TriggerKey($"trigger__{model.Id}");

            var jobrun = typeof(GetAndSyncDataHRMJob);

            var job = JobBuilder.Create(jobrun)
                .WithIdentity(jobKey)
                .Build();

            var triggerBuilder = TriggerBuilder.Create();

            if (model.CycleType == Infrastructures.Enums.ScheduleCycleType.Monthly)
            {
                triggerBuilder = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(model.DayInMonth == null ? 1 : model.DayInMonth.Value, model.Hour, model.Minute))
                .ForJob(job);
            }
            else if (model.CycleType == Infrastructures.Enums.ScheduleCycleType.Weekly)
            {
                triggerBuilder = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(model.DayOfWeek == null ? DayOfWeek.Monday : model.DayOfWeek.Value, model.Hour, model.Minute))
                .ForJob(job);
            }
            else if (model.CycleType == Infrastructures.Enums.ScheduleCycleType.Daily)
            {
                triggerBuilder = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(model.Hour, model.Minute))
                .ForJob(job);
            }
            else if (model.CycleType == Infrastructures.Enums.ScheduleCycleType.RepeatBy)
            {
                WeeklyCalendar wcal = new WeeklyCalendar();

                if ((model.ExcludeTimeFrom != null && model.ExcludeTimeTo != null) && (model.ExcludeTime2From != null && model.ExcludeTime2To != null))
                {
                    var dcal1 = new DailyCalendar(model.ExcludeTimeFrom.Value.Hours, model.ExcludeTimeFrom.Value.Minutes, model.ExcludeTimeFrom.Value.Seconds, model.ExcludeTimeFrom.Value.Milliseconds,
                        model.ExcludeTimeTo.Value.Hours, model.ExcludeTimeTo.Value.Minutes, model.ExcludeTimeTo.Value.Seconds, model.ExcludeTimeTo.Value.Milliseconds);
                    var dcal2 = new DailyCalendar(model.ExcludeTime2From.Value.Hours, model.ExcludeTime2From.Value.Minutes, model.ExcludeTime2From.Value.Seconds, model.ExcludeTime2From.Value.Milliseconds,
                        model.ExcludeTime2To.Value.Hours, model.ExcludeTime2To.Value.Minutes, model.ExcludeTime2To.Value.Seconds, model.ExcludeTime2To.Value.Milliseconds);
                    dcal2.CalendarBase = dcal1;
                    wcal.CalendarBase = dcal2;
                }
                else if ((model.ExcludeTimeFrom != null && model.ExcludeTimeTo != null) && (model.ExcludeTime2From != null || model.ExcludeTime2To != null))
                {
                    var dcal1 = new DailyCalendar(model.ExcludeTimeFrom.Value.Hours, model.ExcludeTimeFrom.Value.Minutes, model.ExcludeTimeFrom.Value.Seconds, model.ExcludeTimeFrom.Value.Milliseconds,
                        model.ExcludeTimeTo.Value.Hours, model.ExcludeTimeTo.Value.Minutes, model.ExcludeTimeTo.Value.Seconds, model.ExcludeTimeTo.Value.Milliseconds);
                    wcal.CalendarBase = dcal1;
                }
                else if ((model.ExcludeTimeFrom != null || model.ExcludeTimeTo != null) && (model.ExcludeTime2From != null && model.ExcludeTime2To != null))
                {
                    var dcal2 = new DailyCalendar(model.ExcludeTime2From.Value.Hours, model.ExcludeTime2From.Value.Minutes, model.ExcludeTime2From.Value.Seconds, model.ExcludeTime2From.Value.Milliseconds,
                        model.ExcludeTime2To.Value.Hours, model.ExcludeTime2To.Value.Minutes, model.ExcludeTime2To.Value.Seconds, model.ExcludeTime2To.Value.Milliseconds);
                    wcal.CalendarBase = dcal2;
                }

                if (!string.IsNullOrEmpty(model.ExcludeDayOfWeek))
                {
                    var lstdayExclude = model.ExcludeDayOfWeek.Split(",").Select(x => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), x)).ToList();
                    if(lstdayExclude != null && lstdayExclude.Any())
                    {
                        var dayofweek = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().ToList();
                        foreach (var dayinw in dayofweek)
                        {
                            if (lstdayExclude.Contains(dayinw))
                            {
                                wcal.SetDayExcluded(dayinw, true);
                            }
                            else
                            {
                                wcal.SetDayExcluded(dayinw, false);
                            }
                        }
                    }
                }
                await scheduler.AddCalendar("WeeklyCalendar", wcal, true, false);
                var tspstart = model.StartTime != null ? model.StartTime.Value : new TimeSpan(00, 00, 05);
                var starttime = new DateTimeOffset(DateTime.Now.Date.Add(tspstart));
                int value = model.RepeatByValue != null ? model.RepeatByValue.Value : model.RepeatByType == RepeatByType.ByHour ? 1 : 30;
                int minutecycle = model.RepeatByType == RepeatByType.ByMinute ? value : value * 60;

                triggerBuilder = TriggerBuilder.Create().WithIdentity(triggerKey)
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(minutecycle).RepeatForever())
                    .StartAt(starttime)
                    .ModifiedByCalendar("WeeklyCalendar")
                    .ForJob(job);
            }

            var trigger = triggerBuilder.Build();

            await scheduler.ScheduleJob(job, trigger);
        }


        public async Task UpdateScheduleRuningStatus()
        {

            var scheduler = await _schedulerFactory.GetScheduler();
            var jobGroups = await scheduler.GetJobGroupNames();

            foreach(var schedule in ScheduleTasks)
            {
                schedule.IsRuning = false;
                schedule.NextTimeRun = null;
            }

            foreach (var group in jobGroups)
            {
                var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(group));
                foreach (var jobKey in jobKeys)
                {
                    var triggers = await scheduler.GetTriggersOfJob(jobKey);
                    var nextFireTime = triggers.FirstOrDefault()?.GetNextFireTimeUtc()?.ToLocalTime();

                    var idstring = jobKey.Name.Split("__")[1];

                    var schedule = ScheduleTasks.FirstOrDefault(x => x.Id.ToString() == idstring);

                    if(schedule != null)
                    {
                        schedule.IsRuning = true;
                        schedule.NextTimeRun = nextFireTime == null ? null : nextFireTime.Value.LocalDateTime;
                    }
                }
            }
        }

        public async Task InitScheduleTask()
        {
            var lstScheduleDb = await _unitOfWork.ScheduleTaskRepository.GetAllAsync();
            var lstScheduleDone = new List<ScheduleTaskModel>();

            if (lstScheduleDb != null && lstScheduleDb.Any())
            {
                foreach (var schedule in lstScheduleDb)
                {
                    var scheduleadd = new ScheduleTaskModel()
                    {
                        Id = schedule.Id,
                        Name = schedule.Name,
                        LastTimeRun = schedule.LastTimeRun,
                        Hour = schedule.Hour,
                        Minute = schedule.Minute,
                        DayInMonth = schedule.DayInMonth,
                        DayOfWeek = schedule.DayOfWeek,
                        RepeatByType = schedule.RepeatByType,
                        RepeatByValue = schedule.RepeatByValue,
                        CycleType = schedule.CycleType,
                        IsRuning = false,
                        IsLoading = false,
                        StartTime = schedule.StartTime,
                        ExcludeDayOfWeek = schedule.ExcludeDayOfWeek,
                        ExcludeTimeFrom = schedule.ExcludeTimeFrom,
                        ExcludeTimeTo = schedule.ExcludeTimeTo,
                        ExcludeTime2From = schedule.ExcludeTime2From,
                        ExcludeTime2To = schedule.ExcludeTime2To,
                    };
                    lstScheduleDone.Add(scheduleadd);
                }
            }
            ScheduleTasks = new ObservableCollection<ScheduleTaskModel>(lstScheduleDone);
        }

        public async void UpdateScheduleTaskSingleton(Guid id)
        {
            var schedule = await _unitOfWork.ScheduleTaskRepository.GetByIdAsync(id);
            if (schedule != null)
            {
                var scheduleadd = new ScheduleTaskModel()
                {
                    Id = schedule.Id,
                    Name = schedule.Name,
                    LastTimeRun = schedule.LastTimeRun,
                    Hour = schedule.Hour,
                    Minute = schedule.Minute,
                    DayInMonth = schedule.DayInMonth,
                    DayOfWeek = schedule.DayOfWeek,
                    RepeatByType = schedule.RepeatByType,
                    RepeatByValue = schedule.RepeatByValue,
                    CycleType = schedule.CycleType,
                    IsRuning = false,
                    IsLoading = false,
                    StartTime = schedule.StartTime,
                    ExcludeDayOfWeek = schedule.ExcludeDayOfWeek,
                    ExcludeTimeFrom = schedule.ExcludeTimeFrom,
                    ExcludeTimeTo = schedule.ExcludeTimeTo,
                    ExcludeTime2From = schedule.ExcludeTime2From,
                    ExcludeTime2To = schedule.ExcludeTime2To,
                };
                ScheduleTasks.Add(scheduleadd);
            }
        }
    }
}
