﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Altinn.Dan.Plugin.Trad.Models
{
    public record PracticeInternal
    {
        [JsonProperty("orgNumber")]
        public int OrganizationNumber { get; set; }

        [JsonProperty("auditExcempt")]
        public bool AuditExcempt { get; set; }

        [JsonProperty("authorizedRepresentatives", NullValueHandling = NullValueHandling.Ignore)]
        public List<PersonInternal> AuthorizedRepresentatives;

        [JsonProperty("isAuthorizedRepresentativeFor", NullValueHandling = NullValueHandling.Ignore)]
        public List<PersonInternal> IsAnAuthorizedRepresentativeFor;
    }
}
