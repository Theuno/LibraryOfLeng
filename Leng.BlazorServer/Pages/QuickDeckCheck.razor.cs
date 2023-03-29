using Leng.Application.Services;
using Leng.Domain.Models;
using Leng.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Leng.BlazorServer.Pages {
    public partial class QuickDeckCheck {
        private string _deckList = "";
        private string _resultList = "";
        [Inject] IDbContextFactory<LengDbContext> cf { get; set; } = default!;
        private readonly Regex cardLineRegex = new Regex(@"^(?<count>\d+)\s+(?<name>[\w\s',!\?\.]+)\s*(?<isFoil>\(Foil\))?");


        private async void HandleDeckListChange(string deckList) {
            var dbService = new MTGDbService(cf.CreateDbContext());

            // Parse the deck list here
            Console.WriteLine(deckList);

            var cards = new List<MTGCards>();

            string missingCards = "";
            var lines = deckList.Split('\n', '\r')
                                .Select(line => line.Trim())
                                .Where(line => !string.IsNullOrEmpty(line));

            foreach (var line in lines) {
                var match = cardLineRegex.Match(line);
                if (!match.Success) {
                    throw new Exception($"Invalid line format: {line}");
                }

                var count = int.Parse(match.Groups["count"].Value);
                var name = match.Groups["name"].Value;
                var isFoil = match.Groups["isFoil"].Success;

                //var card = dbService.getCardAsync(name);
                var collectedCards = await dbService.GetCardFromUserCollectionAsync(name);
                if (collectedCards == null) {
                    throw new Exception($"Card not found: {name}");
                    missingCards += $"{name}\r";
                }

                int missingCount = count;
                foreach (var card in collectedCards) {
                    // if missingcount is 0, we have enough of this card
                    missingCount = missingCount - card.count - card.countFoil;
                    
                    if (missingCount <= 0) {
                        missingCount = 0;
                    }

                    cards.Add(card.MTGCards);
                }

                if (missingCount > 0) {
                    missingCards += $"{name} x{missingCount}\r";
                }
            }

            _resultList = "";
            _resultList += "Available cards: \r";
            foreach (var card in cards) {
                // TODO: card.MTGSets.name is not reverse navigated
                _resultList += $"{card.name} {card.MTGSets.setCode} {card.number}\r"; // {card.MTGSets.name}\r";
            }
            _resultList += "\r";
            _resultList += "Missing cards: \r";
            _resultList += missingCards;

            InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }
    }
}
