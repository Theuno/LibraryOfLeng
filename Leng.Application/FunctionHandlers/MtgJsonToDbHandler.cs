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
using System.Text.Json.Serialization;

namespace Leng.Application.FunctionHandlers
{
    public class MtgJsonToDbHandler
    {
        private readonly ILogger<MtgJsonToDbHandler> _logger;
        private readonly IMTGDbService _dbService;

        string fileName = "AllSetFiles.zip";

        public MtgJsonToDbHandler(ILogger<MtgJsonToDbHandler> logger, IMTGDbService dbService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
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
                client.Dispose();
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
                using FileStream importStream = File.OpenRead(file);

                await ImportMTGSet(importStream);
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

        public async Task ImportMTGSet(Stream openStream)
        {
            JsonNode mtgNodes = JsonNode.Parse(openStream);
            JsonObject mtgData = mtgNodes["data"]!.AsObject();

            try
            {
                MTGSets set = JsonSerializer.Deserialize<MTGSets>(mtgData.ToString());

                if(set.isOnlineOnly)
                {
                    _logger.LogInformation("Skipping online only set: " + set.name);
                    return;
                } 
                else if (set.isPartialPreview)
                {
                    _logger.LogInformation("Skipping partial preview set: " + set.name);
                    return;
                }

                if (!set.isOnlineOnly && !set.isPartialPreview)
                {
                    await _dbService.AddSetAsync(set);

                    // Add cards
                    JsonArray mtgCards = mtgNodes["data"]["cards"]!.AsArray();
                    List<MTGCards> setCards = new List<MTGCards>();

                    for (int i = 0; i < mtgCards.Count; i++)
                    {
                        // Foils? Forest317★
                        // Dead?  El-Hajjâj134†

                        MTGCards card = null;
                        try
                        {
                            card = JsonSerializer.Deserialize<MTGCards>(mtgCards[i].ToString());
                        }
                        catch (JsonException je)
                        {
                            Console.WriteLine($"Error when deserializing card! Exception: {je.Message}");
                            return;
                        }
                        if (card != null)
                        {
                            SetCardProperties(card, mtgCards[i]);
                            setCards.Add(card);
                        }
                    }

                    if (setCards.Count > 0)
                    {
                        _logger.LogInformation("Adding " + setCards.Count + " cards for set: " + set.name);
                        await _dbService.AddCardsAsync(setCards);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogInformation("Error: " + ex.Message);
            }
        }

        private void SetCardProperties(MTGCards card, JsonNode cardNode)
        {
            if (!card.isOnlineOnly)
            {
                // Set the colors property to a list of MTGColor objects
                JsonArray colorArray = cardNode["colors"].AsArray();

                if (colorArray.Count > 0)
                {
                    card.color = "";
                    // For each color, add the value to the string
                    foreach (var color in colorArray)
                    {
                        card.color += color;
                    }
                }

                var identifiers = cardNode["identifiers"].AsObject();
                card.scryfallId = identifiers["scryfallId"].ToString();
            }
            else
            {
                _logger.LogInformation("Skipping onlineOnly card: " + card.name);
            }
        }
    }
}