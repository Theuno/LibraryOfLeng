using Leng.Data.Models;
using Leng.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leng.Application.Services {
    internal class MTGDbService {
        private readonly LengDbContext _dbContext;
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
                        (c.setCode == card.setCode)
                        ).SingleOrDefaultAsync();

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
    }
}
