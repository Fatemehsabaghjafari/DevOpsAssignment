using System.Net.Http;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;

namespace DevOpsAssignment;

public class GenerateImageFunction
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient = new();

    public GenerateImageFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<GenerateImageFunction>();
    }

    [Function("GenerateImage")]
    public async Task<GenerateImageOutputs> Run(
        [QueueTrigger("image-processing-queue", Connection = "AzureWebJobsStorage")] string msg)
    {
        var jobData = JsonSerializer.Deserialize<JobMessage>(msg)!;
        _logger.LogInformation("Generating image for job {JobId}", jobData.JobId);

        // Download the image
        var imageBytes = await _httpClient.GetByteArrayAsync(jobData.ImageUrl);
        using var image = Image.Load(imageBytes);

        // Draw the weather text
        var font = SystemFonts.CreateFont("Arial", 24);
        var text = $"Weather: {jobData.WeatherInfo}";
        var color = Color.White;
        image.Mutate(ctx => ctx.DrawText(text, font, color, new PointF(20, 40)));

        // Save to memory stream as PNG
        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        ms.Position = 0;

        // Return as blob output
        return new GenerateImageOutputs
        {
            Image = ms.ToArray(),
            JobId = jobData.JobId
        };
    }
}





