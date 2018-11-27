using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SkiaSharp;

namespace SkiaSharpFunction
{
    public static class Images
    {
        [FunctionName("images")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("Creating image with SkiaSharp...");

            // get the request body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic body = JsonConvert.DeserializeObject(requestBody);

            // get the query value
            var queryText = (string)req.Query["text"];

            // try get some text
            var text = body?.text ?? queryText ?? "SkiaSharp";

            // create a surface
            var info = new SKImageInfo(256, 256);
            using (var surface = SKSurface.Create(info))
            {
                // the the canvas and properties
                var canvas = surface.Canvas;

                // make sure the canvas is blank
                canvas.Clear(SKColors.White);

                // draw some text
                var paint = new SKPaint
                {
                    Color = SKColors.Black,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                    TextAlign = SKTextAlign.Center,
                    TextSize = 24
                };
                var coord = new SKPoint(info.Width / 2, (info.Height + paint.TextSize) / 2);
                canvas.DrawText(text, coord, paint);

                // retrieve the encoded image
                using (var image = surface.Snapshot())
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                {
                    return new FileContentResult(data.ToArray(), "image/png");
                }
            }
        }
    }
}
