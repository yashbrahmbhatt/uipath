# WriteTableToExcel
Class: WriteTableToExcel

Writes a table to an excel file.

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
- System.Runtime.Serialization
- System.Xml.Serialization
- UiPath.Core.Activities
- UiPath.Excel
- UiPath.Excel.Activities.Business
- UiPath.Excel.Model
- UiPath.Shared.Activities


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
- UiPath.Excel
- UiPath.Excel.Activities
- UiPath.Excel.Activities.Design
- UiPath.Mail.Activities
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
| in_Path | InArgument | x:String | The path to the file to write to. File must exist already. |
| in_SheetName | InArgument | x:String | The name of the sheet to write the table to. |
| in_dt_Table | InArgument | sd:DataTable | The datatable to write to a sheet. |

    
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


Sequence_1: Sequence - WriteTableToExcel
state Sequence_1 {
direction TB
LogMessage_2 : LogMessage - LM -- Start
LogMessage_2 --> ExcelProcessScopeX_1
ExcelProcessScopeX_1: ExcelProcessScopeX - Excel
state ExcelProcessScopeX_1 {
direction TB

ExcelApplicationCard_1: ExcelApplicationCard - Use File
state ExcelApplicationCard_1 {
direction TB

Sequence_2: Sequence - Workflow Analyzer Gives a Warning If I Don't Have This Sequence
state Sequence_2 {
direction TB
WriteRangeX_2 : WriteRangeX - Write Table
}
}
}
LogMessage_1 : LogMessage - LM -- Complete
ExcelProcessScopeX_1 --> LogMessage_1
}
```