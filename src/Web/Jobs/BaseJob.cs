using Quartz;
using Web.Helpers;

namespace Web.Jobs;

public abstract class BaseJob<TJob>(ILogger<TJob> logger) : IJob
    where TJob : class
{
    protected abstract Task ExecuteAsync(IJobExecutionContext context);

    public async Task Execute(IJobExecutionContext context)
    {
        var methodName = typeof(TJob).Name;
        var job = JobDistributionHelper.GetOrAddJob(methodName);
        try
        {
            if (job.Lock)
            {
                logger.LogInformation("{MethodName} is locked", methodName);
                return;
            }

            job.Lock();

            await ExecuteAsync(context);

            job.Unlock();
        }
        catch (Exception ex)
        {
            job.Unlock();
            logger.LogError(ex, "{MethodName} exception", methodName);
        }
    }
}