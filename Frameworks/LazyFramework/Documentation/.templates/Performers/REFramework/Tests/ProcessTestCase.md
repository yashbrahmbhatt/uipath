# ProcessTestCase
Class: ProcessTestCase

Verify if the Process workflow works as expected.
The verification should check whether the output of the Process workflow is the expected one.

<hr />

## Workflow Details
<details>
    <summary>
    <b>Namespaces</b>
    </summary>
    
- Microsoft.VisualBasic
- Microsoft.VisualBasic.Activities
- System
- System.Activities
- System.Activities.DynamicUpdate
- System.Activities.Expressions
- System.Activities.Statements
- System.Activities.Validation
- System.Activities.XamlIntegration
- System.Collections
- System.Collections.Generic
- System.Collections.ObjectModel
- System.Data
- System.Diagnostics
- System.Drawing
- System.IO
- System.Linq
- System.Linq.Expressions
- System.Net.Mail
- System.Runtime.Serialization
- System.Text
- System.Windows.Markup
- System.Xml
- System.Xml.Linq
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

- Microsoft.CSharp
- Microsoft.VisualBasic
- PresentationCore
- PresentationFramework
- System
- System.Activities
- System.ComponentModel
- System.ComponentModel.Composition
- System.ComponentModel.Primitives
- System.ComponentModel.TypeConverter
- System.Core
- System.Data
- System.Data.Common
- System.Drawing
- System.Linq
- System.ObjectModel
- System.Private.CoreLib
- System.Private.Xml
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
- UiPath.Testing
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

- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Framework\InitAllSettings.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Framework\InitAllApplications.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Framework\GetTransactionData.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Framework\Process.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Framework\CloseAllApplications.xaml

    
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


Sequence_1: Sequence - ProcessTestCase
state Sequence_1 {
direction TB
LogMessage_1 : LogMessage - Log Message - ProcessTestCase
LogMessage_1 --> Sequence_2
Sequence_2: Sequence - ... Given
state Sequence_2 {
direction TB
InvokeWorkflowFile_2 : InvokeWorkflowFile - Invoke InitAllSettings workflow
InvokeWorkflowFile_3 : InvokeWorkflowFile - Invoke InitAllApplications workflow
InvokeWorkflowFile_2 --> InvokeWorkflowFile_3
Assign_1 : Assign - Assign TransactionNumber
InvokeWorkflowFile_3 --> Assign_1
InvokeWorkflowFile_4 : InvokeWorkflowFile - Invoke GetTransactionData workflow
Assign_1 --> InvokeWorkflowFile_4
}
Sequence_2 --> Sequence_3
Sequence_3: Sequence - ... When
state Sequence_3 {
direction TB
InvokeWorkflowFile_1 : InvokeWorkflowFile - Invoke Process workflow
}
Sequence_3 --> Sequence_4
Sequence_4: Sequence - ... Then
state Sequence_4 {
direction TB

CommentOut_1: CommentOut - Enable and change as needed
state CommentOut_1 {
direction TB

Sequence_5: Sequence - Ignored Activities
state Sequence_5 {
direction TB
VerifyExpressionWithOperator_1 : VerifyExpressionWithOperator - Verify process output
}
}
InvokeWorkflowFile_5 : InvokeWorkflowFile - Invoke CloseAllApplications workflow
CommentOut_1 --> InvokeWorkflowFile_5
}
}
```