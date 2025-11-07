using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace DevOpsAssignment;

public class JobStatusFunction
{
    private readonly ILogger _logger;
    private readonly BlobServiceClient _blobServiceClient;

    public JobStatusFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<JobStatusFunction>();
        // Connect to your Azurite or Azure Storage using connection string
        var connStr = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        _blobServiceClient = new BlobServiceClient(connStr);
    }

    [Function("JobStatus")]
    public async Task<HttpResponseData> Run(
     [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "job-status/{jobId}")]
    HttpRequestData req,
     string jobId)
    {
        _logger.LogInformation("Checking status for job {JobId}", jobId);

        var containerClient = _blobServiceClient.GetBlobContainerClient("images");

        // Log 
        if (!await containerClient.ExistsAsync())
        {
            _logger.LogWarning("Container 'images' not found.");
        }

        var jobImages = new List<string>();
        await foreach (var blob in containerClient.GetBlobsAsync())
        {
            _logger.LogInformation("Found blob: {Name}", blob.Name);
            if (blob.Name.Contains(jobId))
            {
                jobImages.Add(containerClient.Uri + "/" + blob.Name);
            }
        }

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");

        if (jobImages.Count == 0)
        {
            _logger.LogInformation("No images found for job {JobId}", jobId);
            await response.WriteStringAsync("{\"status\":\"processing\"}");
        }
        else
        {
            _logger.LogInformation("Found {Count} images for job {JobId}", jobImages.Count, jobId);
            var json = JsonSerializer.Serialize(new
            {
                status = "done",
                images = jobImages
            });
            await response.WriteStringAsync(json);
        }

        return response;
    }

}
