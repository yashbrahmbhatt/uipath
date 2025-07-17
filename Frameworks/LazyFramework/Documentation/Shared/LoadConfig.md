# LoadConfig
Class: LoadConfig

Reads the config file, ignoring the sheets defined, and outputs the config and textfiles arguments.

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
- UiPath.Core.Activities
- UiPath.Core.Activities.Orchestrator
- UiPath.Core.Activities.Storage
- UiPath.Excel
- UiPath.Excel.Activities.Business
- UiPath.Excel.Model
- UiPath.Platform.ResourceHandling
- UiPath.Shared.Activities.Business
- UiPath.Core
- GlobalVariablesNamespace
- GlobalConstantsNamespace
- System.Data
- System.ComponentModel
- System.Xml.Serialization
- System.Runtime.Serialization
- System.IO
- UiPath.Shared.Activities


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
- System.Collections.Immutable
- System.ComponentModel
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
- UiPath.Excel
- UiPath.Excel.Activities
- UiPath.Excel.Activities.Design
- UiPath.Mail.Activities
- UiPath.Platform
- UiPath.Studio.Constants
- UiPath.System.Activities
- UiPath.System.Activities.Design
- UiPath.System.Activities.ViewModels
- UiPath.Testing.Activities
- UiPath.Workflow
- System.Data.SqlClient
- System.ComponentModel.EventBasedAsync
- PresentationFramework
- WindowsBase
- Microsoft.Win32.Primitives
- System.ComponentModel.Primitives
- System.Private.Xml
- System.IO.FileSystem.Watcher
- System.IO.Packaging
- System.IO.FileSystem.AccessControl
- System.IO.FileSystem.DriveInfo


</details>
<details>
    <summary>
    <b>Arguments</b>
    </summary>

| Name | Direction | Type | Description |
|  --- | --- | --- | ---  |
| in_ConfigPath | InArgument | x:String | The path to the config file to read. |
| in_IgnoreSheets | InArgument | s:String[] | An array of sheet names to ignore loading into the config variable. |
| out_Config | OutArgument | scg:Dictionary(x:String, x:String) | The loaded config dictionary. |
| out_TextFiles | OutArgument | scg:Dictionary(x:String, x:String) | The loaded dictionary of text resources. |
| out_ExcelFiles | OutArgument | scg:Dictionary(x:String, sd:DataSet) |  |

    
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

- Shared\Tests\LoadConfig\LoadConfigSuccess.xaml

    
</details>

<hr />

## Outline (Beta)

```mermaid
stateDiagram-v2


Sequence_1: Sequence - LoadConfig
state Sequence_1 {
direction TB
LogMessage_4 : LogMessage - LM -- Start
MultipleAssign_1 : MultipleAssign - Initialize Outputs
LogMessage_4 --> MultipleAssign_1
MultipleAssign_1 --> ExcelProcessScopeX_1
ExcelProcessScopeX_1: ExcelProcessScopeX - Using Excel App
state ExcelProcessScopeX_1 {
direction TB

ExcelApplicationCard_1: ExcelApplicationCard - Using Config File
state ExcelApplicationCard_1 {
direction TB

ForEachSheetX_1: ForEachSheetX - For Each Sheet
state ForEachSheetX_1 {
direction TB

Sequence_2: Sequence - Process Sheet
state Sequence_2 {
direction TB
LogMessage_1 : LogMessage - LM -- Processing sheet
LogMessage_1 --> If_1
If_1: If - Ignorable Sheet?
state If_1 {
direction TB

Sequence_3: Sequence - Skip
state Sequence_3 {
direction TB
LogMessage_2 : LogMessage - LM -- Skip
Continue_1 : Continue - Skip
LogMessage_2 --> Continue_1
}
LogMessage_5 : LogMessage - LM -- Continue Sheet
}
If_1 --> ExcelForEachRowX_1
ExcelForEachRowX_1: ExcelForEachRowX - For Each Row
state ExcelForEachRowX_1 {
direction TB

If_3: If - Not Empty Row?
state If_3 {
direction TB

Switch1_3: Switch - Sheet Name?
state Switch1_3 {
direction TB

Sequence_11: Sequence - Process Assets Row
state Sequence_11 {
direction TB

RetryScope_4: RetryScope - Asset Retry
state RetryScope_4 {
direction TB
GetRobotAsset_2 : GetRobotAsset - Get Current Asset
}
Assign_6 : Assign - Set Asset Value
RetryScope_4 --> Assign_6
}
Sequence_11 --> Sequence_12
Sequence_12: Sequence - Process TextFiles Row
state Sequence_12 {
direction TB

If_4: If - NOT Storage Bucket Resource?
state If_4 {
direction TB

RetryScope_9: RetryScope - Retry Network/Local
state RetryScope_9 {
direction TB
ReadTextFile_4 : ReadTextFile - Read Local File
}

RetryScope_10: RetryScope - Retry Orch
state RetryScope_10 {
direction TB
ReadStorageText_4 : ReadStorageText - Get Storage Text
}
}
Assign_7 : Assign - Set TextFiles Value
If_4 --> Assign_7
}
Sequence_12 --> Sequence_19
Sequence_19: Sequence - Process ExcelFiles Row
state Sequence_19 {
direction TB
MultipleAssign_5 : MultipleAssign - Initialize DataSet
MultipleAssign_5 --> If_6
If_6: If - Storage Bucket Resource? (Excel)
state If_6 {
direction TB
MultipleAssign_4 : MultipleAssign - Set Path (Local)
}
If_6 --> RetryScope_11
RetryScope_11: RetryScope - Retry Network/Local (Excel)
state RetryScope_11 {
direction TB

ExcelApplicationCard_3: ExcelApplicationCard - Using Excel File
state ExcelApplicationCard_3 {
direction TB

ForEachSheetX_2: ForEachSheetX - For Each Excel Sheet
state ForEachSheetX_2 {
direction TB

Sequence_20: Sequence - Read Sheet
state Sequence_20 {
direction TB
ReadRangeX_1 : ReadRangeX - Read Sheet Range
MultipleAssign_6 : MultipleAssign - Set Table Name
ReadRangeX_1 --> MultipleAssign_6
InvokeMethod_1 : InvokeMethod - Add Table to DataSet
MultipleAssign_6 --> InvokeMethod_1
}
}
}
}
RetryScope_11 --> If_7
If_7: If - Storage Bucket Resource? (Delete Excel)
state If_7 {
direction TB
DeleteFileX_1 : DeleteFileX - Delete Temp File
}
}
}
LogMessage_6 : LogMessage - LM -- Empty Row
}
}
}
}
}
}
LogMessage_3 : LogMessage - LM -- Complete
ExcelProcessScopeX_1 --> LogMessage_3
}
```