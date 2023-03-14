using Leng.Application.Services;
using Leng.Data.Models;
using System.ComponentModel;
using System.IO.Compression;
using System.Net;
using System.Text.Json.Nodes;
using System.Text.Json;
using Leng.Infrastructure;

namespace Leng.Application.FunctionHandlers
{
    public class MtgJsonToDbHandler
    {
        MTGDbService dbService = null;
        string fileName = "AllSetFiles.zip";

        public MtgJsonToDbHandler(LengDbContext lengDbContext) {
            dbService = new MTGDbService(lengDbContext);
        }

        public async Task Handle()
        {
            Console.WriteLine("Handling");
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
                Console.WriteLine("Importing file: " + file);
                await ImportMTGSet(file);
            }
        }

        private void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine($"Downloaded {e.BytesReceived} of {e.TotalBytesToReceive} bytes. " +
                $"{e.ProgressPercentage}% complete...");
        }

        private void DownloadFileCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Console.WriteLine("Download has been cancelled.");
            }
            else
            {
                Console.WriteLine("Download completed!");
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
                    for (int i = 0; i < mtgCards.Count; i++) {
                        // Foils? Forest317★
                        // Dead?  El-Hajjâj134†

                        MTGCards card = JsonSerializer.Deserialize<MTGCards>(mtgCards[i].ToString());
                        if(!card.isOnlineOnly) {
                            await dbService.AddCardAsync(card);
                        }
                    }
                }

            }
            catch (Exception ex) {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}