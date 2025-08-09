# Yash.Config

A set of utilities and activities for configuration management and wizard support in UiPath workflows.

---

## 🚀 Getting Started

1. Add the `Yash.Config` NuGet package as a dependency in your UiPath project.  
2. Use the **LoadConfig** activity to load Excel configuration data into your workflow as a `Dictionary<string, object>`.  
3. Run the **ConfigureSettingsWizard** to set or update required settings like file paths and output directories.  
4. Use the **GenerateConfigClassesWizard** to generate strongly-typed C# config classes automatically based on your Excel config file.

---

## 🛠️ Activities

- **LoadConfig**: Loads an Excel configuration file into a `Dictionary<string, object>`, enabling dynamic and flexible access to configuration data within UiPath workflows.

---

## 🧙‍♂️ Wizards

- **ConfigureSettingsWizard**  
  Helps you configure essential settings for the wizards and activities, including file paths and output directories. It prompts you if any required setting is missing or needs to be updated.

- **GenerateConfigClassesWizard**  
  Generates strongly-typed C# configuration classes from your Excel config file, based on your specified namespace, class name, and additional using directives. This makes working with configuration data type-safe and easier to maintain.

---

## ⚙️ Settings

The package includes a set of configurable settings to control the behavior of the wizards and code generation:

| Setting Key                                 | Label               | Description                                         | Validation                                         |
|---------------------------------------------|---------------------|-----------------------------------------------------|----------------------------------------------------|
| `Yash.Config.AutoGeneration.FilePath`       | Config File Path    | The Excel file path used to generate config classes.| Must be a valid existing file path.                 |
| `Yash.Config.AutoGeneration.OutputDirectory`| Output Directory    | The folder where generated config classes are saved.| Must be a valid existing directory.                 |
| `Yash.Config.AutoGeneration.ClassName`      | Class Name          | The name of the generated C# class.                 | Must start with a letter and not be empty.          |
| `Yash.Config.AutoGeneration.Namespace`      | Namespace           | Namespace for the generated C# class.               | Must not be empty.                                   |
| `Yash.Config.AutoGeneration.AdditionalUsings`| Additional Usings  | Comma-separated list of additional using directives.| Optional, default is none.                           |

---

## 📚 Core Models and Type Parsing

### Config Class

- A dynamic dictionary-like class that supports property-based access and serialization.  
- Supports JSON serialization and binary serialization (via `[Serializable]` and `ISerializable`).  
- Provides runtime-safe dynamic access to config keys as properties, throwing exceptions for missing keys to help catch errors early.

### Type Parsers

- The package includes a comprehensive set of type parsers that convert string representations into various primitive and collection types for seamless deserialization.  
- Supported primitive types include:  
  `string`, `int`, `double`, `bool`, `DateTime`, `TimeSpan`.  
- Supported collection types include:  
  `List<T>` and `T[]` for the above primitives (e.g., `List<string>`, `int[]`).  

### Expected Formats

- **Primitive types:**  
  - Integers, doubles, and booleans follow their usual string representations (e.g., `"42"`, `"true"`, `"3.14"`).  
  - DateTime strings should be in a valid parseable format such as ISO 8601 (e.g., `"2025-07-30T14:25:00"`).  
  - TimeSpan strings should follow the standard TimeSpan formats (e.g., `"01:30:00"` for 1 hour 30 minutes).  

- **Collections:**  
  - Lists and arrays are parsed from comma-separated strings without spaces. For example:  
    - `"value1,value2,value3"` for `List<string>` or `string[]`  
    - `"1,2,3,4"` for `List<int>` or `int[]`  
  - Empty strings are parsed as empty lists or arrays.  

### ConfigFactory

- Provides a method to create strongly typed config class instances from a raw dictionary by mapping and parsing dictionary entries to properties, ensuring proper types and validation.

---

## 📖 License

This project is licensed under the MIT License.

---

## 🔗 Links

- GitHub Repository: [https://github.com/yashbrahmbhatt/Yash.Config](https://github.com/yashbrahmbhatt/Yash.Config)  
- NuGet Package: [https://www.nuget.org/packages/Yash.Config](https://www.nuget.org/packages/Yash.Config)

---


*Built and maintained by Yash Brahmbhatt.*  
