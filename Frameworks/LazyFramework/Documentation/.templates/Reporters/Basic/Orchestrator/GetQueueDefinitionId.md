# GetQueueDefinitionId
Class: GetQueueDefinitionId

Gets the queue definition based on the queue folder and name.

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
- System.Runtime.Serialization
- UiPath.Core.Activities.Orchestrator
- Newtonsoft.Json
- Newtonsoft.Json.Linq
- System.Dynamic
- System.ComponentModel
- System.Collections.Specialized


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
| in_QueueName | InArgument | x:String | The name of the queue to get the id for. |
| in_QueueFolder | InArgument | x:String | The folder that houses the queue to get the id for. |
| out_Id | OutArgument | x:Int32 | The id retrieved. |

    
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


Sequence_1: Sequence - GetQueueDefinitionId
state Sequence_1 {
direction TB
OrchestratorHttpRequest_1 : OrchestratorHttpRequest - Orchestrator API Call
OrchestratorHttpRequest_1 --> If_1
If_1: If - Status Not 2xx?
state If_1 {
direction TB
Throw_1 : Throw - Throw Orchestrator Invalid Status
}
MultipleAssign_1 : MultipleAssign - Parse Response
If_1 --> MultipleAssign_1
MultipleAssign_1 --> If_2
If_2: If - Validate ID Count
state If_2 {
direction TB
MultipleAssign_2 : MultipleAssign - Set Output
Throw_2 : Throw - Throw CouldNotFind
}
LogMessage_1 : LogMessage - LM -- Complete
If_2 --> LogMessage_1
}
```