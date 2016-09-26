using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCDemo.BlobDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Program p = new Program();
            p.Run().Wait();
        }


        private async Task Run()
        {
            string storageConnectionString = ConfigurationManager.ConnectionStrings["AzureStorage"].ConnectionString;
            CloudStorageAccount account = CloudStorageAccount.Parse(storageConnectionString);

            CloudBlobClient blobClient = account.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("cloudcamp");
            await container.CreateIfNotExistsAsync();

            BlobContinuationToken token = null;
            int counter = 0;
            do
            {
                BlobResultSegment resultSegment = await container.ListBlobsSegmentedAsync(token);
                token = resultSegment.ContinuationToken;
                foreach (IListBlobItem listBlobItem in resultSegment.Results)
                {
                    if (listBlobItem is CloudBlockBlob)
                    {
                        CloudBlockBlob blockBlob = listBlobItem as CloudBlockBlob;
                        Console.WriteLine($"Name: {blockBlob.Name}; Type: {blockBlob.Properties.BlobType}; Size: {blockBlob.Properties.Length} bytes; Type: {blockBlob.Properties.ContentType}");
                        ++counter;
                    }
                    else if (listBlobItem is CloudPageBlob)
                    {
                        CloudPageBlob pageBlob = listBlobItem as CloudPageBlob;
                        Console.WriteLine($"Name: {pageBlob.Name}; Type: {pageBlob.Properties.BlobType}; Size: {pageBlob.Properties.Length} bytes; Type: {pageBlob.Properties.ContentType}");
                        ++counter;
                    }
                    else if (listBlobItem is CloudBlobDirectory)
                    {
                        CloudBlobDirectory diectoryBlob = listBlobItem as CloudBlobDirectory;
                        Console.WriteLine($"Name: {diectoryBlob.Prefix}; Type: Virtual Directory");
                    }
                }
            }
            while (token != null);
            Console.WriteLine("----------------------------");
            Console.WriteLine($"Total Files: {counter}");

            Console.WriteLine("Enter a blob name to download it (or leave empty to skip)");
            string blobName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(blobName))
            {
                CloudBlob blob = container.GetBlobReference(blobName);
                if (blob.Exists())
                {
                    blob.DownloadToFile(Path.Combine(@"C:\Users\eyalb\Downloads\FromAzure", blob.Name), FileMode.Create);
                }
            }

            Console.WriteLine("Enter full path to file for to upload: ");
            string pathToFile = Console.ReadLine();

            string name = pathToFile.Split('\\').Last();
            CloudBlockBlob newBlockBlob = container.GetBlockBlobReference(name);

            await newBlockBlob.UploadFromFileAsync(pathToFile);

            Console.WriteLine("Upload completed. Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
