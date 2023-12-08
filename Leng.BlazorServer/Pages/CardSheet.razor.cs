using Leng.Application.Services;
using Leng.BlazorServer.Shared;
using Leng.Domain.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using static MudBlazor.CategoryTypes;

namespace Leng.BlazorServer.Pages
{
    public partial class CardSheet
    {
        private string? selectedSet;
        private IEnumerable<MTGCards>? cards = new List<MTGCards>();
        private List<ShowSheet>? sheet = new List<ShowSheet>();

        [Inject]
        public IMTGDbService DbService { get; set; }
        [Inject]
        public ILogger<CardSheet> Logger { get; set; }

        [CascadingParameter] private Task<AuthenticationState>? authenticationState { get; set; }
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public void Dispose()
        {
            _cts?.Dispose();
        }

        private async Task CommittedItemChanges(ShowSheet contextCard)
        {
            var card = contextCard;

            var msalId = LengAuthenticationService.getMsalId(await authenticationState);
            var lengUser = await DbService.GetLengUserAsync(msalId);

            if (selectedSet != null)
            {
                await DbService.updateCardOfUserAsync(card.number, card.name, card.setCode, card.count, card.countFoil, lengUser);
            }

            Logger.LogInformation("CardSheet: {name} {number} {count}", card.name, card.number, card.count);
        }

        // Custom comparer to handle special characters in cardNumber
        public class CustomCardNumberComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                // Handle nulls
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                // Split the card numbers into parts (numeric and non-numeric)
                var xParts = SplitCardNumber(x);
                var yParts = SplitCardNumber(y);

                for (int i = 0; i < Math.Min(xParts.Length, yParts.Length); i++)
                {
                    if (int.TryParse(xParts[i], out int xPartNum) && int.TryParse(yParts[i], out int yPartNum))
                    {
                        // Compare numeric parts
                        int comparison = xPartNum.CompareTo(yPartNum);
                        if (comparison != 0) return comparison;
                    }
                    else
                    {
                        // Compare non-numeric parts
                        int comparison = string.Compare(xParts[i], yParts[i], StringComparison.Ordinal);
                        if (comparison != 0) return comparison;
                    }
                }

                // If one number is longer than the other, it should come after
                return xParts.Length.CompareTo(yParts.Length);
            }

            private string[] SplitCardNumber(string cardNumber)
            {
                // Split the card number into numeric and non-numeric parts
                // This can be adjusted based on the exact format of your card numbers
                return System.Text.RegularExpressions.Regex.Split(cardNumber, "([^0-9]+)");
            }
        }

        private async Task<IEnumerable<string>> SetSearch(string query)
        {
            // Cancel the previous task (if any)
            _cts.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                // Call the search method, passing the CancellationToken
                return await LoadDataForSearchQuery(query, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                Logger.LogInformation("Search operation cancelled");
                return Enumerable.Empty<string>();
                // TODO: actually returning the list from before the cancellation is better
            }
        }

        private async Task<IEnumerable<string>> LoadDataForSearchQuery(string query, CancellationToken cancellationToken)
        {
            // Fetch sets based on the query, but frequently check for cancellation
            List<MTGSets> setsList = await DbService.SearchSetsContainingCardsAsync(query, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            return setsList.Select(set => set.name);
        }

        public async Task OnSetSelected(string set)
        {
            selectedSet = set;
            if (selectedSet == null)
            {
                return;
            }
            else
            {
                sheet.Clear();
            }

            var msalId = LengAuthenticationService.getMsalId(await authenticationState);
            var LengUser = await DbService.GetLengUserAsync(msalId);

            var setCode = await DbService.GetSetCodeAsync(set);
            var cardTuples = await DbService.GetCardsInSetForUserAsync(LengUser, setCode);

            // Sort the cards using the CustomCardNumberComparer
            var comparer = new CustomCardNumberComparer();
            cardTuples = cardTuples.OrderBy(card => card.card.number, comparer).ToList();

            foreach (var card in cardTuples)
            {
                try
                {
                    var imageUrl = $"https://api.scryfall.com/cards/{card.card.scryfallId}?format=image&version=small";
                    sheet.Add(new ShowSheet { setCode = card.card.setCode, cardImageUrl = imageUrl, name = card.card.name, number = card.card.number, count = card.count, countFoil = card.countFoil });
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error when adding card to sheet: {message}", ex.Message);
                }
            }
        }

        protected override async Task OnInitializedAsync()
        {
            var msalId = LengAuthenticationService.getMsalId(await authenticationState);
            var LengUser = await DbService.GetLengUserAsync(msalId);
            if (LengUser == null) 
            {
                await DbService.AddLengUserAsync(msalId);
                _ = await DbService.GetLengUserAsync(msalId);
            }
        }
    }
}
