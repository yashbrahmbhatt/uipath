# InitAllApplications
Class: InitAllApplications

Open applications used in the process and do necessary initialization procedures (e.g., login).

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
- System.Linq.Expressions
- System.Collections.ObjectModel


</details>
<details>
    <summary>
    <b>References</b>
    </summary>

- Microsoft.CSharp
- System
- System.Activities
- System.ComponentModel.TypeConverter
- System.Core
- System.Data
- System.Data.Common
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
- UiPath.System.Activities
- UiPath.System.Activities.Design


</details>
<details>
    <summary>
    <b>Arguments</b>
    </summary>

| Name | Direction | Type | Description |
|  --- | --- | --- | ---  |
| in_Config | InArgument | scg:Dictionary(x:String, x:Object) |  |

    
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


Sequence_2: Sequence - Initialize All Applications
state Sequence_2 {
direction TB
LogMessage_1 : LogMessage - Log message (Initialize applications)
}
```