﻿// File: Services/IFileStorageService.cs
namespace Medlemsnavet.Services;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName);
}