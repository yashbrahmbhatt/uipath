# AddCalculatedColumns
Class: AddCalculatedColumns

Adds calculated columns for all queue items.

Current Fields:
- Time Saved
- Execution Time

<hr />

## Workflow Details
<details>
    <summary>
    <b>Namespaces</b>
    </summary>
    
- Microsoft.VisualBasic
- System
- System.Activities
- System.Activities.Statements
- System.Collections
- System.Collections.Generic
- System.Collections.ObjectModel
- System.ComponentModel
- System.Data
- System.Linq
- System.Reflection
- System.Runtime.Serialization
- System.Xml.Serialization
- UiPath.Core
- UiPath.Core.Activities
- UiPath.DataTableUtilities


</details>
<details>
    <summary>
    <b>References</b>
    </summary>

- Microsoft.CSharp
- Microsoft.VisualBasic
- Microsoft.VisualBasic.Core
- Microsoft.VisualBasic.Forms
- Microsoft.Win32.Primitives
- NPOI
- PresentationFramework
- System
- System.Activities
- System.CodeDom
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
- System.IO.FileSystem.AccessControl
- System.IO.FileSystem.DriveInfo
- System.IO.FileSystem.Watcher
- System.IO.Packaging
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
- UiPath.Workflow
- WindowsBase


</details>
<details>
    <summary>
    <b>Arguments</b>
    </summary>

| Name | Direction | Type | Description |
|  --- | --- | --- | ---  |
| in_SuccessTimeSaved | InArgument | x:Double | Time saved in minutes for successful queue items. |
| in_BusExTimeSaved | InArgument | x:Double | Time saved in minutes for business exception queue items. |
| in_SysExTimeSaved | InArgument | x:Double | Time saved in minutes for application exception queue items. |
| io_dt_Table | InOutArgument | sd:DataTable | The table to add the calculated columns to. |

    
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


Sequence_1: Sequence - AddCalculatedColumns
state Sequence_1 {
direction TB
LogMessage_1 : LogMessage - LM -- Start
AddDataColumn1_2 : AddDataColumn - Add Time Saved
LogMessage_1 --> AddDataColumn1_2
AddDataColumn1_3 : AddDataColumn - Add Execution Time
AddDataColumn1_2 --> AddDataColumn1_3
AddDataColumn1_3 --> ForEachRow_1
ForEachRow_1: ForEachRow - For Each Row
state ForEachRow_1 {
direction TB

Sequence_6: Sequence - Update Rows
state Sequence_6 {
direction TB

If_3: If - Item not completed?
state If_3 {
direction TB
Continue_1 : Continue - Skip Row
}
MultipleAssign_4 : MultipleAssign - Update Execution Time
If_3 --> MultipleAssign_4
MultipleAssign_4 --> If_1
If_1: If - Failed?
state If_1 {
direction TB

If_2: If - System Or Business?
state If_2 {
direction TB
MultipleAssign_1 : MultipleAssign - Set System Exception Time Saved
MultipleAssign_2 : MultipleAssign - Set Business Exception Time Saved
}
MultipleAssign_3 : MultipleAssign - Set Success Time Saved
}
}
}
LogMessage_2 : LogMessage - LM -- Complete
ForEachRow_1 --> LogMessage_2
}
```