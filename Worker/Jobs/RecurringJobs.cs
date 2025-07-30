using Hangfire;
using Worker.Jobs;

public class RecurringJobs
{
    private readonly IConfiguration _config;

    public RecurringJobs(IConfiguration config)
    {
        _config = config;
    }

    public void AddOrUpdate()
    {
        RecurringJob.AddOrUpdate<KapJob>("kap-job", x => x.Run(), Cron.Daily);
    }
}
