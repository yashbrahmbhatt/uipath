#!/bin/bash

# Simple Swagger Model Generator for UiPath Orchestrator
# This script creates basic model classes based on the existing pattern
# without requiring jq for JSON parsing

set -e

# Configuration
SWAGGER_FILE="${1:-vs-libraries/Yash.Orchestrator/swagger.json}"
OUTPUT_DIR="${2:-vs-libraries/Yash.Orchestrator}"
NAMESPACE_PREFIX="${3:-Yash.Orchestrator.Generated}"
SERVICE_NAME="${4:-OrchestratorService}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to extract JSON value without jq (simple string matching)
extract_json_value() {
    local json_file="$1"
    local key="$2"
    local context="$3"
    
    if [ -n "$context" ]; then
        # Extract value within a specific context
        sed -n "/$context/,/^[[:space:]]*}/p" "$json_file" | grep "\"$key\":" | head -1 | sed 's/.*"'"$key"'":[[:space:]]*"\([^"]*\)".*/\1/' | sed 's/.*"'"$key"'":[[:space:]]*\([^,}]*\).*/\1/'
    else
        # Extract value globally
        grep "\"$key\":" "$json_file" | head -1 | sed 's/.*"'"$key"'":[[:space:]]*"\([^"]*\)".*/\1/' | sed 's/.*"'"$key"'":[[:space:]]*\([^,}]*\).*/\1/'
    fi
}

# Function to extract OData endpoints from swagger
get_odata_endpoints() {
    local swagger_file="$1"
    
    echo -e "${BLUE}� Extracting OData endpoints...${NC}"
    
    # Extract all paths that contain "/odata/" and use GET method
    grep -n "/odata/" "$swagger_file" | grep -v "UiPath.Server.Configuration.OData" | while read -r line; do
        local line_num=$(echo "$line" | cut -d: -f1)
        local path=$(echo "$line" | sed 's/.*"\(\/odata\/[^"]*\)".*/\1/')
        
        # Check if this path has a GET method by looking at the next few lines
        local get_method_line=$((line_num + 1))
        local method=$(sed -n "${get_method_line}p" "$swagger_file" | grep -o '"get"')
        
        if [ "$method" = '"get"' ]; then
            local entity=$(echo "$path" | sed 's/\/odata\/\([^\/]*\).*/\1/')
            echo "$entity|$path"
        fi
    done | sort -u
}

# Function to extract definitions/models from swagger
get_swagger_models() {
    local swagger_file="$1"
    
    echo -e "${BLUE}🔍 Extracting model definitions...${NC}"
    
    # Find the definitions section and extract model names
    sed -n '/^  "definitions": {$/,/^  }$/p' "$swagger_file" | \
    grep '^    "[^"]*": {' | \
    sed 's/^    "\([^"]*\)".*/\1/' | \
    grep -E "(Dto|Response)$" | \
    sort -u
}

# Function to determine parent-child relationships for KVP collections
get_parent_child_relationships() {
    local swagger_file="$1"
    
    echo -e "${BLUE}🔍 Determining parent-child relationships...${NC}"
    
    # Define known parent-child relationships based on UiPath Orchestrator structure
    cat << 'EOF'
FolderDto|AssetDto
FolderDto|BucketDto
FolderDto|ProcessDto
FolderDto|RobotDto
FolderDto|QueueDto
BucketDto|BucketFileDto
ProcessDto|JobDto
QueueDto|QueueItemDto
EOF
}

echo -e "${BLUE}🚀 Dynamic Swagger Model Generator for UiPath Orchestrator${NC}"
echo "============================================================="
echo "Swagger File: $SWAGGER_FILE"
echo "Output Directory: $OUTPUT_DIR"
echo "Namespace: $NAMESPACE_PREFIX"
echo "Service Name: $SERVICE_NAME"
echo ""

# Check if swagger file exists
if [ ! -f "$SWAGGER_FILE" ]; then
    echo -e "${RED}❌ Error: Swagger file not found: $SWAGGER_FILE${NC}"
    exit 1
fi

# Create output directories
mkdir -p "$OUTPUT_DIR/Models/DTOs"
mkdir -p "$OUTPUT_DIR/Models/Responses"
mkdir -p "$OUTPUT_DIR/Services"
mkdir -p "$OUTPUT_DIR/Extensions"

