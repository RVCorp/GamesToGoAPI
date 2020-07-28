using Microsoft.AspNetCore.Http;

namespace GamesToGoAPI.Models.File
{
    public class ImageFile
    {
        public string Name { get; set; }
        public IFormFile File { get; set; }
    }
}
