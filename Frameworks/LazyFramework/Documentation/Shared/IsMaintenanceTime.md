# IsMaintenanceTime
Class: IsMaintenanceTime

Given a CRON expression for the maintenance schedule, checks whether the current date/time is within the maintenance window. Outputs a boolean.

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
- System.Linq
- UiPath.Core.Activities


</details>
<details>
    <summary>
    <b>References</b>
    </summary>

- Microsoft.CSharp
- Microsoft.VisualBasic
- NPOI
- System
- System.Activities
- System.ComponentModel
- System.ComponentModel.TypeConverter
- System.Configuration.ConfigurationManager
- System.Console
- System.Core
- System.Data
- System.Data.Common
- System.Linq
- System.Linq.Expressions
- System.Memory
- System.Memory.Data
- System.ObjectModel
- System.Private.CoreLib
- System.Private.Uri
- System.Reflection.DispatchProxy
- System.Reflection.Metadata
- System.Reflection.TypeExtensions
- System.Runtime.Serialization
- System.Security.Permissions
- System.ServiceModel
- System.ServiceModel.Activities
- System.Xaml
- System.Xml
- System.Xml.Linq
- UiPath.Studio.Constants
- UiPath.System.Activities
- UiPath.Workflow


</details>
<details>
    <summary>
    <b>Arguments</b>
    </summary>

| Name | Direction | Type | Description |
|  --- | --- | --- | ---  |
| in_Start | InArgument | x:TimeSpan | The start time of the maintenance period. |
| in_End | InArgument | x:TimeSpan | The end time of the maintenance period. |
| out_IsMaintenanceTime | OutArgument | x:Boolean | Output boolean as to whether current time is within the maintenance period. |

    
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


Sequence_1: Sequence - IsMaintenanceTime
state Sequence_1 {
direction TB

If_1: If - No Boundary?
state If_1 {
direction TB
MultipleAssign_1 : MultipleAssign - Set to General Case
MultipleAssign_2 : MultipleAssign - Set To Boundary Condition
}
LogMessage_1 : LogMessage - LM -- Complete
If_1 --> LogMessage_1
}
```