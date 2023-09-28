using Leng.Application.FunctionHandlers;
using Leng.Application.Services;
using Leng.Domain.Models;
using Leng.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text;

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
                    new MTGCards { name = "Urza's Mine", number = "83b", setCode = sets[0].setCode, MTGSets = sets[0] },
                    new MTGCards { name = "Urza's Power Plant", number = "84a", setCode = sets[0].setCode, MTGSets = sets[0] },
                    new MTGCards { name = "Urza's Tower", number = "85c", setCode = sets[0].setCode, MTGSets = sets[0] },
                    new MTGCards { name = "Black Lotus", number = "232", setCode = sets[1].setCode, MTGSets = sets[1] },
                    new MTGCards { name = "Mox Pearl", number = "263", setCode = sets[1].setCode, MTGSets = sets[1] },
                    new MTGCards { name = "Mox Sapphire", number = "265", setCode = sets[1].setCode, MTGSets = sets[1] }
                };

                context.MTGCard.AddRange(cards);
                context.SaveChanges();
            }
        }
    }

    public class MTGSetsTests
    {
        public ILogger<MTGDbService> StubLogger { get; private set; }

        [SetUp]
        public void Setup()
        {
            // Initialize the stub logger
            StubLogger = Substitute.For<ILogger<MTGDbService>>();
        }

        [Test]
        public async Task AddSetAsync_AddsSet_WhenSetCodeDoesNotExist()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_AddsSet_WhenSetCodeDoesNotExist");
            MTGTestGenerics.SeedBasicTestData(options);

            // Use a clean instance of the context to run the test
            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context, StubLogger);
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
                var service = new MTGDbService(context, StubLogger);
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
                var service = new MTGDbService(context, StubLogger);

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
            string setCode = string.Empty;
            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context, StubLogger);
                if (service == null)
                {
                    Assert.Fail("service is null");
                }
                else
                {
                    setCode = await service.GetSetCodeAsync(setName);
                }
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
                var service = new MTGDbService(context, StubLogger);

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
                var service = new MTGDbService(context, StubLogger);

                // Act & Assert
                var ex = Assert.ThrowsAsync<ArgumentException>(async () => await service.GetSetCodeAsync(""));
                Assert.Multiple(() =>
                {
                    Assert.That(ex.Message, Is.EqualTo("Set name cannot be null or empty (Parameter 'setName')"));
                    Assert.That(ex.ParamName, Is.EqualTo("setName"));
                });
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
                var service = new MTGDbService(context, StubLogger);
                setCode = await service.GetSetCodeAsync("This set name does not exist");
            }

            // Assert
            Assert.That(setCode, Is.Null);
        }

        [Test]
        public async Task GetSetAsync_ReturnsCorrectSet_WhenGivenValidSetCode()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_ReturnsCorrectSet_WhenGivenValidSetCode");
            MTGTestGenerics.SeedBasicTestData(options);

            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context, StubLogger);

                // Act
                var set = await service.GetSetAsync("ATQ");

                // Assert
                Assert.Multiple(() =>
                {
                    Assert.That(set, Is.Not.Null);
                    Assert.That(set, Has.Property("setCode").EqualTo("ATQ"));
                });
            }
        }

        [Test]
        public void GetSetAsync_ThrowsException_WhenGivenInvalidSetCode()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_ThrowsException_WhenGivenInvalidSetCode");
            MTGTestGenerics.SeedBasicTestData(options);

            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context, StubLogger);

                // Act & Assert
                var ex = Assert.ThrowsAsync<ArgumentException>(() => service.GetSetAsync("A"));
                Assert.That(ex.Message, Is.EqualTo("Value must be between 3 and 6 characters. (Parameter 'setCode')"));
            }
        }

        [Test]
        public async Task GetSetAsync_ReturnsEmptyMTGSets_WhenGivenNullOrEmptySetCode()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_ReturnsEmptyMTGSets_WhenGivenNullOrEmptySetCode");
            MTGTestGenerics.SeedBasicTestData(options);

            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context, StubLogger);

                // Act
                var set1 = await service.GetSetAsync(null);
                var set2 = await service.GetSetAsync("");

                // Assert
                Assert.Multiple(() =>
                {
                    Assert.That(set1, Is.Not.Null);
                    Assert.That(set1, Has.Property("setCode").Null.Or.Empty);

                    Assert.That(set2, Is.Not.Null);
                    Assert.That(set2, Has.Property("setCode").Null.Or.Empty);
                });
            }
        }

        [Test]
        public async Task SearchSetsContainingCardsAsync_ReturnsAllSets_WhenEmptyStringIsPassed()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_ReturnsAllSets_WhenEmptyStringIsPassed");
            MTGTestGenerics.SeedBasicTestData(options);
            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context, StubLogger);

                // Act
                var sets = await service.SearchSetsContainingCardsAsync("", CancellationToken.None);

                // Assert
                Assert.That(sets, Has.Count.EqualTo(context.MTGSets.Count()));
            }
        }

        [Test]
        public async Task SearchSetsContainingCardsAsync_ReturnsSetsWithNameContainingPassedString_AndSetsHaveCards()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_ReturnsSetsWithNameContainingPassedString_AndSetsHaveCards");
            MTGTestGenerics.SeedBasicTestData(options);
            var setToSearch = "Alpha";

            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context, StubLogger);

                // Act
                var sets = await service.SearchSetsContainingCardsAsync(setToSearch, CancellationToken.None);

                // Assert
                Assert.That(sets, Is.All.Matches<MTGSets>(s => s.name.Contains(setToSearch) && s.Cards.Count != 0));
            }
        }

        [Test]
        public async Task SearchSetsContainingCardsAsync_ReturnsNoSetsWithNameContainingPassedString_AndSetHasNoCards()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_ReturnsNoSetsWithNameContainingPassedString_AndSetHasNoCards");
            MTGTestGenerics.SeedBasicTestData(options);
            var setToSearch = "Spark";
            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context, StubLogger);

                // Act
                var sets = await service.SearchSetsContainingCardsAsync(setToSearch, CancellationToken.None);

                // Assert
                Assert.That(sets, Is.All.Matches<MTGSets>(s => s.name.Contains(setToSearch) && s.Cards.Count != 0));
            }
        }

        [Test]
        public async Task SearchSetsContainingCardsAsync_CancellationToken_CancelsOperation()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_CancellationToken_CancelsOperation");
            MTGTestGenerics.SeedBasicTestData(options);
            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context, StubLogger);
                var cts = new CancellationTokenSource();

                // Delay task to simulate a long-running search
                Task.Delay(400, cts.Token).ContinueWith(t => service.SearchSetsContainingCardsAsync("Spa", cts.Token));
                Task.Delay(400, cts.Token).ContinueWith(t => service.SearchSetsContainingCardsAsync("Spa", cts.Token));
                Task.Delay(400, cts.Token).ContinueWith(t => service.SearchSetsContainingCardsAsync("Spa", cts.Token));
                Task.Delay(400, cts.Token).ContinueWith(t => service.SearchSetsContainingCardsAsync("Spa", cts.Token));

                // Act
                // Cancel the token after a short delay to interrupt the above search
                Task.Delay(10).ContinueWith(t => cts.Cancel());

                // Assert
                Assert.ThrowsAsync<OperationCanceledException>(() => service.SearchSetsContainingCardsAsync("Alpha", cts.Token));
            }
        }

        [Test]
        public async Task GetAllSetsAsync_ReturnsExpectedSets()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_GetAllSetsAsync_ReturnsExpectedSets");
            MTGTestGenerics.SeedBasicTestData(options);

            // Create the schema in the database
            using (var context = new LengDbContext(options))
            {
                context.MTGSets.Add(new MTGSets { name = "Wilds of Eldraine", setCode = "WOE", baseSetSize = 95 });
                context.MTGSets.Add(new MTGSets { name = "Party on Eldraine", setCode = "POE", baseSetSize = 10 });
                await context.SaveChangesAsync();
            }

            // Act
            IEnumerable<MTGSets> sets;
            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context, StubLogger);
                sets = await service.GetAllSetsAsync(CancellationToken.None);
            }

            // Assert
            Assert.That(sets.Count(), Is.EqualTo(5));
        }


        [Test]
        public async Task GetUserCollectionSummaryAsync_NullUser_ReturnsZeroes()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_GetUserCollectionSummaryAsync_NullUser_ReturnsZeroes");

            // Act
            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context, StubLogger);
                var summary = await service.GetUserCollectionSummaryAsync(null);

                // Assert
                Assert.That(summary, Is.EqualTo((0, 0)));
            }
        }

        [Test]
        public async Task GetUserCollectionSummaryAsync_EmptyCollection_ReturnsZeroes()
        {
            // Arrange
            var user = new LengUser();

            var options = MTGTestGenerics.CreateOptions("TestDatabase_GetUserCollectionSummaryAsync_EmptyCollection_ReturnsZeroes");
            MTGTestGenerics.SeedBasicTestData(options);

            using (var context = new LengDbContext(options))
            {
                context.LengUser.Add(user);
                await context.SaveChangesAsync();

                var service = new MTGDbService(context, StubLogger);

                // Act
                var summary = await service.GetUserCollectionSummaryAsync(user);

                // Assert
                Assert.That(summary, Is.EqualTo((0, 0)));
            }
        }

        [Test]
        public async Task GetUserCollectionSummaryAsync_PopulatedCollection_ReturnsExpectedSummary()
        {
            // Arrange
            var user = new LengUser();

            var options = MTGTestGenerics.CreateOptions("TestDatabase_GetUserCollectionSummaryAsync_PopulatedCollection_ReturnsExpectedSummary");
            MTGTestGenerics.SeedBasicTestData(options);

            using (var context = new LengDbContext(options))
            {
                context.LengUser.Add(user);

                var service = new MTGDbService(context, StubLogger);

                await service.updateCardOfUserAsync("83b", "Urza's Mine", "ATQ", 1, 0, user);
                await service.updateCardOfUserAsync("84a", "Urza's Power Plant", "ATQ", 1, 0, user);
                await service.updateCardOfUserAsync("85c", "Urza's Tower", "ATQ", 1, 0, user);

                var summary = await service.GetUserCollectionSummaryAsync(user);

                // Assert
                Assert.That(summary, Is.EqualTo((3, 0)));  // PlaysetCount is always 0 in the current implementation
            }
        }

        [Test]
        public async Task GetAllCardsFromUserCollectionAsync_SingleUserTest()
        {
            // Arrange
            var user = new LengUser();

            var options = MTGTestGenerics.CreateOptions("TestDatabase_GetAllCardsFromUserCollectionAsync_SingleUserTest");
            MTGTestGenerics.SeedBasicTestData(options);

            using (var context = new LengDbContext(options))
            {
                context.LengUser.Add(user);

                var service = new MTGDbService(context, StubLogger);

                await service.updateCardOfUserAsync("83b", "Urza's Mine", "ATQ", 1, 0, user);
                await service.updateCardOfUserAsync("84a", "Urza's Power Plant", "ATQ", 1, 0, user);
                await service.updateCardOfUserAsync("85c", "Urza's Tower", "ATQ", 1, 0, user);

                var collection = await service.GetAllCardsFromUserCollectionAsync(user);

                // Assert
                Assert.That(collection, Is.Not.Null);
                Assert.That(collection, Has.Exactly(3).Items);

                // Verify the details of the first card as an example
                var firstCard = collection.First();
                Assert.That(firstCard.MTGCards.number, Is.EqualTo("83b"));
                Assert.That(firstCard.MTGCards.name, Is.EqualTo("Urza's Mine"));
                Assert.That(firstCard.MTGCards.MTGSets.setCode, Is.EqualTo("ATQ"));
                Assert.That(firstCard.count, Is.EqualTo(1));
                Assert.That(firstCard.countFoil, Is.EqualTo(0));
            }
        }

        [Test]
        public async Task GetAllCardsFromUserCollectionAsync_MultipleUsersTest()
        {
            // Arrange
            var user1 = new LengUser();
            var user2 = new LengUser();

            var options = MTGTestGenerics.CreateOptions("TestDatabase_GetAllCardsFromUserCollectionAsync_MultipleUsersTest");
            MTGTestGenerics.SeedBasicTestData(options);

            using (var context = new LengDbContext(options))
            {
                context.LengUser.AddRange(user1, user2);
                await context.SaveChangesAsync();  // Ensure the users are saved to the database

                var service = new MTGDbService(context, StubLogger);

                // Assigning different cards to different users
                await service.updateCardOfUserAsync("83b", "Urza's Mine", "ATQ", 1, 0, user1);
                await service.updateCardOfUserAsync("84a", "Urza's Power Plant", "ATQ", 1, 0, user1);
                await service.updateCardOfUserAsync("85c", "Urza's Tower", "ATQ", 1, 0, user1);
                await service.updateCardOfUserAsync("232", "Black Lotus", "LEA", 1, 0, user2);
                await service.updateCardOfUserAsync("263", "Mox Pearl", "LEA", 1, 0, user2);
                await service.updateCardOfUserAsync("265", "Mox Sapphire", "LEA", 1, 0, user2);

                var collectionUser1 = await service.GetAllCardsFromUserCollectionAsync(user1);
                var collectionUser2 = await service.GetAllCardsFromUserCollectionAsync(user2);

                // Assert
                // Verifying user1's collection
                Assert.That(collectionUser1, Is.Not.Null);
                Assert.That(collectionUser1, Has.Exactly(3).Items);
                Assert.That(collectionUser1.Select(c => c.MTGCards.name), Does.Not.Contain("Black Lotus"));
                Assert.That(collectionUser1.Select(c => c.MTGCards.name), Does.Not.Contain("Mox Pearl"));
                Assert.That(collectionUser1.Select(c => c.MTGCards.name), Does.Not.Contain("Mox Sapphire"));

                // Verifying user2's collection
                Assert.That(collectionUser2, Is.Not.Null);
                Assert.That(collectionUser2, Has.Exactly(3).Items);
                Assert.That(collectionUser2.Select(c => c.MTGCards.name), Does.Not.Contain("Urza's Mine"));
                Assert.That(collectionUser2.Select(c => c.MTGCards.name), Does.Not.Contain("Urza's Power Plant"));
                Assert.That(collectionUser2.Select(c => c.MTGCards.name), Does.Not.Contain("Urza's Tower"));
            }
        }

    }

    [TestFixture]
    public class MTGSetImportTests
    {
        private ILogger<MtgJsonToDbHandler> _mockLogger;
        private string _sampleFile;

        [SetUp]
        public void Setup()
        {
            _mockLogger = Substitute.For<ILogger<MtgJsonToDbHandler>>();

            // Sample MTG set data in JSON format
            _sampleFile = @"{
                ""meta"": {
                    ""date"": ""2023-09-16"",
                    ""version"": ""5.2.2+20230916""
                },
                ""data"": {
                    ""baseSetSize"": 350,
                    ""block"": ""Mirage"",
                    ""cards"": [
                        {
                            ""artist"": ""Pete Venters"",
                            ""borderColor"": ""black"",
                            ""colorIdentity"": [ ""W"" ],
                            ""colors"": [ ""W"" ],
                            ""convertedManaCost"": 3.0,
                            ""edhrecRank"": 12036,
                            ""finishes"": [ ""nonfoil"" ],
                            ""hasFoil"": false,
                            ""hasNonFoil"": true,
                            ""identifiers"": {
                                ""mcmId"": ""8256"",
                                ""mcmMetaId"": ""68"",
                                ""scryfallId"": ""4644694d-52e6-4d00-8cad-748899eeea84""
                            },
                            ""language"": ""English"",
                            ""name"": ""Afterlife"",
                            ""number"": ""1"",
                            ""originalText"": ""Bury target creature and put an Essence token into play under the control of that creature's controller. Treat this token as a 1/1 white creature with flying."",
                            ""rarity"": ""uncommon"",
                            ""setCode"": ""MIR"",
                            ""text"": ""Destroy target creature. It can't be regenerated. Its controller creates a 1/1 white Spirit creature token with flying."",
                            ""type"": ""Instant"",
                            ""uuid"": ""5476b4a0-5ce9-5b16-9272-5d2be623c26f""
                        }
                    ],
                    ""code"": ""MIR""
                }
            }";
        }

        [Test]
        public async Task ImportMTGSet_ValidFile_AddsSetAndCardsToDatabase()
        {
            // Arrange
            var mockFile = new MemoryStream(Encoding.UTF8.GetBytes(_sampleFile));
            var mockDbService = Substitute.For<IMTGDbService>();
            mockDbService.AddSetAsync(Arg.Any<MTGSets>()).Returns(Task.CompletedTask);
            mockDbService.AddCardsAsync(Arg.Any<List<MTGCards>>()).Returns(Task.CompletedTask);

            var _mtgJsonToDbHandler = new MtgJsonToDbHandler(_mockLogger, mockDbService);

            // Act
            await _mtgJsonToDbHandler.ImportMTGSet(mockFile);

            // Assert
            await mockDbService.Received(1).AddSetAsync(Arg.Any<MTGSets>());
            await mockDbService.Received(1).AddCardsAsync(Arg.Any<List<MTGCards>>());
        }

        [Test]
        public async Task ImportMTGSet_ValidFile_DontAddOnlineOnly()
        {
            // Arrange
            var _sampleFileOnlineOnly = @"{
                ""data"": {
                    ""name"": ""Masters Edition"",
                    ""isOnlineOnly"": true,
                    ""code"": ""MED""
                }
            }";

            using (var mockFile = new MemoryStream(Encoding.UTF8.GetBytes(_sampleFileOnlineOnly)))
            {
                var mockDbService = Substitute.For<IMTGDbService>();
                mockDbService.AddSetAsync(Arg.Any<MTGSets>()).Returns(Task.CompletedTask);
                mockDbService.AddCardsAsync(Arg.Any<List<MTGCards>>()).Returns(Task.CompletedTask);

                var _mtgJsonToDbHandler = new MtgJsonToDbHandler(_mockLogger, mockDbService);

                // Act
                await _mtgJsonToDbHandler.ImportMTGSet(mockFile);

                // Assert
                await mockDbService.Received(0).AddSetAsync(Arg.Any<MTGSets>());
                await mockDbService.Received(0).AddCardsAsync(Arg.Any<List<MTGCards>>());
                _mockLogger.Received().LogInformation("Skipping online only set: Masters Edition");
            }
        }

        [Test]
        public async Task ImportMTGSet_ValidFile_DontAddPartialPreview()
        {
            // Arrange
            var _sampleFileOnlineOnly = @"{
                ""data"": {
                    ""name"": ""The Lost Caverns of Ixalan"",
                    ""isOnlineOnly"": false,
                    ""isPartialPreview"": true,
                    ""code"": ""LCI""
                }
            }";

            using (var mockFile = new MemoryStream(Encoding.UTF8.GetBytes(_sampleFileOnlineOnly)))
            {
                var mockDbService = Substitute.For<IMTGDbService>();
                mockDbService.AddSetAsync(Arg.Any<MTGSets>()).Returns(Task.CompletedTask);
                mockDbService.AddCardsAsync(Arg.Any<List<MTGCards>>()).Returns(Task.CompletedTask);

                var _mtgJsonToDbHandler = new MtgJsonToDbHandler(_mockLogger, mockDbService);

                // Act
                await _mtgJsonToDbHandler.ImportMTGSet(mockFile);

                // Assert
                await mockDbService.Received(0).AddSetAsync(Arg.Any<MTGSets>());
                await mockDbService.Received(0).AddCardsAsync(Arg.Any<List<MTGCards>>());
                _mockLogger.Received().LogInformation("Skipping partial preview set: The Lost Caverns of Ixalan");
            }
        }
    }


    public class MTGCardsTests
    {
        public ILogger<MTGDbService> StubLogger { get; private set; }

        [SetUp]
        public void Setup()
        {
            // Initialize the stub logger
            StubLogger = Substitute.For<ILogger<MTGDbService>>();
        }

        [Test]
        public async Task AddCardsAsync_AddsNewCardsToDatabase_WhenNoExistingCards()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<LengDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_AddsNewCardsToDatabase_WhenNoExistingCards")
                .Options;

            var newCards = new List<MTGCards>
            {
                new MTGCards
                {
                    name = "Card 1",
                    number = "1",
                    setCode = "SET1",
                    asciiName = "Card One",
                    color = "Red",
                    edhrecRank = 10,
                    edhrecSaltiness = 5,
                    faceName = "Face 1",
                    scryfallId = "scry1",
                    text = "This is Card One"
                },
                new MTGCards
                {
                    name = "Card 2",
                    number = "2",
                    setCode = "SET1",
                    asciiName = "Card Two",
                    color = "Blue",
                    edhrecRank = 20,
                    edhrecSaltiness = 10,
                    faceName = "Face 2",
                    scryfallId = "scry2",
                    text = "This is Card Two"
                }
            };

            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context, StubLogger);
                var set = new MTGSets { setCode = "SET1" };
                context.MTGSets.Add(set);
                await context.SaveChangesAsync();

                // Act
                await service.AddCardsAsync(newCards);

                // Assert
                var cardsInDatabase = await context.MTGCard.ToListAsync();
                Assert.Multiple(() =>
                {
                    Assert.That(cardsInDatabase, Has.Count.EqualTo(2));
                    Assert.That(cardsInDatabase, Has.One.Matches<MTGCards>(card => card.name == "Card 1" && card.number == "1"));
                    Assert.That(cardsInDatabase, Has.One.Matches<MTGCards>(card => card.name == "Card 2" && card.number == "2"));
                });
            }
        }

        [Test]
        public async Task AddCardsAsync_UpdatesExistingCardsInDatabase_WhenExistingCards()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<LengDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_UpdatesExistingCardsInDatabase_WhenExistingCards")
                .Options;

            var newCards = new List<MTGCards>
            {
                new MTGCards
                {
                    name = "Urza's Mine",
                    number = "83b",
                    setCode = "ATQ",
                    asciiName = "Updated Urza's Mine",
                    color = "Updated Red",
                    edhrecRank = 100,
                    edhrecSaltiness = 50,
                    faceName = "Updated Face 1",
                    scryfallId = "Updated scry1",
                    text = "Updated This is Card One"
                }
            };

            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context, StubLogger);
                MTGTestGenerics.SeedBasicTestData(options);

                // Act
                await service.AddCardsAsync(newCards);

                // Assert
                var updatedCardInDatabase = await context.MTGCard.FirstOrDefaultAsync(c => c.name == "Urza's Mine" && c.number == "83b");

                Assert.That(updatedCardInDatabase, Is.Not.Null);
                if(updatedCardInDatabase != null)
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(updatedCardInDatabase.asciiName, Is.EqualTo("Updated Urza's Mine"));
                        Assert.That(updatedCardInDatabase.color, Is.EqualTo("Updated Red"));
                        Assert.That(updatedCardInDatabase.edhrecRank, Is.EqualTo(100));
                        Assert.That(updatedCardInDatabase.edhrecSaltiness, Is.EqualTo(50));
                        Assert.That(updatedCardInDatabase.faceName, Is.EqualTo("Updated Face 1"));
                        Assert.That(updatedCardInDatabase.scryfallId, Is.EqualTo("Updated scry1"));
                        Assert.That(updatedCardInDatabase.text, Is.EqualTo("Updated This is Card One"));
                    });
                }
            }
        }

        [Test]
        public async Task AddCardsAsync_HandlesMultipleSetsProperly()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<LengDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_HandlesMultipleSetsProperly")
                .Options;

            var newCards = new List<MTGCards>
            {
                new MTGCards { name = "New Card 1", number = "1", setCode = "ATQ" },
                new MTGCards { name = "New Card 2", number = "2", setCode = "LEA" },
                new MTGCards { name = "New Card 3", number = "3", setCode = "WAR" }
            };

            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context, StubLogger);
                MTGTestGenerics.SeedBasicTestData(options);

                // Act
                await service.AddCardsAsync(newCards);

                // Assert
                var card1 = await context.MTGCard.FirstOrDefaultAsync(c => c.name == "New Card 1" && c.number == "1");
                var card2 = await context.MTGCard.FirstOrDefaultAsync(c => c.name == "New Card 2" && c.number == "2");
                var card3 = await context.MTGCard.FirstOrDefaultAsync(c => c.name == "New Card 3" && c.number == "3");

                Assert.Multiple(() =>
                {
                    Assert.That(card1, Is.Not.Null);
                    Assert.That(card2, Is.Not.Null);
                    Assert.That(card3, Is.Not.Null);

                    Assert.That(card1?.MTGSets, Is.Not.Null);
                    Assert.That(card2?.MTGSets, Is.Not.Null);
                    Assert.That(card3?.MTGSets, Is.Not.Null);

                    Assert.That(card1?.MTGSets.setCode, Is.EqualTo("ATQ"));
                    Assert.That(card2?.MTGSets.setCode, Is.EqualTo("LEA"));
                    Assert.That(card3?.MTGSets.setCode, Is.EqualTo("WAR"));
                });
            }
        }

        [Test]
        public void AddCardsAsync_ThrowsException_WhenSetNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<LengDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_ThrowsException_WhenSetNotFound")
                .Options;

            var newCards = new List<MTGCards>
            {
                new MTGCards { name = "New Card 1", number = "1", setCode = "NONEX" }
            };

            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context, StubLogger);
                MTGTestGenerics.SeedBasicTestData(options);

                // Assert
                var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await service.AddCardsAsync(newCards));
                Assert.That(ex.Message, Is.EqualTo("Set with code NONEX not found."));
            }
        }


        [Test]
        public async Task GetCardsAsync_ReturnsCards_WhenCardNamesStartWithProvidedString()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_ReturnsCards_WhenCardNamesStartWithProvidedString");
            MTGTestGenerics.SeedBasicTestData(options);

            var cardNameToSearch = "Black";
            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context, StubLogger);

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
                var service = new MTGDbService(context, StubLogger);

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
                var service = new MTGDbService(context, StubLogger);

                // Act
                var cards = await service.getCardsAsync(cardNameToSearch, CancellationToken.None);

                // Assert
                Assert.That(cards, Is.Empty);
            }
        }

        [Test]
        public async Task GetCardsAsync_ReturnsCorrectCards_WhenGivenCardName()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_ReturnsCorrectCards_WhenGivenCardName");
            MTGTestGenerics.SeedBasicTestData(options);

            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context, StubLogger);
                var cardName = "Mox Sapphire";

                // Act
                var cards = await service.getCardsAsync(cardName, CancellationToken.None);

                // Assert
                Assert.That(cards, Is.Not.Empty);
                Assert.That(cards.All(c => c.name.Contains(cardName)));
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
                var service = new MTGDbService(context, StubLogger);

                // Act
                var cards = await service.getCardsAsync(cardNameToSearch, CancellationToken.None);

                // Assert
                Assert.That(cards.Count(), Is.EqualTo(20));
            }
        }

        [Test]
        public async Task SearchForCardAsync_HappyPath_ReturnsExpectedResults()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_SearchForCardAsync_HappyPath_ReturnsExpectedResults");
            MTGTestGenerics.SeedBasicTestData(options);

            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context, StubLogger);

                // Act
                var result = await service.SearchForCardAsync("Urza's Tower", CancellationToken.None);

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Not.Empty);
                Assert.That(result, Has.Exactly(1).Items);
            }
        }

        [Test]
        public async Task SearchForCardAsync_NonExistingCardName_ReturnsNull()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_SearchForCardAsync_NonExistingCardName_ReturnsNull");
            MTGTestGenerics.SeedBasicTestData(options);

            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context, StubLogger);

                // Act
                var result = await service.SearchForCardAsync("Urza's Rocketship", CancellationToken.None);

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Empty);
                StubLogger.Received(1).LogInformation("Found 0 cards for search term: Urza's Rocketship");
            }
        }

        [Test]
        public async Task SearchForCardAsync_CancelledToken_ReturnsNull()
        {
            // Arrange
            var options = MTGTestGenerics.CreateOptions("TestDatabase_SearchForCardAsync_CancelledToken_ReturnsNull");
            MTGTestGenerics.SeedBasicTestData(options);

            using (var context = new LengDbContext(options))
            {
                var service = new MTGDbService(context, StubLogger);

                // Create a CancellationToken that is already cancelled
                var cts = new CancellationTokenSource();
                cts.Cancel();
                var cancellationToken = cts.Token;

                // Act
                var result = await service.SearchForCardAsync("Urza's Tower", cancellationToken);

                // Assert
                Assert.That(result, Is.Null);
                StubLogger.Received(1).LogDebug("Search for card cancelled.");
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
                var service = new MTGDbService(context, StubLogger);

                // Act
                var set = await service.GetSetAsync("ATQ");
                if (set == null)
                {
                    Assert.Fail("Set is null");
                }
                else
                {
                    var card = await service.getCardAsync("Urza's Tower", set, "85c");

                    // Assert
                    Assert.Multiple(() =>
                    {
                        Assert.That(card, Is.Not.Null);
                        Assert.That(card.name, Is.EqualTo("Urza's Tower"));
                        Assert.That(card.number, Is.EqualTo("85c"));
                        Assert.That(card.MTGSets.setCode, Is.EqualTo("ATQ"));
                    });
                }
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
                var service = new MTGDbService(context, StubLogger);

                // Act
                var set = await service.GetSetAsync("LEA");
                if (set == null)
                {
                    Assert.Fail("Set is null");
                }
                else
                {
                    var card = await service.getCardAsync("Mox Sapphire", set, 265);

                    // Assert
                    Assert.Multiple(() =>
                    {
                        Assert.That(card, Is.Not.Null);
                        Assert.That(card.name, Is.EqualTo("Mox Sapphire"));
                        Assert.That(card.number, Is.EqualTo("265"));
                        Assert.That(card.MTGSets.setCode, Is.EqualTo("LEA"));
                    });
                }
            }
        }

    }
}