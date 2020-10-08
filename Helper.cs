using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace WebUpload
{
    public static class Helper
    {
        public static bool IsFileAllowed(IFormFile file)
        {

            string[] formats = new string[] { ".jpeg", ".jpg", ".png", ".xlsx", ".xls" };

            return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }
    }
}
