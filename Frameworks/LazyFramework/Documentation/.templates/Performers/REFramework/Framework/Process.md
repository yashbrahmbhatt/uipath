# Process
Class: Process

Invoke major steps of the business process, which are usually implemented by multiple subworkflows.

If a BusinessRuleException is thrown, the transaction is skipped. 
If another kind of exception occurs, the current transaction can be retried. 

<hr />

## Workflow Details
<details>
    <summary>
    <b>Namespaces</b>
    </summary>
    
- System
- System.Collections.Generic
- System.Data
- System.Linq
- System.Text
- UiPath.Core
- UiPath.Core.Activities
- System.Activities
- System.Activities.Statements
- System.Activities.DynamicUpdate
- System.Runtime.Serialization
- System.Runtime.InteropServices
- System.Linq.Expressions
- System.Collections.ObjectModel


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
- UiPath.System.Activities


</details>
<details>
    <summary>
    <b>Arguments</b>
    </summary>

| Name | Direction | Type | Description |
|  --- | --- | --- | ---  |
| in_TransactionItem | InArgument | ui:QueueItem | Transaction item to be processed. |
| in_Config | InArgument | scg:Dictionary(x:String, x:Object) | Dictionary structure to store configuration data of the process (settings, constants and assets). |

    
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


Sequence_2: Sequence - Process
state Sequence_2 {
direction TB
LogMessage_1 : LogMessage - Log Message Process Start
Comment_1 : Comment - Comment (placeholder)
LogMessage_1 --> Comment_1
}
```