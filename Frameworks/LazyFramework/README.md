# Methodology
This project template was created as a result of having to make the same changes to the REFramework template over and over again across multiple processes. It felt like there had to be a better way to set myself up to develop more robust processes and make development easier on myself at the same time. These are the key fundamental practices/ideas that guided the decision making for how to create this framework.

<details>
  <summary>
    <b>What's Wrong With REFramework</b>
  </summary>
  
  I think there are a few fundamental flaws with the REFramework, outlined below:

  1. No separation between system exceptions for transactions vs framework components. This creates unnecessary confusion at the framework level, and requires the user to do the heavy lifting of understanding when the SystemException variable is coming from a framework exception (ie. initialization/get transaction data/set transaction status failure) or a transaction (process.xaml exception). The answer is to just create separate variables for these different scenarios and modify the transitions to make it clearer.
  2. Lack of sending emails for exceptions. When the bot encounters a Business, System, or Framework exception, there is usually some action that a human must take. If its a business exception, the business must take action. If its a system or framework exception, the RPA or IT Ops team must take action. Therefore, it's just something that should be included.
  3. GetTransactionData.xaml is useless. As far as I can tell, it is used to retrieve and parse the QueueItem.SpecificContent dictionary and prepare the data for the Process.xaml workflow. Considering the above point, in case the input data is incorrect and the bot fails, the REFramework just ends the process, without notifiying the business that they need to take action on this item. Parsing the input data should be a task within Process.xaml. You can then just have the GetQueueItem activity directly in Main.xaml.
  4. There is no need to support non-orchestrator queues. It is such an edge case, and considering how much bloat/complexity it adds to the framework, it doesn't seem worth including it. Just try refactoring the base REFramework template to only support Orchestrator queues, and you'll see SetTransactionStatus.xaml be simplified extensiely, even completely removing RetryTransaction.xaml (or whatever its called).
  5. I think log messages are fine to hard code within the code, and should not be included in the Config file. I find that the only time I change a log message is when I want to add additional variable information at runtime, not to change the semantics of the message, in which case, I would have to make a code change and publish a new version anyways. The benefit of hard-coding messages is that it declutters the Config file to only the important stuff.
  6. The base InitAllSettings.xaml isn't great. While it doesn't require Excel to be installed, it cannot deal with the config file being 'locked' by another user (ie. ReadOnly Open). It also could have more functionality like reading mapping files or text resources from storage buckets or local paths.

</details>
  
<details>
  <summary>
    <b>Why Can't I Find The Click Activity?</b>
  </summary>

  I wanted to completely remove the UiAutomation package as a direct dependancy of the project template. This signals you to try to isolate all UI logic to libraries, which is the best practice. This will ensure that as you create automations, you will have an ever expanding set of workflows organized by libraries at your disposal to reuse as needed. No more copy pasting. The only portion of the REFramework that uses the UiAutomation package is TakeScreenshot.xaml, which uses it to take a screenshot of the screen during exceptions. This project template works around that by using the core System.Drawing and System.Windows.Forms imports from the System.Activities package.
  
</details>

