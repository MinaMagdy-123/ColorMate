using ColorMate.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColorMate.Core.DTOs.ObjDetectionDto
{
    public class DetectionHistoryResponseDto
    {
        public string ImageBase64 { get; set; }
        public int TotalObjects { get; set; }
        public List<ObjFromDetection> Objects { get; set; } = new();
    }
}
