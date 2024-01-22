using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libra.Contract
{
    public enum QueueItemSubtype
    {
        // Email subtypes
        EmailWithoutAttachment = 400,
        EmailPolicyIssueCmtpl = 401,
        EmailPolicyIssueTravel = 402,
        EmailPolicyIssueTravelWithRulesAttachment = 403,
        EmailPolicyIssueCmtplSupplementYellow = 404,
        EmailPolicyIssueCmtplSupplementOrange = 405,
        EmailPolicyIssueWithAttachment = 406,
        EmailInvoiceWithAttachment = 407,
        EmailPolicyIssueProperty = 408,
        EmailPolicyIssueCascoCombi = 409,
    }
}
