# TakeScreenshot
Class: TakeScreenshot

Takes a screenshot and saves it to a folder.

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
- System.Drawing
- System.Drawing.Imaging
- System.IO
- System.Linq
- System.Reflection
- System.Runtime.Serialization
- System.Windows.Forms
- UiPath.Core.Activities
- UiPath.Platform.ResourceHandling


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
- System.Drawing.Common
- System.Drawing.Primitives
- System.IO.FileSystem.AccessControl
- System.IO.FileSystem.DriveInfo
- System.IO.FileSystem.Watcher
- System.IO.Packaging
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
- System.Windows.Forms
- System.Windows.Forms.Primitives
- System.Xaml
- System.Xml
- System.Xml.Linq
- UiPath.Excel.Activities
- UiPath.Mail.Activities
- UiPath.Platform
- UiPath.Studio.Constants
- UiPath.System.Activities
- UiPath.System.Activities.Design
- UiPath.System.Activities.ViewModels
- UiPath.Testing.Activities
- UiPath.Workflow


</details>
<details>
    <summary>
    <b>Arguments</b>
    </summary>

| Name | Direction | Type | Description |
|  --- | --- | --- | ---  |
| in_FolderPath | InArgument | x:String | The path to the folder to save screenshots to. |
| io_FilePath | InOutArgument | x:String | If specified, overrides the folder path and uses this path for the file name. Otherwise, it just outputs the full path to the screenshot. |

    
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

- Shared\Tests\TakeScreenshot\TakeScreenshotNoPath.xaml
- Shared\Tests\TakeScreenshot\TakeScreenshotPath.xaml

    
</details>

<hr />

## Outline (Beta)

```mermaid
stateDiagram-v2


Sequence_1: Sequence - TakeScreenshot
state Sequence_1 {
direction TB

If_2: If - Empty File Path?
state If_2 {
direction TB
MultipleAssign_1 : MultipleAssign - Set File Name
MultipleAssign_3 : MultipleAssign - Set Folder Path
}
FolderExistsX_1 : FolderExistsX - Get Folder Exists
If_2 --> FolderExistsX_1
FolderExistsX_1 --> If_1
If_1: If - Folder Doesn't Exist?
state If_1 {
direction TB
CreateDirectory_1 : CreateDirectory - Create Folder!
}
MultipleAssign_2 : MultipleAssign - Get Primary Screenshot
If_1 --> MultipleAssign_2
InvokeMethod_1 : InvokeMethod - Copy Screen to Graphic
MultipleAssign_2 --> InvokeMethod_1
InvokeMethod_2 : InvokeMethod - Dispose Graphic
InvokeMethod_1 --> InvokeMethod_2
InvokeMethod_3 : InvokeMethod - Save Screenshot
InvokeMethod_2 --> InvokeMethod_3
InvokeMethod_4 : InvokeMethod - Dispose Screenshot
InvokeMethod_3 --> InvokeMethod_4
LogMessage_1 : LogMessage - LM -- Complete
InvokeMethod_4 --> LogMessage_1
}
```