﻿using Leng.Application.Services;
using Leng.BlazorServer.Shared;
using Leng.Domain.Models;
using Leng.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;
using static MudBlazor.CategoryTypes;

namespace Leng.BlazorServer.Pages
{
    public partial class QuickDeckCheck
    {
        private LengUser? _lengUser;
        [CascadingParameter] private Task<AuthenticationState>? authenticationState { get; set; }

        public int LoadingValue { get; set; }
        private string _resultList = "";
        private List<ShowSheet>? _resultSheet = new List<ShowSheet>();

        [Inject] IDbContextFactory<LengDbContext> cf { get; set; } = default!;
        //private readonly Regex arenaCardLineRegex = new Regex(@"^(?<count>\d+)\s+(?<name>[\w\s',!\?\.]+)\s*(?<isFoil>\(Foil\))?");
        private readonly Regex arenaCardLineRegex = new Regex(@"^(?<count>\d+)\s+(?<name>[\w\s',!\?\.-]+(?:\s*//\s*[\w\s',!\?\.]+)?)\s*(?<isFoil>\(Foil\))?");
        private readonly Regex mtgoCardLineRegex = new Regex(@"^(?<count>\d+)\s+(?<name>[\w\s',!\?\.-]+)\s+\[(?<setCode>[A-Za-z0-9]+)\]\s+\[(?<cardNumber>\d+)\]");

        protected override async Task OnInitializedAsync()
        {
            var msalId = LengAuthenticationService.getMsalId(await authenticationState);
            var dbService = new MTGDbService(cf.CreateDbContext());

            _lengUser = await dbService.GetLengUserAsync(msalId);
        }

        private async void CommittedItemChanges(ShowSheet contextCard)
        {

        }

        private async void HandleDeckListChange(string deckList)
        {
            LoadingValue = 0;
            var dbService = new MTGDbService(cf.CreateDbContext());

            StringBuilder missingCards = new StringBuilder();
            missingCards.Append("Missing cards: \r");

            StringBuilder errorList = new StringBuilder();
            errorList.Append("Problems found: \r");

            _resultList = "";
            _resultList += missingCards;

            _resultSheet.Clear();

            var lines = deckList.Split('\n', '\r')
                                .Select(line => line.Trim())
                                .Where(line => !string.IsNullOrEmpty(line));

            // Calculate the progress per line
            var progressPerLine = (int)Math.Round(100.0 / lines.Count(), 1);

            foreach (var line in lines)
            {
                LoadingValue += progressPerLine;
                _ = InvokeAsync(StateHasChanged);

                var arenaMatch = arenaCardLineRegex.Match(line);
                var mtgoMatch = mtgoCardLineRegex.Match(line);
                string name;
                int count;

                // Lines from mtggoldfish formats can be skipped
                if (line.StartsWith("Sideboard") ||
                    line.StartsWith("Commander") ||
                    line.StartsWith("Companion") ||
                    line.StartsWith("Deck") ||
                    line.StartsWith("Lands") ||
                    line.StartsWith("Creatures") ||
                    line.StartsWith("Spells") ||
                    line.StartsWith("Planeswalkers") ||
                    line.StartsWith("Artifacts") ||
                    line.StartsWith("Enchantments") ||
                    line.StartsWith("Instants") ||
                    line.StartsWith("Sorceries") || 
                    line.StartsWith("Sideboard") || 
                    line.StartsWith("Maybeboard") || 
                    line.StartsWith("Tok"))
                {
                    continue;
                }
                

                if (arenaMatch.Success)
                {
                    count = int.Parse(arenaMatch.Groups["count"].Value);
                    name = arenaMatch.Groups["name"].Value;
                }
                else if (mtgoMatch.Success)
                {
                    count = int.Parse(mtgoMatch.Groups["count"].Value);
                    name = mtgoMatch.Groups["name"].Value;
                }
                else
                {
                    errorList.Append("Invalid line format: ");
                    errorList.Append(line);
                    continue;
                }

                var collectedCards = await dbService.GetCardFromUserCollectionAsync(_lengUser, name); // .GetCardFromUserCollectionAsync(name);
                if (collectedCards == null)
                {
                    missingCards.AppendLine(name);
                }

                int missingCount = count;
                foreach (var card in collectedCards)
                {
                    // If missingcount is 0, we have enough of this card
                    missingCount = missingCount - card.count - card.countFoil;

                    if (missingCount <= 0)
                    {
                        missingCount = 0;
                    }

                    var imageUrl = $"https://api.scryfall.com/cards/{card.MTGCards.scryfallId}?format=image&version=small";
                    _resultSheet.Add(new ShowSheet { setCode = card.MTGCards.setCode, cardImageUrl = imageUrl, name = card.MTGCards.name, number = card.MTGCards.number, count = card.count, countFoil = card.countFoil });
                }

                if (missingCount > 0)
                {
                    _resultList += $"{missingCount} x {name}\r";
                    _ = InvokeAsync(StateHasChanged);
                }
            }

            LoadingValue = 100;

            _resultList += "\r";
            _resultList += errorList;

            await InvokeAsync(StateHasChanged);
        }
    }

}
