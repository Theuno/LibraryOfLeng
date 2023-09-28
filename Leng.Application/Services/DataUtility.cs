using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leng.Application.Services
{
    public static class DataUtility
    {
        public static bool ValidateImportFile(IBrowserFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file), "File cannot be null");
            }

            if (file.Size > 10485760) // 10 MB limit
            {
                throw new ArgumentException("File size exceeds the 10MB limit", nameof(file));
            }

            if (!file.Name.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid file type. Only .xlsx files are allowed", nameof(file));
            }

            return true;
        }
    }
}
