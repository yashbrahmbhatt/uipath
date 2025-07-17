# GetTransactionDataTestCase
Class: GetTransactionDataTestCase

Given the TransactionNumber, verify if GetTransactionData workflow works as expected.
Once a Transaction Item has been processed, its status will be In Progress.
Please make sure the queue name is configured.

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
- UiPath.Testing.Activities


</details>
<details>
    <summary>
    <b>References</b>
    </summary>

- Microsoft.Bcl.AsyncInterfaces
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
- System.Memory
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
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Framework\GetTransactionData.xaml

    
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


Sequence_1: Sequence - GetTransactionDataTestCase
state Sequence_1 {
direction TB
LogMessage_1 : LogMessage - Log Message - GetTransactionDataTestCase
LogMessage_1 --> Sequence_2
Sequence_2: Sequence - ... Given
state Sequence_2 {
direction TB
InvokeWorkflowFile_1 : InvokeWorkflowFile - Invoke InitAllSettings workflow
Assign_1 : Assign - Assign TransactionNumber
InvokeWorkflowFile_1 --> Assign_1
}
Sequence_2 --> Sequence_4
Sequence_4: Sequence - ... When
state Sequence_4 {
direction TB
InvokeWorkflowFile_2 : InvokeWorkflowFile - Invoke GetTransactionData workflow
}
Sequence_4 --> Sequence_3
Sequence_3: Sequence - ... Then
state Sequence_3 {
direction TB
VerifyExpression_1 : VerifyExpression - Verify transaction item was retrieved
VerifyExpression_1 --> If_1
If_1: If - If transaction item is not null
state If_1 {
direction TB
SetTransactionProgress_1 : SetTransactionProgress - Set transaction item progress to tested
}
}
}
```