echo -e "${YELLOW}📁 Created directory structure${NC}"

# Function to extract properties from swagger model definition
get_model_properties() {
    local swagger_file="$1"
    local model_name="$2"
    
    # Try to find the model definition
    if ! grep -q "\"$model_name\": {" "$swagger_file"; then
        echo -e "${YELLOW}⚠️  No definition found for $model_name, using basic properties${NC}" >&2
        echo "Id:integer Name:string Description:string"
        return
    fi
    
    # Extract the properties from the model definition
    # Find the line number where the model starts
    local start_line
    start_line=$(grep -n "\"$model_name\": {" "$swagger_file" | head -1 | cut -d: -f1)
    
    if [ -z "$start_line" ]; then
        echo -e "${YELLOW}⚠️  Could not locate $model_name definition${NC}" >&2
        echo "Id:integer Name:string Description:string"
        return
    fi
    
    # Extract the model section and look for properties  
    sed -n "${start_line},/^    },/p" "$swagger_file" | \
    sed -n '/^      "properties": {$/,/^      }$/p' | \
    grep '^ *"[^"]*": {' | \
    sed 's/^ *"\([^"]*\)": {.*/\1/' | \
    while read -r prop_name; do
        if [ -n "$prop_name" ]; then
            # Extract the property type by looking at the specific property definition
            prop_type="string"
            
            # Look for the property definition to determine type
            prop_def=$(sed -n "${start_line},/^    },/p" "$swagger_file" | \
                      sed -n "/\"$prop_name\": {/,/^        }/p")
            
            if echo "$prop_def" | grep -q '"type": "integer"'; then
                prop_type="integer"
            elif echo "$prop_def" | grep -q '"type": "boolean"'; then
                prop_type="boolean"
            elif echo "$prop_def" | grep -q '"type": "array"'; then
                prop_type="array"
            elif echo "$prop_def" | grep -q '"format": "date-time"'; then
                prop_type="datetime"
            elif echo "$prop_def" | grep -q '"\$ref":'; then
                ref_model=$(echo "$prop_def" | grep '"\$ref":' | sed 's/.*"#\/definitions\/\([^"]*\)".*/\1/')
                prop_type="ref:$ref_model"
            fi
            
            echo "$prop_name:$prop_type"
        fi
    done
}

# Function to convert swagger type to C# type
swagger_type_to_csharp() {
    local swagger_type="$1"
    local is_nullable="$2"
    
    case "$swagger_type" in
        "integer") echo "int$([ "$is_nullable" = "true" ] && echo "?")" ;;
        "boolean") echo "bool$([ "$is_nullable" = "true" ] && echo "?")" ;;
        "datetime") echo "DateTime$([ "$is_nullable" = "true" ] && echo "?")" ;;
        "array") echo "List<object>$([ "$is_nullable" = "true" ] && echo "?")" ;;
        ref:*) 
            local ref_type="${swagger_type#ref:}"
            echo "$ref_type$([ "$is_nullable" = "true" ] && echo "?")"
            ;;
        *) echo "string$([ "$is_nullable" = "true" ] && echo "?")" ;;
    esac
}

