using System;
using System.Collections.Generic;

namespace Warsim.API.Authentication.External
{
    public class GoogleDto
    {
        public string Id { get; set; }

        public IList<GoogleEmailDto> Emails { get; set; }

        public class GoogleEmailDto
        {
            public string Value { get; set; }

            public string Type { get; set; }
        }
    }
}