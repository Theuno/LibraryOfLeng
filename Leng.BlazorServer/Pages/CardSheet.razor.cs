﻿using Leng.Application.Services;
using Leng.BlazorServer.Shared;
using Leng.Domain.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

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

        public string SortBySetNumber(MTGCards card)
        {
            try
            {
                if (card.number == null)
                {
                    return "0";
                }
                else
                {
                    int parseResult = 0;
                    if (int.TryParse(card.number, out parseResult))
                    {
                        return parseResult.ToString();
                    }
                    else
                    {
                        char[] trimChars = { ' ', '★', '†', 'a', 'b', 'c' };
                        return card.number.Trim(trimChars);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error when sorting by set number: {message}", ex.Message);
                return "0";
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
