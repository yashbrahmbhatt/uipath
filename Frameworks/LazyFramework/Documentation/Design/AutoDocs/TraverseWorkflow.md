# TraverseWorkflow
Class: TraverseWorkflow

Recursive workflow to traverse and document the workflow into mermaid diagrams.
 
/_\/_\/_\
WIP
/_\/_\/_\

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
- System.Xml.Serialization
- Newtonsoft.Json.Linq
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
| in_XElement | InArgument | sxl:XElement | The current element to document/traverse. |
| io_Markdown | InOutArgument | x:String | The current markdown generated. |
| io_PreviousActivity | InOutArgument | x:String | The parent element of the current node. |

    
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


Sequence_1: Sequence - TraverseWorkflow
state Sequence_1 {
direction TB
MultipleAssign_1 : MultipleAssign - Parse Element
LogMessage_1 : LogMessage - LM -- Start
MultipleAssign_1 --> LogMessage_1
InvokeCode_1 : InvokeCode - Get Closest Descendants with Display Names
LogMessage_1 --> InvokeCode_1
InvokeCode_1 --> If_1
If_1: If - ActivityName Not Empty?
state If_1 {
direction TB

If_3: If - Activity Has No Children?
state If_3 {
direction TB
MultipleAssign_11 : MultipleAssign - Update Markdown for Single Element

Switch1_2: Switch - Activity Type?
state Switch1_2 {
direction TB

Sequence_13: Sequence - If
state Sequence_13 {
direction TB
MultipleAssign_12 : MultipleAssign - Update Markdown for If Statement
MultipleAssign_12 --> ForEach1_7
ForEach1_7: ForEach - Recurse If.Then and If.Else
state ForEach1_7 {
direction TB

Sequence_15: Sequence - Process Child
state Sequence_15 {
direction TB
MultipleAssign_13 : MultipleAssign - Update Previous Element to Blank
InvokeWorkflowFile_4 : InvokeWorkflowFile - Recurse Child
MultipleAssign_13 --> InvokeWorkflowFile_4
}
}
MultipleAssign_14 : MultipleAssign - Close Markdown for If
ForEach1_7 --> MultipleAssign_14
}
}
}

ForEach1_5: ForEach - Recurse Children
state ForEach1_5 {
direction TB
InvokeWorkflowFile_6 : InvokeWorkflowFile - Traverse Child
}
}
}
```