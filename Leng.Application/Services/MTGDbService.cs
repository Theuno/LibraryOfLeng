using Leng.Domain.Models;
using Leng.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Leng.Application.Services
{
    public class MTGDbService
    {
        private readonly LengDbContext _dbContext;
        public MTGDbService(LengDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddSetAsync(MTGSets set)
        {
            if (set != null)
            {
                var dbSetWhere = _dbContext.MTGSets.Where(r => r.setCode == set.setCode).SingleOrDefault();

                if (dbSetWhere == null)
                {
                    await _dbContext.MTGSets.AddAsync(set);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
        public async Task<List<MTGSets>> GetAllSetsAsync()
        {
            return await _dbContext.MTGSets.OrderBy(x => x.name).ToListAsync();
        }

        // Get SetCode for set
        public async Task<string>? GetSetCodeAsync(string setName)
        {
            if (string.IsNullOrEmpty(setName))
            {
                throw new ArgumentException("Set name cannot be null or empty", nameof(setName));
            }

            var set = await _dbContext.MTGSets.FirstOrDefaultAsync(s => s.name == setName);

            return set?.setCode;
        }

        public async Task<MTGSets?> GetSetAsync(string setCode)
        {
            if (!string.IsNullOrEmpty(setCode) && _dbContext.MTGSets != null)
            {
                if (setCode.Length >= 3 && setCode.Length <= 5)
                {
                    var set = await _dbContext.MTGSets.Where(set => set.setCode == setCode).FirstOrDefaultAsync();
                    return set;
                }
                else
                {
                    throw new ArgumentException("Value must be between 3 and 5 characters.", nameof(setCode));
                }
            }

            return new MTGSets();
        }

        public async Task<List<MTGSets>> SearchSetsContainingCardsAsync(string mtgset)
        {
            // if text is null or empty, show complete list
            if (string.IsNullOrEmpty(mtgset))
            {
                return await GetAllSetsAsync();
            }
            else
            {
                List<MTGSets> list = await _dbContext.MTGSets
                        .Include(s => s.Cards)
                        .Where(x => x.name.Contains(mtgset) && x.Cards.Count != 0).ToListAsync();
                return list;
            }
        }

        // This function gets the cards from the database matching a part of the name
        // It has preference for cards that start with the name, but if there are no matches, it will return cards that contain the name
        public async Task<IEnumerable<MTGCards>> getCardsAsync(string cardName, CancellationToken cancellationToken)
        {
            var cards = await _dbContext.MTGCard
                .Where(c => c.name.StartsWith(cardName))
                .Include(cards => cards.LengUserMTGCards)
                .Take(20) // Limit the results to a certain number
                .OrderBy(c => c.name)
                .ToListAsync(cancellationToken);

            // if no cards match, do a contains match.
            if (cards.Count == 0)
            {
                cards = await _dbContext.MTGCard
                    .Where(c => c.name.Contains(cardName))
                    .Include(cards => cards.LengUserMTGCards)
                    .Take(20) // Limit the results to a certain number
                    .OrderBy(c => c.name)
                    .ToListAsync(cancellationToken);
            }
            return cards;
        }

        public async Task AddCardAsync(MTGCards card)
        {
            var set = _dbContext.MTGSets.Where(r => r.setCode == card.setCode).SingleOrDefault();

            if (set != null)
            {
                if (set.Cards == null)
                {
                    // TODO: This should have been part of the init somewhere?
                    set.Cards = new List<MTGCards>();
                }

                MTGCards dbCard = await _dbContext.MTGCard.Where(
                    c =>
                        (c.name == card.name) &&
                        (c.number == card.number) &&
                        (c.setCode == card.setCode) &&
                        (card.side == "a" || card.side == null)).SingleOrDefaultAsync();

                if (dbCard == null)
                {
                    set.Cards.Add(card);
                    _dbContext.SaveChanges();
                }
                //else
                //{
                //    TODO: Fix update
                //    dbCard.artist = card.artist;
                //    dbCard.text = card.text;
                //    dbCard.setCode = card.setCode;
                //    dbCard.hasFoil = card.hasFoil;
                //    dbCard.hasNonFoil = card.hasNonFoil;
                //    dbCard.number = card.number;

                //    _dbContext.MTGCard.Update(dbCard);
                //    _dbContext.SaveChanges();
                //}
            }
        }

        internal async Task AddCardsAsync(List<MTGCards> setCards)
        {
            var set = _dbContext.MTGSets.Where(r => r.setCode == setCards.FirstOrDefault().setCode).SingleOrDefault();
            if (set != null && set.Cards == null)
            {
                set.Cards = new List<MTGCards>();
            }

            foreach (MTGCards card in setCards)
            {
                if (card.name != null &&
                    card.number != null &&
                    card.setCode != null &&
                    (card.side == "a" || card.side == null))
                {

                    MTGCards dbCard = await _dbContext.MTGCard.Where(
                        c =>
                        (c.name == card.name) &&
                        (c.number == card.number) &&
                        (c.setCode == card.setCode)
                        ).SingleOrDefaultAsync();

                    if (dbCard == null)
                    {
                        set.Cards.Add(card);
                    }
                    else
                    {
                        // Update properties
                        dbCard.color = card.color;
                        dbCard.scryfallId = card.scryfallId;
                        dbCard.faceName = card.faceName;

                        //    dbCard.colors = card.colors;
                    }
                }
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<MTGCards> getCardAsync(string cardName, MTGSets set, string cardNumber)
        {
            return await _dbContext.MTGCard
                .Include(c => c.MTGSets)
                .Where(c => c.name == cardName && c.number == cardNumber && c.MTGSets.setCode == set.setCode)
                .SingleOrDefaultAsync();
        }

        public async Task<MTGCards> getCardAsync(string cardName, MTGSets set, int cardNumber)
        {
            return await getCardAsync(cardName, set, cardNumber.ToString());
        }

        public async Task AddLengUserAsync(string aduuid)
        {
            if (!string.IsNullOrEmpty(aduuid))
            {
                LengUser user = new LengUser();
                user.aduuid = aduuid;

                await _dbContext.LengUser.AddAsync(user);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<LengUser> GetLengUserAsync(string aduuid)
        {
            if (!string.IsNullOrEmpty(aduuid))
            {
                var user = await _dbContext.LengUser.Where(u => u.aduuid == aduuid).SingleOrDefaultAsync();
                return user;
            }

            return null;
        }

        public async Task<IEnumerable<LengUserMTGCards>> GetAllCardsFromUserCollectionAsync(LengUser user)
        {
            var cards = await _dbContext.LengUserMTGCards
                .Include(c => c.MTGCards)
                .Include(s => s.MTGCards.MTGSets)
                .ToListAsync();
            // What, no where for the user check? Is this then also not required in the functions after this?

            return cards;
        }

        public async Task<IEnumerable<LengUserMTGCards>> GetCardFromUserCollectionAsync(LengUser user, string cardName)
        {
            var cards = await _dbContext.LengUserMTGCards
                .Include(c => c.MTGCards)
                .Include(s => s.MTGCards.MTGSets)
                .Where(c => c.MTGCards.name == cardName && c.LengUser.LengUserID == user.LengUserID)
                .ToListAsync();

            var faceCards = await _dbContext.LengUserMTGCards
                .Include(c => c.MTGCards)
                .Include(s => s.MTGCards.MTGSets)
                .Where(c => c.MTGCards.faceName == cardName && c.LengUser.LengUserID == user.LengUserID)
                .ToListAsync();

            // Merge cards and faceCards
            foreach (var card in faceCards)
            {
                cards.Add(card);
            }

            return cards;
        }

        public async Task<IEnumerable<MTGCards>> GetCardsForUserAsync(LengUser user, string cardName)
        {
            List<MTGCards> cardsList = await _dbContext.MTGCard
                .Where(cards => cards.name == cardName)
                .Include(cards => cards.LengUserMTGCards)
                .Include(set => set.MTGSets)
                .ToListAsync();
            cardsList.Sort();
            return cardsList;
        }

        public async Task<IEnumerable<MTGCards>> GetCardsInSetForUserAsync(LengUser user, string setCode)
        {
            var set = await _dbContext.MTGSets.FirstOrDefaultAsync(set => set.setCode == setCode);

            if (set != null)
            {
                List<MTGCards> cardsList = await _dbContext.MTGCard
                    .Where(cards => cards.setCode == setCode)
                    .Include(cards => cards.LengUserMTGCards)
                    .ToListAsync();

                cardsList.Sort();
                return cardsList;
            }

            return null;
        }

        public async Task updateCardOfUserAsync(string number, string name, string setCode, int count, int countFoil, LengUser user)
        {
            var set = await GetSetAsync(setCode);
            var card = await getCardAsync(name, set, number);

            var dbCard = _dbContext.LengUserMTGCards.Where(c => c.LengUser == user && c.MTGCards == card).SingleOrDefault();
            if (dbCard != null)
            {
                dbCard.count = count;
                dbCard.countFoil = countFoil;
            }
            else
            {
                await _dbContext.LengUserMTGCards.AddAsync(new LengUserMTGCards { LengUser = user, MTGCards = card, count = count, countFoil = countFoil });
            }

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task<(int cards, int playsets)> GetUserCollectionSummaryAsync(LengUser user)
        {
            // TODO: Fix playset information
            var query = _dbContext.LengUserMTGCards
                .Where(lumc => lumc.LengUser.LengUserID == user.LengUserID)
                .GroupBy(lumc => lumc.MTGCards.MTGCardsID)
                .Select(g => new
                {
                    Count = g.Sum(lumc => lumc.count + lumc.countFoil),
                    PlaysetCount = 0
                });

            var results = await query.ToListAsync();
            int cards = results.Sum(r => r.Count);
            int playsets = results.Sum(r => r.PlaysetCount);
            return (cards, playsets);
        }
    }
}
