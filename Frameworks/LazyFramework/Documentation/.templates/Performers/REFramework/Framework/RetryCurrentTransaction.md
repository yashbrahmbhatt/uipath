# RetryCurrentTransaction
Class: RetryCurrentTransaction

Manage the retrying mechanism for the framework and it is invoked in SetTransactionStatus.xaml when a system exception occurs. 
The retrying method is based on the configurations defined in Config.xlsx.

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
| in_Config | InArgument | scg:Dictionary(x:String, x:Object) | Dictionary structure to store configuration data of the process (settings, constants and assets). |
| io_RetryNumber | InOutArgument | x:Int32 | Used to control the number of attempts of retrying the transaction processing in case of system exceptions. |
| io_TransactionNumber | InOutArgument | x:Int32 | Sequential counter of transaction items. |
| in_SystemException | InArgument | s:Exception | Used during transitions between states to represent exceptions other than business exceptions. |
| in_QueueRetry | InArgument | x:Boolean | Used to indicate whether the retry procedure is managed by an Orchestrator queue. |

    
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


Flowchart_2: Flowchart - Retry Current Transaction
state Flowchart_2 {
direction TB

FlowDecision_3: FlowDecision - Retry transaction?
state FlowDecision_3 {
direction TB

FlowDecision_2: FlowDecision - Max retries reached?
state FlowDecision_2 {
direction TB
LogMessage_1 : LogMessage - Log message (Max retries reached)
LogMessage_2 : LogMessage - Log message (Retry)
LogMessage_1 --> LogMessage_2
}
}
}
```