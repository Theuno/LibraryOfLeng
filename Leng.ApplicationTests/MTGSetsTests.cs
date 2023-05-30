using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Leng.Application.Services;
using Leng.Domain.Models;
using Leng.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

public class MTGSetsTests
{
    private MTGDbService _MTGDbService;

    private Mock<LengDbContext> _mockDbContext;
    private Mock<DbSet<MTGSets>> _mockDbSet;

    [SetUp]
    public void Setup()
    {
        var data = new List<MTGSets>
        {
            new MTGSets { setCode = "ATQ", /* other properties */ },
            // Add more existing sets as needed
        }.AsQueryable();

        _mockDbSet = new Mock<DbSet<MTGSets>>();
        _mockDbSet.As<IQueryable<MTGSets>>().Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<MTGSets>>().Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<MTGSets>>().Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<MTGSets>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

        _mockDbContext = new Mock<LengDbContext>();
        _mockDbContext.Setup(c => c.MTGSets).Returns(_mockDbSet.Object);
        
        _MTGDbService = new MTGDbService(_mockDbContext.Object);
    }

    [Test]
    public async Task AddSetAsync_AddsSet_WhenSetCodeDoesNotExist()
    {
        var newSet = new MTGSets { setCode = "4ED", /* other properties */ };

        await _MTGDbService.AddSetAsync(newSet);

        _mockDbSet.Verify(m => m.AddAsync(It.IsAny<MTGSets>(), default(CancellationToken)), Times.Once);
        _mockDbContext.Verify(m => m.SaveChangesAsync(default(CancellationToken)), Times.Once);
    }


    [Test]
    public async Task AddSetAsync_DoesNotAddSet_WhenSetCodeExists()
    {
        var existingSet = new MTGSets { setCode = "ATQ", /* other properties */ };

        await _MTGDbService.AddSetAsync(existingSet);

        _mockDbSet.Verify(m => m.AddAsync(It.IsAny<MTGSets>(), default(CancellationToken)), Times.Never);
        _mockDbContext.Verify(m => m.SaveChangesAsync(default(CancellationToken)), Times.Never);
    }

    [Test]
    public async Task AddSetAsync_HandlesNullArg()
    {
        await _MTGDbService.AddSetAsync(null);

        _mockDbSet.Verify(m => m.AddAsync(It.IsAny<MTGSets>(), default(CancellationToken)), Times.Never);
        _mockDbContext.Verify(m => m.SaveChangesAsync(default(CancellationToken)), Times.Never);
    }
}
