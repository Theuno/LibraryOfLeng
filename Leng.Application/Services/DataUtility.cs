using Leng.Application.Dtos;
using Microsoft.AspNetCore.Components.Forms;
using OfficeOpenXml; // EPPlus

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

        public static bool ValidateHeaders(string[] headers)
        {
            if (headers == null || headers.Length != 10)
            {
                return false;
            }

            var validHeaders1 = new[] { "Card Name", "Card Number", "Set Code", "Count", "Count Foil", "have", "have foil", "want", "want foil", "note" };
            var validHeaders2 = new[] { "kaartnaam", "kaartnummer", "set_code", "c", "c_foil", "h", "h_foil", "w", "w_foil", "notitie" };
            var validHeaders3 = new[] { "Card Name", "Card Number", "Set Code", "Count", "Count Foil", "", "", "", "", "" };
            var validHeaders4 = new[] { "kaartnaam", "kaartnummer", "set_code", "c", "c_foil", "", "", "", "", "" };

            return headers.SequenceEqual(validHeaders1) ||
                   headers.SequenceEqual(validHeaders2) ||
                   headers.SequenceEqual(validHeaders3) ||
                   headers.SequenceEqual(validHeaders4);
        }


        private static async Task<List<UserCardInfo>> ReadCardsAsync(ExcelWorksheet worksheet)
        {
            return await Task.Run(() =>
            {
                var cards = new List<UserCardInfo>();

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    var userCardInfo = new UserCardInfo
                    {
                        CardName = worksheet.Cells[row, 1].Text,
                        CardNumber = worksheet.Cells[row, 2].Text,
                        SetCode = worksheet.Cells[row, 3].Text,
                        Count = int.Parse(worksheet.Cells[row, 4].Text),
                        CountFoil = int.Parse(worksheet.Cells[row, 5].Text)
                    };

                    cards.Add(userCardInfo);
                }

                return cards;
            });
        }

        public static (ExcelPackage, ExcelWorksheet) OpenWorksheet(string file)
        {
            // Open file
            var fileInfo = new FileInfo(file);
            var package = new ExcelPackage(fileInfo);

            // Using the non commercial license of EPPlus
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            // Validate file
            var worksheet = package.Workbook.Worksheets[0];

            if (worksheet == null)
            {
                // Handle invalid file error
                throw new InvalidOperationException("Worksheet could not be opened.");
            }

            return (package, worksheet);
        }

        public static async Task<List<UserCardInfo>> ImportCardsAsync(ExcelWorksheet worksheet)
        {
            var headers = Enumerable.Range(1, 10).Select(col => worksheet.Cells[1, col].Text).ToArray();
            bool validHeaders = ValidateHeaders(headers);
            if (!validHeaders)
            {
                // Handle invalid header error
                throw new ArgumentException("Invalid headers in worksheet", worksheet.Name);
            }
            var cards = await ReadCardsAsync(worksheet);

            return cards;
        }
    }
}
