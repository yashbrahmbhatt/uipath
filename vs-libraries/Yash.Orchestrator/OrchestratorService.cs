using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TraceEventType = System.Diagnostics.TraceEventType;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UiPath.Activities.Api.Base;
using UiPath.Studio.Activities.Api;
using Yash.Orchestrator.GetBucketFileReadUri;
using Yash.Orchestrator.GetBuckets;
using Yash.Orchestrator.GetFolders;
using Yash.Orchestrator.GetToken;
using Yash.Orchestrator.GetBucketFiles;
using Yash.Orchestrator.GetAssets;

namespace Yash.Orchestrator
{
    public class OrchestratorService : IOrchestratorService
    {
        private void Log(string message, TraceEventType eventType)
        {
            if (eventType <= _minLogLevel)
                LogAction?.Invoke($"[OrchestratorService] {message}", eventType);
        }

        private async Task EnsureTokenAsync()
        {
            if (string.IsNullOrWhiteSpace(Token))
            {
                await UpdateTokenAsync();
            }
        }
        public Action<string, TraceEventType>? LogAction { get; }
        private readonly RestClient _client;
        private IAccessProvider? _accessProvider;
        private TraceEventType _minLogLevel = TraceEventType.Information;
        public string? BaseURL { get; set; }
        public string Token { get; set; } = null!;
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string[] Scopes { get; set; } = new[] { "OR.Assets.Read", "OR.Folders.Read" };

        public ObservableCollection<Folder> Folders { get; set; } = new();
        public ObservableCollection<KeyValuePair<Folder, ObservableCollection<Asset>>> Assets { get; set; } = new();
        public ObservableCollection<KeyValuePair<Folder, ObservableCollection<Bucket>>> Buckets { get; set; } = new();
        public ObservableCollection<KeyValuePair<Bucket, ObservableCollection<BucketFile>>> BucketFiles { get; set; } = new();

        private OrchestratorService(Action<string, TraceEventType>? log = null, TraceEventType minLogLevel = TraceEventType.Information) {
            LogAction = log;
            _minLogLevel = minLogLevel;
            
            // Initialize RestClient with timeout configuration
            var options = new RestClientOptions()
            {
                Timeout = TimeSpan.FromMinutes(2), // 2 minute timeout
                ThrowOnAnyError = false
            };
            _client = new RestClient(options);
        }
        public OrchestratorService(IAccessProvider accessProvider, Action<string, TraceEventType>? log = null, TraceEventType minLogLevel = TraceEventType.Information) : this(log,minLogLevel)
        {
            if (accessProvider == null) throw new ArgumentNullException(nameof(accessProvider));
            _accessProvider = accessProvider;
        }

        public OrchestratorService(string baseUrl, string clientId, string clientSecret, string[] scopes, Action<string, TraceEventType>? log = null, TraceEventType minLogLevel = TraceEventType.Information) : this(log, minLogLevel)
        {
            BaseURL = baseUrl;
            ClientId = clientId;
            ClientSecret = clientSecret;
            Scopes = scopes;
        }

        public async Task InitializeAsync()
        {
            Log("Initializing OrchestratorService...", TraceEventType.Information);
            Log($"BaseURL resolved to {BaseURL}", TraceEventType.Verbose);

            try
            {
                await UpdateTokenAsync();
                await RefreshFoldersAsync();
                await RefreshAssetsAsync();
                await RefreshBucketsAsync();
                await RefreshBucketFilesAsync();

                Log("Initialization complete.", TraceEventType.Information);
            }
            catch (Exception ex)
            {
                Log($"Initialization failed: {ex.Message}", TraceEventType.Error);
                throw;
            }
        }

