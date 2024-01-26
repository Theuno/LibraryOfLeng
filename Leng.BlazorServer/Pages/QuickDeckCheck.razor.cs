using Leng.Application.Services;
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

        public int _loadingValue { get; set; }
        private StringBuilder _resultList;
        private readonly List<ShowSheet> _resultSheet = new List<ShowSheet>();

        [Inject]
        public IMTGDbService? DbService { get; set; }

        private readonly Regex arenaCardLineRegex = new Regex(@"^(?<count>\d+)\s+(?<name>[\w\s',!\?\.-]+(?:\s*//\s*[\w\s',!\?\.]+)?)\s*(?<isFoil>\(Foil\))?");
        private readonly Regex mtgoCardLineRegex = new Regex(@"^(?<count>\d+)\s+(?<name>[\w\s',!\?\.-]+)\s+\[(?<setCode>[A-Za-z0-9]+)\]\s+\[(?<cardNumber>\d+)\]");

        public QuickDeckCheck()
        {
            _resultList = new StringBuilder();
        }

        protected override async Task OnInitializedAsync()
        {
            if(authenticationState != null || DbService != null)
            {
                var msalId = LengAuthenticationService.getMsalId(await authenticationState);
                _lengUser = await DbService.GetLengUserAsync(msalId);
            }
        }

        private async Task CommittedItemChanges(ShowSheet contextCard)
        {

        }

        private async Task HandleDeckListChangeAsync(string deckList)
        {
            _loadingValue = 0;

            if(DbService == null)
            {
                _resultList.Append("No database connection");
                return;
            }

            /*
            // The cards we have in our collection, grouped by name
            var cardsByName = cards.GroupBy(c => c.Name).ToDictionary(g => g.Key, g => g.ToList());
            // The cards we have in our collection, grouped by name and set
            var cardsByNameAndSet = cards.GroupBy(c => new { c.Name, c.SetCode }).ToDictionary(g => g.Key, g => g.ToList());
            // The cards we have in our collection, grouped by name and set and number
            var cardsByNameAndSetAndNumber = cards.GroupBy(c => new { c.Name, c.SetCode, c.Number }).ToDictionary(g => g.Key, g => g.ToList());
            // The cards we have in our collection, grouped by name and set and number and isFoil
            var cardsByNameAndSetAndNumberAndIsFoil = cards.GroupBy(c => new { c.Name, c.SetCode, c.Number, c.IsFoil }).ToDictionary(g => g.Key, g => g.ToList());
            */

            StringBuilder missingCards = new StringBuilder();
            missingCards.Append("Missing cards: \r");

            StringBuilder errorList = new StringBuilder();
            errorList.Append("Problems found: \r");

            _resultList = new StringBuilder();
            _resultList.Append(missingCards);

            _resultSheet.Clear();

            var lines = deckList.Split('\n', '\r')
                                .Select(line => line.Trim())
                                .Where(line => !string.IsNullOrEmpty(line));

            // Calculate the progress per line
            var progressPerLine = (int)Math.Round(100.0 / lines.Count(), 1);

            foreach (var line in lines)
            {
                _loadingValue += progressPerLine;
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

                var collectedCards = await DbService.GetCardFromUserCollectionAsync(_lengUser, name);
                if (collectedCards == null)
                {
                    missingCards.AppendLine(name);
                    continue;
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
                    _resultList.Append($"{missingCount} x {name}\r");
                    _ = InvokeAsync(StateHasChanged);
                }
            }

            _loadingValue = 100;

            _resultList.Append('\r');
            _resultList.Append(errorList);

            await InvokeAsync(StateHasChanged);
        }
    }
}
