using System.ComponentModel;
using System.IO.Compression;
using System.Net;

namespace Leng.Application {
    public class MtgJsonToDbHandler {
        string fileName = "AllSetFiles.zip";

        public async Task Handle() {
            Console.WriteLine("Handling");
            await DownloadFileFromURLAsync();
            ExtractDownloadedJson();
            await ImportFiles();
        }
        private async Task DownloadFileFromURLAsync() {
            string url = "https://mtgjson.com/api/v5/" + fileName;
            string target = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            if (!File.Exists(target)) {
                var client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompletedCallback);

                await client.DownloadFileTaskAsync(url, target);
            }
        }

        private void ExtractDownloadedJson() {
            string target = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            var targetDirectory = Directory.GetCurrentDirectory() + "\\AllSetFiles";
            ZipFile.ExtractToDirectory(target, targetDirectory, true);
        }

        private async Task ImportFiles() {
            var targetDirectory = Directory.GetCurrentDirectory() + "\\AllSetFiles";
            string[] filesToImport = Directory.GetFiles(targetDirectory, "*.json");

            foreach (var file in filesToImport) {
                Console.WriteLine("Importing file: " + file);
                //await ImportMTGSet(file);
            }
        }

        private void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e) {
            Console.WriteLine($"Downloaded {e.BytesReceived} of {e.TotalBytesToReceive} bytes. " +
                $"{e.ProgressPercentage}% complete...");
        }

        private void DownloadFileCompletedCallback(object sender, AsyncCompletedEventArgs e) {
            if (e.Cancelled) {
                Console.WriteLine("Download has been cancelled.");
            }
            else {
                Console.WriteLine("Download completed!");
            }
        }


    }
}