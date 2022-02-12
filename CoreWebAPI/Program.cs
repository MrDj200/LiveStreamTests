using Microsoft.AspNetCore.Mvc;

namespace CoreWebAPI
{
    class CoreWebAPI
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers(options =>
            {
                options.RespectBrowserAcceptHeader = true;
                options.OutputFormatters.Insert(0, new DjOutputFormatterM3u8());
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();


            app.MapGet("/test", Testing);

            app.MapGet("/index/{route?}", GetIndex);
            app.MapGet("/liveOld", GetLive);
            app.MapGet("/tests/{route?}", GetTest);

            app.MapGet("/livetest/{file?}", GetLiveStream);

            app.MapPut("/put/{route?}", PutShit);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();


            static void PutShit(IConfiguration cfg, HttpContext context, IFormFile file, [FromRoute] string? route = "")
            {
                string contentType = "text/html";
                contentType = route?.Split(".").Last().ToLower() switch
                {
                    "m3u8" => "application/x-mpegURL",
                    "ts" => "video/MP2T",
                    "mp4" => "video/mp4",
                    "webm" => "video/webm",
                    _ => "text/html"
                };
                //context.Request.ContentType = "multipart/form-data";
                //context.Request.ContentType = contentType;

                Console.WriteLine($"ContentType: {context.Request.ContentType}");
                Console.WriteLine($"{route}");
                //Console.WriteLine(files.Count);

            }

            static IResult Testing(IConfiguration cfg)
            {
                //var dir = @"C:\Users\Dj\source\repos\LiveStreamTests\CoreWebAPI\Video\";
                var dir = @"Video\";
                var files = Directory.GetFiles(dir).Select(f => f.Replace(dir, "")).ToArray();

                //return Results.Ok(files.Where(x => x.EndsWith(".m3u8") || x.EndsWith(".ts")));
                return Results.File(@$"{Path.GetFullPath(dir)}2022-02-05 23-51-56.m3u8");
            }

            static IResult GetLive(IConfiguration cfg)
            {
                var dir = @"Video\";
                var myFile = new DirectoryInfo(dir).GetFiles().Where(f => f.Extension == ".m3u8").OrderByDescending(f => f.LastWriteTime).FirstOrDefault()?.Name;

                Console.WriteLine(myFile);

                if (myFile == null)
                {
                    return Results.NotFound();
                }

                return Results.LocalRedirect($"/index/{myFile}");
            }

            static IResult GetIndex(IConfiguration cfg, [FromRoute] string route = "NOTHING")
            {
                var dir = @"Video/";
                var files = Directory.GetFiles(dir).Select(f => f.Replace(dir, "")).ToArray();

                Console.WriteLine($"Getting request for {route}");

                if (route == "NOTHING")
                {
                    //return Results.Ok(files.Where(x => x.EndsWith(".m3u8") || x.EndsWith(".ts")));
                    return Results.Ok(files);
                }

                string contentType = "text/html";

                contentType = route.Split(".").Last().ToLower() switch
                {
                    "m3u8" => "application/x-mpegURL",
                    "ts" => "video/MP2T",
                    "mp4" => "video/mp4",
                    "webm" => "video/webm",
                    _ => "text/html"
                };

                if (!File.Exists(dir + route))
                {
                    return Results.NotFound();
                }

                return Results.File(@$"{Path.GetFullPath(dir)}{route}", contentType: contentType, enableRangeProcessing: true);
            }

            static IResult GetTest(IConfiguration cfg, [FromRoute] string route = "NOTHING")
            {
                var dir = @"output/stream/";
                var files = Directory.GetFiles(dir).Select(f => f.Replace(dir, "")).ToArray();

                Console.WriteLine($"Getting request for {route}");

                if (route == "NOTHING")
                {
                    //return Results.Ok(files.Where(x => x.EndsWith(".m3u8") || x.EndsWith(".ts")));
                    return Results.Ok(files);
                }

                string contentType = "text/html";

                contentType = route.Split(".").Last().ToLower() switch
                {
                    "m3u8" => "application/x-mpegURL",
                    "ts" => "video/MP2T",
                    "mp4" => "video/mp4",
                    "webm" => "video/webm",
                    _ => "text/html"
                };

                if (!File.Exists(dir + route))
                {
                    return Results.NotFound();
                }

                return Results.File(@$"{Path.GetFullPath(dir)}{route}", contentType: contentType, enableRangeProcessing: true);
            }

            //[HttpGet("/livetest/{file?}")]
            [Produces("application/x-mpegURL", "video/MP2T")]
            static async Task<IResult> GetLiveStream([FromRoute] string? file = "")
            {
                if (string.IsNullOrEmpty(file))
                {
                    return Results.BadRequest();
                }
                if (file.EndsWith(".m3u8"))
                {
                    using (var stream = FileBuffer.playlistFile)
                    {
                        //Request.Headers.Accept = "application/x-mpegURL";
                        //Response.Headers.Accept = "application/x-mpegURL";
                        return Results.Bytes(stream.GetBuffer(), contentType: "application/x-mpegURL", fileDownloadName: file, enableRangeProcessing: true);
                    }
                    //return Results.Stream(FileBuffer.playlistFile, contentType: "application/x-mpegURL", fileDownloadName: file, enableRangeProcessing: true);
                }
                return Results.NotFound();
            }

        }
    }
}