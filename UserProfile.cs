// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.BotBuilderSamples
{
    using System.Collections.Generic;

    /// <summary>Contains information about a user.</summary>
    public class UserProfile
    {

        //public string name { get; set; }
        public string name { get; set; }

        public string unum { get; set; }
        public int qnum { get; set; }  //푼 문제

        public string answer { get; set; }
        public float attendence { get; set; }
        // The list of companies the user wants to review.
        public List<string> CompaniesToReview { get; set; } = new List<string>();
    }
}
