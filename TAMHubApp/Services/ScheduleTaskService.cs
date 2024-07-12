using Quartz;
using Quartz.Impl.Matchers;
using System.Collections.ObjectModel;
using TAMHubApp.Data;
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

        public async void AddJob(DateTimeOffset startTime, int intervalSeconds, Type jobTypeObj1)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.Start();
            var jobTypeObj = Type.GetType("TAMHubApp.ScheduleTasks.Jobs.TestJob");
            if (jobTypeObj == null)
            {
                return;
            }
            var jobId = Guid.NewGuid().ToString();
            var jobKey = new JobKey($"job-{jobId}");
            var triggerKey = new TriggerKey($"trigger-{jobId}");

            var job = JobBuilder.Create(jobTypeObj)
                .WithIdentity(jobKey)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .StartAt(startTime)
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(intervalSeconds)
                    .RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
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

            if(model.CycleType == Infrastructures.Enums.ScheduleCycleType.Monthly)
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
            else
            {
                triggerBuilder = TriggerBuilder.Create()
                .WithIdentity(triggerKey)
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(model.Hour, model.Minute))
                .ForJob(job);
            }

            var trigger = triggerBuilder.Build();

            await scheduler.ScheduleJob(job, trigger);
        }


        public async Task UpdateScheduleRuningStatus()
        {

            var scheduler = await _schedulerFactory.GetScheduler();
            var jobGroups = await scheduler.GetJobGroupNames();

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


        public async void GetRuningSchedule()
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobGroups = await scheduler.GetJobGroupNames();

            foreach (var group in jobGroups)
            {
                var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(group));
                foreach (var jobKey in jobKeys)
                {
                    var triggers = await scheduler.GetTriggersOfJob(jobKey);
                    var nextFireTime = triggers.FirstOrDefault()?.GetNextFireTimeUtc()?.ToLocalTime();
                    //Log.Information($"listJob: " + jobKey.Name + " | " + nextFireTime.ToString());
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
                        CycleType = schedule.CycleType,
                        IsRuning = false,
                        IsLoading = false
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
                    CycleType = schedule.CycleType,
                    IsRuning = false,
                    IsLoading = false
                };
                ScheduleTasks.Add(scheduleadd);
            }
        }
    }
}
