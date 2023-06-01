using Leng.Application.Services;
using Leng.Domain.Models;
using Leng.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Leng.Application.Tests
{
    public class MTGSetsTests
    {
        private static void SeedBasicTestData(DbContextOptions<LengDbContext> options)
        {
            // Insert seed data into the database using one instance of the context
            using (var context = new LengDbContext(options))
            {
                context.MTGSets.AddRange(
                    new MTGSets { setCode = "ATQ", name = "Antiquities" },
                    new MTGSets { setCode = "LEA", name = "Limited Edition Alpha" },
                    new MTGSets { setCode = "WAR", name = "War of the Spark" });
                context.SaveChanges();

                context.MTGCard.AddRange(
                    new MTGCards { name = "Urza's Mine", setCode = "ATQ", MTGSets = context.MTGSets.Where(c => c.setCode == "ATQ").FirstOrDefault() },
                    new MTGCards { name = "Urza's Power Plant", setCode = "ATQ", MTGSets = context.MTGSets.Where(c => c.setCode == "ATQ").FirstOrDefault() },
                    new MTGCards { name = "Urza's Tower", setCode = "ATQ", MTGSets = context.MTGSets.Where(c => c.setCode == "ATQ").FirstOrDefault() },
                    new MTGCards { name = "Black Lotus", setCode = "LEA", MTGSets = context.MTGSets.Where(c => c.setCode == "LEA").FirstOrDefault() },
                    new MTGCards { name = "Mox Pearl", setCode = "LEA", MTGSets = context.MTGSets.Where(c => c.setCode == "LEA").FirstOrDefault() },
                    new MTGCards { name = "Mox Sapphire", setCode = "LEA", MTGSets = context.MTGSets.Where(c => c.setCode == "LEA").FirstOrDefault() });
                context.SaveChanges();
            }
        }

        private static DbContextOptions<LengDbContext> CreateOptions(string databaseName)
        {
            return new DbContextOptionsBuilder<LengDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
        }

        [Test]
        public async Task AddSetAsync_AddsSet_WhenSetCodeDoesNotExist()
        {
            // Arrange
            var options = CreateOptions("TestDatabase_AddsSet_WhenSetCodeDoesNotExist");
            SeedBasicTestData(options);

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
            var options = CreateOptions("TestDatabase_AddSetAsync_DoesNotAddSet_WhenSetCodeExists");
            SeedBasicTestData(options);

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
            var options = CreateOptions("TestDatabase_AddSetAsync_HandlesNullArg");
            SeedBasicTestData(options);

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
            var options = CreateOptions("TestDatabase_GetSetCodeAsync_ValidInput_ReturnsSetCode");
            SeedBasicTestData(options);

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
            var options = CreateOptions("TestDatabase_GetSetCodeAsync_Null");
            SeedBasicTestData(options);

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
            var options = CreateOptions("TestDatabase_GetSetCodeAsync_WhenSetNameIsEmpty");
            SeedBasicTestData(options);

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
            var options = CreateOptions("TestDatabase_GetSetCodeAsync_ReturnsNull_WhenSetNameDoesNotExist");
            SeedBasicTestData(options);

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
            var options = CreateOptions("TestDatabase_ReturnsAllSets_WhenEmptyStringIsPassed");
            SeedBasicTestData(options);
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
            var options = CreateOptions("TestDatabase_ReturnsSetsWithNameContainingPassedString_AndSetsHaveCards");
            SeedBasicTestData(options); // Make sure you seed data with at least one set containing cards.
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
            var options = CreateOptions("TestDatabase_ReturnsNoSetsWithNameContainingPassedString_AndSetHasNoCards");
            SeedBasicTestData(options); // Make sure you seed data with at least one set containing cards.
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
}