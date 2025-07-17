# GenerateDiagnosticDictionary
Class: GenerateDiagnosticDictionary

Generates a dictinoary that contains diagnostic information in cases of system exceptions.

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
- System.Collections
- System.ComponentModel
- System.ComponentModel.TypeConverter
- System.Configuration.ConfigurationManager
- System.Console
- System.Core
- System.Data
- System.Data.Common
- System.Linq
- System.Memory
- System.Memory.Data
- System.ObjectModel
- System.Private.CoreLib
- System.Private.DataContractSerialization
- System.Private.ServiceModel
- System.Private.Uri
- System.Reflection.DispatchProxy
- System.Reflection.Metadata
- System.Reflection.TypeExtensions
- System.Runtime.Serialization
- System.Runtime.Serialization.Formatters
- System.Runtime.Serialization.Primitives
- System.Security.Permissions
- System.ServiceModel
- System.ServiceModel.Activities
- System.Xaml
- System.Xml
- System.Xml.Linq
- UiPath.Studio.Constants
- UiPath.System.Activities
- UiPath.System.Activities.Design
- UiPath.System.Activities.ViewModels
- UiPath.Workflow


</details>
<details>
    <summary>
    <b>Arguments</b>
    </summary>

| Name | Direction | Type | Description |
|  --- | --- | --- | ---  |
| in_Exception | InArgument | s:Exception | The exception to use to generate diagnostics. |
| out_TemplateData | OutArgument | scg:Dictionary(x:String, x:Object) | A dictionary with the diagnostic data filled in. |

    
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

- Shared\Tests\GenerateDiagnosticDictionary\GenerateDiagnosticDictionarySuccess.xaml

    
</details>

<hr />

## Outline (Beta)

```mermaid
stateDiagram-v2


Sequence_1: Sequence - GenerateDiagnosticDictionary
state Sequence_1 {
direction TB
LogMessage_1 : LogMessage - LM -- Start
MultipleAssign_1 : MultipleAssign - Setup Dictionary
LogMessage_1 --> MultipleAssign_1
}
```