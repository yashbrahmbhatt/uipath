# CreateTestData
Class: CreateTestData

Helper to create test data in a queue to test the reporter.

<hr />

## Workflow Details
<details>
    <summary>
    <b>Namespaces</b>
    </summary>
    
- System
- System.Activities
- System.Activities.Statements
- System.Collections
- System.Collections.Generic
- System.Collections.ObjectModel
- System.Linq
- System.Reflection
- System.Runtime.Serialization
- UiPath.Core
- UiPath.Core.Activities
- GlobalVariablesNamespace
- GlobalConstantsNamespace


</details>
<details>
    <summary>
    <b>References</b>
    </summary>

- Microsoft.CSharp
- Microsoft.VisualBasic
- NPOI
- System
- System.Activities
- System.Collections
- System.ComponentModel
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
- System.Threading
- System.Threading.AccessControl
- System.Xaml
- System.Xml
- System.Xml.Linq
- UiPath.Studio.Constants
- UiPath.System.Activities
- UiPath.System.Activities.Design
- UiPath.System.Activities.ViewModels
- UiPath.Workflow


</details>
<details>
    <summary>
    <b>Arguments</b>
    </summary>

| Name | Direction | Type | Description |
|  --- | --- | --- | ---  |
| in_ConfigPath | InArgument | x:String | Path to the config file to load. |

    
</details>
<details>
    <summary>
    <b>Workflows Used</b>
    </summary>

- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Shared\LoadConfig.xaml

    
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


Sequence_1: Sequence - CreateTestData
state Sequence_1 {
direction TB
LogMessage_1 : LogMessage - LM -- Start
InvokeWorkflowFile_1 : InvokeWorkflowFile - Utility\\LoadConfig.xaml - Invoke Workflow File
LogMessage_1 --> InvokeWorkflowFile_1
InvokeWorkflowFile_1 --> ForEach1_1
ForEach1_1: ForEach - Loop through counts
state ForEach1_1 {
direction TB

Sequence_2: Sequence - Add Item to Queue
state Sequence_2 {
direction TB
AddTransactionItem_1 : AddTransactionItem - Start Transaction
InvokeCode_1 : InvokeCode - Adding Delay for Execution Time
AddTransactionItem_1 --> InvokeCode_1
InvokeCode_1 --> If_1
If_1: If - Lucky?
state If_1 {
direction TB
SetTransactionStatus_1 : SetTransactionStatus - Set Successful

If_2: If - App or Bus?
state If_2 {
direction TB
SetTransactionStatus_3 : SetTransactionStatus - Set Business
SetTransactionStatus_5 : SetTransactionStatus - Set Application
}
}
}
}
LogMessage_2 : LogMessage - LM -- Complete
ForEach1_1 --> LogMessage_2
}
```