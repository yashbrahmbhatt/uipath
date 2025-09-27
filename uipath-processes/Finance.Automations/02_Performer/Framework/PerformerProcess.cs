using System.Linq;
using Finance.Automations.CodedWorkflows;
using Finance.Automations.Configs;
using Finance.Automations.Prompts;
using Newtonsoft.Json;
using UiPath.CodedWorkflows;
using UiPath.Core;


namespace Finance.Automations._02_Performer.Framework
{
    public class PerformerProcess : BaseCodedWorkflow
    {
        [Workflow]
        public string Execute(
            SharedConfig config_Shared,
            PerformerConfig config_Performer,
            QueueItem transaction
            )
        {
            var current = system.GetAsset(config_Performer.TagsAssetName, config_Performer.TagsAssetFolder).ToString();
            var oldTags = current.Split(",").ToList();
            var template = JsonConvert.DeserializeObject<PromptTemplate>(config_Shared.Prompt_ClassifyTransaction);
            foreach (var key in transaction.SpecificContent.Keys)
            {
                template.Instructions = template.Instructions.Replace("{" + key + "}", transaction.SpecificContent[key]?.ToString() ?? "");
                template.Prompt = template.Prompt.Replace("{" + key + "}", transaction.SpecificContent[key]?.ToString() ?? "");
            }
            template.Prompt = "[" + template.Prompt.Replace("{AvailableTags}", string.Join(", ", oldTags)) + "]";
            ClassifyTransactionPromptResponse response = JsonConvert.DeserializeObject<ClassifyTransactionPromptResponse>(workflows.GenerateChatCompletionProxy(template.Instructions, template.Prompt));

            if (response.NewTags.Count > 0)
            {

                var newValue = string.Join(",", oldTags.Concat(response.NewTags));
                system.SetAsset(newValue, config_Performer.TagsAssetName, config_Performer.TagsAssetFolder, default);
            }
            return JsonConvert.SerializeObject(response);
        }
    }




}