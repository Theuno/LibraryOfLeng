using Leng.Application.Services;
using Leng.Domain.Models;
using Leng.Infrastructure;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Leng.Application.Tests
{
    public static class MTGTestGenerics 
    {
        public static DbContextOptions<LengDbContext> CreateOptions(string databaseName)
        {
            return new DbContextOptionsBuilder<LengDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
        }

        public static void SeedBasicTestData(DbContextOptions<LengDbContext> options)
        {
            // Insert seed data into the database using one instance of the context
            using (var context = new LengDbContext(options))
            {
                var sets = new MTGSets[]
                {
                    new MTGSets { setCode = "ATQ", name = "Antiquities" },
                    new MTGSets { setCode = "LEA", name = "Limited Edition Alpha" },
                    new MTGSets { setCode = "WAR", name = "War of the Spark" }
                };

                context.MTGSets.AddRange(sets);
                context.SaveChanges();

                var cards = new MTGCards[]
                {
                    new MTGCards { name = "Urza's Mine", number = "83b", MTGSets = sets[0] },
                    new MTGCards { name = "Urza's Power Plant", number = "84a", MTGSets = sets[0] },
                    new MTGCards { name = "Urza's Tower", number = "85c", MTGSets = sets[0] },
                    new MTGCards { name = "Black Lotus", number = "232", MTGSets = sets[1] },
                    new MTGCards { name = "Mox Pearl", number = "263", MTGSets = sets[1] },
                    new MTGCards { name = "Mox Sapphire", number = "265", MTGSets = sets[1] }
                };

                context.MTGCard.AddRange(cards);
                context.SaveChanges();
            }
        }
    }

    public class MTGSetsTests
    {
        [Test]
        public async Task AddSetAsync_AddsSet_WhenSetCodeDoesNotExist()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_AddsSet_WhenSetCodeDoesNotExist");
            MTGTestGenerics.SeedBasicTestData(options);

            // Use a clean instance of the context to run the test
            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context);
                var newSet = new MTGSets { setCode = "4ED", /* other properties */ };

                // Act
                await service.AddSetAsync(newSet);
            }

            // Use another instance of the context to verify the results
            using (var context = new LengDbContext(options))
            {
                // Assert
                Assert.That(context.MTGSets.Any(set => set.setCode == "4ED"));
            }
        }

        [Test]
        public async Task AddSetAsync_DoesNotAddSet_WhenSetCodeExists()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_AddSetAsync_DoesNotAddSet_WhenSetCodeExists");
            MTGTestGenerics.SeedBasicTestData(options);

            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context);
                var existingSet = new MTGSets { setCode = "ATQ", /* other properties */ };

                // Act
                await service.AddSetAsync(existingSet);
            }

            // Use another instance of the context to verify the results
            using (var context = new LengDbContext(options))
            {
                // Assert
                // Check that the count of sets hasn't increased
                Assert.That(context.MTGSets.Count(), Is.EqualTo(3));
            }
        }

        [Test]
        public async Task AddSetAsync_HandlesNullArg()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_AddSetAsync_HandlesNullArg");
            MTGTestGenerics.SeedBasicTestData(options);

            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context);

                // Act
                await service.AddSetAsync(null);
            }

            // Use another instance of the context to verify the results
            using (var context = new LengDbContext(options))
            {
                // Assert
                // Check that the count of sets hasn't increased
                Assert.That(context.MTGSets.Count(), Is.EqualTo(3));
            }
        }


        [Test]
        public async Task GetSetCodeAsync_ValidInput_ReturnsSetCode()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_GetSetCodeAsync_ValidInput_ReturnsSetCode");
            MTGTestGenerics.SeedBasicTestData(options);

            // Arrange
            var setName = "Antiquities";
            var expectedSetCode = "ATQ";

            // Act
            string setCode;
            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context);
                setCode = await service.GetSetCodeAsync(setName);
            }

            // Assert
            Assert.That(setCode, Is.EqualTo(expectedSetCode));
        }

        [Test]
        public async Task GetSetCodeAsync_Throws_WhenSetNameIsNull()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_GetSetCodeAsync_Null");
            MTGTestGenerics.SeedBasicTestData(options);

            // Act
            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context);

                // Act & Assert
                var ex = Assert.ThrowsAsync<ArgumentException>(async () => await service.GetSetCodeAsync(null));
                Assert.That(ex.Message, Is.EqualTo("Set name cannot be null or empty (Parameter 'setName')"));
                Assert.That(ex.ParamName, Is.EqualTo("setName"));
            }
        }

        [Test]
        public async Task GetSetCodeAsync_Throws_WhenSetNameIsEmpty()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_GetSetCodeAsync_WhenSetNameIsEmpty");
            MTGTestGenerics.SeedBasicTestData(options);

            // Act
            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context);

                // Act & Assert
                var ex = Assert.ThrowsAsync<ArgumentException>(async () => await service.GetSetCodeAsync(""));
                Assert.That(ex.Message, Is.EqualTo("Set name cannot be null or empty (Parameter 'setName')"));
                Assert.That(ex.ParamName, Is.EqualTo("setName"));
            }
        }

        [Test]
        public async Task GetSetCodeAsync_ReturnsNull_WhenSetNameDoesNotExist()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_GetSetCodeAsync_ReturnsNull_WhenSetNameDoesNotExist");
            MTGTestGenerics.SeedBasicTestData(options);

            // Act
            string setCode;
            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context);
                setCode = await service.GetSetCodeAsync("This set name does not exist");
            }

            // Assert
            Assert.That(setCode, Is.Null);
        }

        [Test]
        public async Task SearchSetsContainingCardsAsync_ReturnsAllSets_WhenEmptyStringIsPassed()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_ReturnsAllSets_WhenEmptyStringIsPassed");
            MTGTestGenerics.SeedBasicTestData(options);
            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context);

                // Act
                var sets = await service.SearchSetsContainingCardsAsync("");

                // Assert
                Assert.That(sets, Has.Count.EqualTo(context.MTGSets.Count()));
                // Please adjust the assertion as needed. This assumes GetAllSetsAsync returns all sets.
            }
        }

        [Test]
        public async Task SearchSetsContainingCardsAsync_ReturnsSetsWithNameContainingPassedString_AndSetsHaveCards()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_ReturnsSetsWithNameContainingPassedString_AndSetsHaveCards");
            MTGTestGenerics.SeedBasicTestData(options); // Make sure you seed data with at least one set containing cards.
            var setToSearch = "Alpha"; // Make sure "Alpha" appears in at least one of your seed sets with cards.
            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context);

                // Act
                var sets = await service.SearchSetsContainingCardsAsync(setToSearch);

                // Assert
                Assert.That(sets, Is.All.Matches<MTGSets>(s => s.name.Contains(setToSearch) && s.Cards.Count != 0));
            }
        }

        [Test]
        public async Task SearchSetsContainingCardsAsync_ReturnsNoSetsWithNameContainingPassedString_AndSetHasNoCards()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_ReturnsNoSetsWithNameContainingPassedString_AndSetHasNoCards");
            MTGTestGenerics.SeedBasicTestData(options); // Make sure you seed data with at least one set containing cards.
            var setToSearch = "Spark"; // Make sure "Alpha" appears in at least one of your seed sets with cards.
            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context);

                // Act
                var sets = await service.SearchSetsContainingCardsAsync(setToSearch);

                // Assert
                Assert.That(sets, Is.All.Matches<MTGSets>(s => s.name.Contains(setToSearch) && s.Cards.Count != 0));
            }
        }
    }

    public class MTGCardsTests
    {
        [Test]
        public async Task GetCardsAsync_ReturnsCards_WhenCardNamesStartWithProvidedString()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_ReturnsCards_WhenCardNamesStartWithProvidedString");
            MTGTestGenerics.SeedBasicTestData(options);

            var cardNameToSearch = "Black";
            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context);

                // Act
                var cards = await service.getCardsAsync(cardNameToSearch, CancellationToken.None);

                // Assert
                Assert.That(cards, Is.All.Matches<MTGCards>(c => c.name.StartsWith(cardNameToSearch)));
            }
        }

        [Test]
        public async Task GetCardsAsync_ReturnsCards_WhenCardNamesContainProvidedString_ButNoCardsStartWithIt()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_ReturnsCards_WhenCardNamesContainProvidedString_ButNoCardsStartWithIt");
            MTGTestGenerics.SeedBasicTestData(options);

            var cardNameToSearch = "Lotus";
            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context);

                // Act
                var cards = await service.getCardsAsync(cardNameToSearch, CancellationToken.None);

                // Assert
                Assert.That(cards, Is.All.Matches<MTGCards>(c => c.name.Contains(cardNameToSearch)));
            }
        }

        [Test]
        public async Task GetCardsAsync_ReturnsEmpty_WhenNoCardNamesStartWithOrContainProvidedString()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_ReturnsEmpty_WhenNoCardNamesStartWithOrContainProvidedString");
            MTGTestGenerics.SeedBasicTestData(options);

            var cardNameToSearch = "Zebra";
            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context);

                // Act
                var cards = await service.getCardsAsync(cardNameToSearch, CancellationToken.None);

                // Assert
                Assert.IsEmpty(cards);
            }
        }

        [Test]
        public async Task GetCardsAsync_ReturnsTop20_WhenProvidedStringIsEmpty()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_ReturnsTop20_WhenProvidedStringIsEmpty");

            using (var context = new LengDbContext(options))
            {
                var sets = new MTGSets[]
                {
                    new MTGSets { setCode = "ATQ", name = "Antiquities" },
                    new MTGSets { setCode = "LEA", name = "Limited Edition Alpha" },
                    new MTGSets { setCode = "WAR", name = "War of the Spark" }
                };
                    
                context.MTGSets.AddRange(sets);
                context.SaveChanges();

                var cardData = new List<MTGCards>();
                for (int i = 1; i <= 32; i++)
                {
                    var card = new MTGCards();
                    card.name = $"Animate Wall";
                    card.number = $"{i}";
                    card.MTGSets = sets[1];

                    cardData.Add(card);
                }

                context.MTGCard.AddRange(cardData);
                context.SaveChanges();
            }

            var cardNameToSearch = "";
            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context);

                // Act
                var cards = await service.getCardsAsync(cardNameToSearch, CancellationToken.None);

                // Assert
                Assert.AreEqual(20, cards.Count());
            }
        }

        [Test]
        public async Task GetCardAsync_ReturnsCorrectCard_WhenGivenStringNumber()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_ReturnsCorrectCard_WhenGivenStringNumber");
            MTGTestGenerics.SeedBasicTestData(options);

            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context);

                // Act
                var set = await service.GetSetAsync("ATQ");
                var card = await service.getCardAsync("Urza's Tower", set, "85c");

                // Assert
                Assert.That(card, Is.Not.Null);
                Assert.That(card.name, Is.EqualTo("Urza's Tower"));
                Assert.That(card.number, Is.EqualTo("85c"));
                Assert.That(card.MTGSets.setCode, Is.EqualTo("ATQ"));
            }
        }

        [Test]
        public async Task GetCardAsync_ReturnsCorrectCard_WhenGivenIntNumber()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_ReturnsCorrectCard_WhenGivenIntNumber");
            MTGTestGenerics.SeedBasicTestData(options);

            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context);

                // Act
                var set = await service.GetSetAsync("LEA");
                var card = await service.getCardAsync("Mox Sapphire", set, 265);

                // Assert
                Assert.That(card, Is.Not.Null);
                Assert.That(card.name, Is.EqualTo("Mox Sapphire"));
                Assert.That(card.number, Is.EqualTo("265"));
                Assert.That(card.MTGSets.setCode, Is.EqualTo("LEA"));
            }
        }

    }
}