<details>
  <summary>
    <b>So Many Entry Points</b>
  </summary>
  
  Entry points map to 'Modules' within your automation design. Entry Points allow you to create multiple processes from a single package, simplifying deployment, version control/git, and making maintenance easier by being able to share workflows between entry points (and in the future C# source code files as well).
  
  The idea is that all your code for a particular automation (end to end) should be within the same package. The one downside to this is that it makes the package larger and memory constraints may have to be taken into account, however, this is mitigated significantly due to the improvements in UiPath's compiler and runtime, where only the dependencies and code for a particular entry point are loaded into memory.
    
</details>

<details>
  <summary>
    <b>Templates, Templates, Templates</b>
  </summary>
    You'll notice that there are no entry points defined within the project when you first open it. This is because this is an all-purpose template and leverages the .template folder of a project to do so. The project template should be able to be able to support all sorts of combinations of modules into a single project, because you will occassionally have a more complex design than 1 Dispatcher, 1 Performer, 1 Reporter. Currently, within this project, there are templates for the below modules:

  1. Dispatcher
  2. Performer
  3. Reporter
  4. Data

  This lets us be able to customize the project depending on the design. Do you need multiple dispatchers because you need to look at different sources of input at different schedules? Just copy a Dispatcher template as needed. Do you have multiple units of work for this automation and require multiple queues and performers? Just copy a Performer subfolder into your root directory as needed. Do you need a tasker in between different modules of the automation? No problem, just copy the folders as needed. Maybe some DU Extraction stuff?

  The idea is to have a modular template that can accommodate a large variety of designs, instead of having to create a completely different project.

  Another amazing benefit is that it uncouples the adoption of a module template from adoption of the project template. Don't like a module that someone created? Cool, just don't use it. This also reduces the barrier for people to contribute to the template as well as adopt other's contributions because it is low-risk.
  
</details>

<details>
  <summary>
    <b>VB vs C#</b>
  </summary>
  
  > "Going forward, we do not plan to evolve Visual Basic as a language," the .NET team said. "This supports language stability and maintains compatibility between the .NET Core and .NET Framework versions of Visual Basic. Future features of .NET Core that require language changes may not be supported in Visual Basic. Due to differences in the platform, there will be some differences between Visual Basic on .NET Framework and .NET Core."
> 
> \- Microsoft, 2020 ([source](https://visualstudiomagazine.com/articles/2020/03/12/vb-in-net-5.aspx))

Continuing to code in VB would be just poor planning for the future, and after 1 or 2 processes using C#, you'll realize how much easier and cleaner C# is.

It also allows you to get familiar with a language that's used across the industry for other development scenarios like web front end, web back end, desktop applications, etc., instead of something that's almost exclusively used for Excel Macros. 

Do you not like job security?

</details>

<hr />

# Usage
This is a Project Template, meaning that after you clone this repo, you will have to publish, and then create a new project from the published template before coding for your automation.

Once your Project is set up, you essentially just copy modules from the .templates folder into the root directory of the project based on your needs. This saves you some time instead of manually creating those workflows from the templates through Studio one by one. 

Once you've copied a module, you will need to do is update all of the Invoke Workflow activities within the module to the path you copied it to. This is because they currently point to the files in the .templates folder, and the .templates folder shouldn't really be modified unless you want to make a change to the template.

Next, copy any Data templates you may need for your modules including Config files, template files, and update values to your development environment (Assets, Paths, etc.) and voila!

You should be good to go.

<hr />

# Module Templates
A description of the module templates that are currently available and planned to be supported.

<details>
  <summary>
    <b>Dispatchers</b>
  </summary>
  Dispatchers are workflows designed to read data from sources and add them to the Orchestrator Queue. Included OOB:

  1. <details>
      <summary>
        <b>BasicDispatcher</b>
      </summary>
      A basic dispatcher template that's essentially a sequence with a try-catch around it that sends an email when any exceptions occur.
     </details>
  2. <details>
      <summary>
        <b>ApplicationDispatcher</b>
      </summary>
      A more complex dispatcher designed for when you need to do steps within an application to collect information in order to add to the queue. Useful for scenarios where you read a table, and iterate through it, get additional information for each row, and then add it to the queue. This is because it provides exception handling at the 'Transaction' level so that errors processing particular rows do not impact the entire dispatcher.
     </details>

</details>

<details>
  <summary>
    <b>Performers</b>
  </summary>
  Performers are workflows designed to read data from queue items and perform tasks, typically within an application. Included OOB:

  1. <details>
       <summary>
         <b>BasicPerformer</b>
       </summary>
       A basic performer template that's has the same overall design as the REFramework, but addresses the concerns listen in the Methodology section above.
     </details>
  2. <details>
       <summary>
         <b>REFramework</b>
       </summary>
       The REFramework as you know and love. Here in case you are inclined to continue using it. Not recommended though.
     </details>
  3. <details>
       <summary>
         <b>BasicTasker (TBD)</b>
       </summary>
       A framework for a persistent process that creates an Action Center task, suspends until it is completed, and then parses the response and forwards the data to the next queue.
     </details>
  4. <details>
       <summary>
         <b>PostExceptionStepsPerformer (TBD)</b>
       </summary>
       An extension of the basic performer that has logic built in to handle and conduct steps after a business or application exception has been identified.
     </details>
  5. <details>
       <summary>
         <b>Extractor (TBD)</b>
       </summary>
       A framework for using DU to extract data from documents. I don't really know DU, so I don't know what else to put here.
     </details>
  6. <details>
       <summary>
         <b>Classifier (TBD)</b>
       </summary>
       A framework for using DU to classify documents. I don't really know DU, so I don't know what else to put here.
     </details>
</details>

<details>
  <summary>
    <b>Reporter</b>
  </summary>
  Reporters are workflows designed to read the transaction data and report on how the bot performed. Included OOB:

  1. <details>
       <summary>
         <b>BasicReporter</b>
       </summary>
       A basic reporting template that uses the Orchestrator OData API to load queue data and write to an excel template. The template has some built in visualizations as well with a pivot table/chart. It uses a CRON expression argument to be able determine the reporting period, as well as a built in overload in case you want to specify the reporting range yourself. Lastly, it sends and email with a summary of the outcomes and attaches the excel file created.
     </details>
  2. <details>
       <summary>
         <b>PowerQueryReporter</b>
       </summary>
       This is an excel file that uses the built in Power Query capabilities to connect to the Orchestrator as a built-in connection. This provides the same visualizations, and increases the scope to the entire Orchestrator, instead of a single queue/folder.
     </details>
</details>

<details>
  <summary>
    <b>Data</b>
  </summary>
  There are a variety of data resources available as a template. Included OOB:

  1. <details>
       <summary>
         <b>Configs</b>
       </summary>
       There is a config file available for each of the out of the box dispatchers and performers.
     </details>
  2. <details>
       <summary>
         <b>Templates</b>
       </summary>
       The templates folder includes mainly some .html and .txt files that contain the subject and body of the emails to send across various module templates.
     </details>
</details>

<hr />

# Roadmap
This is used to keep track of features to be implemented and what has been accomplished so far. This will eventually be replaced with a change log/release notes once the framework is fully functional.

### Modules
- [x] Create BasicDispatcher - Yash Brahmbhatt 7/7/2023
- [x] Create BasicPerformer - Yash Brahmbhatt 12/7/2023
- [x] Create BasicReporter - Yash Brahmbhatt 15/7/2023
- [x] Create Excel PowerQuery Reporter - Yash Brahmbhatt 15/7/2023
- [x] Import REFramework - Yash brahmbhatt 16/7/2023
- [x] Add Tests for all modules so far - Yash Brahmbhatt 19/07/2023
- [x] Add Auto-Documentation Module - Yash Brahmbhatt 18/07/2023
- [x] Add Support for Mermaid Diagrams in AutoDocs - Yash Brahmbhatt 21-07-2023 * Not all activities are currently supported.
- [x] Create ApplicationDispatcher - Yash brahmbhatt 23/017/2023
- [x] <strike>Create ExcelDispatcher</strike> - Removed from department because LoadConfig supports Excel Files. Yash brahmbhatt 23/017/2023
- [x] <strike>Create FileDispatcher</strike> - Removed from department because LoadConfig supports Text Files. Yash brahmbhatt 23/017/2023
- [ ] Create Tasker
- [ ] Create PostExceptionStepsPerformer
- [ ] Create Extractor
- [ ] Create Classifier

### Framework Changes
- [x] Add a 'Mapping' sheet to the Configs that reads an excel file into a DataSet with each sheet being a named DataTable within it. - Yash Brahmbhatt 24/07/2023
- [x] Add a Setup.xaml workflow that helps with initial setup of the modules. - Yash Brahmbhatt 24/07/2023

<br />

#### Legend
\* asterisks mark items that are still being considered.

\* bolded items are the next priority.

<hr />

# Contributing and Troubleshooting
Feel free to fork and create a pull request for any changes or additions you would like to make! I will eventually get around to reviewing it, I promise :smile:.

If you just have an idea, please create an 'Issue' here on GitHub and we can figure it out.

If you are having trouble understanding/using a module, or come across a bug, please also create an 'Issue' here on GitHub!


