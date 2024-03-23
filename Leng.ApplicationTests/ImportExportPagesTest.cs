using Leng.Application.Services;
using Leng.BlazorServer.Pages;
using Leng.Infrastructure;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NSubstitute;
using System.Security.Claims;

namespace Leng.BlazorServer.Tests.Pages
{
    public class ImportExportPagesTest
    {
        private IJSRuntime _jsRuntime;
        private IMTGDbService _dbService;
        private ILogger<CardSheet> _logger;
        private IDbContextFactory<LengDbContext> _dbContextFactory;
        private ImportExport _importExport;
        private Task<AuthenticationState> _authenticationState;

        [SetUp]
        public void SetUp()
        {
            _jsRuntime = Substitute.For<IJSRuntime>();
            _dbService = Substitute.For<IMTGDbService>();
            _logger = Substitute.For<ILogger<CardSheet>>();
            _dbContextFactory = Substitute.For<IDbContextFactory<LengDbContext>>();
            _authenticationState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal()));

            _importExport = new ImportExport
            {
                JS = _jsRuntime,
                DbService = _dbService,
                Logger = _logger,
                DbContextFactory = _dbContextFactory,
                authenticationState = _authenticationState
            };
        }

        [TearDown]
        public void TearDown()
        {
            _jsRuntime = null;
            _dbService = null;
            _logger = null;
            _dbContextFactory = null;
            _importExport = null;
            _authenticationState.Dispose();
        }

        [Test]
        public async Task UploadFiles_ValidFile_ImportCardsAsyncCalled()
        {
            // Arrange
            var file = CreateMockFile(size: 1024, name: "valid.xlsx");
            var stream = Substitute.For<Stream>();
            file.OpenReadStream().Returns(stream);

            // Act
            await _importExport.UploadFiles(file);

            // Assert
            await _dbService.Received(1).ImportCardsAsync(Arg.Any<string>(), null, Arg.Is<Action<string>>(_ => true));


            // Cleanup
            stream.Dispose();
        }

        private IBrowserFile CreateMockFile(long size, string name)
        {
            var file = Substitute.For<IBrowserFile>();
            file.Size.Returns(size);
            file.Name.Returns(name);
            file.OpenReadStream().Returns(Substitute.For<Stream>());
            return file;
        }
    }
}
