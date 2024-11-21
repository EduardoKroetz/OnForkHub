using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var corsPolicyName = "CorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(corsPolicyName);
app.UseRouting();

var tempDir = Path.Combine(Directory.GetCurrentDirectory(), "temp");
var finalDir = Path.Combine(Directory.GetCurrentDirectory(), "videos");
Directory.CreateDirectory(tempDir);
Directory.CreateDirectory(finalDir);

app.MapPost(
        "/upload",
        async (IFormFile chunk, string fileId, int chunkIndex) =>
        {
            var chunkPath = Path.Combine(tempDir, $"{fileId}_chunk_{chunkIndex}");

            using (var stream = new FileStream(chunkPath, FileMode.Create))
            {
                await chunk.CopyToAsync(stream);
            }

            return Results.Ok(new { message = "Chunk uploaded", chunkIndex });
        }
    )
    .DisableAntiforgery();

app.MapPost(
    "/finalize-upload",
    async (string fileId, string fileName) =>
    {
        var finalPath = Path.Combine(finalDir, fileName);
        var chunks = Directory.GetFiles(tempDir, $"{fileId}_chunk_*").OrderBy(f => f);

        await using (var finalStream = new FileStream(finalPath, FileMode.Create))
        {
            foreach (var chunk in chunks)
            {
                using (var chunkStream = new FileStream(chunk, FileMode.Open))
                {
                    await chunkStream.CopyToAsync(finalStream);
                }

                // Remove the chunk after joining.
                // File.Delete(chunk);
            }
        }

        return Results.Ok(new { message = "Upload finalized", videoPath = $"/videos/{fileName}" });
    }
);

app.MapGet(
    "/videos/{fileName}",
    async (string fileName, HttpContext context) =>
    {
        var videoPath = Path.Combine(finalDir, fileName);

        if (!System.IO.File.Exists(videoPath))
        {
            return Results.NotFound(new { message = "Video not found" });
        }

        context.Response.ContentType = "video/mp4";
        await context.Response.SendFileAsync(videoPath);

        return Results.Empty;
    }
);

app.Run();
