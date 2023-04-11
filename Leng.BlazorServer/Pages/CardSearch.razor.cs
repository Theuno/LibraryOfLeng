using Leng.Application.Services;
using Leng.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Leng.BlazorServer.Shared;
using static MudBlazor.CategoryTypes;
using Leng.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication;

namespace Leng.BlazorServer.Pages
{
    public partial class CardSearch 
    {
        [Inject] IDbContextFactory<LengDbContext> cf { get; set; } = default!;
        [CascadingParameter] private Task<AuthenticationState>? authenticationState { get; set; }

        private string? _selectedCard = string.Empty;
        private List<ShowSheet>? sheet = new List<ShowSheet>();
        private IEnumerable<MTGCards>? cards = new List<MTGCards>();

        private MTGDbService? _dbService { get; set; }
        private LengUser? _lengUser { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _dbService = new MTGDbService(cf.CreateDbContext());

            var msalId = LengAuthenticationService.getMsalId(await authenticationState);
            _lengUser = await _dbService.GetLengUserAsync(msalId);

            if (_lengUser == null)
            {
                await _dbService.AddLengUserAsync(msalId);
                _lengUser = await _dbService.GetLengUserAsync(msalId);
            }
        }

        private async Task<IEnumerable<string>> SearchForCard(string card)
        {
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
                var cards = await _dbService.getCardsAsync(card);
                cards = cards.DistinctBy(c => c.name);
                return await Task.FromResult(cards.Select(x => x.name).ToArray());
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

            cards = await _dbService.GetCardsForUserAsync(_lengUser, _selectedCard);

            foreach (var card in cards)
            {
                var usersCard = card.LengUserMTGCards
                    .Where(u => u.LengUser == _lengUser && u.MTGCards == card).SingleOrDefault();
                if (usersCard == null)
                {
                    sheet.Add(new ShowSheet { setCode = card.MTGSets.setCode, name = card.name, number = card.number, count = 0, countFoil = 0 });
                }
                else
                {
                    sheet.Add(new ShowSheet { setCode = card.MTGSets.setCode, name = card.name, number = card.number, count = usersCard.count, countFoil = usersCard.countFoil });
                }
                Console.WriteLine(card.name);
            }
        }

        private async void CommittedItemChanges(ShowSheet contextCard)
        {
            var card = contextCard;

            if (_selectedCard != null)
            {
                await _dbService.updateCardOfUserAsync(card.number, card.name, card.setCode, card.count, card.countFoil, _lengUser);
            }

            Console.WriteLine(card.name);
            Console.WriteLine(card.number);
            Console.WriteLine(card.count);
            Console.WriteLine(card.countFoil);
        }
    }
}
