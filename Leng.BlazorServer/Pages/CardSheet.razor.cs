using Leng.Domain.Models;
using Leng.Application.Services;
using Leng.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Leng.BlazorServer.Shared;
using Microsoft.AspNetCore.Components.Authorization;

namespace Leng.BlazorServer.Pages {
    public partial class CardSheet {
        private string? selectedSet;
        private IEnumerable<MTGCards>? cards = new List<MTGCards>();
        private List<ShowSheet>? sheet = new List<ShowSheet>();

        [Inject] IDbContextFactory<LengDbContext> cf { get; set; } = default!;
        [CascadingParameter] private Task<AuthenticationState>? authenticationState { get; set; }



        private async void CommittedItemChanges(ShowSheet contextCard) {
            var dbService = new MTGDbService(cf.CreateDbContext());
            var card = contextCard;

            var msalId = LengAuthenticationService.getMsalId(await authenticationState);
            var lengUser = await dbService.GetLengUserAsync(msalId);

            if (selectedSet != null) {
                await dbService.updateCardOfUserAsync(card.number, card.name, card.setCode, card.count, card.countFoil, lengUser);
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

            var sets = await dbService.SearchSetsContainingCardsAsync(set);
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

            var msalId = LengAuthenticationService.getMsalId(await authenticationState);
            var LengUser = await dbService.GetLengUserAsync(msalId);

            var setCode = await dbService.getSetCodeAsync(set);
            cards = await dbService.GetCardsInSetForUserAsync(LengUser, setCode);

            foreach (var card in cards) {
                try
                {
                    var usersCard = card.LengUserMTGCards
                        .Where(u => u.LengUser == LengUser && u.MTGCards == card).SingleOrDefault();


                    var imageUrl = $"https://api.scryfall.com/cards/{card.scryfallId}?format=image&version=small";

                    if (usersCard == null) {
                        sheet.Add(new ShowSheet { setCode = card.MTGSets.setCode, cardImageUrl = imageUrl, name = card.name, number = card.number, count = 0, countFoil = 0 });
                    }
                    else {
                        sheet.Add(new ShowSheet { setCode = card.MTGSets.setCode, cardImageUrl = imageUrl, name = card.name, number = card.number, count = usersCard.count, countFoil = usersCard.countFoil });
                    }
                    Console.WriteLine(card.name);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        protected override async Task OnInitializedAsync() {
            var dbService = new MTGDbService(cf.CreateDbContext());

            var msalId = LengAuthenticationService.getMsalId(await authenticationState);
            var LengUser = await dbService.GetLengUserAsync(msalId);
            if (LengUser == null) {
                await dbService.AddLengUserAsync(msalId);
                _ = await dbService.GetLengUserAsync(msalId);
            }
        }
    }
}
