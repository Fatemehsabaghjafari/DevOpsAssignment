using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DevOpsAssignment;

public class ProcessJobFunction
{
    private readonly ILogger _logger;

    public ProcessJobFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ProcessJobFunction>();
    }

    [Function("ProcessJob")]
    public ProcessJobOutputs Run(
        [QueueTrigger("jobs", Connection = "AzureWebJobsStorage")] string jobId)
    {
        _logger.LogInformation("Processing job {JobId}", jobId);

        var messages = new[]
        {
            new JobMessage { JobId = jobId, ImageUrl = "https://picsum.photos/400", WeatherInfo = "Sunny" },
            new JobMessage { JobId = jobId, ImageUrl = "https://picsum.photos/400", WeatherInfo = "Rainy" }
        };

        return new ProcessJobOutputs
        {
            QueueItems = messages.Select(m => JsonSerializer.Serialize(m)).ToArray()
        };
    }
}


