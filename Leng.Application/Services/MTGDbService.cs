using Leng.Domain.Models;
using Leng.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Leng.Application.Services {
    public class MTGDbService {
        private LengDbContext _dbContext;
        public MTGDbService(LengDbContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task AddSetAsync(MTGSets set) {
            if (set != null) {
                var dbSetWhere = _dbContext.MTGSets.Where(r => r.setCode == set.setCode).SingleOrDefault();

                if (dbSetWhere == null) {
                    await _dbContext.MTGSets.AddAsync(set);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
        public async Task<List<MTGSets>> GetAllSetsAsync() {
            return await _dbContext.MTGSets.OrderBy(x => x.name).ToListAsync();
        }

        public async Task<MTGSets?> getSetFromNameAsync(string setName) {
            if (!string.IsNullOrEmpty(setName) && _dbContext.MTGSets != null) {
                var setCode = await getSetCodeAsync(setName);
                return await GetSetAsync(setCode);
            }

            return null;
        }

        // Get SetCode for set
        public async Task<string>? getSetCodeAsync(string setName) {
            string setCode = null;
            try {
                var set = await _dbContext.MTGSets.Where(set => set.name == setName).FirstOrDefaultAsync();
                setCode = set.setCode;
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }

            if (setCode != null) {
                return setCode;
            }
            else {
                return null;
            }
        }

        public async Task<MTGSets?> GetSetAsync(string setCode) {
            if (!string.IsNullOrEmpty(setCode) && _dbContext.MTGSets != null) {
                if (setCode.Length != 3 && setCode.Length != 4) {
                    throw new ArgumentException("Value must be 3 or 4 characters.", nameof(setCode));
                }
                var set = await _dbContext.MTGSets.Where(set => set.setCode == setCode).FirstOrDefaultAsync();
                return set;
            }

            return null;
        }

        public async Task<List<MTGSets>> SearchSetsContainingCardsAsync(string mtgset) {
            // if text is null or empty, show complete list
            if (string.IsNullOrEmpty(mtgset)) {
                return await GetAllSetsAsync();
            }
            else {
                List<MTGSets> list = await _dbContext.MTGSets.Where(x => x.name.Contains(mtgset) && x.Cards.Count != 0).ToListAsync();
                return list;
            }
        }

        public async Task<IEnumerable<MTGCards>> getCardsAsync(string cardName) {
            var cards = await _dbContext.MTGCard
                .Where(c => c.name.Contains(cardName))
                .ToListAsync();
            return cards;
        }

        public async Task AddCardAsync(MTGCards card) {
            var set = _dbContext.MTGSets.Where(r => r.setCode == card.setCode).SingleOrDefault();

            if (set != null) {
                if (set.Cards == null) {
                    // TODO: This should have been part of the init somewhere?
                    set.Cards = new List<MTGCards>();
                }

                MTGCards dbCard = await _dbContext.MTGCard.Where(
                    c =>
                        (c.name == card.name) &&
                        (c.number == card.number) &&
                        (c.setCode == card.setCode) &&
                        (card.side == "a" || card.side == null)).SingleOrDefaultAsync();

                if (dbCard == null) {
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

        internal async Task AddCardsAsync(List<MTGCards> setCards) {
            var set = _dbContext.MTGSets.Where(r => r.setCode == setCards.FirstOrDefault().setCode).SingleOrDefault();
            if (set != null) {
                if (set.Cards == null) {
                    set.Cards = new List<MTGCards>();
                }
            }

            foreach (MTGCards card in setCards) {
                if (card.name != null &&
                    card.number != null && 
                    card.setCode != null && 
                    (card.side == "a" || card.side == null)) {

                    MTGCards dbCard = await _dbContext.MTGCard.Where(
                        c =>
                        (c.name == card.name) &&
                        (c.number == card.number) &&
                        (c.setCode == card.setCode)
                        ).SingleOrDefaultAsync();

                    if (dbCard == null) {
                        set.Cards.Add(card);
                    }
                }
            }
            await _dbContext.SaveChangesAsync();
        }

        // Get single card - GetCard
        public async Task<MTGCards> getCardAsync(string cardName, MTGSets set, int cardNumber) {
            MTGCards card = await _dbContext.MTGCard
                .Where(c => c.name == cardName &&
                    c.number == cardNumber.ToString() &&
                    c.setCode == set.setCode)
                .SingleOrDefaultAsync();
            return card;
        }

        // Get single card - GetCard
        public async Task<MTGCards> getCardAsync(string cardName, MTGSets set, string cardNumber) {
            MTGCards card = await _dbContext.MTGCard
                .Where(c => c.name == cardName &&
                    c.number == cardNumber &&
                    c.setCode == set.setCode)
                .SingleOrDefaultAsync();
            return card;
        }

        public async Task AddLengUserAsync(string aduuid) {
            if (!string.IsNullOrEmpty(aduuid)) {
                LengUser user = new LengUser();
                user.aduuid = aduuid;

                await _dbContext.LengUser.AddAsync(user);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<LengUser> GetLengUserAsync(string aduuid) {
            if (!string.IsNullOrEmpty(aduuid)) {
                var user = _dbContext.LengUser.Where(u => u.aduuid == aduuid).SingleOrDefault();
                return user;
            }

            return null;
        }

        public async Task<IEnumerable<LengUserMTGCards>> GetCardFromUserCollectionAsync(string cardName) {
            var cards = await _dbContext.LengUserMTGCards
                .Include(c => c.MTGCards)
                .Include(s => s.MTGCards.MTGSets)
                .Where(c => c.MTGCards.name == cardName)
                //.Where(c => c.MTGCards.name.Contains(cardName, StringComparison.OrdinalIgnoreCase))
                .ToListAsync();

            return cards;
        }

        public async Task<IEnumerable<MTGCards>> GetCardsForUser(LengUser user, string cardName) {
            List<MTGCards> cardsList = await _dbContext.MTGCard
                .Where(cards => cards.name == cardName)
                .Include(cards => cards.LengUserMTGCards)
                .ToListAsync();

            cardsList.Sort();
            return cardsList;
        }

        public async Task<IEnumerable<MTGCards>> GetCardsInSetForUser(LengUser user, string setCode) {
            var set = await _dbContext.MTGSets.FirstOrDefaultAsync(set => set.setCode == setCode);

            if (set != null) {
                // Get cards for user, with set joined
                //IEnumerable<KeyValuePair<MTGCards, int>> cards = new List<KeyValuePair<MTGCards, int>>();

                List<MTGCards> cardsList = await _dbContext.MTGCard
                    .Where(cards => cards.setCode == setCode)
                    .Include(cards => cards.LengUserMTGCards)
                    .ToListAsync();

                //foreach(MTGCards card in cardsList) {
                //int count = card.LengUserMTGCards.count;


                //card.LengUserMTGCards = card.LengUserMTGCards.Where(x => x.LengUser == user).ToList();
                //}

                //private IEnumerable<KeyValuePair<MTGCards, int>>? cards = new List<KeyValuePair<MTGCards, int>>();

                //cards = cardsList, cardList.count;

                cardsList.Sort();
                return cardsList;

            }

            return null;
        }

        public async Task updateCardOfUserAsync(string number, string name, string setName, int count, int countFoil, LengUser user) {
            var set = await getSetFromNameAsync(setName);
            var card = await getCardAsync(name, set, number);

            var dbCard = _dbContext.LengUserMTGCards.Where(c => c.LengUser == user && c.MTGCards == card).SingleOrDefault();
            if (dbCard != null) {
                dbCard.count = count;
                dbCard.countFoil = countFoil;
            }
            else {
                await _dbContext.LengUserMTGCards.AddAsync(new LengUserMTGCards { LengUser = user, MTGCards = card, count = count, countFoil = countFoil });
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
