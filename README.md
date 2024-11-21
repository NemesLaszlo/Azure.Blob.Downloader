# Azure.Blob.Downloader
Download content from Azure Blob Storage, file, "folders" not folders, but you can see them in the UI as they are or full container contents

#### Download from Azure blob storage

In the case of Azure blob, we cannot talk about folders, but when navigating on the UI, we can see a similar structure, which is why it refers to the implementation folders when using different prefixes.

##### Proposed Command-Line Parameters

- `--storage-account-key` (or `-k`): Specifies the Azure Storage account key.
- `--storage-account-name` (or `-a`): Specifies the Azure Storage account name.
- `--container` (or `-c`): Specifies the container name in Azure Blob Storage.
- `--path` (or `-p`): Specifies the path inside the container to either a specific file or a "folder" (prefix).
- `--destination` (or `-d`): Specifies the local destination folder for downloading the blobs.
- `--recursive` (or `-r`): A flag to indicate recursive downloading if a "folder" path is specified.

##### Example Usage

- Download a specific file:

`Azure.Blob.Downloader -k "myaccountkey" -a "mystorage" -c "mycontainer" -p "folder1/file.txt" -d "./downloads"`

- Download a "folder" (prefix) recursively:

`Azure.Blob.Downloader -k "myaccountkey" -a "mystorage" -c "mycontainer" -p "folder1" -d "./downloads" -r`

- Download the entire container:

`Azure.Blob.Downloader -k "myaccountkey" -a "mystorage" -c "mycontainer" -d "./downloads" -r`