# Function to generate model with actual swagger properties
generate_swagger_model() {
    local model_name="$1"
    local namespace="$2"
    local swagger_file="$3"
    
    echo -e "${BLUE}  📝 Generating swagger-based model: $model_name${NC}"
    
    # Get properties from swagger definition
    local properties
    properties=$(get_model_properties "$swagger_file" "$model_name")
    
    cat > "$OUTPUT_DIR/Models/DTOs/${model_name}.cs" << EOF
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace $namespace.Models.DTOs
{
    /// <summary>
    /// Model class for $model_name
    /// Auto-generated from Swagger specification
    /// </summary>
    public class $model_name
    {
EOF

    # Generate properties from swagger definition
    if [ -n "$properties" ]; then
        echo "$properties" | while IFS=':' read -r prop_name prop_type; do
            if [ -n "$prop_name" ] && [ -n "$prop_type" ]; then
                local csharp_type
                csharp_type=$(swagger_type_to_csharp "$prop_type" "true")
                
                cat >> "$OUTPUT_DIR/Models/DTOs/${model_name}.cs" << EOF
        /// <summary>
        /// $prop_name property
        /// </summary>
        [JsonProperty("$prop_name", NullValueHandling = NullValueHandling.Ignore)]
        public $csharp_type $prop_name { get; set; }

EOF
            fi
        done
    else
        # Fallback to basic properties if swagger parsing fails
        cat >> "$OUTPUT_DIR/Models/DTOs/${model_name}.cs" << EOF
        /// <summary>
        /// Unique identifier
        /// </summary>
        [JsonProperty("Id", NullValueHandling = NullValueHandling.Ignore)]
        public int? Id { get; set; }

        /// <summary>
        /// Display name
        /// </summary>
        [JsonProperty("Name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name { get; set; }

EOF
    fi

    cat >> "$OUTPUT_DIR/Models/DTOs/${model_name}.cs" << EOF
    }
}
EOF
}

# Function to generate response wrapper classes
generate_response() {
    local response_name="$1"
    local model_name="$2"
    local namespace="$3"
    local is_collection="$4"
    
    echo -e "${BLUE}  📝 Generating response: $response_name${NC}"
    
    cat > "$OUTPUT_DIR/Models/Responses/${response_name}.cs" << EOF
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using $namespace.Models.DTOs;

namespace $namespace.Models.Responses
{
    /// <summary>
    /// Response wrapper for $model_name
    /// Auto-generated from Swagger specification
    /// </summary>
    public class $response_name
    {
        /// <summary>
        /// OData context
        /// </summary>
        [JsonProperty("@odata.context", NullValueHandling = NullValueHandling.Ignore)]
        public string? OdataContext { get; set; }

        /// <summary>
        /// OData count
        /// </summary>
        [JsonProperty("@odata.count", NullValueHandling = NullValueHandling.Ignore)]
        public int? OdataCount { get; set; }

EOF

    if [ "$is_collection" = "true" ]; then
        cat >> "$OUTPUT_DIR/Models/Responses/${response_name}.cs" << EOF
        /// <summary>
        /// Collection of $model_name items
        /// </summary>
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public List<$model_name>? Items { get; set; }
EOF
    else
        cat >> "$OUTPUT_DIR/Models/Responses/${response_name}.cs" << EOF
        /// <summary>
        /// Single $model_name item
        /// </summary>
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public $model_name? Item { get; set; }
EOF
    fi

    cat >> "$OUTPUT_DIR/Models/Responses/${response_name}.cs" << EOF
    }
}
EOF
}

# Extract OData endpoints and models from swagger
echo -e "${YELLOW}� Analyzing Swagger specification...${NC}"
odata_endpoints=($(get_odata_endpoints "$SWAGGER_FILE"))
swagger_models=($(get_swagger_models "$SWAGGER_FILE"))
parent_child_relationships=($(get_parent_child_relationships "$SWAGGER_FILE"))

echo "Found ${#odata_endpoints[@]} OData endpoints"
echo "Found ${#swagger_models[@]} model definitions"
echo "Found ${#parent_child_relationships[@]} parent-child relationships"
echo ""

# Generate basic models based on swagger definitions
echo -e "${YELLOW}📦 Generating model classes from swagger...${NC}"

# Only generate models that are referenced in OData endpoints
declared_models=()
for endpoint_info in "${odata_endpoints[@]}"; do
    if [ -n "$endpoint_info" ]; then
        entity=$(echo "$endpoint_info" | cut -d'|' -f1)
        model_name="${entity}Dto"
        
        # Simple check for model existence
        model_exists=$(echo "${swagger_models[@]}" | grep -o "$model_name" | head -1)
        already_declared=$(echo "${declared_models[@]}" | grep -o "$model_name" | head -1)
        
        if [ "$model_exists" = "$model_name" ] && [ "$already_declared" != "$model_name" ]; then
            declared_models+=("$model_name")
            generate_swagger_model "$model_name" "$NAMESPACE_PREFIX" "$SWAGGER_FILE"
            
            # Generate response wrappers
            response_name="${entity}Response"
            generate_response "$response_name" "$model_name" "$NAMESPACE_PREFIX" "false"
            
            # Generate collection response
            collection_response_name="${entity}CollectionResponse"
            generate_response "$collection_response_name" "$model_name" "$NAMESPACE_PREFIX" "true"
        fi
    fi
done

# Function to generate dynamic Observable collections
generate_observable_collections() {
    local namespace="$1"
    
    echo ""
    echo "        #region Dynamic Observable Collections"
    echo "        "
    
    # Generate simple collections for each entity
    for endpoint_info in "${odata_endpoints[@]}"; do
        if [ -n "$endpoint_info" ]; then
            local entity=$(echo "$endpoint_info" | cut -d'|' -f1)
            local model_name="${entity}Dto"
            
            # Check if this model was declared
            model_declared=$(echo "${declared_models[@]}" | grep -o "$model_name" | head -1)
            if [ "$model_declared" = "$model_name" ]; then
                echo "        /// <summary>"
                echo "        /// Collection of $entity items"
                echo "        /// </summary>"
                echo "        public ObservableCollection<${model_name}> ${entity}s { get; set; } = new();"
                echo "        "
            fi
        fi
    done
    
    # Generate KVP collections for parent-child relationships
    for relationship in "${parent_child_relationships[@]}"; do
        if [ -n "$relationship" ]; then
            local parent=$(echo "$relationship" | cut -d'|' -f1)
            local child=$(echo "$relationship" | cut -d'|' -f2)
            local parent_entity=$(echo "$parent" | sed 's/Dto$//')
            local child_entity=$(echo "$child" | sed 's/Dto$//')
            
            # Check if both parent and child are in our declared models
            parent_declared=$(echo "${declared_models[@]}" | grep -o "$parent" | head -1)
            child_declared=$(echo "${declared_models[@]}" | grep -o "$child" | head -1)
            
            if [ "$parent_declared" = "$parent" ] && [ "$child_declared" = "$child" ]; then
                echo "        /// <summary>"
                echo "        /// Collection of $child_entity items by $parent_entity"
                echo "        /// </summary>"
                echo "        public ObservableCollection<KeyValuePair<${parent}, ObservableCollection<${child}>>> ${child_entity}sByParent { get; set; } = new();"
                echo "        "
            fi
        fi
    done
    
    echo "        #endregion"
}

# Function to generate dynamic refresh methods
generate_refresh_methods() {
    local namespace="$1"
    
    echo ""
    echo "        #region Dynamic Refresh Methods"
    echo "        "
    
    # Generate refresh methods for each OData endpoint
    for endpoint_info in "${odata_endpoints[@]}"; do
        if [ -n "$endpoint_info" ]; then
            local entity=$(echo "$endpoint_info" | cut -d'|' -f1)
            local path=$(echo "$endpoint_info" | cut -d'|' -f2)
            local model_name="${entity}Dto"
            local collection_response="${entity}CollectionResponse"
            
            # Check if this model was declared
            model_declared=$(echo "${declared_models[@]}" | grep -o "$model_name" | head -1)
            if [ "$model_declared" = "$model_name" ]; then
                echo "        /// <summary>"
                echo "        /// Refreshes the $entity collection"
                echo "        /// </summary>"
                echo "        /// <returns>REST response</returns>"
                echo "        public async Task<RestResponse> Refresh${entity}sAsync()"
                echo "        {"
                echo "            Log(\"Refreshing $entity items...\", TraceEventType.Information);"
                echo "            var url = \$\"{BaseURL}$path\";"
                echo "            var request = new RestRequest(url, Method.Get)"
                echo "                .AddHeader(\"Authorization\", \$\"Bearer {Token}\");"
                echo ""
                echo "            var response = await _client.ExecuteAsync(request);"
                echo "            if (response.IsSuccessful)"
                echo "            {"
                echo "                ${entity}s.Clear();"
                echo "                try"
                echo "                {"
                echo "                    var ${entity,,}Response = JsonConvert.DeserializeObject<${collection_response}>(response.Content!)!;"
                echo "                    if (${entity,,}Response?.Items != null)"
                echo "                    {"
                echo "                        foreach (var ${entity,,} in ${entity,,}Response.Items)"
                echo "                        {"
                echo "                            ${entity}s.Add(${entity,,});"
                echo "                            Log(\$\"${entity} added: {${entity,,}.Name}\", TraceEventType.Verbose);"
                echo "                        }"
                echo "                    }"
                echo "                }"
                echo "                catch (Exception ex)"
                echo "                {"
                echo "                    Log(\$\"Failed to parse $entity response: {ex.Message}\", TraceEventType.Error);"
                echo "                }"
                echo "            }"
                echo "            else"
                echo "            {"
                echo "                Log(\$\"Failed to fetch $entity items: {response.ErrorMessage}\", TraceEventType.Error);"
                echo "            }"
                echo "            return response;"
                echo "        }"
                echo "        "
            fi
        fi
    done
    
    echo "        #endregion"
}

echo -e "${GREEN}✅ Generated basic models${NC}"

# Generate main service class
echo -e "${YELLOW}🏗️  Generating service class...${NC}"

cat > "$OUTPUT_DIR/Services/${SERVICE_NAME}.cs" << EOF
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;
using UiPath.Activities.Api.Base;
using UiPath.Studio.Activities.Api;
using $NAMESPACE_PREFIX.Models.DTOs;
using $NAMESPACE_PREFIX.Models.Responses;

namespace $NAMESPACE_PREFIX.Services
{
    /// <summary>
    /// Service class for UiPath Orchestrator API
    /// Dynamically generated from Swagger specification
    /// </summary>
    public class $SERVICE_NAME : I$SERVICE_NAME
    {
        #region Private Fields
        
        private readonly RestClient _client = new RestClient();
        private IAccessProvider? _accessProvider;
        private readonly TraceEventType _minLogLevel = TraceEventType.Information;
        private readonly bool _useProvidedHttpClient = false;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Logging action
        /// </summary>
        public readonly Action<string, TraceEventType>? LogAction;
        
        /// <summary>
        /// Base URL for the Orchestrator API
        /// </summary>
        public string? BaseURL { get; set; }
        
        /// <summary>
        /// Authentication token
        /// </summary>
        public string Token { get; set; } = null;
        
        /// <summary>
        /// Client ID for authentication
        /// </summary>
        public string? ClientId { get; set; }
        
        /// <summary>
        /// Client Secret for authentication
        /// </summary>
        public string? ClientSecret { get; set; }
        
        /// <summary>
        /// OAuth scopes
        /// </summary>
        public string[] Scopes { get; set; } = new[] { "OR.Assets.Read", "OR.Folders.Read" };
        $(generate_observable_collections "$NAMESPACE_PREFIX")
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Private constructor with logging configuration
        /// </summary>
        /// <param name="log">Optional logging action</param>
        /// <param name="minLogLevel">Minimum log level</param>
        private $SERVICE_NAME(Action<string, TraceEventType>? log = null, TraceEventType minLogLevel = TraceEventType.Information)
        {
            LogAction = log;
            _minLogLevel = minLogLevel;
        }
        
        /// <summary>
        /// Constructor with HttpClient
        /// </summary>
        /// <param name="client">HTTP client to use for requests</param>
        /// <param name="log">Optional logging action</param>
        /// <param name="minLogLevel">Minimum log level</param>
        public $SERVICE_NAME(HttpClient client, Action<string, TraceEventType>? log = null, TraceEventType minLogLevel = TraceEventType.Information) 
            : this(log, minLogLevel)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            _client = new RestClient(client);
            _useProvidedHttpClient = true;
        }
        
        /// <summary>
        /// Constructor with IAccessProvider
        /// </summary>
        /// <param name="accessProvider">Access provider for authentication</param>
        /// <param name="log">Optional logging action</param>
        /// <param name="minLogLevel">Minimum log level</param>
        public $SERVICE_NAME(IAccessProvider accessProvider, Action<string, TraceEventType>? log = null, TraceEventType minLogLevel = TraceEventType.Information) 
            : this(log, minLogLevel)
        {
            if (accessProvider == null) throw new ArgumentNullException(nameof(accessProvider));
            _accessProvider = accessProvider;
        }
        
        /// <summary>
        /// Constructor with client credentials
        /// </summary>
        /// <param name="baseUrl">Base URL for the API</param>
        /// <param name="clientId">Client ID for OAuth</param>
        /// <param name="clientSecret">Client secret for OAuth</param>
        /// <param name="scopes">OAuth scopes</param>
        /// <param name="log">Optional logging action</param>
        /// <param name="minLogLevel">Minimum log level</param>
        public $SERVICE_NAME(string baseUrl, string clientId, string clientSecret, string[] scopes, Action<string, TraceEventType>? log = null, TraceEventType minLogLevel = TraceEventType.Information) 
            : this(log, minLogLevel)
        {
            BaseURL = baseUrl;
            ClientId = clientId;
            ClientSecret = clientSecret;
            Scopes = scopes;
        }
        
        #endregion
        
        #region Logging
        
        /// <summary>
        /// Logs a message with the specified event type
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="eventType">Event type</param>
        private void Log(string message, TraceEventType eventType)
        {
            if (eventType <= _minLogLevel)
                LogAction?.Invoke(message, eventType);
        }
        
        #endregion
        
        #region Authentication
        
        /// <summary>
        /// Updates the authentication token
        /// </summary>
        /// <param name="force">Force token update</param>
        public async Task UpdateTokenAsync(bool force = false)
        {
            Log("Updating access token...", TraceEventType.Information);
            
            // If HttpClient was provided, assume token is handled externally
            if (_useProvidedHttpClient)
            {
                Log("Using provided HttpClient - assuming authentication is handled externally.", TraceEventType.Information);
                if (string.IsNullOrWhiteSpace(Token))
                {
                    Log("Warning: No token set when using provided HttpClient. Set Token property manually if needed.", TraceEventType.Warning);
                }
                return;
            }
            
            if (BaseURL == null)
            {
                if (_accessProvider == null)
                {
                    throw new Exception("BaseURL is not set and IAccessProvider is not provided.");
                }
                BaseURL = await _accessProvider.GetResourceUrl("Orchestrator");
            }
            Log(\$"BaseURL resolved to {BaseURL}", TraceEventType.Information);
            
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
                    Log(\$"Failed to get token: {response.ErrorMessage}", TraceEventType.Error);
                    throw new Exception("Failed to get token: " + JsonConvert.SerializeObject(response));
                }

                var token = JsonConvert.DeserializeObject<dynamic>(response.Content!)!;
                Token = token.access_token ?? throw new Exception("Token not found in response");
            }
            else
            {
                if (_accessProvider != null)
                {
                    Log("Using IAccessProvider to request token.", TraceEventType.Information);
                    var token = await _accessProvider?.GetAccessToken("Orchestrator", false);
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        Log("Token not found from IAccessProvider", TraceEventType.Error);
                        throw new Exception("Token not found from IAccessProvider");
                    }
                    Token = token;
                }
                else
                {
                    throw new Exception("BaseURL, ClientId, ClientSecret are not set and IAccessProvider is not provided.");
                }
            }

            Log("Token updated successfully.", TraceEventType.Information);
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initializes the service and loads basic data
        /// </summary>
        public async Task InitializeAsync()
        {
            Log("Initializing service...", TraceEventType.Information);
            Log(\$"BaseURL resolved to {BaseURL}", TraceEventType.Verbose);

            await UpdateTokenAsync();
            
            // Call all refresh methods for available endpoints
EOF

# Generate dynamic initialization calls
for endpoint_info in "${odata_endpoints[@]}"; do
    if [ -n "$endpoint_info" ]; then
        entity=$(echo "$endpoint_info" | cut -d'|' -f1)
        model_name="${entity}Dto"
        
        # Check if this model was declared
        model_declared=$(echo "${declared_models[@]}" | grep -o "$model_name" | head -1)
        if [ "$model_declared" = "$model_name" ]; then
            echo "            await Refresh${entity}sAsync();" >> "$OUTPUT_DIR/Services/${SERVICE_NAME}.cs"
        fi
    fi
done

cat >> "$OUTPUT_DIR/Services/${SERVICE_NAME}.cs" << 'EOF'

            Log("Initialization complete.", TraceEventType.Information);
        }
        
        #endregion
EOF

# Now generate the dynamic refresh methods
generate_refresh_methods "$NAMESPACE_PREFIX" >> "$OUTPUT_DIR/Services/${SERVICE_NAME}.cs"

cat >> "$OUTPUT_DIR/Services/${SERVICE_NAME}.cs" << 'EOF'
    }
}
EOF

