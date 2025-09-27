#!/bin/bash

# Swagger Model Generator for UiPath Orchestrator
# This script generates C# model classes from a Swagger JSON specification
# and creates service methods similar to the existing OrchestratorService

set -e

# Configuration
SWAGGER_FILE="${1:-vs-libraries/Yash.Orchestrator/swagger.json}"
OUTPUT_DIR="${2:-vs-libraries/Yash.Orchestrator/Generated}"
NAMESPACE_PREFIX="${3:-Yash.Orchestrator.Generated}"
SERVICE_NAME="${4:-GeneratedOrchestratorService}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🚀 Swagger Model Generator for UiPath Orchestrator${NC}"
echo "=================================================="
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

# Check if jq is installed, install if needed
if ! command -v jq &> /dev/null; then
    echo -e "${YELLOW}⚠️  jq is not installed. Installing jq...${NC}"
    
    # Detect OS and install jq accordingly
    if [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "win32" ]] || [[ "$OSTYPE" == "cygwin" ]]; then
        # Windows with Git Bash
        echo -e "${BLUE}📥 Downloading jq for Windows...${NC}"
        JQ_DIR="$HOME/.local/bin"
        mkdir -p "$JQ_DIR"
        curl -L -o "$JQ_DIR/jq.exe" https://github.com/stedolan/jq/releases/latest/download/jq-win64.exe
        chmod +x "$JQ_DIR/jq.exe"
        # Add to PATH if not already there
        if [[ ":$PATH:" != *":$JQ_DIR:"* ]]; then
            export PATH="$JQ_DIR:$PATH"
        fi
        echo -e "${GREEN}✅ jq installed successfully to $JQ_DIR${NC}"
    elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
        # Linux
        if command -v apt-get &> /dev/null; then
            sudo apt-get update && sudo apt-get install -y jq
        elif command -v yum &> /dev/null; then
            sudo yum install -y jq
        elif command -v dnf &> /dev/null; then
            sudo dnf install -y jq
        else
            echo -e "${RED}❌ Error: Cannot install jq automatically on this Linux distribution.${NC}"
            echo "Please install jq manually: https://stedolan.github.io/jq/download/"
            exit 1
        fi
    elif [[ "$OSTYPE" == "darwin"* ]]; then
        # macOS
        if command -v brew &> /dev/null; then
            brew install jq
        else
            echo -e "${RED}❌ Error: Homebrew is required to install jq on macOS.${NC}"
            echo "Please install Homebrew first: https://brew.sh/"
            echo "Or install jq manually: https://stedolan.github.io/jq/download/"
            exit 1
        fi
    else
        echo -e "${RED}❌ Error: Unsupported operating system for automatic jq installation.${NC}"
        echo "Please install jq manually: https://stedolan.github.io/jq/download/"
        exit 1
    fi
    
    # Verify installation
    if ! command -v jq &> /dev/null; then
        echo -e "${RED}❌ Error: jq installation failed. Please install jq manually.${NC}"
        exit 1
    fi
fi

# Create output directories
mkdir -p "$OUTPUT_DIR/Models"
mkdir -p "$OUTPUT_DIR/Responses"
mkdir -p "$OUTPUT_DIR/Services"

echo -e "${YELLOW}📁 Created directory structure${NC}"

# Function to convert camelCase or snake_case to PascalCase
to_pascal_case() {
    local input="$1"
    # Remove non-alphanumeric characters and convert to PascalCase
    echo "$input" | sed 's/[^a-zA-Z0-9]/ /g' | awk '{for(i=1;i<=NF;i++) $i=toupper(substr($i,1,1)) tolower(substr($i,2))}1' | sed 's/ //g'
}

# Function to convert type from swagger to C#
convert_type() {
    local swagger_type="$1"
    local format="$2"
    
    case "$swagger_type" in
        "string")
            case "$format" in
                "date-time") echo "DateTime?" ;;
                "date") echo "DateTime?" ;;
                "uuid") echo "Guid?" ;;
                "email") echo "string?" ;;
                "uri") echo "string?" ;;
                "byte") echo "byte[]?" ;;
                *) echo "string?" ;;
            esac
            ;;
        "integer")
            case "$format" in
                "int32") echo "int?" ;;
                "int64") echo "long?" ;;
                *) echo "int?" ;;
            esac
            ;;
        "number")
            case "$format" in
                "float") echo "float?" ;;
                "double") echo "double?" ;;
                *) echo "decimal?" ;;
            esac
            ;;
        "boolean") echo "bool?" ;;
        "array") echo "List<object>?" ;;
        "object") echo "object?" ;;
        *) echo "object?" ;;
    esac
}

