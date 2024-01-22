using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Libra.Contract.Models
{
    public class EmailMessageModel
    {
        [DataMember(Name = "from")]
        public string From { get; set; }

        [DataMember(Name = "to")]
        public string To { get; set; }
        [DataMember(Name = "subject")]
        public string Subject { get; set; }

        [DataMember(Name = "body")]
        public string Body { get; set; }

        [DataMember(Name = "Isbodyhtml")]
        public bool IsBodyHtml { get; set; }

        [DataMember(Name = "system")]
        public int SystemOid { get; set; }

        [DataMember(Name = "objectguid")]
        public Guid? ObjectGuid { get; set; }
        [DataMember(Name = "SubType")]
        public QueueItemSubtype? SubType { get; set; }
    }
}
