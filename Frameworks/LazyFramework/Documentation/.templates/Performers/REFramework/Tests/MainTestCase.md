# MainTestCase
Class: MainTestCase

Verify if the Main workflow works as expected.
The verification should check whether the status file or report built after the process run is the expected one.

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
- System.ComponentModel
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
- System.Xml.Serialization
- UiPath.Core
- UiPath.Core.Activities
- UiPath.Excel
- UiPath.Excel.Activities
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

- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Main.xaml

    
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


Sequence_1: Sequence - MainTestCase
state Sequence_1 {
direction TB
LogMessage_1 : LogMessage - Log Message - MainTestCase
LogMessage_1 --> Sequence_2
Sequence_2: Sequence - ... Given
state Sequence_2 {
direction TB
Comment_1 : Comment - Prerequisites
}
Sequence_2 --> Sequence_3
Sequence_3: Sequence - ... When
state Sequence_3 {
direction TB
InvokeWorkflowFile_1 : InvokeWorkflowFile - Invoke Main workflow
}
Sequence_3 --> Sequence_4
Sequence_4: Sequence - ... Then
state Sequence_4 {
direction TB
ReadRange_1 : ReadRange - Read Resulted Data
ReadRange_2 : ReadRange - Read Expected Data
ReadRange_1 --> ReadRange_2
VerifyExpressionWithOperator_1 : VerifyExpressionWithOperator - Verify if result data count matches expected data count
ReadRange_2 --> VerifyExpressionWithOperator_1
}
}
```