# Function to generate enum class
generate_enum() {
    local enum_name="$1"
    local enum_values="$2"
    local namespace="$3"
    
    cat > "$OUTPUT_DIR/Models/${enum_name}.cs" << EOF
using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace $namespace.Models
{
    /// <summary>
    /// Enum values for $enum_name
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum $enum_name
    {
EOF

    # Parse enum values and add to file
    echo "$enum_values" | jq -r '.[]' | while read -r value; do
        if [ -n "$value" ]; then
            # Clean the enum value name
            clean_value=$(echo "$value" | sed 's/[^a-zA-Z0-9]/_/g' | sed 's/^[0-9]/Enum&/')
            echo "        /// <summary>" >> "$OUTPUT_DIR/Models/${enum_name}.cs"
            echo "        /// $value" >> "$OUTPUT_DIR/Models/${enum_name}.cs"
            echo "        /// </summary>" >> "$OUTPUT_DIR/Models/${enum_name}.cs"
            echo "        [Description(\"$value\")]" >> "$OUTPUT_DIR/Models/${enum_name}.cs"
            echo "        $clean_value," >> "$OUTPUT_DIR/Models/${enum_name}.cs"
            echo "" >> "$OUTPUT_DIR/Models/${enum_name}.cs"
        fi
    done

    cat >> "$OUTPUT_DIR/Models/${enum_name}.cs" << EOF
    }
}
EOF
}

# Function to generate model class
generate_model() {
    local model_name="$1"
    local model_def="$2"
    local namespace="$3"
    
    echo -e "${BLUE}  📝 Generating model: $model_name${NC}"
    
    cat > "$OUTPUT_DIR/Models/${model_name}.cs" << EOF
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace $namespace.Models
{
    /// <summary>
    /// Model class for $model_name
    /// Auto-generated from Swagger specification
    /// </summary>
    public class $model_name
    {
EOF

    # Extract properties from the model definition
    local properties
    properties=$(echo "$model_def" | jq -r '.properties // empty')
    
    if [ -n "$properties" ] && [ "$properties" != "null" ]; then
        echo "$properties" | jq -r 'to_entries[] | @base64' | while read -r entry; do
            local decoded
            decoded=$(echo "$entry" | base64 --decode)
            local prop_name prop_def prop_type prop_format prop_description prop_required
            
            prop_name=$(echo "$decoded" | jq -r '.key')
            prop_def=$(echo "$decoded" | jq -r '.value')
            prop_type=$(echo "$prop_def" | jq -r '.type // "object"')
            prop_format=$(echo "$prop_def" | jq -r '.format // ""')
            prop_description=$(echo "$prop_def" | jq -r '.description // ""')
            
            # Check if property is required
            local required_props
            required_props=$(echo "$model_def" | jq -r '.required // []')
            prop_required="false"
            if echo "$required_props" | jq -r '.[]' | grep -q "^$prop_name$"; then
                prop_required="true"
            fi
            
            # Handle references to other models
            if echo "$prop_def" | jq -e '."$ref"' > /dev/null; then
                local ref_type
                ref_type=$(echo "$prop_def" | jq -r '."$ref"' | sed 's/.*\///g')
                prop_type="$ref_type?"
            elif [ "$prop_type" = "array" ]; then
                local item_type
                item_type=$(echo "$prop_def" | jq -r '.items.type // "object"')
                if echo "$prop_def" | jq -e '.items."$ref"' > /dev/null; then
                    local ref_type
                    ref_type=$(echo "$prop_def" | jq -r '.items."$ref"' | sed 's/.*\///g')
                    prop_type="List<$ref_type>?"
                else
                    prop_type="List<$(convert_type "$item_type" "")>?"
                fi
            elif echo "$prop_def" | jq -e '.enum' > /dev/null; then
                # Handle enum properties
                prop_type="string?" # For now, treat enums as strings
            else
                prop_type=$(convert_type "$prop_type" "$prop_format")
            fi
            
            # Write property to file
            {
                echo ""
                echo "        /// <summary>"
                if [ -n "$prop_description" ]; then
                    echo "        /// $prop_description"
                else
                    echo "        /// $prop_name property"
                fi
                echo "        /// </summary>"
                if [ "$prop_required" = "true" ]; then
                    echo "        [Required]"
                fi
                echo "        [JsonProperty(\"$prop_name\", NullValueHandling = NullValueHandling.Ignore)]"
                echo "        public $prop_type $prop_name { get; set; }"
            } >> "$OUTPUT_DIR/Models/${model_name}.cs"
        done
    fi

    cat >> "$OUTPUT_DIR/Models/${model_name}.cs" << EOF

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
    
    cat > "$OUTPUT_DIR/Responses/${response_name}.cs" << EOF
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using $namespace.Models;

namespace $namespace.Responses
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
        cat >> "$OUTPUT_DIR/Responses/${response_name}.cs" << EOF
        /// <summary>
        /// Collection of $model_name items
        /// </summary>
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public List<$model_name>? Items { get; set; }
EOF
    else
        cat >> "$OUTPUT_DIR/Responses/${response_name}.cs" << EOF
        /// <summary>
        /// Single $model_name item
        /// </summary>
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public $model_name? Item { get; set; }
EOF
    fi

    cat >> "$OUTPUT_DIR/Responses/${response_name}.cs" << EOF
    }
}
EOF
}

