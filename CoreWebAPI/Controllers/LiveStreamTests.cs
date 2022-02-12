using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace CoreWebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LiveStreamTests : ControllerBase
    {
        private readonly ILogger<LiveStreamTests> _logger;

        public LiveStreamTests(ILogger<LiveStreamTests> logger)
        {
            _logger = logger;
        }

        [HttpGet("/live/{file?}")]
        [Produces("application/x-mpegURL", "video/MP2T")]
        public IResult GetLiveStream([FromRoute] string? file = "")
        {
            _logger.LogInformation($"Got GET Request for \"{file}\"");
            if (string.IsNullOrEmpty(file))
            {
                return Results.BadRequest();
            }
            if (file.EndsWith(".m3u8"))
            {
                using (var stream = FileBuffer.playlistFile)
                {
                    return Results.File(stream.GetBuffer(), contentType: "application/x-mpegURL", fileDownloadName: file, enableRangeProcessing: true);
                }
            }
            if (file.EndsWith(".ts"))
            {
                var matchVal = Regex.Match(file, @".*?(\d+)\..*"); // Match file name 
                if (!matchVal.Success) // Check if match was a success
                {
                    return Results.BadRequest();
                }
                var streamIndex = ulong.Parse(matchVal.Groups[1].Value); // Use the matched number to get the index
                using (var stream = FileBuffer.GetStream(streamIndex))
                {
                    return Results.File(stream.GetBuffer(), contentType: "video/MP2T", fileDownloadName: file, enableRangeProcessing: true);
                }
            }
            return Results.NotFound();
        }

        [HttpPut("/stream/{route?}", Name = "PutShit")]
        public async Task<IResult> PutShit([FromRoute] string route = "")
        {
            _logger.LogInformation($"Got PUT Request for \"{route}\"");
            if (!await FileBuffer.AddStream(Request.Body, route))
            {
                return Results.BadRequest();
            }

            return Results.Ok();
        }

    }
}
