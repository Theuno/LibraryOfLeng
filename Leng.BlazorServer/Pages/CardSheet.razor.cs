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
        [Inject]
        public IMTGDbService DbService { get; set; }
        [CascadingParameter] private Task<AuthenticationState>? authenticationState { get; set; }


        private async Task CommittedItemChanges(ShowSheet contextCard) {
            var card = contextCard;

            var msalId = LengAuthenticationService.getMsalId(await authenticationState);
            var lengUser = await DbService.GetLengUserAsync(msalId);

            if (selectedSet != null) {
                await DbService.updateCardOfUserAsync(card.number, card.name, card.setCode, card.count, card.countFoil, lengUser);
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
            var sets = await DbService.SearchSetsContainingCardsAsync(set);
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

            var msalId = LengAuthenticationService.getMsalId(await authenticationState);
            var LengUser = await DbService.GetLengUserAsync(msalId);

            var setCode = await DbService.GetSetCodeAsync(set);
            cards = await DbService.GetCardsInSetForUserAsync(LengUser, setCode);

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
            var msalId = LengAuthenticationService.getMsalId(await authenticationState);
            var LengUser = await DbService.GetLengUserAsync(msalId);
            if (LengUser == null) {
                await DbService.AddLengUserAsync(msalId);
                _ = await DbService.GetLengUserAsync(msalId);
            }
        }
    }
}
