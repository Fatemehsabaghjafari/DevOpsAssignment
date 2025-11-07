using Microsoft.Azure.Functions.Worker;

namespace DevOpsAssignment;

public class ProcessJobOutputs {

    [QueueOutput("image-processing-queue", Connection = "AzureWebJobsStorage")]
    public string[]? QueueItems { get; set; } 
}
