using Leng.Application.Services;
using Microsoft.AspNetCore.Components.Forms;
using NSubstitute;

namespace BlazorServer.Tests
{
    [TestFixture]
    public class ImportExportUtilityTests
    {
        // The valid headers as per your specified arrays
        private readonly string[] validHeaders1 = { "Card Name", "Card Number", "Set Code", "Count", "Count Foil", "have", "have foil", "want", "want foil", "note" };
        private readonly string[] validHeaders2 = { "kaartnaam", "kaartnummer", "set_code", "c", "c_foil", "h", "h_foil", "w", "w_foil", "notitie" };
        private readonly string[] validHeaders3 = { "Card Name", "Card Number", "Set Code", "Count", "Count Foil", "", "", "", "", "" };
        private readonly string[] validHeaders4 = { "kaartnaam", "kaartnummer", "set_code", "c", "c_foil", "", "", "", "", "" };

        [Test]
        public void ValidateImportFile_NullFile_ThrowsArgumentNullException()
        {
            // Arrange
            IBrowserFile file = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => DataUtility.ValidateImportFile(file));
        }

        [Test]
        public void ValidateImportFile_LargeFileSize_ThrowsArgumentException()
        {
            // Arrange
            var file = Substitute.For<IBrowserFile>();
            file.Size.Returns(10485761);  // Size is just over 10 MB

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => DataUtility.ValidateImportFile(file));
            Assert.That(ex.Message, Is.EqualTo("File size exceeds the 10MB limit (Parameter 'file')"));
        }

        [Test]
        public void ValidateImportFile_InvalidFileType_ThrowsArgumentException()
        {
            // Arrange
            var file = Substitute.For<IBrowserFile>();
            file.Size.Returns(1048576);  // Size is 1 MB
            file.Name.Returns("file.txt");  // Invalid file type

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => DataUtility.ValidateImportFile(file));
            Assert.That(ex.Message, Is.EqualTo("Invalid file type. Only .xlsx files are allowed (Parameter 'file')"));
        }

        [Test]
        public void ValidateImportFile_ValidFile_ReturnsTrue()
        {
            // Arrange
            var file = Substitute.For<IBrowserFile>();
            file.Size.Returns(1048576);  // Size is 1 MB
            file.Name.Returns("file.xlsx");  // Valid file type

            // Act
            var result = DataUtility.ValidateImportFile(file);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ValidateHeaders_ValidHeaders_ReturnsTrue()
        {
            // Arrange & Act
            bool result1 = DataUtility.ValidateHeaders(validHeaders1);
            bool result2 = DataUtility.ValidateHeaders(validHeaders2);
            bool result3 = DataUtility.ValidateHeaders(validHeaders3);
            bool result4 = DataUtility.ValidateHeaders(validHeaders4);

            // Assert
            Assert.That(result1, Is.True, "Failed for validHeaders1");
            Assert.That(result2, Is.True, "Failed for validHeaders2");
            Assert.That(result3, Is.True, "Failed for validHeaders3");
            Assert.That(result4, Is.True, "Failed for validHeaders4");
        }
    }
}
