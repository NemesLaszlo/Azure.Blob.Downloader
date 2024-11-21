using Azure.Storage;
using Azure.Storage.Blobs;
using CommandLine;

class Program
{
    static async Task Main(string[] args)
    {
        var parser = new Parser(settings =>
        {
            settings.HelpWriter = Console.Out;
        });
        var result = parser.ParseArguments<Options>(args);
        await result.MapResult(
            async options => await RunAsync(options),
            errors => Task.FromResult(HandleParseErrors(errors))
        );
    }

    static async Task RunAsync(Options options)
    {
        try
        {
            var blobEndpoint = string.Format("https://{0}.blob.core.windows.net", options.StorageAccountName);
            var credential = new StorageSharedKeyCredential(options.StorageAccountName, options.StorageAccountKey);
            var blobServiceClient = new BlobServiceClient(new Uri(blobEndpoint), credential);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(options.Container);

            var sanitizedPath = SanitizePath(options.Path);
            if (string.IsNullOrEmpty(options.Path))
                await DownloadFolderAsync(blobContainerClient, "", options.Destination); // Download the entire container
            else if (options.Recursive)  
                await DownloadFolderAsync(blobContainerClient, sanitizedPath, options.Destination); // Download "folder" recursively
            else
                await DownloadFileAsync(blobContainerClient, sanitizedPath, options.Destination); // Download single file
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
        }
    }

    static async Task DownloadFileAsync(BlobContainerClient containerClient, string blobPath, string localPath)
    {
        var blobClient = containerClient.GetBlobClient(blobPath);
        var localFilePath = Path.Combine(localPath, Path.GetFileName(blobPath));
        Console.WriteLine($"Downloading {blobPath} to {localFilePath}...");
        await blobClient.DownloadToAsync(localFilePath);
    }

    static async Task DownloadFolderAsync(BlobContainerClient containerClient, string folderPath, string localPath)
    {
        var blobs = containerClient.GetBlobsAsync(prefix: folderPath);
        await foreach (var blob in blobs)
        {
            if (blob.Name.EndsWith("/"))
                continue;
            var localFilePath = Path.Combine(localPath, blob.Name.Substring(folderPath.Length).TrimStart('/'));
            Directory.CreateDirectory(Path.GetDirectoryName(localFilePath));
            Console.WriteLine($"Downloading {blob.Name} to {localFilePath}...");
            await containerClient.GetBlobClient(blob.Name).DownloadToAsync(localFilePath);
        }
    }

    static string SanitizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;
        var sanitizedPath = path.Trim();
        sanitizedPath = sanitizedPath.Replace(" / ", "/")
                                     .Replace("  ", " ");
        sanitizedPath = sanitizedPath.Replace('\\', '/');
        sanitizedPath = sanitizedPath.Replace(" /", "/")
                                     .Replace("/ ", "/");
        return sanitizedPath;
    }

    static int HandleParseErrors(IEnumerable<Error> errors)
    {
        Console.WriteLine("Failed to parse arguments:");
        foreach (var error in errors)
        {
            Console.WriteLine($"  {error.ToString()}");
        }
        return 1;
    }
}

class Options
{
    [Option('k', "storage-account-key", Required = true, HelpText = "Azure Storage Account Key.")]
    public string StorageAccountKey { get; set; }

    [Option('a', "storage-account-name", Required = true, HelpText = "Azure Storage Account Name.")]
    public string StorageAccountName { get; set; }

    [Option('c', "container", Required = true, HelpText = "Container name in Azure Blob Storage.")]
    public string Container { get; set; }

    [Option('p', "path", Required = false, HelpText = "Path to file or 'folder' inside the container.")]
    public string Path { get; set; }

    [Option('d', "destination", Required = true, HelpText = "Local folder where files will be downloaded.")]
    public string Destination { get; set; }

    [Option('r', "recursive", Default = false, HelpText = "Flag to download all contents if the path points to a 'folder'.")]
    public bool Recursive { get; set; }
}