# Generate interface for the service
echo -e "${YELLOW}📋 Generating service interface...${NC}"

cat > "$OUTPUT_DIR/Services/I${SERVICE_NAME}.cs" << EOF
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using RestSharp;
using $NAMESPACE_PREFIX.Models.DTOs;

namespace $NAMESPACE_PREFIX.Services
{
    /// <summary>
    /// Interface for UiPath Orchestrator service
    /// </summary>
    public interface I$SERVICE_NAME
    {
        #region Properties
        
        /// <summary>
        /// Base URL for the Orchestrator API
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
        /// Client Secret for authentication
        /// </summary>
        string? ClientSecret { get; set; }
        
        /// <summary>
        /// OAuth scopes
        /// </summary>
        string[] Scopes { get; set; }
        
        /// <summary>
        /// Collection of folders
        /// </summary>
        ObservableCollection<FolderDto> Folders { get; set; }
        
        /// <summary>
        /// Collection of assets by folder
        /// </summary>
        ObservableCollection<KeyValuePair<FolderDto, ObservableCollection<AssetDto>>> Assets { get; set; }
        
        /// <summary>
        /// Collection of buckets by folder
        /// </summary>
        ObservableCollection<KeyValuePair<FolderDto, ObservableCollection<BucketDto>>> Buckets { get; set; }
        
