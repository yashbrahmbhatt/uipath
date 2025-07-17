# InitAllSettingsTestCase
Class: InitAllSettingsTestCase

Verify if the InitAllSettings workflow works as expected.
The verification should check if initalization of settings was successful: if the Config dictionary was created, if it contains a certain key etc.

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
- System.Collections.ObjectModel
- System.Collections
- System.Collections.Generic
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
- System.ComponentModel.Composition
- System.ComponentModel.TypeConverter
- System.Core
- System.Data
- System.Data.Common
- System.Drawing
- System.Linq
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

- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Framework/InitAllSettings.xaml

    
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


Sequence_1: Sequence - InitAllSettingsTestCase
state Sequence_1 {
direction TB
LogMessage_1 : LogMessage - Log Message - InitAllSettingsTestCase
LogMessage_1 --> Sequence_2
Sequence_2: Sequence - ... Given
state Sequence_2 {
direction TB
Comment_1 : Comment - Prerequisities
}
Sequence_2 --> Sequence_3
Sequence_3: Sequence - ... When
state Sequence_3 {
direction TB
InvokeWorkflowFile_1 : InvokeWorkflowFile - Invoke InitAllSettings workflow
}
Sequence_3 --> Sequence_4
Sequence_4: Sequence - ... Then
state Sequence_4 {
direction TB
VerifyExpressionWithOperator_1 : VerifyExpressionWithOperator - Verify if Config was loaded
VerifyExpression_1 : VerifyExpression - Verify if setting exists in Config
VerifyExpressionWithOperator_1 --> VerifyExpression_1
}
}
```