using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace DevOpsAssignment;

public class StartJobFunction
{
    private readonly ILogger _logger;

    public StartJobFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<StartJobFunction>();
    }

    [Function("StartJob")]
    public async Task<StartJobOutputs> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "start-job")] HttpRequestData req)
    {
        var jobId = Guid.NewGuid().ToString();
        _logger.LogInformation("New job started with ID: {JobId}", jobId);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync($"Job {jobId} started.");

        return new StartJobOutputs
        {
            HttpResponse = response,
            JobQueue = jobId
        };
    }
}


