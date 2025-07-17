# BasicPerformer
Class: Performer

[Author]

[Description of work done for each transaction]

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
- System.ComponentModel
- System.Data
- System.Linq
- System.Reflection
- System.Runtime.Serialization
- System.Windows
- System.Xml.Serialization
- UiPath.Core
- UiPath.Core.Activities


</details>
<details>
    <summary>
    <b>References</b>
    </summary>

- Microsoft.CSharp
- Microsoft.VisualBasic
- Microsoft.Win32.Primitives
- Newtonsoft.Json
- NPOI
- PresentationCore
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
- System.Linq
- System.Linq.Expressions
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
- UiPath.System.Activities.Design
- UiPath.System.Activities.ViewModels
- UiPath.Workflow
- WindowsBase
- System.Private.Xml
- System.Data.SqlClient


</details>
<details>
    <summary>
    <b>Arguments</b>
    </summary>

| Name | Direction | Type | Description |
|  --- | --- | --- | ---  |
| in_ConfigPath | InArgument | x:String | The path to the config file to use to load variables and resources. |
| in_IgnoreSheets | InArgument | s:String[] | A list of sheet names to ignore when loading the config file. |
| in_TestID | InArgument | x:String | Used to modify the workflow in order to test different scenarios. Only used to test exception handling in this workflow. Leave as null for production use. |

    
</details>
<details>
    <summary>
    <b>Workflows Used</b>
    </summary>

- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Shared\LoadConfig.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Shared\IsMaintenanceTime.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\.templates\Performers\Basic\Framework\CloseApplications.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\.templates\Performers\Basic\Framework\KillProcesses.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\.templates\Performers\Basic\Framework\InitializeApplications.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Shared\TakeScreenshot.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Shared\GenerateDiagnosticDictionary.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Shared\SendEmail.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\.templates\Performers\Basic\Framework\Process.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\.templates\Performers\Basic\Framework\HandleTransactionOutcome.xaml

    
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


