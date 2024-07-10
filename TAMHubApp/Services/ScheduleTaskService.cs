using Quartz;
using Quartz.Impl.Matchers;

namespace TAMHubApp.Services
{
    public class ScheduleTaskService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        public ScheduleTaskService(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
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
    }
}
