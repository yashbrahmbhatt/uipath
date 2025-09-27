using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TraceEventType = System.Diagnostics.TraceEventType;
using Yash.Orchestrator.GetAssets;
using Yash.Orchestrator.GetBucketFiles;
using Yash.Orchestrator.GetBuckets;
using Yash.Orchestrator.GetFolders;

namespace Yash.Orchestrator
{
    /// <summary>
    /// Interface for UiPath Orchestrator service operations
    /// </summary>
    public interface IOrchestratorService
    {
        /// <summary>
        /// Action for logging messages
        /// </summary>
        Action<string, TraceEventType>? LogAction { get; }

        /// <summary>
        /// Base URL for the Orchestrator instance
        /// </summary>
        string? BaseURL { get; set; }

        /// <summary>
        /// Authentication token
        /// </summary>
        string Token { get; set; }

        /// <summary>
        /// Client ID for authentication
        /// </summary>
        string? ClientId { get; set; }

        /// <summary>
        /// Client secret for authentication
        /// </summary>
        string? ClientSecret { get; set; }

        /// <summary>
        /// OAuth scopes for authentication
        /// </summary>
        string[] Scopes { get; set; }

        /// <summary>
        /// Collection of folders from Orchestrator
        /// </summary>
        ObservableCollection<Folder> Folders { get; set; }

        /// <summary>
        /// Collection of assets grouped by folder
        /// </summary>
        ObservableCollection<KeyValuePair<Folder, ObservableCollection<Asset>>> Assets { get; set; }

        /// <summary>
        /// Collection of buckets grouped by folder
        /// </summary>
        ObservableCollection<KeyValuePair<Folder, ObservableCollection<Bucket>>> Buckets { get; set; }

        /// <summary>
        /// Collection of bucket files grouped by bucket
        /// </summary>
        ObservableCollection<KeyValuePair<Bucket, ObservableCollection<BucketFile>>> BucketFiles { get; set; }

        /// <summary>
        /// Initialize the service and load all data
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        Task InitializeAsync();

        /// <summary>
        /// Update the authentication token
        /// </summary>
        /// <param name="force">Force token refresh even if current token is valid</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task UpdateTokenAsync(bool force = false);

        /// <summary>
        /// Refresh the list of assets for all folders
        /// </summary>
        /// <returns>A task containing the list of REST responses</returns>
        Task<List<RestResponse>> RefreshAssetsAsync();

        /// <summary>
        /// Refresh the list of folders
        /// </summary>
        /// <returns>A task containing the REST response</returns>
        Task<RestResponse> RefreshFoldersAsync();

        /// <summary>
        /// Refresh the list of buckets for all folders
        /// </summary>
        /// <returns>A task containing the list of REST responses</returns>
        Task<List<RestResponse>> RefreshBucketsAsync();

        /// <summary>
        /// Refresh the list of bucket files for all buckets
        /// </summary>
        /// <returns>A task containing the list of REST responses</returns>
        Task<List<RestResponse>> RefreshBucketFilesAsync();

        /// <summary>
        /// Download a file from a bucket
        /// </summary>
        /// <param name="bucket">The bucket containing the file</param>
        /// <param name="bucketFile">The file to download</param>
        /// <param name="downloadPath">The local path where the file should be saved</param>
        /// <returns>A task containing the REST response</returns>
        Task<RestResponse> DownloadBucketFile(Bucket bucket, BucketFile bucketFile, string downloadPath);
    }
}