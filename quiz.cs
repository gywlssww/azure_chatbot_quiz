namespace Microsoft.BotBuilderSamples
{
    using System.Collections.Generic;

    /// <summary>Contains information about a user.</summary>
    public class quiz
    {
        public string question { get; set; }
        public string answer { get; set; }
        
        public List<string> anwserlist { get; set; } = new List<string>();
    }
}
