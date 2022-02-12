using Microsoft.AspNetCore.Mvc;

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
        //[Produces("application/vnd.apple.mpegurl")]
        public async Task<IResult> GetLiveStream([FromRoute] string? file = "")
        {
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
                //return Results.Stream(FileBuffer.playlistFile, contentType: "application/x-mpegURL", fileDownloadName: file, enableRangeProcessing: true);
            }
            return Results.NotFound();
        }

        [HttpPut("/stream/{route?}", Name = "PutShit")]
        public async Task<IResult> PutShit([FromRoute] string route = "")
        {
            Console.WriteLine($"Got put request for \"{route}\"");
            if (!await FileBuffer.AddStream(Request.Body, route))
            {
                return Results.BadRequest();
            }

            return Results.Ok();
        }

    }
}
