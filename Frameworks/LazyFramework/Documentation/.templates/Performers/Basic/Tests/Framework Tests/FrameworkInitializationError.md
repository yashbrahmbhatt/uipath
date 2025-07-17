# FrameworkInitializationError
Class: BasicPerformerInitializationError

Tests for a framework exception when Initialization state has an error. Validates that the exception email is sent out.

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
- UiPath.Platform.ResourceHandling
- System.Security
- UiPath.Mail
- UiPath.Mail.IMAP.Activities
- UiPath.Mail.Activities
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
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\.templates\Performers\Basic\BasicPerformer.xaml

    
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


Sequence_1: Sequence - BasicPerformerInitializationError
state Sequence_1 {
direction TB
LogMessage_4 : LogMessage - LM -- Start
LogMessage_4 --> TimeoutScope_1
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
LogMessage_3 : LogMessage - LM -- Initialization Complete
Sequence_2 --> LogMessage_3
LogMessage_3 --> TryCatch_1
TryCatch_1: TryCatch - Execute Test
state TryCatch_1 {
direction TB

Sequence_3: Sequence - ... When
state Sequence_3 {
direction TB
InvokeWorkflowFile_2 : InvokeWorkflowFile - .templates\\Performers\\Basic\\BasicPerformer.xaml - Invoke Workflow File
}
}
LogMessage_2 : LogMessage - LM -- Test Executed
TryCatch_1 --> LogMessage_2
LogMessage_2 --> Sequence_4
Sequence_4: Sequence - Validate Results
state Sequence_4 {
direction TB
GetRobotCredential_1 : GetRobotCredential - Get Email Credentials
GetIMAPMailMessages_1 : GetIMAPMailMessages - Get Emails (IMAP)
GetRobotCredential_1 --> GetIMAPMailMessages_1
MultipleAssign_3 : MultipleAssign - Get Exception Screenshot Files
GetIMAPMailMessages_1 --> MultipleAssign_3
MultipleAssign_3 --> ForEach1_1
ForEach1_1: ForEach - Delete Screenshot
state ForEach1_1 {
direction TB
DeleteFileX_1 : DeleteFileX - Delete Screenshot File
}
VerifyExpression_6 : VerifyExpression - Verify TestException
ForEach1_1 --> VerifyExpression_6
VerifyExpression_7 : VerifyExpression - Verify Exception Screenshot
VerifyExpression_6 --> VerifyExpression_7
VerifyExpression_8 : VerifyExpression - Verify EmailCount
VerifyExpression_7 --> VerifyExpression_8
}
}
}
LogMessage_1 : LogMessage - LM -- Complete
TimeoutScope_1 --> LogMessage_1
}
```