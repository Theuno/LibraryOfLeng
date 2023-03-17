using Leng.Data.Models;
using Leng.Application.Services;
using Leng.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Leng.BlazorServer.Pages {
    public partial class CardSheet {
        private string? selectedSet;
        private IEnumerable<MTGCards>? cards = new List<MTGCards>();
        private List<cardSheet>? sheet = new List<cardSheet>();

        [Inject] IDbContextFactory<LengDbContext> cf { get; set; } = default!;

        public CardSheet() {
        }

        private class cardSheet {
            public string name { get; set; }
            public string number { get; set; }
            public int count { get; set; }
        }

        private async void CommittedItemChanges(cardSheet context) {
            var dbService = new MTGDbService(cf.CreateDbContext());

            var card = context;
            var LengUser = dbService.GetLengUserAsync("123TODOUSER");
            LengUser.Wait();

            if (selectedSet != null) {
                await dbService.updateCardOfUserAsync(card.number, card.name, selectedSet, card.count, LengUser.Result);
            }

            Console.WriteLine(card.name);
            Console.WriteLine(card.number);
            Console.WriteLine(card.count);
        }

        public string SortBySetNumber(MTGCards card) {
            try {
                if (card.number == null) {
                    return "0";
                }
                else {
                    int parseResult = 0;
                    if (int.TryParse(card.number, out parseResult)) {
                        return parseResult.ToString();
                    }
                    else {
                        char[] trimChars = { ' ', '★', '†', 'a', 'b', 'c' };
                        return card.number.Trim(trimChars);
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return "0";
            }
        }

        private async Task<IEnumerable<string>> SetSearch(string set) {
            var dbService = new MTGDbService(cf.CreateDbContext());

            var sets = await dbService.SearchSetsAsync(set);
            return await Task.FromResult(sets.Select(x => x.name).ToArray());
        }

        public async Task OnSetSelected(string set) {
            selectedSet = set;
            if (selectedSet == null) {
                return;
            }
            else {
                sheet.Clear();
            }

            var dbService = new MTGDbService(cf.CreateDbContext());

            var LengUser = await dbService.GetLengUserAsync("123TODOUSER");
            var setCode = await dbService.getSetCodeAsync(set);
            cards = await dbService.GetCardsForUser(LengUser, setCode);

            foreach (var card in cards) {
                var usersCard = card.LengUserMTGCards
                    .Where(u => u.LengUserID == LengUser.LengUserID && u.MTGCardsID == card.MTGCardsID).SingleOrDefault();
                if (usersCard == null) {
                    sheet.Add(new cardSheet { name = card.name, number = card.number, count = 0 });
                }
                else {
                    sheet.Add(new cardSheet { name = card.name, number = card.number, count = usersCard.count });
                }
                Console.WriteLine(card.name);
            }
        }

        protected override async Task OnInitializedAsync() {
            var dbService = new MTGDbService(cf.CreateDbContext());
            var LengUser = await dbService.GetLengUserAsync("123TODOUSER");
            if (LengUser == null) {
                await dbService.AddLengUserAsync("123TODOUSER");
                LengUser = await dbService.GetLengUserAsync("123TODOUSER");
            }
        }
    }
}
