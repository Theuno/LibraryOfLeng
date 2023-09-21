using Leng.Application.Services;
using Leng.BlazorServer.Pages;
using Microsoft.AspNetCore.Components.Forms;
using NSubstitute;

namespace TestNamespace
{
    [TestFixture]
    public class ImportExportTests
    {
        //private class TestImportExportBase {
        //    public new Task UploadFiles(IBrowserFile file)
        //    {
        //        return (Task)base.UploadFiles(file);
        //    }
        //}

        [Test]
        public async Task UploadFiles_WhenValidFilePassed_ReturnsSuccessMessage()
        {
            // Arrange
            var browserFile = Substitute.For<IBrowserFile>();
            //var importExport = new ImportExport();

            // Act
            //browserFile.Name
            //await importExport.ImportCardsAsync(browserFile);

            // Assert
            //Assert.AreEqual("File uploaded successfully");
        }
    }
}