Sequence_27: Sequence - Performer
state Sequence_27 {
direction TB

Sequence_28: Sequence - Initialize Settings
state Sequence_28 {
direction TB
LogMessage_21 : LogMessage - LM -- Screen
InvokeWorkflowFile_32 : InvokeWorkflowFile - Utility\\LoadConfig.xaml - Invoke Workflow File
LogMessage_21 --> InvokeWorkflowFile_32
}
Sequence_28 --> StateMachine_3
StateMachine_3: StateMachine - Transactional State Machine
state StateMachine_3 {
direction TB

State_9: State - Initialization
state State_9 {
direction TB

TryCatch_8: TryCatch - Try Initializing
state TryCatch_8 {
direction TB

RetryScope_4: RetryScope - Retry - Initializing
state RetryScope_4 {
direction TB

Sequence_21: Sequence - Initialize
state Sequence_21 {
direction TB

Switch1_1: Switch - @Test Variations - Initialization
state Switch1_1 {
direction TB
MultipleAssign_21 : MultipleAssign - Update Maintenance Times
Throw_1 : Throw - Throw InitializationError
MultipleAssign_21 --> Throw_1
}
LogMessage_12 : LogMessage - LM -- Initializing
Switch1_1 --> LogMessage_12
MultipleAssign_12 : MultipleAssign - Reset System Exception
LogMessage_12 --> MultipleAssign_12
InvokeWorkflowFile_33 : InvokeWorkflowFile - Is Maintenance Time? (Initialize)
MultipleAssign_12 --> InvokeWorkflowFile_33
InvokeWorkflowFile_33 --> If_8
If_8: If - Not Maintenance?
state If_8 {
direction TB

Sequence_29: Sequence - Not Maintenance Time
state Sequence_29 {
direction TB

TryCatch_7: TryCatch - Try Close, Catch Kill (Initialization)
state TryCatch_7 {
direction TB
InvokeWorkflowFile_21 : InvokeWorkflowFile - Close Applications (Initialization)
}
InvokeWorkflowFile_23 : InvokeWorkflowFile - Init All Applications
TryCatch_7 --> InvokeWorkflowFile_23
}
LogMessage_23 : LogMessage - LM -- Maintenance (Initialization)
}
}
}
}
TryCatch_8 --> Transition_18
Transition_18: Transition - Success
state Transition_18 {
direction TB

State_8: State - Get Transaction Data
state State_8 {
direction TB

TryCatch_9: TryCatch - Try Getting Transaction
state TryCatch_9 {
direction TB

Sequence_17: Sequence - Get Transaction
state Sequence_17 {
direction TB
LogMessage_13 : LogMessage - LM -- Get Next Transaction
LogMessage_13 --> Switch1_2
Switch1_2: Switch - @Test Variations - Get Transaction Data
state Switch1_2 {
direction TB
Throw_4 : Throw - Throw GetTransactionData Error
}
ShouldStop_2 : ShouldStop - Stop Requested?
Switch1_2 --> ShouldStop_2
InvokeWorkflowFile_24 : InvokeWorkflowFile - Is Maintenance Time? (GetTransactionData)
ShouldStop_2 --> InvokeWorkflowFile_24
InvokeWorkflowFile_24 --> IfElseIf_2
IfElseIf_2: IfElseIf - Stop Requested/Maintenance Window/Success?
state IfElseIf_2 {
direction TB
LogMessage_14 : LogMessage - LM -- Stop
LogMessage_15 : LogMessage - LM -- Maintenance
LogMessage_14 --> LogMessage_15
LogMessage_15 --> RetryScope_5
RetryScope_5: RetryScope - Retry - Get Transaction
state RetryScope_5 {
direction TB
GetQueueItem_3 : GetQueueItem - Get Next Transaction
}
}
}
}
TryCatch_9 --> Transition_11
Transition_11: Transition - Error - Get Transaction Data
state Transition_11 {
direction TB

State_7: State - End
state State_7 {
direction TB

Sequence_23: Sequence - End Process
state Sequence_23 {
direction TB
LogMessage_16 : LogMessage - LM -- Start End
LogMessage_16 --> If_6
If_6: If - Framework Exception?
state If_6 {
direction TB

Sequence_18: Sequence - Handle Frameowrk Error
state Sequence_18 {
direction TB
LogMessage_17 : LogMessage - LM -- Framework Exception
InvokeWorkflowFile_25 : InvokeWorkflowFile - Take Screenshot
LogMessage_17 --> InvokeWorkflowFile_25
InvokeWorkflowFile_26 : InvokeWorkflowFile - Generate Diagnostic
InvokeWorkflowFile_25 --> InvokeWorkflowFile_26
MultipleAssign_15 : MultipleAssign - Set Variables
InvokeWorkflowFile_26 --> MultipleAssign_15
InvokeWorkflowFile_27 : InvokeWorkflowFile - Send Email
MultipleAssign_15 --> InvokeWorkflowFile_27
}
}
If_6 --> TryCatch_10
TryCatch_10: TryCatch - Try Close, Catch Kill (End)
state TryCatch_10 {
direction TB
InvokeWorkflowFile_28 : InvokeWorkflowFile - Close Applications (End)
}
TryCatch_10 --> If_7
If_7: If - Rethrow?
state If_7 {
direction TB
Throw_2 : Throw - Rethrow FrameworkException
}
}
}
}
Transition_11 --> Transition_15
Transition_15: Transition - Data
state Transition_15 {
direction TB

State_6: State - Process
state State_6 {
direction TB

TryCatch_12: TryCatch - Try Processing Transaction
state TryCatch_12 {
direction TB

Sequence_24: Sequence - Process Transaction
state Sequence_24 {
direction TB
MultipleAssign_16 : MultipleAssign - Initialize State Variables
MultipleAssign_16 --> Switch1_3
Switch1_3: Switch - @Test Variations - Process
state Switch1_3 {
direction TB
Throw_5 : Throw - Throw FrameworkProcess Error
}
InvokeWorkflowFile_30 : InvokeWorkflowFile - Perform Transaction
Switch1_3 --> InvokeWorkflowFile_30
}
Sequence_24 --> TryCatch_11
TryCatch_11: TryCatch - Try Setting Transaction Status
state TryCatch_11 {
direction TB
InvokeWorkflowFile_31 : InvokeWorkflowFile - HandleTransactionOutcome.xaml - Invoke Workflow File
InvokeWorkflowFile_31 --> Switch1_4
Switch1_4: Switch - @Test Variations - Process (Bubble)
state Switch1_4 {
direction TB
Throw_7 : Throw - Throw FrameworkProcess Error (Bubble)
}
}
}
Transition_12 : Transition - Error - Framework
TryCatch_12 --> Transition_12
Transition_12 --> Transition_13
Transition_13: Transition - Success/BRE
state Transition_13 {
direction TB
MultipleAssign_20 : MultipleAssign - Reset Consecutive Exceptions
}
Transition_14 : Transition - Error - Transaction
Transition_13 --> Transition_14
}
}
Transition_16 : Transition - No Data
Transition_15 --> Transition_16
Transition_17 : Transition - Stop
Transition_16 --> Transition_17
Transition_21 : Transition - Maintenance Time (GetTransactionData)
Transition_17 --> Transition_21
}
}
Transition_19 : Transition - Error - Initialization
Transition_18 --> Transition_19
Transition_19 --> Transition_20
Transition_20: Transition - MaxConsecutive
state Transition_20 {
direction TB

Sequence_25: Sequence - Update + Log
state Sequence_25 {
direction TB
MultipleAssign_22 : MultipleAssign - Set FrameworkException to Max Consecutive
LogMessage_19 : LogMessage - LM -- Max Consecutive
MultipleAssign_22 --> LogMessage_19
}
}
Transition_22 : Transition - Maintenance Time (Initialize)
Transition_20 --> Transition_22
}
}
}
```