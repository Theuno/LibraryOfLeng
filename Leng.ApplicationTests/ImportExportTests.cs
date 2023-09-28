using Leng.Application.Services;
using Microsoft.AspNetCore.Components.Forms;
using NSubstitute;

namespace BlazorServer.Tests
{
    [TestFixture]
    public class DataUtilityTests
    {
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
    }
}
