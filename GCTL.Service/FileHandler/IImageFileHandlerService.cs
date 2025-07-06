using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GCTL.Service.ImageFileHandler
{
    public interface IImageFileHandlerService
    {
        Task<string> SaveFileAsync(IFormFile file, string folderName, bool saveThumb = false);

         string ApplyImageOpacity(string originalPath, float opacity);
    }
}
