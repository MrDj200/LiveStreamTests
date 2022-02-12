using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace CoreWebAPI
{
    public class DjOutputFormatterM3u8 : OutputFormatter
    {
        private readonly string MIME = "application/x-mpegURL";

        public DjOutputFormatterM3u8()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(MIME));
        }

        public async override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            var Response = context.HttpContext.Response;
            Response.StatusCode = StatusCodes.Status400BadRequest; // Fallback Status

            if (context.Object == null || context.Object.GetType().GetProperty("FileContents", typeof(byte[]))?.GetValue(context.Object) is not byte[] FileContent)
            {
                return;
            }

            Response.StatusCode = StatusCodes.Status200OK;
            Response.ContentLength = FileContent.Length;
            Response.ContentType = MIME;

            var WriteResult = await Response.BodyWriter.WriteAsync(new ReadOnlyMemory<byte>(FileContent, 0, FileContent.Length));
            if (WriteResult.IsCanceled)
            {
                return; // TODO: Handle this or other error shit
            }
            
        }
    }
}
