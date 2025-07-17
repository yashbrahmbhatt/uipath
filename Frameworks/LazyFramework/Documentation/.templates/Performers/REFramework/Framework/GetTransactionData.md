# GetTransactionData
Class: GetTransactionData

Get a transaction item from a specified source (e.g., Orchestrator queues, spreadsheets, databases, mailboxes or web APIs). 

If there are no transaction items remaining, out_TransactionItem is set to Nothing, which leads to the End Process state. 

For cases in which there is only a single transaction (i.e., a linear process), use an If activity to check whether the argument in_TransactionNumber has the value 1 (meaning it is the first and only transaction) and assign the transaction item to out_TransactionItem. For any other value of in_TransactionNumber, out_TransactionItem should be set to Nothing.

If there are multiple transactions, use the argument in_TransactionNumber as an index to retrieve the correct transaction to be processed. If there are no more transactions left, it is necessary to set out_TransactionItem to Nothing, thus ending the process.

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
- System.Runtime.Serialization
- System.Text
- UiPath.Core
- UiPath.Core.Activities


</details>
<details>
    <summary>
    <b>References</b>
    </summary>

- Microsoft.CSharp
- System
- System.Activities
- System.ComponentModel
- System.ComponentModel.Composition
- System.ComponentModel.Primitives
- System.ComponentModel.TypeConverter
- System.Core
- System.Data
- System.Data.Common
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
- UiPath.System.Activities
- UiPath.System.Activities.Design


</details>
<details>
    <summary>
    <b>Arguments</b>
    </summary>

| Name | Direction | Type | Description |
|  --- | --- | --- | ---  |
| in_TransactionNumber | InArgument | x:Int32 | Sequential counter of transaction items. |
| in_Config | InArgument | scg:Dictionary(x:String, x:Object) | Dictionary structure to store configuration data of the process (settings, constants and assets). |
| out_TransactionItem | OutArgument | ui:QueueItem | Transaction item to be processed. |
| out_TransactionField1 | OutArgument | x:String | Allow the optional addition of information about the transaction item. |
| out_TransactionField2 | OutArgument | x:String | Allow the optional addition of information about the transaction item. |
| out_TransactionID | OutArgument | x:String | Transaction ID used for information and logging purposes. Ideally, the ID should be unique for each transaction.  |
| io_dt_TransactionData | InOutArgument | sd:DataTable | This variable can be used in case transactions are stored in a DataTable (for example, after being retrieved from a spreadsheet). |

    
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


Sequence_3: Sequence - Get Transaction Data
state Sequence_3 {
direction TB
LogMessage_1 : LogMessage - Log Message Get Transaction Item
LogMessage_1 --> RetryScope_1
RetryScope_1: RetryScope - Retry Get transaction item
state RetryScope_1 {
direction TB

TryCatch_1: TryCatch - Try Catch Get transaction item
state TryCatch_1 {
direction TB
GetQueueItem_1 : GetQueueItem - Get transaction item
}
}
RetryScope_1 --> If_1
If_1: If - If a new transaction item is retrieved, get additional information about it
state If_1 {
direction TB

Sequence_2: Sequence - Add transaction information to log fields
state Sequence_2 {
direction TB
Assign_1 : Assign - Assign out_TransactionID
Assign_2 : Assign - Assign out_TransactionField1
Assign_1 --> Assign_2
Assign_3 : Assign - Assign out_TransactionField2
Assign_2 --> Assign_3
}
}
}
```