namespace Web.Helpers;

public static class JobDistributionHelper
{
    private static readonly List<JobStatusRecord> Jobs = [];
    
    public static JobStatusRecord GetOrAddJob(string name)
    {
        var job = Jobs.Find(x => x.Name == name);
        if (job is not null)
        {
            return job;
        }

        job = new JobStatusRecord{ Name = name, Lock = false };
        Jobs.Add(job);
        return job;
    }

    public static void Lock(this JobStatusRecord job)
    {
        job.Lock = true;
    }
    
    public static void Unlock(this JobStatusRecord job)
    {
        job.Lock = false;
    }
}

public class JobStatusRecord
{
    public string Name { get; set; } = null!;
    public bool Lock { get; set; }
}