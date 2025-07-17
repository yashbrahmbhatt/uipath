# LoadConfigSuccess
Class: LoadConfigSuccess

Tests all the paths for loading a config. Verifies:
- All sheets loaded
- Ignored sheets not loaded
- Assets sheet loaded from Orchestrator
- Local TextFiles loaded
- Orchestrator Bucket TextFiles loaded

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
- UiPath.Core.Activities
- System.Collections.ObjectModel
- System.Runtime.Serialization
- System.Reflection
- UiPath.Testing.Activities
- UiPath.Shared.Activities
- System.Activities.Runtime.Collections
- UiPath.Core
- GlobalVariablesNamespace
- GlobalConstantsNamespace
- System.Data
- System.ComponentModel
- System.Xml.Serialization
- System.Data
- System.ComponentModel
- System.Xml.Serialization


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
- System.ComponentModel
- System.ComponentModel.TypeConverter
- System.Configuration.ConfigurationManager
- System.Console
- System.Core
- System.Data
- System.Drawing
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
- UiPath.Excel
- UiPath.Excel.Activities
- UiPath.Mail.Activities
- UiPath.Studio.Constants
- UiPath.System.Activities
- UiPath.Testing.Activities
- UiPath.Workflow
- WindowsBase
- UiPath.System.Activities.Design
- UiPath.System.Activities.ViewModels
- System.Collections
- System.IO.FileSystem.Watcher
- System.IO.Packaging
- System.IO.FileSystem.AccessControl
- System.IO.FileSystem.DriveInfo
- System.Linq.Parallel
- System.Collections.Immutable
- System.Linq.Queryable
- System.Data.Common
- System.ComponentModel.Primitives
- System.Private.Xml
- System.Data.SqlClient
- System.ComponentModel.EventBasedAsync
- Microsoft.Win32.Primitives


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


Sequence_1: Sequence - LoadConfigSuccess
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
InvokeWorkflowFile_1 : InvokeWorkflowFile - Utility\\LoadConfig.xaml - Invoke Workflow File
}
}
LogMessage_3 : LogMessage - LM -- Test Executed
TryCatch_1 --> LogMessage_3
LogMessage_3 --> Sequence_4
Sequence_4: Sequence - Validate Results
state Sequence_4 {
direction TB
VerifyExpression_5 : VerifyExpression - Verify TextException
VerifyExpression_6 : VerifyExpression - Verify Non-IgnoreSheets Values Loaded
VerifyExpression_5 --> VerifyExpression_6
VerifyExpression_7 : VerifyExpression - Verify IgnoreSheets Values Not Loaded
VerifyExpression_6 --> VerifyExpression_7
VerifyExpression_8 : VerifyExpression - Verify Asset Loaded
VerifyExpression_7 --> VerifyExpression_8
VerifyExpression_9 : VerifyExpression - Verify Local TextFile Loaded
VerifyExpression_8 --> VerifyExpression_9
VerifyExpression_10 : VerifyExpression - Verify Storage Bucket TextFile Loaded
VerifyExpression_9 --> VerifyExpression_10
VerifyExpression_11 : VerifyExpression - Verify Local ExcelFile Loaded
VerifyExpression_10 --> VerifyExpression_11
VerifyExpression_12 : VerifyExpression - Verify Storage Bucket ExcelFile Loaded
VerifyExpression_11 --> VerifyExpression_12
}
}
}
LogMessage_4 : LogMessage - LM -- Complete
TimeoutScope_1 --> LogMessage_4
}
```