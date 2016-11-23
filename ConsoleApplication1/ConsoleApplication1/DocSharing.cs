using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YeeOffice.SocketServer.UDBContext.Model.DBModel
{
    [Table("yeeoffice_myworkspace_doc_sharing")]
    public class DocSharing
    {
        [Key]
        public int ID { get; set; }
        public string YGDocID { get; set; }
        public string YGDocName { get; set; }
        public string YGWebURL { get; set; }
        public string YGDocURL { get; set; }
        public string YGDocEditURL { get; set; }
        public DateTime BeginCreated { get; set; }
        public DateTime LastCreated { get; set; }
        public string CreatByUserName { get; set; }
        public string ShareToUserName { get; set; }
        public string ShareRole { get; set; }

        public string YGDocSize { get; set; }
        public bool Effective { get; set; }
        public string Describe { get; set; }
        public string ShareToUserDomainAccount { get; set; }

    }
}
