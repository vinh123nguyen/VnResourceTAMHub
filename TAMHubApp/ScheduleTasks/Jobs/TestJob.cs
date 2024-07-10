using Quartz;
using TAMHubApp.Services.TAMServices;

namespace TAMHubApp.ScheduleTasks.Jobs
{
    [DisallowConcurrentExecution]
    public class TestJob : IJob
    {
        //private readonly ISchedulerFactory _schedulerFactory;


        //public TestJob(ISchedulerFactory schedulerFactory)
        //{
        //    _schedulerFactory = schedulerFactory;
        //}
        public TestJob(ISchedulerFactory schedulerFactory)
        {

        }
        public Task Execute(IJobExecutionContext context)
        {
            //var ddaw = new CZKEM();

            //var dwa = ddaw.Connect_Net("1.1.1.1", 4322);

            return Task.CompletedTask;
        }
    }

    public class SampleJob1 : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            //Log.Information($"Sample Job 1 executed at {DateTime.Now}");
            return Task.CompletedTask;
        }
    }
}
