# BasicDispatcherFailureTest
Class: BasicDispatcherFailureTest

Tests the failure path for the dispatcher. Validates that the exception email is sent out.

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
- System.IO
- System.Linq
- System.Net.Mail
- UiPath.Core.Activities
- System.Collections.ObjectModel
- System.Runtime.Serialization
- System.Reflection
- UiPath.Testing.Activities
- UiPath.Shared.Activities
- System.Security
- UiPath.Mail
- UiPath.Mail.IMAP.Activities
- UiPath.Mail.Activities
- System.Activities.Runtime.Collections
- UiPath.Platform.ResourceHandling
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
- mscorlib
- NPOI
- PresentationCore
- PresentationFramework
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
- System.Drawing
- System.Linq
- System.Linq.Expressions
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
- UiPath.Platform
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
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\.templates\Dispatchers\Basic\BasicDispatcher.xaml

    
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


Sequence_1: Sequence - BasicDispatcherFailureTest
state Sequence_1 {
direction TB
LogMessage_1 : LogMessage - LM -- Start
LogMessage_1 --> TimeoutScope_2
TimeoutScope_2: TimeoutScope - Timed Test
state TimeoutScope_2 {
direction TB

Sequence_13: Sequence - Test
state Sequence_13 {
direction TB

Sequence_11: Sequence - Initialize Test
state Sequence_11 {
direction TB
MultipleAssign_2 : MultipleAssign - Initialize Variables
InvokeWorkflowFile_1 : InvokeWorkflowFile - Load Test Config
MultipleAssign_2 --> InvokeWorkflowFile_1
InvokeWorkflowFile_1 --> If_1
If_1: If - Exception Screenshots Exists?
state If_1 {
direction TB
DeleteFolderX_1 : DeleteFolderX - Delete Exception Screenshots
}
CreateDirectory_1 : CreateDirectory - Create Exception Screenshots
If_1 --> CreateDirectory_1
CreateFile_1 : CreateFile - Create Placeholder
CreateDirectory_1 --> CreateFile_1
}
LogMessage_8 : LogMessage - LM -- Initialization Complete
Sequence_11 --> LogMessage_8
LogMessage_8 --> Sequence_12
Sequence_12: Sequence - Execute Test
state Sequence_12 {
direction TB

TryCatch_1: TryCatch - Execute
state TryCatch_1 {
direction TB
InvokeWorkflowFile_3 : InvokeWorkflowFile - Run BasicDispatcher
}
}
LogMessage_9 : LogMessage - LM -- Test Executed
Sequence_12 --> LogMessage_9
LogMessage_9 --> Sequence_8
Sequence_8: Sequence - Validate Results
state Sequence_8 {
direction TB
GetRobotCredential_1 : GetRobotCredential - Get Email Credentials
GetIMAPMailMessages_2 : GetIMAPMailMessages - Get Emails (IMAP)
GetRobotCredential_1 --> GetIMAPMailMessages_2
MultipleAssign_3 : MultipleAssign - Get Exception Screenshot Files
GetIMAPMailMessages_2 --> MultipleAssign_3
MultipleAssign_3 --> ForEach1_1
ForEach1_1: ForEach - Delete Screenshot
state ForEach1_1 {
direction TB
DeleteFileX_1 : DeleteFileX - Delete Screenshot File
}
VerifyExpression_8 : VerifyExpression - Verify Exception Screenshot
ForEach1_1 --> VerifyExpression_8
VerifyExpression_7 : VerifyExpression - Verify TestException
VerifyExpression_8 --> VerifyExpression_7
VerifyExpression_5 : VerifyExpression - Verify EmailCount
VerifyExpression_7 --> VerifyExpression_5
}
}
}
LogMessage_5 : LogMessage - LM -- Complete
TimeoutScope_2 --> LogMessage_5
}
```