        public async Task UpdateTokenAsync(bool force = false)
        {
            Log("Updating access token...", TraceEventType.Information);
            
            // Skip token update if not forced and token already exists
            if (!force && !string.IsNullOrWhiteSpace(Token))
            {
                Log("Token already exists and force refresh not requested. Skipping token update.", TraceEventType.Verbose);
                return;
            }

            if (BaseURL != null && ClientId != null && ClientSecret != null)
            {
                Log("Using client credentials to request token.", TraceEventType.Verbose);
                if (string.IsNullOrWhiteSpace(ClientId) || string.IsNullOrWhiteSpace(ClientSecret) || Scopes.Length == 0)
                {
                    throw new Exception("ClientId and ClientSecret and Scopes must be provided for client credentials flow.");
                }
                var url = "https://cloud.uipath.com/identity_/connect/token";
                var request = new RestRequest(url, Method.Post)
                    .AddParameter("client_id", ClientId)
                    .AddParameter("client_secret", ClientSecret)
                    .AddParameter("grant_type", "client_credentials")
                    .AddParameter("scope", string.Join(" ", Scopes));

                var response = await _client.ExecuteAsync(request);
                if (!response.IsSuccessful)
                {
                    Log($"Failed to get token: {response.ErrorMessage}", TraceEventType.Error);
                    throw new Exception("Failed to get token: " + JsonConvert.SerializeObject(response));
                }

                var token = JsonConvert.DeserializeObject<GetTokenResponse>(response.Content!)!;
                Token = token.AccessToken ?? throw new Exception("Token not found in response");
            }
            else
            {
                Log("Using IAccessProvider to request token.", TraceEventType.Verbose);
                if (_accessProvider == null)
                {
                    throw new Exception("IAccessProvider is not configured and client credentials are not provided.");
                }
                
                var token = await _accessProvider.GetAccessToken(string.Join(" ", Scopes), false);
                if (string.IsNullOrWhiteSpace(token))
                {
                    Log("Token not found from IAccessProvider", TraceEventType.Error);
                    throw new Exception("Token not found from IAccessProvider");
                }
                Token = token;
            }

            Log("Token updated successfully.", TraceEventType.Information);
        }

        public async Task<List<RestResponse>> RefreshAssetsAsync()
        {
            Log("Refreshing assets...", TraceEventType.Information);
            await EnsureTokenAsync();
            
            Assets.Clear();
            var responses = new List<RestResponse>();

            foreach (var folder in Folders)
            {
                var url = $"{BaseURL}/odata/Assets/UiPath.Server.Configuration.OData.GetFiltered";
                var request = new RestRequest(url, Method.Get)
                    .AddHeader("Authorization", $"Bearer {Token}")
                    .AddHeader("X-UIPATH-OrganizationUnitId", folder.Id?.ToString()!);

                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    Log($"Assets retrieved for folder {folder.DisplayName}", TraceEventType.Verbose);
                    var assets = JsonConvert.DeserializeObject<GetAssetsResponse>(response.Content!)!;
                    var assetCollection = new ObservableCollection<Asset>(assets.Assets ?? new());
                    Assets.Add(new KeyValuePair<Folder, ObservableCollection<Asset>>(folder, assetCollection));
                }
                else
                {
                    Log($"Failed to fetch assets for folder {folder.DisplayName}: {response.ErrorMessage}", TraceEventType.Warning);
                }
                responses.Add(response);
            }
            return responses;
        }