# Function to extract API endpoints and generate service methods
generate_service_methods() {
    local service_file="$1"
    local namespace="$2"
    
    echo -e "${YELLOW}  🔧 Generating service methods${NC}"
    
    # Extract paths from swagger
    local paths
    paths=$(jq -r '.paths | keys[]' "$SWAGGER_FILE")
    
    cat >> "$service_file" << EOF

        #region Generated API Methods
EOF

    echo "$paths" | while read -r path; do
        if [ -n "$path" ]; then
            # Get HTTP methods for this path
            local methods
            methods=$(jq -r ".paths[\"$path\"] | keys[]" "$SWAGGER_FILE")
            
            echo "$methods" | while read -r method; do
                if [[ "$method" =~ ^(get|post|put|delete|patch)$ ]]; then
                    local operation_id summary
                    operation_id=$(jq -r ".paths[\"$path\"][\"$method\"].operationId // \"\"" "$SWAGGER_FILE")
                    summary=$(jq -r ".paths[\"$path\"][\"$method\"].summary // \"\"" "$SWAGGER_FILE")
                    
                    if [ -n "$operation_id" ] && [ "$operation_id" != "null" ]; then
                        # Generate method name from operation ID
                        local method_name
                        method_name=$(to_pascal_case "$operation_id")
                        
                        # Extract parameters
                        local params
                        params=$(jq -r ".paths[\"$path\"][\"$method\"].parameters // []" "$SWAGGER_FILE")
                        
                        # Generate method signature
                        {
                            echo ""
                            echo "        /// <summary>"
                            if [ -n "$summary" ]; then
                                echo "        /// $summary"
                            else
                                echo "        /// $method_name operation"
                            fi
                            echo "        /// Generated from: $method $path"
                            echo "        /// </summary>"
                            echo "        public async Task<RestResponse> ${method_name}Async("
                        } >> "$service_file"
                        
                        # Add parameters
                        local param_list=""
                        if [ "$params" != "[]" ] && [ "$params" != "null" ]; then
                            echo "$params" | jq -r '.[] | @base64' | while read -r param_entry; do
                                local param_decoded param_name param_type param_required
                                param_decoded=$(echo "$param_entry" | base64 --decode)
                                param_name=$(echo "$param_decoded" | jq -r '.name')
                                param_type=$(echo "$param_decoded" | jq -r '.type // "string"')
                                param_required=$(echo "$param_decoded" | jq -r '.required // false')
                                
                                local cs_type
                                cs_type=$(convert_type "$param_type" "")
                                
                                if [ "$param_required" = "true" ]; then
                                    echo "            $cs_type $param_name," >> "$service_file"
                                else
                                    echo "            $cs_type $param_name = null," >> "$service_file"
                                fi
                            done
                        fi
                        
                        {
                            echo "            CancellationToken cancellationToken = default)"
                            echo "        {"
                            echo "            Log(\"Executing $method_name...\", TraceEventType.Information);"
                            echo ""
                            echo "            var url = \$\"{BaseURL}$path\";"
                            echo "            var request = new RestRequest(url, Method.$(echo "$method" | tr '[:lower:]' '[:upper:]'))"
                            echo "                .AddHeader(\"Authorization\", \$\"Bearer {Token}\");"
                            echo ""
                            echo "            // TODO: Add parameter handling"
                            echo "            // TODO: Add request body handling if needed"
                            echo ""
                            echo "            var response = await _client.ExecuteAsync(request, cancellationToken);"
                            echo "            if (!response.IsSuccessful)"
                            echo "            {"
                            echo "                Log(\$\"Failed to execute $method_name: {response.ErrorMessage}\", TraceEventType.Error);"
                            echo "            }"
                            echo ""
                            echo "            return response;"
                            echo "        }"
                        } >> "$service_file"
                    fi
                fi
            done
        fi
    done

    cat >> "$service_file" << EOF

        #endregion
EOF
}

