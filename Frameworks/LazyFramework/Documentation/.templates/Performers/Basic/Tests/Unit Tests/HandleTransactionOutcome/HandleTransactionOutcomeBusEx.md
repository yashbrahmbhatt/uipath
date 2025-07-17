# HandleTransactionOutcomeBusEx
Class: HandleTransactionOutcomeBusEx

Validates the business exception case for this workflow. Email send and validated.

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
- System.Linq
- System.Net.Mail
- UiPath.Core
- UiPath.Core.Activities
- System.Collections.ObjectModel
- System.Runtime.Serialization
- System.Reflection
- UiPath.Testing.Activities
- UiPath.Shared.Activities
- System.Activities.Runtime.Collections
- System.Security
- UiPath.Mail
- UiPath.Mail.IMAP.Activities
- UiPath.Mail.Activities
- UiPath.Core.Activities.Orchestrator
- GlobalVariablesNamespace
- GlobalConstantsNamespace


</details>
<details>
    <summary>
    <b>References</b>
    </summary>

- Microsoft.CSharp
- Microsoft.VisualBasic
- mscorlib
- NPOI
- PresentationCore
- PresentationFramework
- System
- System.Activities
- System.Collections
- System.Collections.Immutable
- System.ComponentModel
- System.ComponentModel.Primitives
- System.ComponentModel.TypeConverter
- System.Configuration.ConfigurationManager
- System.Console
- System.Core
- System.Data
- System.Data.Common
- System.Drawing
- System.Linq
- System.Linq.Expressions
- System.Linq.Parallel
- System.Linq.Queryable
- System.Memory
- System.Memory.Data
- System.Net.Mail
- System.ObjectModel
- System.Private.CoreLib
- System.Private.DataContractSerialization
- System.Private.ServiceModel
- System.Private.Uri
- System.Private.Xml
- System.Reflection.DispatchProxy
- System.Reflection.Metadata
- System.Reflection.TypeExtensions
- System.Runtime.InteropServices
- System.Runtime.Serialization
- System.Runtime.Serialization.Formatters
- System.Runtime.Serialization.Primitives
- System.Security.Permissions
- System.ServiceModel
- System.ServiceModel.Activities
- System.Xaml
- System.Xml
- System.Xml.Linq
- UiPath.Excel
- UiPath.Excel.Activities
- UiPath.Mail
- UiPath.Mail.Activities
- UiPath.Mail.Activities.Design
- UiPath.Studio.Constants
- UiPath.System.Activities
- UiPath.System.Activities.Design
- UiPath.System.Activities.ViewModels
- UiPath.Testing.Activities
- UiPath.Workflow
- WindowsBase


</details>
<details>
    <summary>
    <b>Arguments</b>
    </summary>

| Name | Direction | Type | Description |
|  --- | --- | --- | ---  |

    
</details>
<details>
    <summary>
    <b>Workflows Used</b>
    </summary>

- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Shared\LoadConfig.xaml
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


Sequence_1: Sequence - HandleTransactionOutcomeBusEx
state Sequence_1 {
direction TB
LogMessage_1 : LogMessage - LM -- Start
LogMessage_1 --> TimeoutScope_1
TimeoutScope_1: TimeoutScope - Timed Test
state TimeoutScope_1 {
direction TB

Sequence_5: Sequence - Test
state Sequence_5 {
direction TB

Sequence_2: Sequence - Initialize Test
state Sequence_2 {
direction TB
MultipleAssign_2 : MultipleAssign - Initialize Vars
InvokeWorkflowFile_1 : InvokeWorkflowFile - Load Config
MultipleAssign_2 --> InvokeWorkflowFile_1
AddTransactionItem_1 : AddTransactionItem - Add Test Queue Item
InvokeWorkflowFile_1 --> AddTransactionItem_1
MultipleAssign_3 : MultipleAssign - Set Data
AddTransactionItem_1 --> MultipleAssign_3
}
LogMessage_2 : LogMessage - LM -- Initialization Complete
Sequence_2 --> LogMessage_2
LogMessage_2 --> TryCatch_1
TryCatch_1: TryCatch - Execute Test
state TryCatch_1 {
direction TB

Sequence_3: Sequence - ... When
state Sequence_3 {
direction TB
InvokeWorkflowFile_2 : InvokeWorkflowFile - .templates\\Performers\\Basic\\Framework\\HandleTransactionOutcome.xaml - Invoke Workflow File
}
}
LogMessage_3 : LogMessage - LM -- Test Executed
TryCatch_1 --> LogMessage_3
LogMessage_3 --> Sequence_6
Sequence_6: Sequence - Validate Results
state Sequence_6 {
direction TB
GetRobotCredential_1 : GetRobotCredential - Get Email Credentials
GetIMAPMailMessages_1 : GetIMAPMailMessages - Get Emails (IMAP)
GetRobotCredential_1 --> GetIMAPMailMessages_1
GetQueueItems_1 : GetQueueItems - Get Successful Items
GetIMAPMailMessages_1 --> GetQueueItems_1
VerifyExpression_8 : VerifyExpression - Verify Transaction Status
GetQueueItems_1 --> VerifyExpression_8
VerifyExpression_6 : VerifyExpression - Verify TestException
VerifyExpression_8 --> VerifyExpression_6
VerifyExpression_7 : VerifyExpression - Verify EmailCount
VerifyExpression_6 --> VerifyExpression_7
}
}
}
LogMessage_4 : LogMessage - LM -- Complete
TimeoutScope_1 --> LogMessage_4
}
```