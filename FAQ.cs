namespace Microsoft.BotBuilderSamples
{
    using System.Collections.Generic;

    /// <summary>Contains information about a user.</summary>
    public class FAQ
    {
        public int uid { get; set; }
        public string question { get; set; }
        public string answer { get; set; }
        // The list of companies the user wants to review.
    }
}
