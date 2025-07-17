# ParseWorkflow
Class: ParseWorkflow

Parses the workflow-level details.

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
- System.Collections.ObjectModel
- System.Data
- System.Linq
- System.Xml
- System.Xml.Linq
- UiPath.Core.Activities
- System.Reflection
- System.IO
- Newtonsoft.Json.Linq
- System.Xml.Serialization
- System.ComponentModel
- System.Runtime.Serialization
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
- Microsoft.Win32.Primitives
- netstandard
- Newtonsoft.Json
- NPOI
- PresentationFramework
- System
- System.Activities
- System.Collections
- System.Collections.Immutable
- System.Collections.NonGeneric
- System.Collections.Specialized
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
- System.Private.Xml
- System.Private.Xml.Linq
- System.Reflection.DispatchProxy
- System.Reflection.Metadata
- System.Reflection.TypeExtensions
- System.Runtime.CompilerServices.Unsafe
- System.Runtime.CompilerServices.VisualC
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
- System.Xml.XPath.XDocument
- UiPath.Platform
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
| in_FilePath | InArgument | x:String | The path to the workflow file. |
| out_Document | OutArgument | sxl:XDocument | The XDocument if required for later use. |
| out_Namespaces | OutArgument | scg:List(x:String) | A list of the names of the namespaces used in this workflow. |
| out_References | OutArgument | scg:List(x:String) | A list of the names of the imports used in this workflow. |
| out_DocumentClass | OutArgument | x:String | The class created for this workflow. |
| out_WorkflowName | OutArgument | x:String | The name of the workflow. |
| out_WorkflowDescription | OutArgument | x:String | The description of the workflow. |
| out_OutlineMarkdown | OutArgument | x:String | The mermaid diagram markdown of the workflow. (ALPHA) |
| out_dt_Arguments | OutArgument | sd:DataTable | A table of the names, types, direction, and description of the arguments. |
| out_WorkflowsUsed | OutArgument | scg:IEnumerable(x:String) | A list of the relative paths of all workflows invoked by this one. |

    
</details>
<details>
    <summary>
    <b>Workflows Used</b>
    </summary>

- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Design\AutoDocs\TraverseWorkflow.xaml

    
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


Sequence_1: Sequence - ParseWorkflow
state Sequence_1 {
direction TB
LogMessage_1 : LogMessage - LM -- Start
BuildDataTable_1 : BuildDataTable - Initialize Arguments Table
LogMessage_1 --> BuildDataTable_1
MultipleAssign_1 : MultipleAssign - Parse
BuildDataTable_1 --> MultipleAssign_1
MultipleAssign_1 --> ForEach1_1
ForEach1_1: ForEach - Add to Arguments Table
state ForEach1_1 {
direction TB

Sequence_2: Sequence - Parse Argument
state Sequence_2 {
direction TB
MultipleAssign_5 : MultipleAssign - Parse Current Argument
AddDataRow_2 : AddDataRow - Add to ArgumentsTable
MultipleAssign_5 --> AddDataRow_2
}
}
InvokeWorkflowFile_1 : InvokeWorkflowFile - Get Outline Mermaid
ForEach1_1 --> InvokeWorkflowFile_1
MultipleAssign_6 : MultipleAssign - Close Outline Mermaid
InvokeWorkflowFile_1 --> MultipleAssign_6
LogMessage_2 : LogMessage - LM -- Complete
MultipleAssign_6 --> LogMessage_2
}
```