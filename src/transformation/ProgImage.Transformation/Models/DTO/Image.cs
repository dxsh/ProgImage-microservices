﻿using System;

namespace ProgImage.Transformation.Models.DTO
{
    public class Image
    {
        public Guid ImageId { get; set; }
        public string ImageFilePath { get; set; }
        public long CreatedAt { get; set; }
    }
}