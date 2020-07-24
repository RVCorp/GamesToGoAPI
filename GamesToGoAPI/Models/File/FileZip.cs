﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GamesToGoAPI.Models.File
{
    public class FileZip
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string description { get; set; }
        public string minP { get; set; }
        public string maxP { get; set; }
        public IFormFile File { get; set; }
    }
}