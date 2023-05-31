using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Leng.Application.Services;
using Leng.Domain.Models;
using Leng.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using NUnit.Framework;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class MTGSetsTests
{
    [Test]
    public async Task AddSetAsync_AddsSet_WhenSetCodeDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LengDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_AddsSet_WhenSetCodeDoesNotExist") // Unique name for in-memory database
            .Options;

        // Insert seed data into the database using one instance of the context
        using (var context = new LengDbContext(options))
        {
            context.MTGSets.AddRange(new MTGSets { setCode = "ATQ", /* other properties */ },
                                     new MTGSets { setCode = "LEA", name = "Limited Edition Alpha" /* other properties */});
            context.SaveChanges();
        }

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
            Assert.IsTrue(context.MTGSets.Any(set => set.setCode == "4ED"));
        }
    }

    [Test]
    public async Task AddSetAsync_DoesNotAddSet_WhenSetCodeExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LengDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_AddSetAsync_WhenSetCodeExists") // Unique name for in-memory database
            .Options;

        using (var context = new LengDbContext(options))
        {
            context.MTGSets.AddRange(
                new MTGSets { setCode = "ATQ", /* other properties */ },
                new MTGSets { setCode = "LEA", name = "Limited Edition Alpha", /* other properties */ });
            context.SaveChanges();
        }

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
            Assert.AreEqual(2, context.MTGSets.Count());  // Check that the count of sets hasn't increased
        }
    }

    [Test]
    public async Task AddSetAsync_HandlesNullArg()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LengDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_AddSetAsync_HandlesNullArg") // Unique name for in-memory database
            .Options;

        using (var context = new LengDbContext(options))
        {
            context.MTGSets.AddRange(
                new MTGSets { setCode = "ATQ", /* other properties */ },
                new MTGSets { setCode = "LEA", name = "Limited Edition Alpha", /* other properties */ });
            context.SaveChanges();
        }

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
            Assert.AreEqual(2, context.MTGSets.Count());  // Check that the count of sets hasn't increased
        }
    }


    [Test]
    public async Task GetSetCodeAsync_ValidInput_ReturnsSetCode()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LengDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_GetSetCodeAsync_Null") // Unique name for in-memory database
            .Options;

        // Insert seed data into the database using one instance of the context
        using (var context = new LengDbContext(options))
        {
            context.MTGSets.AddRange(
                new MTGSets { setCode = "4ED", name = "Fourth Edition" },
                new MTGSets { setCode = "LEA", name = "Limited Edition Alpha" });
            context.SaveChanges();
        }

        // Arrange
        var setName = "Fourth Edition";
        var expectedSetCode = "4ED";  // Expected setCode for "Limited Edition Alpha" set

        // Act
        string setCode;
        using (var context = new LengDbContext(options))
        {
            var service = new MTGDbService(context);
            setCode = await service.GetSetCodeAsync(setName);
        }

        // Assert
        Assert.AreEqual(expectedSetCode, setCode);
    }

    [Test]
    public async Task GetSetCodeAsync_Throws_WhenSetNameIsNull()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LengDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_GetSetCodeAsync_Null") // Unique name for in-memory database
            .Options;

        // Insert seed data into the database using one instance of the context
        using (var context = new LengDbContext(options))
        {
            context.MTGSets.AddRange(
                new MTGSets { setCode = "ATQ", /* other properties */ },
                new MTGSets { setCode = "LEA", name = "Limited Edition Alpha" /* other properties */});
            context.SaveChanges();
        }

        // Act
        using (var context = new LengDbContext(options))
        {
            var service = new MTGDbService(context);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await service.GetSetCodeAsync(null));
            Assert.AreEqual("Set name cannot be null or empty (Parameter 'setName')", ex.Message);
            Assert.AreEqual("setName", ex.ParamName);
        }
    }

    [Test]
    public async Task GetSetCodeAsync_Throws_WhenSetNameIsEmpty()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LengDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_GetSetCodeAsync_WhenSetNameIsEmpty") // Unique name for in-memory database
            .Options;

        // Insert seed data into the database using one instance of the context
        using (var context = new LengDbContext(options))
        {
            context.MTGSets.AddRange(
                new MTGSets { setCode = "ATQ", },
                new MTGSets { setCode = "LEA", name = "Limited Edition Alpha" });
            context.SaveChanges();
        }

        // Act
        using (var context = new LengDbContext(options))
        {
            var service = new MTGDbService(context);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(async () => await service.GetSetCodeAsync(""));
            Assert.AreEqual("Set name cannot be null or empty (Parameter 'setName')", ex.Message);
            Assert.AreEqual("setName", ex.ParamName);
        }
    }

    [Test]
    public async Task GetSetCodeAsync_ReturnsNull_WhenSetNameDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LengDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_DoesNotExist") // Unique name for in-memory database
            .Options;

        // Insert seed data into the database using one instance of the context
        using (var context = new LengDbContext(options))
        {
            context.MTGSets.AddRange(
                new MTGSets { setCode = "ATQ", /* other properties */ },
                new MTGSets { setCode = "LEA", name = "Limited Edition Alpha" /* other properties */});
            context.SaveChanges();
        }

        // Act
        string setCode;
        using (var context = new LengDbContext(options))
        {
            var service = new MTGDbService(context);
            setCode = await service.GetSetCodeAsync("This set name does not exist");
        }

        // Assert
        Assert.IsNull(setCode);
    }
}