        public async Task<RestResponse> RefreshFoldersAsync()
        {
            Log("Refreshing folders...", TraceEventType.Information);
            await EnsureTokenAsync();
            
            var url = $"{BaseURL}/odata/Folders";
            var request = new RestRequest(url, Method.Get)
                .AddHeader("Authorization", $"Bearer {Token}");

            var response = await _client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                Folders.Clear();
                var folders = JsonConvert.DeserializeObject<GetFoldersResponse>(response.Content!)!;
                foreach (var folder in folders.Folders ?? new())
                {
                    Folders.Add(folder);
                    Log($"Folder added: {folder.DisplayName}", TraceEventType.Verbose);
                }
            }
            else
            {
                Log($"Failed to fetch folders: {response.ErrorMessage}", TraceEventType.Error);
            }
            return response;
        }

        public async Task<List<RestResponse>> RefreshBucketsAsync()
        {
            Log("Refreshing buckets...", TraceEventType.Information);
            await EnsureTokenAsync();
            
            Buckets.Clear();
            var responses = new List<RestResponse>();
            var url = $"{BaseURL}/odata/Buckets";

            foreach (var folder in Folders)
            {
                var request = new RestRequest(url, Method.Get)
                    .AddHeader("Authorization", $"Bearer {Token}")
                    .AddHeader("X-UIPATH-OrganizationUnitId", folder.Id?.ToString()!);

                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    Log($"Buckets retrieved for folder {folder.DisplayName}", TraceEventType.Verbose);
                    var buckets = JsonConvert.DeserializeObject<GetBucketsResponse>(response.Content!)!;
                    var bucketCollection = new ObservableCollection<Bucket>(buckets.Buckets ?? new());
                    Buckets.Add(new KeyValuePair<Folder, ObservableCollection<Bucket>>(folder, bucketCollection));
                }
                else
                {
                    Log($"Failed to fetch buckets for folder {folder.DisplayName}: {response.ErrorMessage}", TraceEventType.Warning);
                }
                responses.Add(response);
            }
            return responses;
        }

        public async Task<List<RestResponse>> RefreshBucketFilesAsync()
        {
            Log("Refreshing bucket files...", TraceEventType.Information);
            await EnsureTokenAsync();
            
            BucketFiles.Clear();
            var responses = new List<RestResponse>();

            foreach (var folder in Folders)
            {
                var buckets = Buckets.FirstOrDefault(b => b.Key.Id == folder.Id).Value;
                if (buckets == null)
                {
                    Log($"No buckets found for folder {folder.DisplayName}", TraceEventType.Warning);
                    continue;
                }

                foreach (var bucket in buckets)
                {
                    var url = $"{BaseURL}/odata/Buckets({bucket.Id})/UiPath.Server.Configuration.OData.GetFiles?directory=/&recursive=true";
                    var request = new RestRequest(url, Method.Get)
                        .AddHeader("Authorization", $"Bearer {Token}");

                    var response = await _client.ExecuteAsync(request);
                    if (response.IsSuccessful)
                    {
                        Log($"Files retrieved for bucket {bucket.Name}", TraceEventType.Information);
                        var bucketFiles = JsonConvert.DeserializeObject<GetBucketFilesResponse>(response.Content!)!;
                        var bucketCollection = new ObservableCollection<BucketFile>(bucketFiles.BucketFiles ?? new());
                        BucketFiles.Add(new KeyValuePair<Bucket, ObservableCollection<BucketFile>>(bucket, bucketCollection));
                    }
                    else
                    {
                        Log($"Failed to fetch files for bucket {bucket.Name}: {response.ErrorMessage}", TraceEventType.Warning);
                    }
                    responses.Add(response);
                }
            }
            return responses;
        }

        public async Task<RestResponse> DownloadBucketFile(Bucket bucket, BucketFile bucketFile, string downloadPath)
        {
            Log($"Downloading file {bucketFile.FullPath} from bucket {bucket.Name}...", TraceEventType.Information);

            // Ensure we have a valid token
            await EnsureTokenAsync();

            var url = $"{BaseURL}/odata/Buckets({bucket.Id})/UiPath.Server.Configuration.OData.GetReadUri?path={bucketFile.FullPath}";
            var request = new RestRequest(url, Method.Get).AddHeader("Authorization", $"Bearer {Token}");

            var response = await _client.ExecuteAsync(request);
            if (!response.IsSuccessful)
            {
                Log($"Failed to get read URI for file {bucketFile.FullPath}: {response.ErrorMessage}", TraceEventType.Error);
                return response;
            }

            var res = JsonConvert.DeserializeObject<GetBucketFileReadUriResponse>(response.Content!)!;
            if (string.IsNullOrWhiteSpace(res.Uri))
            {
                Log($"Empty download URI received for file {bucketFile.FullPath}", TraceEventType.Error);
                return response;
            }

            var downloadReq = new RestRequest(res.Uri, Method.Get);
            var downloadResponse = await _client.DownloadDataAsync(downloadReq);

            if (downloadResponse != null)
            {
                var directory = Path.GetDirectoryName(downloadPath) ?? throw new Exception("Could not determine download directory");
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                await File.WriteAllBytesAsync(downloadPath, downloadResponse);
                Log($"File {bucketFile.FullPath} downloaded successfully to {downloadPath}", TraceEventType.Information);
            }
            else
            {
                Log($"Failed to download file data for {bucketFile.FullPath}", TraceEventType.Error);
            }

            return response;
        }
    }

}
