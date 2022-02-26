using System;

namespace BT0301.DBClass
{


    class CTCreateTemplate
    {
        public int createTemplate_id { get; set; }
        public int wireListPubNoId { get; set; }
        public string statusCd { get; set; }
        public string title { get; set; }
        public string memo { get; set; }
        public DateTime startDt { get; set; }
        public DateTime endDt { get; set; }
        public int? insertUserId { get; set; }
        public DateTime insertDt { get; set; }
        public int? updateUserId { get; set; }
        public DateTime? updateDt { get; set; }
    }
  
}
