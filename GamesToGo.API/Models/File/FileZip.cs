using Microsoft.AspNetCore.Http;

namespace GamesToGo.API.Models.File
{
    public class FileZip
    {
#pragma warning disable IDE1006 // Estilos de nombres
        public string ID { get; set; }
        public string Name { get; set; }
        public string description { get; set; }
        public string minP { get; set; }
        public string maxP { get; set; }
        public string imageName { get; set; }
        public string LastEdited { get; set; }
        public int Status { get; set; }
        public IFormFile File { get; set; }
        public string FileName { get; set; }
#pragma warning restore IDE1006 // Estilos de nombres
    }
}
