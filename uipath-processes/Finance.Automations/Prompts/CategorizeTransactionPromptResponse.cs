using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Finance.Automations.Prompts
{
    /// <summary>
    /// Represents the response from the LLM for transaction classification.
    /// Matches the JSON output format defined in TransactionClassification.json.
    /// </summary>
    [Serializable]
    public class ClassifyTransactionPromptResponse 
    {
        /// <summary>
        /// Main business category (e.g., 'Food & Dining', 'Automotive & Gas', 'Bills & Utilities', 'Financial Services')
        /// </summary>
        [JsonProperty("PrimaryCategory")]
        public string PrimaryCategory { get; set; }

        /// <summary>
        /// Type of transaction (e.g., 'Debit Card Purchase', 'Payroll Deposit', 'Mortgage Payment', 'E-Transfer')
        /// </summary>
        [JsonProperty("TransactionType")]
        public string TransactionType { get; set; }

        /// <summary>
        /// Extracted merchant name from the transaction descriptions
        /// </summary>
        [JsonProperty("MerchantName")]
        public string MerchantName { get; set; }

        /// <summary>
        /// Array of selected tags from the available tags list
        /// </summary>
        [JsonProperty("SelectedTags")]
        public List<string> SelectedTags { get; set; } = new List<string>();

        /// <summary>
        /// Array of new tags suggested by the AI if existing tags are insufficient
        /// </summary>
        [JsonProperty("NewTags")]
        public List<string> NewTags { get; set; } = new List<string>();

        /// <summary>
        /// Currency type used in the transaction (CAD/USD/Foreign)
        /// </summary>
        [JsonProperty("CurrencyType")]
        public string CurrencyType { get; set; }

        /// <summary>
        /// Confidence level in the classification (High/Medium/Low)
        /// </summary>
        [JsonProperty("Confidence")]
        public string Confidence { get; set; }

        /// <summary>
        /// Brief explanation of the classification logic
        /// </summary>
        [JsonProperty("Reasoning")]
        public string Reasoning { get; set; }
    }
}