        /// <summary>
        /// Collection of bucket files by bucket
        /// </summary>
        ObservableCollection<KeyValuePair<BucketDto, ObservableCollection<BucketFileDto>>> BucketFiles { get; set; }
        
        #endregion
        
        #region Authentication
        
        /// <summary>
        /// Updates the authentication token
        /// </summary>
        /// <param name="force">Force token update</param>
        Task UpdateTokenAsync(bool force = false);
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initializes the service and loads basic data
        /// </summary>
        Task InitializeAsync();
        
        #endregion
        
        #region Data Operations
        
        /// <summary>
        /// Refreshes the folders collection
        /// </summary>
        /// <returns>REST response</returns>
        Task<RestResponse> RefreshFoldersAsync();
        
        /// <summary>
        /// Refreshes the assets collection for all folders
        /// </summary>
        /// <returns>List of REST responses</returns>
        Task<List<RestResponse>> RefreshAssetsAsync();
        
        /// <summary>
        /// Refreshes the buckets collection for all folders
        /// </summary>
        /// <returns>List of REST responses</returns>
        Task<List<RestResponse>> RefreshBucketsAsync();
        
        /// <summary>
        /// Refreshes bucket files for all buckets
        /// </summary>
        /// <returns>List of REST responses</returns>
        Task<List<RestResponse>> RefreshBucketFilesAsync();
        
