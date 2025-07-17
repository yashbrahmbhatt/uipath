# SetTransactionStatus
Class: SetTransactionStatus

Set and log the transaction's status along with extra log fields. 
There can be three possible statuses: Success, Business Exception and System Exception.

Business Rule Exception characterizes an irregular situation according to the process's rules and prevents the transaction to be processed. The transaction is not retried in this case, since the result will be the same until the problem that causes the exception is solved.
For example, it can be considered a BusinessRuleException if a process expects to read an email's attachment, but the sender didn't attach any file. In this case, immediate retries of the transaction will not yield a different result.

On the other hand, system exceptions are characterized by exceptions whose types are different than BusinessRuleException. When this kind of exception happens, the transaction item can be retried after closing and reopening the applications involved in the process. The rationale behind this is that the exception was caused by a problem in the applications, which might be solved by restarting them.

If Orchestrator queues are the source of transactions, the Set Transaction Status activity is used to update the status. In addition, the retry mechanism is also implemented by Orchestrator.

If Orchestrator queues are not used, the status can be set, for example, by writing to a specific column in a spreadsheet. In such cases, the retry mechanism is covered by the framework and the number of retries is defined in the configuration file.

At the end, io_TransactionNumber is incremented, which makes the framework get the next transaction to be processed.

<hr />

## Workflow Details
<details>
    <summary>
    <b>Namespaces</b>
    </summary>
    
- GlobalConstantsNamespace
- GlobalVariablesNamespace
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


</details>
<details>
    <summary>
    <b>References</b>
    </summary>

- Microsoft.Bcl.AsyncInterfaces
- Microsoft.CSharp
- NPOI
- System
- System.Activities
- System.ComponentModel
- System.ComponentModel.Composition
- System.ComponentModel.TypeConverter
- System.Configuration.ConfigurationManager
- System.Console
- System.Core
- System.Data
- System.Data.Common
- System.Linq
- System.Linq.Expressions
- System.Memory
- System.ObjectModel
- System.Private.CoreLib
- System.Private.Uri
- System.Runtime.Serialization
- System.Security.Permissions
- System.ServiceModel
- System.ServiceModel.Activities
- System.ValueTuple
- System.Xaml
- System.Xml
- System.Xml.Linq
- UiPath.Excel
- UiPath.Studio.Constants
- UiPath.System.Activities
- UiPath.System.Activities.Design
- UiPath.System.Activities.ViewModels
- System.Memory.Data
- UiPath.Workflow


</details>
<details>
    <summary>
    <b>Arguments</b>
    </summary>

| Name | Direction | Type | Description |
|  --- | --- | --- | ---  |
| in_BusinessException | InArgument | ui:BusinessRuleException | Exception variable that is used during transitions between states and represents a situation that does not conform to the rules of the process being automated. |
| in_Config | InArgument | scg:Dictionary(x:String, x:Object) | Dictionary structure to store configuration data of the process (settings, constants and assets). |
| in_TransactionItem | InArgument | ui:QueueItem | Transaction item to be processed. |
| io_RetryNumber | InOutArgument | x:Int32 | Used to control the number of attempts of retrying the transaction processing in case of system exceptions. |
| io_TransactionNumber | InOutArgument | x:Int32 | Sequential counter of transaction items. |
| in_TransactionField1 | InArgument | x:String | Optionally used to include additional information about the transaction item. |
| in_TransactionField2 | InArgument | x:String | Optionally used to include additional information about the transaction item. |
| in_TransactionID | InArgument | x:String | Used for information and logging purposes. Ideally, the ID should be unique for each transaction.  |
| in_SystemException | InArgument | s:Exception | Used during transitions between states to represent exceptions other than business exceptions. |
| io_ConsecutiveSystemExceptions | InOutArgument | x:Int32 | Used to control the number of consecutive system exceptions. |

    
</details>
<details>
    <summary>
    <b>Workflows Used</b>
    </summary>

- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\Shared\TakeScreenshot.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\.templates\Performers\REFramework\Framework\RetryCurrentTransaction.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\.templates\Performers\REFramework\Framework\CloseAllApplications.xaml
- C:\Users\yash.brahmbhatt\Documents\UiPath\LazyFramework\.templates\Performers\REFramework\Framework\KillAllProcesses.xaml

    
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


Flowchart_2: Flowchart - Set Transaction Status
state Flowchart_2 {
direction TB

FlowDecision_2: FlowDecision - Is successful?
state FlowDecision_2 {
direction TB

FlowDecision_1: FlowDecision - Is Business Exception?
state FlowDecision_1 {
direction TB

Sequence_5: Sequence - Business Exception
state Sequence_5 {
direction TB

If_2: If - If TransactionItem is a QueueItem (Business Exception)
state If_2 {
direction TB

RetryScope_2: RetryScope - Retry Set Transaction Status (Business Exception)
state RetryScope_2 {
direction TB

TryCatch_6: TryCatch - Try Catch Set Transaction Status (Business Exception)
state TryCatch_6 {
direction TB
SetTransactionStatus_4 : SetTransactionStatus - Set transaction status (Business Exception status)
}
}
}
If_2 --> Sequence_4
Sequence_4: Sequence - Log business exception with additional logging fields
state Sequence_4 {
direction TB
AddLogFields_2 : AddLogFields - Add transaction log fields (Business Exception)
LogMessage_2 : LogMessage - Log Message (Business Exception)
AddLogFields_2 --> LogMessage_2
RemoveLogFields_2 : RemoveLogFields - Remove transaction log fields (Business Exception)
LogMessage_2 --> RemoveLogFields_2
}
}
Sequence_5 --> Sequence_8
Sequence_8: Sequence - System Exception
state Sequence_8 {
direction TB
LogMessage_10 : LogMessage - Log Message (Consecutive exceptions)
Assign_3 : Assign - Assign QueryRetry
LogMessage_10 --> Assign_3
Assign_3 --> TryCatch_4
TryCatch_4: TryCatch - Try taking screenshot
state TryCatch_4 {
direction TB
InvokeWorkflowFile_5 : InvokeWorkflowFile - TakeScreenshot.xaml - Invoke Workflow File
}
TryCatch_4 --> If_3
If_3: If - If TransactionItem is a QueueItem (System Exception)
state If_3 {
direction TB

RetryScope_1: RetryScope - Retry Set Transaction Status (System Exception)
state RetryScope_1 {
direction TB

TryCatch_5: TryCatch - Try Catch Set Transaction Status (System Exception)
state TryCatch_5 {
direction TB

Sequence_6: Sequence - Try Set Transaction Status (System Exception)
state Sequence_6 {
direction TB
SetTransactionStatus_3 : SetTransactionStatus - Set transaction status (System Exception)
Assign_4 : Assign - Assign RetryNumber from Queue
SetTransactionStatus_3 --> Assign_4
}
}
}
}
AddLogFields_3 : AddLogFields - Add transaction log fields (System Exception)
If_3 --> AddLogFields_3
Assign_6 : Assign - Increment consecutive exceptions counter
AddLogFields_3 --> Assign_6
InvokeWorkflowFile_1 : InvokeWorkflowFile - RetryCurrentTransaction.xaml - Invoke Workflow File
Assign_6 --> InvokeWorkflowFile_1
RemoveLogFields_3 : RemoveLogFields - Remove transaction log fields (System Exception)
InvokeWorkflowFile_1 --> RemoveLogFields_3
RemoveLogFields_3 --> TryCatch_3
TryCatch_3: TryCatch - Try closing applications
state TryCatch_3 {
direction TB
InvokeWorkflowFile_3 : InvokeWorkflowFile - CloseAllApplications.xaml - Invoke Workflow File
}
}
}
}
}
```