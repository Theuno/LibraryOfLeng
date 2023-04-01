using Leng.Application.Services;
using Leng.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Leng.BlazorServer.Shared;
using static MudBlazor.CategoryTypes;
using Leng.Domain.Models;

namespace Leng.BlazorServer.Pages {
    public partial class CardSearch {

        [Inject] IDbContextFactory<LengDbContext> cf { get; set; } = default!;

        private string? _selectedCard = string.Empty;
        private List<ShowSheet>? sheet = new List<ShowSheet>();
        private IEnumerable<MTGCards>? cards = new List<MTGCards>();


        private async Task<IEnumerable<string>> SearchForCard(string card) {
            if (card == null) {
                return null;
            }
            else {
                sheet.Clear();
            }

            var dbService = new MTGDbService(cf.CreateDbContext());

            try {
                var cards = await dbService.getCardsAsync(card);
                cards = cards.DistinctBy(c => c.name);
                return await Task.FromResult(cards.Select(x => x.name).ToArray());
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }

            return null;
        }

        public async Task OnCardSelected(string selectedCard) {
            _selectedCard = selectedCard;
            if (_selectedCard == null) {
                return;
            }
            else {
                sheet.Clear();
            }

            var dbService = new MTGDbService(cf.CreateDbContext());

            var LengUser = await dbService.GetLengUserAsync("123TODOUSER");

            cards = await dbService.GetCardsForUserAsync(LengUser, _selectedCard);

            foreach (var card in cards) {
                var usersCard = card.LengUserMTGCards
                    .Where(u => u.LengUser == LengUser && u.MTGCards == card).SingleOrDefault();
                if (usersCard == null) {
                    sheet.Add(new ShowSheet { setCode = card.MTGSets.setCode, name = card.name, number = card.number, count = 0, countFoil = 0 });
                }
                else {
                    sheet.Add(new ShowSheet { setCode = card.MTGSets.setCode, name = card.name, number = card.number, count = usersCard.count, countFoil = usersCard.countFoil });
                }
                Console.WriteLine(card.name);
            }
        }

        private async void CommittedItemChanges(ShowSheet contextCard) {
            var dbService = new MTGDbService(cf.CreateDbContext());

            var card = contextCard;
            var LengUser = dbService.GetLengUserAsync("123TODOUSER");
            LengUser.Wait();

            // TODO: Commit changes for cards from search page
            if (_selectedCard != null) {
                await dbService.updateCardOfUserAsync(card.number, card.name, card.setCode, card.count, card.countFoil, LengUser.Result);
            }

            Console.WriteLine(card.name);
            Console.WriteLine(card.number);
            Console.WriteLine(card.count);
            Console.WriteLine(card.countFoil);
        }
    }
}
