using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GamesToGoAPI.Models.File
{
    public class ImageFile
    {
        public string Name { get; set; }
        public IFormFile File { get; set; }
    }
}
