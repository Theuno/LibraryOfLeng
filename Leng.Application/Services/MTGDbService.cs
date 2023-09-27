﻿using Leng.Domain.Models;
using Leng.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Leng.Application.Services
{
    public interface IMTGDbService
    {
        Task AddSetAsync(MTGSets set);
        Task<List<MTGSets>> GetAllSetsAsync(CancellationToken cancellationToken);
        Task<string?> GetSetCodeAsync(string setName);
        Task<MTGSets?> GetSetAsync(string? setCode);
        Task<List<MTGSets>> SearchSetsContainingCardsAsync(string mtgset, CancellationToken cancellationToken = default);
        Task<IEnumerable<MTGCards>> getCardsAsync(string cardName, CancellationToken cancellationToken);
        Task AddCardsAsync(List<MTGCards> setCards);
        Task<IEnumerable<string>> SearchForCardAsync(string cardName, CancellationToken cancellationToken);
        Task<MTGCards?> getCardAsync(string cardName, MTGSets set, string cardNumber);
        Task<MTGCards?> getCardAsync(string cardName, MTGSets set, int cardNumber);
        Task AddLengUserAsync(string aduuid);
        Task<LengUser?> GetLengUserAsync(string aduuid);
        Task<IEnumerable<LengUserMTGCards>> GetAllCardsFromUserCollectionAsync(LengUser user);
        Task<IEnumerable<LengUserMTGCards>> GetCardFromUserCollectionAsync(LengUser user, string cardName);
        Task<IEnumerable<MTGCards>> GetCardsForUserAsync(LengUser user, string cardName);
        Task<IEnumerable<MTGCards>> GetCardsInSetForUserAsync(LengUser user, string setCode);
        Task updateCardOfUserAsync(string number, string name, string setCode, int count, int countFoil, LengUser user);
        Task<(int cards, int playsets)> GetUserCollectionSummaryAsync(LengUser user);
    }

    public class MTGDbService : IMTGDbService
    {
        private readonly LengDbContext _dbContext;
        private readonly ILogger<MTGDbService> _logger;

        [ActivatorUtilitiesConstructor]
        public MTGDbService(IDbContextFactory<LengDbContext> contextFactory, ILogger<MTGDbService> logger)
        {
            _dbContext = contextFactory.CreateDbContext();
            _logger = logger;
        }

        // Constructor used for testing
        public MTGDbService(LengDbContext dbContext, ILogger<MTGDbService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger;
        }


        public async Task AddSetAsync(MTGSets set)
        {
            if (set != null)
            {
                var dbSetWhere = _dbContext.MTGSets.Where(r => r.setCode == set.setCode).SingleOrDefault();

                if (dbSetWhere == null)
                {
                    try
                    {
                        await _dbContext.MTGSets.AddAsync(set);
                        await _dbContext.SaveChangesAsync();
                    }
                    catch (SqlException)
                    {
                        throw;
                    }
                }
            }
        }
        public async Task<List<MTGSets>> GetAllSetsAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.MTGSets.OrderBy(x => x.name).ToListAsync(cancellationToken);
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

        public async Task<MTGSets?> GetSetAsync(string? setCode)
        {
            if (!string.IsNullOrEmpty(setCode) && _dbContext.MTGSets != null)
            {
                if (setCode.Length >= 3 && setCode.Length <= 6)
                {
                    var set = await _dbContext.MTGSets
                        .Include(s => s.Cards).
                        Where(set => set.setCode == setCode).FirstOrDefaultAsync();
                    return set;
                }
                else
                {
                    throw new ArgumentException("Value must be between 3 and 6 characters.", nameof(setCode));
                }
            }

            return new MTGSets();
        }

        public async Task<List<MTGSets>> SearchSetsContainingCardsAsync(string mtgset, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // if text is null or empty, show complete list
            if (string.IsNullOrEmpty(mtgset))
            {
                _logger.LogInformation("Search query is null or empty.");
                return await GetAllSetsAsync(cancellationToken);
            }
            else
            {
                List<MTGSets> list = await _dbContext.MTGSets
                        .Include(s => s.Cards)
                        .Where(x => x.name.Contains(mtgset) && x.Cards.Count != 0)
                        .ToListAsync(cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                return list;
            }
        }

        // This function gets the cards from the database matching a part of the name
        // It has preference for cards that start with the name, but if there are no matches, it will return cards that contain the name
        public async Task<IEnumerable<MTGCards>> getCardsAsync(string cardName, CancellationToken cancellationToken)
        {
            var cards = await _dbContext.MTGCard
                .Where(c => c.name.StartsWith(cardName)) // Dereference of a possibly null reference.
                .Include(cards => cards.LengUserMTGCards)
                .OrderBy(c => c.name)
                .Take(20) // Limit the results to a certain number
                .ToListAsync(cancellationToken);

            // if no cards match, do a contains match.
            if (cards.Count == 0)
            {
                cards = await _dbContext.MTGCard
                    .Where(c => c.name.Contains(cardName))
                    .Include(cards => cards.LengUserMTGCards)  // Dereference of a possibly null reference.
                    .OrderBy(c => c.name)
                    .Take(20) // Limit the results to a certain number
                    .ToListAsync(cancellationToken);
            }
            return cards;
        }

        public async Task AddCardsAsync(List<MTGCards> setCards)
        {
            // Group cards by set code
            var groupedCards = setCards.GroupBy(card => card.setCode);

            // Process each group
            foreach (var cardGroup in groupedCards)
            {
                await AddCardsToSetAsync(cardGroup.ToList());
            }
        }

        private async Task AddCardsToSetAsync(List<MTGCards> setCards)
        {
            var setCode = setCards.FirstOrDefault()?.setCode;

            if (string.IsNullOrEmpty(setCode)) return;

            // Find the set
            var set = await GetSetAsync(setCode);

            // If set does not exist, we can't add cards to it
            if (set == null)
            {
                throw new InvalidOperationException($"Set with code {setCode} not found.");
            }

            if (set.Cards == null)
            {
                throw new InvalidOperationException($"Set with code {setCode} found, but no Cards list is present.");
            }

            foreach (var card in setCards)
            {
                if (card.name != null && card.number != null && (card.side == "a" || card.side == null))
                {
                    var dbCard = await _dbContext.MTGCard.Where(
                        c => c.name == card.name &&
                             c.number == card.number &&
                             c.setCode == card.setCode
                    ).SingleOrDefaultAsync();

                    if (dbCard == null)
                    {
                        set.Cards.Add(card);
                    }
                    else
                    {
                        // Update properties
                        dbCard.asciiName = card.asciiName;
                        dbCard.color = card.color;
                        dbCard.edhrecRank = card.edhrecRank;
                        dbCard.edhrecSaltiness = card.edhrecSaltiness;
                        dbCard.faceName = card.faceName;
                        dbCard.scryfallId = card.scryfallId;
                        dbCard.text = card.text;
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<string>> SearchForCardAsync(string cardName, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Searching for card: {cardName}");

                var searchedCards = await getCardsAsync(cardName, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                _logger.LogInformation($"Found {searchedCards.Count()} cards for search term: {cardName}");
                searchedCards = searchedCards.DistinctBy(c => c.name);
                return await Task.FromResult(searchedCards.Select(x => x.name).ToArray());
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Search for card cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while searching for cards: {ex.Message}");
            }

            return null;
        }


        public async Task<MTGCards?> getCardAsync(string cardName, MTGSets set, string cardNumber)
        {
            return await _dbContext.MTGCard
                .Include(c => c.MTGSets)
                .Where(c => c.name == cardName && c.number == cardNumber && c.MTGSets.setCode == set.setCode)
                .SingleOrDefaultAsync();
        }

        public async Task<MTGCards?> getCardAsync(string cardName, MTGSets set, int cardNumber)
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

        public async Task<LengUser?> GetLengUserAsync(string aduuid)
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
                .Where(c => c.LengUser.LengUserID == user.LengUserID)
                .ToListAsync();

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

            var dbCard = _dbContext.LengUserMTGCards
                           .Where(c => c.LengUserId == user.LengUserID && c.MTGCardsId == card.MTGCardsID)
                           .SingleOrDefault();
            if (dbCard != null)
            {
                dbCard.count = count;
                dbCard.countFoil = countFoil;
            }
            else
            {
                await _dbContext.LengUserMTGCards.AddAsync(new LengUserMTGCards
                {
                    LengUserId = user.LengUserID,
                    MTGCardsId = card.MTGCardsID,
                    count = count,
                    countFoil = countFoil
                });
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
            if (user == null)
            {
                return (0, 0);
            }
            else
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
}
