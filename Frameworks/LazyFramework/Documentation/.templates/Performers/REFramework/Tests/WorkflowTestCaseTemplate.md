# WorkflowTestCaseTemplate
Class: TestWorkflowTemplate

Template workflow used to create tests for workflows in the process.
Create a new test workflow by copying and renaming this file.

<hr />

## Workflow Details
<details>
    <summary>
    <b>Namespaces</b>
    </summary>
    
- System
- System.Activities
- System.Activities.DynamicUpdate
- System.Activities.Statements
- System.Collections
- System.Collections.Generic
- System.Collections.ObjectModel
- System.Data
- System.Linq
- System.Linq.Expressions
- System.Reflection
- System.Runtime.InteropServices
- System.Runtime.Serialization
- System.Text
- UiPath.Core
- UiPath.Core.Activities
- UiPath.Shared.Activities
- UiPath.Testing
- UiPath.Testing.Activities


</details>
<details>
    <summary>
    <b>References</b>
    </summary>

- Microsoft.Bcl.AsyncInterfaces
- Microsoft.CSharp
- System
- System.Activities
- System.ComponentModel.Composition
- System.ComponentModel.TypeConverter
- System.Core
- System.Data
- System.Data.Common
- System.Linq
- System.Memory
- System.ObjectModel
- System.Private.CoreLib
- System.Runtime.Serialization
- System.ServiceModel
- System.ServiceModel.Activities
- System.ValueTuple
- System.Xaml
- System.Xml
- System.Xml.Linq
- UiPath.Excel
- UiPath.Excel.Activities
- UiPath.System.Activities
- UiPath.System.Activities.Design
- UiPath.Testing
- UiPath.Testing.Activities


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

- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Framework\InitAllSettings.xaml

    
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


Sequence_2: Sequence - Test Template
state Sequence_2 {
direction TB
LogMessage_1 : LogMessage - Log Message - Test Template
LogMessage_1 --> Sequence_3
Sequence_3: Sequence - ... Given
state Sequence_3 {
direction TB
InvokeWorkflowFile_2 : InvokeWorkflowFile - Invoke InitAllSettings workflow
}
Sequence_3 --> Sequence_4
Sequence_4: Sequence - ... When
state Sequence_4 {
direction TB
Comment_1 : Comment - Actions to be performed
}
Sequence_4 --> Sequence_6
Sequence_6: Sequence - ... Then
state Sequence_6 {
direction TB

CommentOut_1: CommentOut - Enable and change as needed
state CommentOut_1 {
direction TB

Sequence_5: Sequence - Ignored Activities
state Sequence_5 {
direction TB
VerifyControlAttribute_1 : VerifyControlAttribute - Verify activity output
}
}
}
}
```