# ApplicationDispatcher
Class: ApplicationDispatcher

Reads data from the source of work and adds it to a queue. Specialized for when dispatching requires information from an application.

<hr />

## Workflow Details
<details>
    <summary>
    <b>Namespaces</b>
    </summary>
    
- GlobalConstantsNamespace
- GlobalVariablesNamespace
- System
- System.Activities
- System.Activities.Runtime.Collections
- System.Activities.Statements
- System.Collections
- System.Collections.Generic
- System.Collections.ObjectModel
- System.Linq
- System.Reflection
- System.Runtime.Serialization
- UiPath.Core
- UiPath.Core.Activities
- System.Linq.Expressions


</details>
<details>
    <summary>
    <b>References</b>
    </summary>

- Microsoft.CSharp
- Microsoft.VisualBasic
- Microsoft.Win32.Primitives
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
- System.Linq.Expressions


</details>
<details>
    <summary>
    <b>Arguments</b>
    </summary>

| Name | Direction | Type | Description |
|  --- | --- | --- | ---  |
| in_ConfigPath | InArgument | x:String | The path to the config file to use to load variables and resources. |
| in_IgnoreSheets | InArgument | s:String[] | A list of the sheets to ignore loading from the config. |
| in_TestID | InArgument | x:String | Used to modify the workflow in order to test different scenarios. Only used to test exception handling in this workflow. Leave as null for production use. |

    
</details>
<details>
    <summary>
    <b>Workflows Used</b>
    </summary>

- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Shared\LoadConfig.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Shared\IsMaintenanceTime.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\.templates\Dispatchers\Application\Framework\CloseApplications.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\.templates\Dispatchers\Application\Framework\KillProcesses.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\.templates\Dispatchers\Application\Framework\InitializeApplications.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Shared\TakeScreenshot.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Shared\GenerateDiagnosticDictionary.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Shared\SendEmail.xaml

    
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


Sequence_1: Sequence - ApplicationDispatcher
state Sequence_1 {
direction TB
InvokeWorkflowFile_1 : InvokeWorkflowFile - LoadConfig.xaml - Invoke Workflow File
InvokeWorkflowFile_1 --> Switch1_2
Switch1_2: Switch - @Test Variations - Initialization
state Switch1_2 {
direction TB
MultipleAssign_5 : MultipleAssign - Update Maintenance Times
}
InvokeWorkflowFile_16 : InvokeWorkflowFile - IsMaintenanceTime.xaml - Invoke Workflow File
Switch1_2 --> InvokeWorkflowFile_16
InvokeWorkflowFile_16 --> If_1
If_1: If - Not Maintenance Time?
state If_1 {
direction TB

TryCatch_1: TryCatch - Try Dispatching
state TryCatch_1 {
direction TB

Sequence_2: Sequence - Dispatching
state Sequence_2 {
direction TB
LogMessage_4 : LogMessage - LM -- Initializing
LogMessage_4 --> RetryScope_2
RetryScope_2: RetryScope - Retry Initialize
state RetryScope_2 {
direction TB

Sequence_7: Sequence - Retry Initializing Applications
state Sequence_7 {
direction TB

TryCatch_2: TryCatch - Try Close, Catch Kill (Initialization)
state TryCatch_2 {
direction TB
InvokeWorkflowFile_5 : InvokeWorkflowFile - Close Applications (Initialization)
}
InvokeWorkflowFile_12 : InvokeWorkflowFile - .templates\\Dispatchers\\Application\\Framework\\InitializeApplications.xaml - Invoke Workflow File
TryCatch_2 --> InvokeWorkflowFile_12
}
}
RetryScope_2 --> Switch1_1
Switch1_1: Switch - TestID?
state Switch1_1 {
direction TB
Throw_1 : Throw - Throw Test Exception
}
LogMessage_1 : LogMessage - LM -- Start
Switch1_1 --> LogMessage_1
LogMessage_1 --> ForEach1_1
ForEach1_1: ForEach - For Each Item In Dispatch List
state ForEach1_1 {
direction TB

TryCatch_5: TryCatch - Try Dispatching Item
state TryCatch_5 {
direction TB

Sequence_5: Sequence - Do Steps and Dispatch
state Sequence_5 {
direction TB
MultipleAssign_2 : MultipleAssign - Setup Queue Data
MultipleAssign_2 --> RetryScope_1
RetryScope_1: RetryScope - Retry - Orchestrator
state RetryScope_1 {
direction TB
AddQueueItem_1 : AddQueueItem - Add Item to Queue
}
MultipleAssign_3 : MultipleAssign - ItemsAdded++
RetryScope_1 --> MultipleAssign_3
}
}
}
LogMessage_3 : LogMessage - LM -- Terminating Applications
ForEach1_1 --> LogMessage_3
LogMessage_3 --> TryCatch_4
TryCatch_4: TryCatch - Try Close, Catch Kill (Termination)
state TryCatch_4 {
direction TB
InvokeWorkflowFile_10 : InvokeWorkflowFile - Close Applications (Termination)
}
LogMessage_2 : LogMessage - LM -- Complete
TryCatch_4 --> LogMessage_2
}
}
LogMessage_7 : LogMessage - LM -- Maintenance
}
}
```