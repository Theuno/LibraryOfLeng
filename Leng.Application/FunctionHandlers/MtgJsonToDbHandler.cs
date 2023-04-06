using Leng.Application.Services;
using Leng.Domain.Models;
using System.ComponentModel;
using System.IO.Compression;
using System.Net;
using System.Text.Json.Nodes;
using System.Text.Json;
using Leng.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Leng.Application.FunctionHandlers
{
    public class MtgJsonToDbHandler
    {
        MTGDbService dbService = null;
        string fileName = "AllSetFiles.zip";
        private readonly ILogger _logger;


        public MtgJsonToDbHandler(ILogger logger, LengDbContext lengDbContext) {
            _logger = logger;

            // This doesn't work from the Function, it will return that the migrations are already done.
            lengDbContext.Database.Migrate();

            dbService = new MTGDbService(lengDbContext);
        }

        public async Task Handle()
        {
            _logger.LogInformation($"Handling MtgJsonToDbHandler");
            await DownloadFileFromURLAsync();
            ExtractDownloadedJson();
            await ImportFiles();
        }
        private async Task DownloadFileFromURLAsync()
        {
            string url = "https://mtgjson.com/api/v5/" + fileName;
            string target = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            if (!File.Exists(target))
            {
                var client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompletedCallback);

                await client.DownloadFileTaskAsync(url, target);
            }
        }

        private void ExtractDownloadedJson()
        {
            string target = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            var targetDirectory = Directory.GetCurrentDirectory() + "\\AllSetFiles";
            ZipFile.ExtractToDirectory(target, targetDirectory, true);
        }

        private async Task ImportFiles()
        {
            var targetDirectory = Directory.GetCurrentDirectory() + "\\AllSetFiles";
            string[] filesToImport = Directory.GetFiles(targetDirectory, "*.json");

            foreach (var file in filesToImport)
            {
                _logger.LogInformation("Importing file: " + file);

                await ImportMTGSet(file);
            }
        }

        private void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            _logger.LogInformation($"Downloaded {e.BytesReceived} of {e.TotalBytesToReceive} bytes. " +
                $"{e.ProgressPercentage}% complete...");
        }

        private void DownloadFileCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                _logger.LogInformation("Download has been cancelled.");
            }
            else
            {
                _logger.LogInformation("Download completed!");
            }
        }

        private async Task ImportMTGSet(string file) {
            using FileStream openStream = File.OpenRead(file);

            JsonNode mtgNodes = JsonNode.Parse(openStream);
            JsonObject mtgData = mtgNodes["data"]!.AsObject();

            try {
                MTGSets set = JsonSerializer.Deserialize<MTGSets>(mtgData.ToString());

                if (!set.isOnlineOnly && !set.isPartialPreview) {
                    await dbService.AddSetAsync(set);

                    // Add cards
                    JsonArray mtgCards = mtgNodes["data"]["cards"]!.AsArray();
                    List<MTGCards> setCards = new List<MTGCards>();

                    for (int i = 0; i < mtgCards.Count; i++) {
                        // Foils? Forest317★
                        // Dead?  El-Hajjâj134†

                        MTGCards card = JsonSerializer.Deserialize<MTGCards>(mtgCards[i].ToString());
                        if(!card.isOnlineOnly) {
                            setCards.Add(card);
                        }
                    }

                    if (setCards.Count > 0) {
                        await dbService.AddCardsAsync(setCards);
                    }
                }

            }
            catch (Exception ex) {
                _logger.LogInformation("Error: " + ex.Message);
            }
        }
    }
}