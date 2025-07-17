# GetQueueData
Class: GetQueueData

Gets the queue data list and parses it into a table.

<hr />

## Workflow Details
<details>
    <summary>
    <b>Namespaces</b>
    </summary>
    
- Newtonsoft.Json.Linq
- System
- System.Activities
- System.Activities.Statements
- System.Collections
- System.Collections.Generic
- System.Collections.ObjectModel
- System.Data
- System.Linq
- System.Runtime.Serialization
- UiPath.Core.Activities


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
- System.Linq
- System.Linq.Expressions
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
- UiPath.System.Activities.Design
- UiPath.System.Activities.ViewModels
- UiPath.Workflow
- WindowsBase


</details>
<details>
    <summary>
    <b>Arguments</b>
    </summary>

| Name | Direction | Type | Description |
|  --- | --- | --- | ---  |
| out_QueueList | OutArgument | scg:List(njl:JToken) | The unformatted list of queue items retrieved, as a List of JToken. |
| in_QueueName | InArgument | x:String | The name of the queue to report. |
| in_QueueFolder | InArgument | x:String | The path fo the folder that houses the queue to report. |
| in_From | InArgument | s:DateTime | The start of the reporting range. |
| in_To | InArgument | s:DateTime | The end of the reporting range. |
| in_Statuses | InArgument | s:String[] | The statuses to include when retrieving queue items. |
| out_dt_QueueTable | OutArgument | sd:DataTable | The queue items retrieved, formatted as a flattened table. |

    
</details>
<details>
    <summary>
    <b>Workflows Used</b>
    </summary>

- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\.templates\Reporters\Basic\Orchestrator\GetQueueDefinitionId.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\.templates\Reporters\Basic\Orchestrator\GetQueueItems.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\.templates\Reporters\Basic\Logic\Flatten.xaml

    
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


Sequence_1: Sequence - GetQueueData
state Sequence_1 {
direction TB
LogMessage_1 : LogMessage - LM -- Start
MultipleAssign_1 : MultipleAssign - Initialize Outputs
LogMessage_1 --> MultipleAssign_1
InvokeWorkflowFile_1 : InvokeWorkflowFile - Get Queue ID
MultipleAssign_1 --> InvokeWorkflowFile_1
InvokeWorkflowFile_2 : InvokeWorkflowFile - Get Queue Items
InvokeWorkflowFile_1 --> InvokeWorkflowFile_2
InvokeWorkflowFile_3 : InvokeWorkflowFile - Flatten Queue Items Into Table
InvokeWorkflowFile_2 --> InvokeWorkflowFile_3
LogMessage_2 : LogMessage - LM -- Complete
InvokeWorkflowFile_3 --> LogMessage_2
}
```