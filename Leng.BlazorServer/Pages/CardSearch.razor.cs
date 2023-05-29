using Leng.Application.Services;
using Leng.BlazorServer.Shared;
using Leng.Domain.Models;
using Leng.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Leng.BlazorServer.Pages
{
    public partial class CardSearch 
    {
        [Inject] IDbContextFactory<LengDbContext> cf { get; set; } = default!;
        [CascadingParameter] private Task<AuthenticationState>? authenticationState { get; set; }

        private string? _selectedCard = string.Empty;
        private List<ShowSheet>? sheet = new List<ShowSheet>();
        private IEnumerable<MTGCards>? cards = new List<MTGCards>();
        
        private CancellationTokenSource _searchCancellationTokenSource = new CancellationTokenSource();

        //private MTGDbService? _dbService { get; set; }
        private LengUser? _lengUser { get; set; }

        protected override async Task OnInitializedAsync()
        {
            //_dbService = new MTGDbService(cf.CreateDbContext());
            var dbService = new MTGDbService(cf.CreateDbContext());


            var msalId = LengAuthenticationService.getMsalId(await authenticationState);
            _lengUser = await dbService.GetLengUserAsync(msalId);

            if (_lengUser == null)
            {
                await dbService.AddLengUserAsync(msalId);
                _lengUser = await dbService.GetLengUserAsync(msalId);
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

                var dbService = new MTGDbService(cf.CreateDbContext());

                var searchedCards = await dbService.getCardsAsync(card, _searchCancellationTokenSource.Token);
                searchedCards = searchedCards.DistinctBy(c => c.name);
                return await Task.FromResult(searchedCards.Select(x => x.name).ToArray());
            }
            catch (OperationCanceledException)
            {
                // Search was canceled, do nothing
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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

            var dbService = new MTGDbService(cf.CreateDbContext());
            cards = await dbService.GetCardsForUserAsync(_lengUser, _selectedCard);
           
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

        private async void CommittedItemChanges(ShowSheet contextCard)
        {
            var card = contextCard;

            if (_selectedCard != null)
            {
                var dbService = new MTGDbService(cf.CreateDbContext());

                //var msalId = LengAuthenticationService.getMsalId(await authenticationState);
                _lengUser = await dbService.GetLengUserAsync(_lengUser.aduuid);

                await dbService.updateCardOfUserAsync(card.number, card.name, card.setCode, card.count, card.countFoil, _lengUser);
            }

            Console.WriteLine(card.name);
            Console.WriteLine(card.number);
            Console.WriteLine(card.count);
            Console.WriteLine(card.countFoil);
        }
    }
}