# Main generation process
echo -e "${YELLOW}🔍 Analyzing Swagger specification...${NC}"

# Extract definitions from swagger file
definitions=$(jq -r '.definitions // {}' "$SWAGGER_FILE")

if [ "$definitions" = "{}" ] || [ "$definitions" = "null" ]; then
    echo -e "${RED}❌ No definitions found in swagger file${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Found definitions section${NC}"

# Generate models
echo -e "${YELLOW}📦 Generating model classes...${NC}"
model_count=0

echo "$definitions" | jq -r 'keys[]' | while read -r def_name; do
    if [ -n "$def_name" ]; then
        model_def=$(echo "$definitions" | jq -r ".\"$def_name\"")
        
        # Skip if it's not an object type
        model_type=$(echo "$model_def" | jq -r '.type // "object"')
        
        if [ "$model_type" = "object" ]; then
            # Generate model class
            generate_model "$def_name" "$model_def" "$NAMESPACE_PREFIX"
            
            # Check if we should generate a response wrapper
            if [[ "$def_name" =~ (Dto|Response)$ ]]; then
                response_name="${def_name%Dto}Response"
                response_name="${response_name%Response}Response"
                generate_response "$response_name" "$def_name" "$NAMESPACE_PREFIX" "false"
                
                # Also generate collection response
                collection_response_name="${def_name%Dto}CollectionResponse"
                collection_response_name="${collection_response_name%Response}CollectionResponse"
                generate_response "$collection_response_name" "$def_name" "$NAMESPACE_PREFIX" "true"
            fi
            
            ((model_count++))
        elif [ "$model_type" = "string" ] && echo "$model_def" | jq -e '.enum' > /dev/null; then
            # Generate enum
            enum_values=$(echo "$model_def" | jq -r '.enum')
            generate_enum "$def_name" "$enum_values" "$NAMESPACE_PREFIX"
        fi
    fi
done

echo -e "${GREEN}✅ Generated models${NC}"

# Generate main service class
echo -e "${YELLOW}🏗️  Generating service class...${NC}"

cat > "$OUTPUT_DIR/Services/${SERVICE_NAME}.cs" << EOF
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using RestSharp;
using $NAMESPACE_PREFIX.Models;
using $NAMESPACE_PREFIX.Responses;

namespace $NAMESPACE_PREFIX.Services
{
    /// <summary>
    /// Generated service class for UiPath Orchestrator API
    /// Auto-generated from Swagger specification
    /// Based on the pattern from Yash.Orchestrator.OrchestratorService
    /// </summary>
    public class $SERVICE_NAME
    {
        #region Private Fields
        
        private readonly RestClient _client;
        private readonly Action<string, TraceEventType>? _logAction;
        private readonly TraceEventType _minLogLevel;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Base URL for the Orchestrator API
        /// </summary>
        public string? BaseURL { get; set; }
        
        /// <summary>
        /// Authentication token
        /// </summary>
        public string? Token { get; set; }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Initializes a new instance of the $SERVICE_NAME class
        /// </summary>
        /// <param name="client">HTTP client to use for requests</param>
        /// <param name="log">Optional logging action</param>
        /// <param name="minLogLevel">Minimum log level</param>
        public $SERVICE_NAME(HttpClient client, Action<string, TraceEventType>? log = null, TraceEventType minLogLevel = TraceEventType.Information)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            _client = new RestClient(client);
            _logAction = log;
            _minLogLevel = minLogLevel;
        }
        
        /// <summary>
        /// Initializes a new instance of the $SERVICE_NAME class
        /// </summary>
        /// <param name="baseUrl">Base URL for the API</param>
        /// <param name="log">Optional logging action</param>
        /// <param name="minLogLevel">Minimum log level</param>
        public $SERVICE_NAME(string baseUrl, Action<string, TraceEventType>? log = null, TraceEventType minLogLevel = TraceEventType.Information)
        {
            BaseURL = baseUrl;
            _client = new RestClient();
            _logAction = log;
            _minLogLevel = minLogLevel;
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
                _logAction?.Invoke(message, eventType);
        }
        
        #endregion
        
        #region Authentication
        
