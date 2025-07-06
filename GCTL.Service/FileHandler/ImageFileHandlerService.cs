using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;




namespace GCTL.Service.ImageFileHandler
{
    public class ImageFileHandlerService : IImageFileHandlerService
    {



        public string ApplyImageOpacity(string originalPath, float opacity)
        {
            if (!File.Exists(originalPath)) return null;

            var folder = Path.GetDirectoryName(originalPath);
            var fileName = Path.GetFileNameWithoutExtension(originalPath);
            var extension = Path.GetExtension(originalPath);
            var outputPath = Path.Combine(folder, $"{fileName}_transparent{extension}");

            using (var image = Image.Load<Rgba32>(originalPath))
            {
                //image.Mutate(ctx =>
                //{
                //    ctx.DrawImage(image.Clone(x => x.Opacity(opacity)), new Point(0, 0), 1f);
                //});

                float desiredOpacity = 0.1f;        // Transparency level inside the image
                float blendPercentage = 0.3f;       // Strength of watermark (0 = invisible, 1 = full)

                // Apply watermark
                image.Mutate(ctx =>
                {
                    ctx.DrawImage(
                        image.Clone(x => x.Opacity(desiredOpacity)),
                        new Point(0, 0),
                        blendPercentage);
                });



                image.Save(outputPath);
            }

            return outputPath;
        }



        public async Task<string> SaveFileAsync(IFormFile file, string folderName, bool saveThumb = false)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);
            var thumbFolder = Path.Combine(uploadsFolder, "thumbs");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save original file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Save thumbnail if requested
            if (saveThumb)
            {
                Directory.CreateDirectory(thumbFolder);
                var thumbPath = Path.Combine(thumbFolder, fileName);
                await CreateThumbnailAsync(filePath, thumbPath, 100, 100);
            }

            return fileName;
        }

        private async Task CreateThumbnailAsync(string originalPath, string thumbnailPath, int width, int height)
        {
            using var inputStream = File.OpenRead(originalPath);
            using var original = SKBitmap.Decode(inputStream);

            if (original == null)
                throw new ArgumentException("Could not decode image", nameof(originalPath));

            // Calculate dimensions maintaining aspect ratio
            var (newWidth, newHeight) = CalculateResizeDimensions(original.Width, original.Height, width, height);

            using var resized = original.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);
            using var image = SKImage.FromBitmap(resized);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 85);

            await using var output = File.OpenWrite(thumbnailPath);
            data.SaveTo(output);
        }

        private (int width, int height) CalculateResizeDimensions(int originalWidth, int originalHeight, int targetWidth, int targetHeight)
        {
            var ratioX = (double)targetWidth / originalWidth;
            var ratioY = (double)targetHeight / originalHeight;
            var ratio = Math.Min(ratioX, ratioY);

            return ((int)(originalWidth * ratio), (int)(originalHeight * ratio));
        }

        // Alternative method for creating thumbnail from stream
        private async Task CreateThumbnailFromStreamAsync(Stream sourceStream, string thumbnailPath, int width, int height)
        {
            sourceStream.Position = 0;

            using var original = SKBitmap.Decode(sourceStream);
            if (original == null)
                throw new ArgumentException("Could not decode image from stream");

            var (newWidth, newHeight) = CalculateResizeDimensions(original.Width, original.Height, width, height);

            using var resized = original.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);
            using var image = SKImage.FromBitmap(resized);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 85);

            await using var output = File.OpenWrite(thumbnailPath);
            data.SaveTo(output);
        }



    }
}