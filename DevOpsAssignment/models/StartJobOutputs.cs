using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace DevOpsAssignment;


public class StartJobOutputs
{
    public HttpResponseData? HttpResponse { get; set; }

    [QueueOutput("jobs", Connection = "AzureWebJobsStorage")]
    public string? JobQueue { get; set; }
}