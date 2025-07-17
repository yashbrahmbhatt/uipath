# ParseProjectJSON
Class: ParseProjectJSON

Parses the information to be retrieved from the project.json file.

<hr />

## Workflow Details
<details>
    <summary>
    <b>Namespaces</b>
    </summary>
    
- System.Activities
- System.Activities.Statements
- System
- System.Collections
- System.Collections.Generic
- System.Collections.ObjectModel
- System.Data
- System.Linq
- UiPath.Core.Activities
- System.Reflection
- System.IO
- Newtonsoft.Json
- Newtonsoft.Json.Linq
- System.Dynamic
- System.ComponentModel
- System.Collections.Specialized
- System.Runtime.Serialization
- System.Xml.Serialization
- UiPath.Core
- GlobalVariablesNamespace
- GlobalConstantsNamespace


</details>
<details>
    <summary>
    <b>References</b>
    </summary>

- Microsoft.CSharp
- Microsoft.VisualBasic
- Microsoft.Win32.Primitives
- netstandard
- Newtonsoft.Json
- NPOI
- PresentationFramework
- System
- System.Activities
- System.Collections
- System.Collections.Immutable
- System.Collections.NonGeneric
- System.Collections.Specialized
- System.ComponentModel
- System.ComponentModel.EventBasedAsync
- System.ComponentModel.Primitives
- System.ComponentModel.TypeConverter
- System.Configuration.ConfigurationManager
- System.Console
- System.Core
- System.Data
- System.Data.Common
- System.Data.SqlClient
- System.IO.FileSystem.AccessControl
- System.IO.FileSystem.DriveInfo
- System.IO.FileSystem.Watcher
- System.IO.Packaging
- System.Linq
- System.Linq.Expressions
- System.Linq.Parallel
- System.Linq.Queryable
- System.Memory
- System.Memory.Data
- System.ObjectModel
- System.Private.CoreLib
- System.Private.DataContractSerialization
- System.Private.ServiceModel
- System.Private.Uri
- System.Private.Xml
- System.Reflection.DispatchProxy
- System.Reflection.Metadata
- System.Reflection.TypeExtensions
- System.Runtime.Serialization
- System.Runtime.Serialization.Formatters
- System.Runtime.Serialization.Primitives
- System.Security.Permissions
- System.ServiceModel
- System.ServiceModel.Activities
- System.Xaml
- System.Xml
- System.Xml.Linq
- UiPath.Studio.Constants
- UiPath.System.Activities
- UiPath.Workflow
- WindowsBase


</details>
<details>
    <summary>
    <b>Arguments</b>
    </summary>

| Name | Direction | Type | Description |
|  --- | --- | --- | ---  |
| in_ProjectJSONPath | InArgument | x:String | The path to the project.json. |
| out_Name | OutArgument | x:String | The name of the project. |
| out_Description | OutArgument | x:String | The description of the project. |
| out_EntryPoints | OutArgument | scg:IEnumerable(x:String) | The path to the entry points of the project. |
| out_Language | OutArgument | x:String | The .NET language used to write this project. |
| out_ProjectVersion | OutArgument | x:String | The version of the project. |
| out_StudioVersion | OutArgument | x:String | The version of the studio used to publish this package. |
| out_Type | OutArgument | x:String | The type of the project. |
| out_dt_Dependencies | OutArgument | sd:DataTable | A table of the name and versions of the dependencies of the project. |
| out_dt_FileInfoCollection | OutArgument | sd:DataTable | The FileInfoCollection of the project.json file. Contains information about templates and test cases. |

    
</details>
<details>
    <summary>
    <b>Workflows Used</b>
    </summary>



    
</details>
<details>
    <summary>
    <b>Tests</b>
    </summary>



    
</details>

<hr />

## Outline (Beta)

```mermaid
stateDiagram-v2


Sequence_1: Sequence - ParseProjectJSON
state Sequence_1 {
direction TB
LogMessage_1 : LogMessage - LM -- Start
BuildDataTable_2 : BuildDataTable - Build Dependencies Table
LogMessage_1 --> BuildDataTable_2
MultipleAssign_1 : MultipleAssign - Parse File
BuildDataTable_2 --> MultipleAssign_1
MultipleAssign_1 --> ForEach1_1
ForEach1_1: ForEach - For Each DependencyArray
state ForEach1_1 {
direction TB
AddDataRow_2 : AddDataRow - Add Row
}
LogMessage_2 : LogMessage - LM -- Complete
ForEach1_1 --> LogMessage_2
}
```