        #endregion
    }
}
EOF

# Generate README for the generated code
echo -e "${YELLOW}📄 Generating documentation...${NC}"

cat > "$OUTPUT_DIR/README.md" << EOF
# Generated UiPath Orchestrator API Models and Service

This directory contains auto-generated C# classes for the UiPath Orchestrator API, based on common patterns from the existing codebase.

## Generated Structure

\`\`\`
Generated/
├── Models/              # Data models and DTOs
├── Responses/           # Response wrapper classes
├── Services/           # Service classes and interfaces
└── README.md           # This file
\`\`\`

## Usage

### Basic Service Usage

\`\`\`csharp
using $NAMESPACE_PREFIX.Services;
using System.Net.Http;

// Create service instance
var httpClient = new HttpClient();
var service = new $SERVICE_NAME(httpClient);

// Set base URL and token
service.BaseURL = "https://cloud.uipath.com/your-org/orchestrator_";
service.SetToken("your-bearer-token");

// Refresh data
await service.RefreshFoldersAsync();
await service.RefreshAssetsAsync();
await service.RefreshBucketsAsync();

// Access collections
foreach (var folder in service.Folders)
{
    Console.WriteLine(\$"Folder: {folder.Name}");
}
\`\`\`

### Model Usage

\`\`\`csharp
using $NAMESPACE_PREFIX.Models;
using Newtonsoft.Json;

// Deserialize API response
var jsonResponse = "{ ... }";
var asset = JsonConvert.DeserializeObject<AssetDto>(jsonResponse);
\`\`\`

## Generated Models

The following basic models have been generated:

- **AssetDto** - Asset information
- **FolderDto** - Folder/Organization Unit information  
- **BucketDto** - Storage bucket information
- **BucketFileDto** - Files in storage buckets
- **ProcessDto** - Process/automation information
- **JobDto** - Job execution information
- **QueueDto** - Queue information
- **QueueItemDto** - Queue item information
- **RobotDto** - Robot information
- **UserDto** - User information
- **RoleDto** - Role information
- **ScheduleDto** - Schedule information
- **LogDto** - Log entry information
- **AlertDto** - Alert/notification information
- And more...

Each model has corresponding response wrapper classes for single items and collections.

## Service Methods

The generated service includes methods following the pattern from \`OrchestratorService\`:

- **RefreshFoldersAsync()** - Loads all accessible folders
- **RefreshAssetsAsync()** - Loads assets for all folders
- **RefreshBucketsAsync()** - Loads buckets for all folders
- **GetODataCollectionAsync<T>()** - Generic method for OData endpoints

## Customization

The generated code provides a foundation that needs customization:

1. **Model Properties**: Update model classes with actual properties from API responses
2. **Validation**: Add appropriate validation attributes
3. **Business Logic**: Implement specific business rules
4. **Error Handling**: Add robust error handling
5. **Additional Methods**: Implement more API endpoints as needed

## Next Steps

1. **Test the service** with your actual Orchestrator instance
2. **Update model properties** based on real API responses
3. **Add missing API methods** for jobs, queues, processes, etc.
4. **Implement proper error handling** and logging
5. **Add unit tests** for the service methods

## Notes

- This is a basic template - customize based on your specific needs
- Model properties are generic - update them based on actual API responses
- The service follows patterns from the existing \`OrchestratorService\`
- Consider adding caching, retry logic, and advanced error handling

Generated on: $(date)
Generator: Simple Swagger Model Generator (no jq dependency)
EOF

# Generate a sample usage file
cat > "$OUTPUT_DIR/USAGE_EXAMPLE.cs" << EOF
using System;
using System.Threading.Tasks;
using System.Net.Http;
using $NAMESPACE_PREFIX.Services;

namespace $NAMESPACE_PREFIX.Examples
{
    /// <summary>
    /// Example usage of the generated service
    /// </summary>
    public class ServiceUsageExample
    {
        public static async Task ExampleUsage()
        {
            // Create HTTP client
            using var httpClient = new HttpClient();
            
            // Create service instance
            var service = new $SERVICE_NAME(httpClient, 
                (message, level) => Console.WriteLine(\$"[{level}] {message}"));
            
            // Configure service
            service.BaseURL = "https://cloud.uipath.com/your-org/orchestrator_";
            service.SetToken("your-bearer-token-here");
            
            try
            {
                // Load folders
                Console.WriteLine("Loading folders...");
                await service.RefreshFoldersAsync();
                Console.WriteLine(\$"Loaded {service.Folders.Count} folders");
                
                // Load assets
                Console.WriteLine("Loading assets...");
                await service.RefreshAssetsAsync();
                Console.WriteLine(\$"Loaded assets for {service.Assets.Count} folders");
                
                // Load buckets
                Console.WriteLine("Loading buckets...");
                await service.RefreshBucketsAsync();
                Console.WriteLine(\$"Loaded buckets for {service.Buckets.Count} folders");
                
                // Display results
                foreach (var folder in service.Folders)
                {
                    Console.WriteLine(\$"Folder: {folder.Name} (ID: {folder.Id})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(\$"Error: {ex.Message}");
            }
        }
    }
}
EOF

echo ""
echo -e "${GREEN}🎉 Generation completed successfully!${NC}"
echo "============================================="
echo -e "${BLUE}📊 Summary:${NC}"
echo "  📁 Output directory: $OUTPUT_DIR"
echo "  📦 Generated ${#declared_models[@]} dynamic models in: $OUTPUT_DIR/Models/"
echo "  📋 Generated response wrappers in: $OUTPUT_DIR/Models/Responses/"
echo "  🏗️  Generated service in: $OUTPUT_DIR/Services/"
echo "  📄 Documentation: $OUTPUT_DIR/README.md"
echo "  💡 Usage example: $OUTPUT_DIR/USAGE_EXAMPLE.cs"
echo ""
echo -e "${YELLOW}⚠️  Important Notes:${NC}"
echo "  • This generates BASIC models - you need to customize them"
echo "  • Update model properties based on actual API responses"
echo "  • Add missing API methods as needed"
echo "  • Test with your actual Orchestrator instance"
echo ""
echo -e "${GREEN}✅ Ready to customize and use!${NC}"