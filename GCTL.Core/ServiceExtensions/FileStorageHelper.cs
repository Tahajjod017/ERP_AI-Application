using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GCTL.Core.ServiceExtensions
{
    public static class FileStorageHelper
    {
        public static async Task<string> SaveFileAsync(IFormFile file, string folderPath, string webRootPath)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(webRootPath, folderPath);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path (used for storing in DB or rendering)
            return Path.Combine(folderPath, fileName).Replace("\\", "/");
        }
    }
}
