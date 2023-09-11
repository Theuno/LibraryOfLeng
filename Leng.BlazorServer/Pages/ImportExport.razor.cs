﻿using Leng.Application.Services;
using Leng.Domain.Models;
using Leng.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;
using OfficeOpenXml; //? EPPlus
using System.IO;

namespace Leng.BlazorServer.Pages
{
    public partial class ImportExportBase : ComponentBase
    {
        protected MudDialog importDialog;
        protected bool uploading = false;



        protected async Task UploadFiles(IBrowserFile file)
        {
            if (file == null)
            {
                // Handle null file error
                return;
            }

            if (file.Size > 10485760) // 10 MB limit
            {
                // Handle file size exceeded error
                return;
            }

            if (!file.Name.EndsWith(".xlsx"))
            {
                // Handle invalid file type error
                return;
            }

            Stream stream = file.OpenReadStream();
            string filename = file.Name;
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var path = Path.Combine(uploadDir, filename);

            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            FileStream fs = File.Create(path);
            await stream.CopyToAsync(fs);
            stream.Close();
            fs.Close();
        }

        protected void CloseImportDialog()
        {
            importDialog.Close();
        }
    }

    public partial class ImportExport
    {
        private LengUser? _lengUser { get; set; }
        [CascadingParameter] private Task<AuthenticationState>? authenticationState { get; set; }

        [Inject] IJSRuntime JS { get; set; } = default!;
        [Inject] IDbContextFactory<LengDbContext> cf { get; set; } = default!;


        protected override async Task OnInitializedAsync()
        {
            var msalId = LengAuthenticationService.getMsalId(await authenticationState);
            var dbService = new MTGDbService(cf.CreateDbContext());

            _lengUser = await dbService.GetLengUserAsync(msalId);
        }

        // Function to export all cards on a single sheet
        public async Task ExportAllCardsAsync()
        {
            var dbService = new MTGDbService(cf.CreateDbContext());
            var cards = await dbService.GetAllCardsFromUserCollectionAsync(_lengUser);

            // Using the non commercial license of EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("All Cards");
                worksheet.Cells[1, 1].Value = "Card Name";
                worksheet.Cells[1, 2].Value = "Card Number";
                worksheet.Cells[1, 3].Value = "Set Code";
                worksheet.Cells[1, 4].Value = "Count";
                worksheet.Cells[1, 5].Value = "Count Foil";

                int row = 2;
                foreach (var card in cards)
                {
                    worksheet.Cells[row, 1].Value = card.MTGCards.name;
                    worksheet.Cells[row, 2].Value = card.MTGCards.number;
                    worksheet.Cells[row, 3].Value = card.MTGCards.setCode;
                    //worksheet.Cells[row, 4].Value = card.LengUserMTGCards.Sum(c => c.count);
                    worksheet.Cells[row, 4].Value = card.count;
                    worksheet.Cells[row, 5].Value = card.countFoil;
                    row++;
                }

                await SaveAndDownloadExcelPackage(package, "AllCards.xlsx");
            }
        }

        //public async Task ImportCardsAsync()
        //{
        //    // Validate headers
        //    var validHeaders1 = new[] { "Card Name", "Card Number", "Set Code", "Count", "Count Foil", "have", "have foil", "want", "want foil", "note" };
        //    var validHeaders2 = new[] { "kaartnaam", "kaartnummer", "set_code", "c", "c_foil", "h", "h_foil", "w", "w_foil", "notitie" };

        //    var headers = Enumerable.Range(1, 10).Select(col => worksheet.Cells[1, col].Text).ToArray();
        //    if (!headers.SequenceEqual(validHeaders1) && !headers.SequenceEqual(validHeaders2))
        //    {
        //        // Handle invalid header error
        //        return;
        //    }
        //}

        // Function to export cards per set on a separate sheet
        public async Task ExportCardsPerSetAsync()
        {
            var dbService = new MTGDbService(cf.CreateDbContext());
            var sets = await dbService.GetAllSetsAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            foreach (var set in sets)
            {
                var worksheet = package.Workbook.Worksheets.Add(set.setCode);
                worksheet.Cells[1, 1].Value = "Card Name";
                worksheet.Cells[1, 2].Value = "Card Number";
                worksheet.Cells[1, 3].Value = "Count";
                worksheet.Cells[1, 4].Value = "Count Foil";

                int row = 2;
                foreach (var card in set.Cards)
                {
                    var userCard = card.LengUserMTGCards.SingleOrDefault(c => c.LengUser.LengUserID == _lengUser.LengUserID);
                    if (userCard != null)
                    {
                        worksheet.Cells[row, 1].Value = card.name;
                        worksheet.Cells[row, 2].Value = card.number;
                        worksheet.Cells[row, 3].Value = userCard.count;
                        worksheet.Cells[row, 4].Value = userCard.countFoil;
                        row++;
                    }
                }

                worksheet.Cells[row, 1].Value = "Total:";
                worksheet.Cells[row, 3].Formula = $"SUM(C2:C{row - 1})";
                worksheet.Cells[row, 4].Formula = $"SUM(D2:D{row - 1})";
            }

            //var tempFilePath = await SaveExcelPackageToFileAsync(package, "CardsPerSet.xlsx");

            // Return a download link to the client
            //var fileName = Path.GetFileName(tempFilePath);
            //var downloadUrl = Url.Action("DownloadExcelFile", new { filePath = tempFilePath, fileName });
            //await MudDialogService.ShowMessageBoxAsync($"The Excel file has been generated. <a href='{downloadUrl}'>Click here to download</a>.", "Success");
            await SaveAndDownloadExcelPackage(package, "CardsPerSet.xlsx");
        }

        private async Task SaveAndDownloadExcelPackage(ExcelPackage package, string fileName)
        {
            var stream = new MemoryStream(package.GetAsByteArray());

            using var streamRef = new DotNetStreamReference(stream: stream);
            await JS.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
        }
    }
}
