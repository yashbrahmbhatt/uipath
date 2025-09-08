using System;

namespace Finance.Automations.Prompts
{
    [Serializable]
    public class PromptTemplate {
        public string Instructions {get; set;}
        public string Prompt {get; set;}
        
        public PromptTemplate(){}
    }
}