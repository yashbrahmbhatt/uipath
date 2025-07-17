# DataTableToHTML
Class: DataTableToHTML

Convert a DataTable into HTML. Uses only .ToString for all data types so transform the column to string first for different formatting. Use the id argument to specify an id to add to the table tag for css purposes.

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
- System.ComponentModel
- System.Data
- System.Linq
- System.Reflection
- System.Runtime.Serialization
- System.Xml.Serialization
- UiPath.Core.Activities
- UiPath.DataTableUtilities


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
- System.Xaml
- System.Xml
- System.Xml.Linq
- UiPath.Studio.Constants
- UiPath.System.Activities
- UiPath.System.Activities.Design
- UiPath.System.Activities.ViewModels
- UiPath.Workflow
- WindowsBase


</details>
<details>
    <summary>
    <b>Arguments</b>
    </summary>

| Name | Direction | Type | Description |
|  --- | --- | --- | ---  |
| in_dt_ToConvert | InArgument | sd:DataTable | The DataTable to convert to HTML. |
| out_HTMLTable | OutArgument | x:String | The output HTML. |

    
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

- Shared\Tests\DataTableToHTML\DataTableToHTMLSuccess.xaml

    
</details>

<hr />

## Outline (Beta)

```mermaid
stateDiagram-v2


Sequence_1: Sequence - DataTableToHTML
state Sequence_1 {
direction TB
LogMessage_1 : LogMessage - LM -- Start
MultipleAssign_1 : MultipleAssign - Initialize
LogMessage_1 --> MultipleAssign_1
MultipleAssign_1 --> ForEach1_1
ForEach1_1: ForEach - Add Header Row
state ForEach1_1 {
direction TB
MultipleAssign_2 : MultipleAssign - Add Header
}
MultipleAssign_3 : MultipleAssign - Close Header Row
ForEach1_1 --> MultipleAssign_3
MultipleAssign_3 --> ForEachRow_1
ForEachRow_1: ForEachRow - Add Table Rows
state ForEachRow_1 {
direction TB

Sequence_2: Sequence - Add Row
state Sequence_2 {
direction TB
MultipleAssign_4 : MultipleAssign - Open Row
MultipleAssign_4 --> ForEach1_2
ForEach1_2: ForEach - Add Columns
state ForEach1_2 {
direction TB
MultipleAssign_5 : MultipleAssign - Add Column
}
MultipleAssign_6 : MultipleAssign - Close Row
ForEach1_2 --> MultipleAssign_6
}
}
MultipleAssign_7 : MultipleAssign - Close Table
ForEachRow_1 --> MultipleAssign_7
LogMessage_2 : LogMessage - LM -- Complete
MultipleAssign_7 --> LogMessage_2
}
```