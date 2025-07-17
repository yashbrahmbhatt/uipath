# InitAllSettings
Class: InitAllSettings

Initialize, populate and output a configuration Dictionary to be used throughout the project. 
Settings and constants are read from the local configuration file, and assets are fetched from Orchestrator. 
Asset values overwrite settings and constant values if they are defined with the same name.

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
- System.ComponentModel
- System.Data
- System.Linq
- System.Linq.Expressions
- System.Reflection
- System.Runtime.InteropServices
- System.Runtime.Serialization
- System.Text
- System.Xml.Serialization
- UiPath.Core
- UiPath.Core.Activities
- UiPath.Excel


</details>
<details>
    <summary>
    <b>References</b>
    </summary>

- Microsoft.Bcl.AsyncInterfaces
- Microsoft.CSharp
- NPOI
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
- System.Linq
- System.Memory
- System.ObjectModel
- System.Private.CoreLib
- System.Private.Xml
- System.Reflection.Metadata
- System.Runtime.Serialization
- System.ServiceModel
- System.ServiceModel.Activities
- System.ValueTuple
- System.Xaml
- System.Xml
- System.Xml.Linq
- UiPath.Excel
- UiPath.Excel.Activities
- UiPath.Studio.Constants
- UiPath.System.Activities
- UiPath.System.Activities.Design
- WindowsBase


</details>
<details>
    <summary>
    <b>Arguments</b>
    </summary>

| Name | Direction | Type | Description |
|  --- | --- | --- | ---  |
| out_Config | OutArgument | scg:Dictionary(x:String, x:Object) | Dictionary structure to store configuration data of the process (settings, constants and assets). |
| in_ConfigFile | InArgument | x:String | Path to the configuration file that defines settings, constants and assets. |
| in_ConfigSheets | InArgument | s:String[] | Names of the sheets corresponding to settings and constants in the configuration file. |

    
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


Sequence_5: Sequence - Initialize All Settings
state Sequence_5 {
direction TB
LogMessage_2 : LogMessage - Log Message (Initialize All Settings)
Assign_1 : Assign - Assign out_Config (initialization)
LogMessage_2 --> Assign_1
Assign_1 --> ForEach1_1
ForEach1_1: ForEach - For each configuration sheet
state ForEach1_1 {
direction TB

Sequence_2: Sequence - Get local settings and constants
state Sequence_2 {
direction TB
ReadRange_1 : ReadRange - Read range (Settings and Constants sheets)
ReadRange_1 --> ForEachRow_1
ForEachRow_1: ForEachRow - For each configuration row
state ForEachRow_1 {
direction TB

If_1: If - If configuration row is not empty
state If_1 {
direction TB
Assign_2 : Assign - Add Config key/value pair
}
}
}
}
ForEach1_1 --> TryCatch_2
TryCatch_2: TryCatch - Try initializing assets
state TryCatch_2 {
direction TB

Sequence_4: Sequence - Get Orchestrator assets
state Sequence_4 {
direction TB
ReadRange_2 : ReadRange - Read range (Assets sheet)
ReadRange_2 --> ForEachRow_2
ForEachRow_2: ForEachRow - For each asset row
state ForEachRow_2 {
direction TB

TryCatch_1: TryCatch - Try retrieving asset from Orchestrator
state TryCatch_1 {
direction TB

Sequence_3: Sequence - Get asset from Orchestrator
state Sequence_3 {
direction TB
GetRobotAsset_1 : GetRobotAsset - Get Orchestrator asset
Assign_3 : Assign - Assign asset value in Config
GetRobotAsset_1 --> Assign_3
}
}
}
}
}
}
```