        /// <summary>
        /// Sets the authentication token
        /// </summary>
        /// <param name="token">Bearer token</param>
        public void SetToken(string token)
        {
            Token = token;
            Log("Authentication token updated", TraceEventType.Information);
        }
        
        #endregion
EOF

# Generate service methods from API endpoints
generate_service_methods "$OUTPUT_DIR/Services/${SERVICE_NAME}.cs" "$NAMESPACE_PREFIX"

cat >> "$OUTPUT_DIR/Services/${SERVICE_NAME}.cs" << EOF
    }
}
EOF

# Generate interface for the service
echo -e "${YELLOW}📋 Generating service interface...${NC}"

cat > "$OUTPUT_DIR/Services/I${SERVICE_NAME}.cs" << EOF
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using RestSharp;

namespace $NAMESPACE_PREFIX.Services
{
    /// <summary>
    /// Interface for $SERVICE_NAME
    /// Auto-generated from Swagger specification
    /// </summary>
    public interface I$SERVICE_NAME
    {
        /// <summary>
        /// Base URL for the Orchestrator API
        /// </summary>
        string? BaseURL { get; set; }
        
        /// <summary>
        /// Authentication token
        /// </summary>
        string? Token { get; set; }
        
        /// <summary>
        /// Sets the authentication token
        /// </summary>
        /// <param name="token">Bearer token</param>
        void SetToken(string token);
        
        // TODO: Add method signatures from the generated service
        // This would require additional parsing of the generated methods
    }
}
EOF

# Generate README for the generated code
echo -e "${YELLOW}📄 Generating documentation...${NC}"

cat > "$OUTPUT_DIR/README.md" << EOF
# Generated UiPath Orchestrator API Models and Service

This directory contains auto-generated C# classes based on the UiPath Orchestrator Swagger specification.

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

// Use service methods
var response = await service.SomeMethodAsync();
\`\`\`

### Model Usage

\`\`\`csharp
using $NAMESPACE_PREFIX.Models;
using Newtonsoft.Json;

// Deserialize API response
var jsonResponse = "{ ... }";
var model = JsonConvert.DeserializeObject<SomeModel>(jsonResponse);
\`\`\`

## Generated Files

- **Models**: Contains all the data transfer objects (DTOs) and model classes
- **Responses**: Contains wrapper classes for API responses with OData metadata
- **Services**: Contains the main service class and interface

## Customization

The generated code is designed to be a starting point. You may need to:

1. **Add specific parameter handling** in service methods
2. **Implement proper error handling** and response parsing
3. **Add validation attributes** to model properties
4. **Extend interfaces** with additional method signatures
5. **Add custom business logic** as needed

## Regeneration

To regenerate the code with updated Swagger specification:

\`\`\`bash
./generate-swagger-models.sh [swagger-file] [output-dir] [namespace] [service-name]
\`\`\`

## Notes

- This code is auto-generated and may require manual adjustments
- Review the generated service methods and add proper parameter handling
- Consider adding unit tests for the generated service
- The service follows the pattern established in \`Yash.Orchestrator.OrchestratorService\`

Generated on: $(date)
Swagger file: $SWAGGER_FILE
EOF

# Generate a sample project file
cat > "$OUTPUT_DIR/Generated.csproj" << EOF
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <RootNamespace>$NAMESPACE_PREFIX</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageReference Include="RestSharp" Version="110.2.0" />
    <PackageReference Include="System.ComponentModel.Annotations" Version="5.0.0" />
  </ItemGroup>

</Project>
EOF

echo ""
echo -e "${GREEN}🎉 Generation completed successfully!${NC}"
echo "=================================================="
echo -e "${BLUE}📊 Summary:${NC}"
echo "  📁 Output directory: $OUTPUT_DIR"
echo "  📦 Generated models in: $OUTPUT_DIR/Models/"
echo "  📋 Generated responses in: $OUTPUT_DIR/Responses/"
echo "  🏗️  Generated service in: $OUTPUT_DIR/Services/"
echo "  📄 Documentation: $OUTPUT_DIR/README.md"
echo "  📦 Project file: $OUTPUT_DIR/Generated.csproj"
echo ""
echo -e "${YELLOW}⚠️  Next steps:${NC}"
echo "  1. Review the generated service methods and add parameter handling"
echo "  2. Implement proper error handling and response parsing"
echo "  3. Add unit tests for the generated code"
echo "  4. Customize the code as needed for your specific requirements"
echo ""
echo -e "${GREEN}✅ Happy coding!${NC}"