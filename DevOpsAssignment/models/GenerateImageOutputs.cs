using Microsoft.Azure.Functions.Worker;

namespace DevOpsAssignment;


public class GenerateImageOutputs
{
    [BlobOutput("images/{JobId}-{rand-guid}.png", Connection = "AzureWebJobsStorage")]
    public byte[]? Image { get; set; }

    // stores the Job ID
    [System.Text.Json.Serialization.JsonIgnore]
    public string? JobId { get; set; }
}