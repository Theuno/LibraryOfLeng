using Leng.Application.FunctionHandlers;
using Leng.Application.Services;
using Leng.BlazorServer.Shared;
using Leng.Domain.Models;
using Leng.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection.Metadata;

namespace Leng.BlazorServer.Pages
{
    public partial class CardSearch 
    {
        //private MtgJsonToDbHandler _handler;

        [Inject] IDbContextFactory<LengDbContext> cf { get; set; } = default!;

        [Inject]
        public IMTGDbService DbService { get; set; }

        [CascadingParameter] private Task<AuthenticationState>? authenticationState { get; set; }

        private string? _selectedCard = string.Empty;
        private readonly List<ShowSheet>? sheet = new List<ShowSheet>();
        private IEnumerable<MTGCards>? cards = new List<MTGCards>();
        
        private CancellationTokenSource _searchCancellationTokenSource = new CancellationTokenSource();

        private LengUser? _lengUser { get; set; }

        public CardSearch(IDbContextFactory<LengDbContext> contextFactory)
        {
            DbService = new MTGDbService(contextFactory);
            //_handler = new MtgJsonToDbHandler(LoggerFactory.CreateLogger<MtgJsonToDbHandler>(), DbService);
        }

        protected override async Task OnInitializedAsync()
        {
            var msalId = LengAuthenticationService.getMsalId(await authenticationState);
            _lengUser = await DbService.GetLengUserAsync(msalId);

            if (_lengUser == null)
            {
                await DbService.AddLengUserAsync(msalId);
                _lengUser = await DbService.GetLengUserAsync(msalId);
            }
        }

        private async Task<IEnumerable<string>> SearchForCard(string card)
        {
            // Debounce the search input
            await Task.Delay(300, _searchCancellationTokenSource.Token);

            if (card == null)
            {
                return null;
            }
            else
            {
                sheet.Clear();
            }

            try
            {
                // Cancel any previous searches
                _searchCancellationTokenSource.Cancel();
                _searchCancellationTokenSource = new CancellationTokenSource();

                var searchedCards = await DbService.getCardsAsync(card, _searchCancellationTokenSource.Token);
                searchedCards = searchedCards.DistinctBy(c => c.name);
                return await Task.FromResult(searchedCards.Select(x => x.name).ToArray());
            }
            catch (OperationCanceledException)
            {
                // Search was canceled, do nothing
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return null;
        }

        public async Task OnCardSelected(string selectedCard)
        {
            _selectedCard = selectedCard;
            if (_selectedCard == null)
            {
                return;
            }
            else
            {
                sheet.Clear();
            }

            cards = await DbService.GetCardsForUserAsync(_lengUser, _selectedCard);
           
            foreach (var card in cards)
            {
                //var usersCard = card.LengUserMTGCards
                //    .Where(u => u.LengUser == _lengUser && u.MTGCards == card).SingleOrDefault();
                var usersCard = card.LengUserMTGCards.Where(c => c.MTGCards.MTGCardsID == card.MTGCardsID).SingleOrDefault();
                var imageUrl = $"https://api.scryfall.com/cards/{card.scryfallId}?format=image&version=small";

                if (usersCard == null)
                {
                    sheet.Add(new ShowSheet { setCode = card.MTGSets.setCode, cardImageUrl = imageUrl, name = card.name, number = card.number, count = 0, countFoil = 0, want = 0, wantFoil = 0 });
                }
                else
                {
                    sheet.Add(new ShowSheet { setCode = card.MTGSets.setCode, cardImageUrl = imageUrl, name = card.name, number = card.number, count = usersCard.count, countFoil = usersCard.countFoil });
                }
                Console.WriteLine(card.name);
            }
        }

        private async Task CommittedItemChanges(ShowSheet contextCard)
        {
            var card = contextCard;

            if (_selectedCard != null)
            {
                _lengUser = await DbService.GetLengUserAsync(_lengUser.aduuid);

                await DbService.updateCardOfUserAsync(card.number, card.name, card.setCode, card.count, card.countFoil, _lengUser);
            }

            Console.WriteLine(card.name);
            Console.WriteLine(card.number);
            Console.WriteLine(card.count);
            Console.WriteLine(card.countFoil);
        }
    }
}
