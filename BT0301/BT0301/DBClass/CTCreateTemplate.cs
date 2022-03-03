using System;

namespace BT0301.DBClass
{


    class CTCreateTemplate
    {
        private int _createTemplateId;
        private int _wireListPubNoId;
        private string _statusCd;
        private string _title;
        private string _memo;
        private DateTime _startDt;
        private DateTime _endDt;
        private int? _insertUserId;
        private DateTime _insertDt;
        private int? _updateUserId;
        private DateTime? _updateDt;

        public int createTemplateId { get => _createTemplateId; set => _createTemplateId = value; }
        public int wireListPubNoId { get => _wireListPubNoId; set => _wireListPubNoId = value; }
        public string statusCd { get => _statusCd; set => _statusCd = value; }
        public string title { get => _title; set => _title = value; }
        public string memo { get => _memo; set => _memo = value; }
        public DateTime startDt { get => _startDt; set => _startDt = value; }
        public DateTime endDt { get => _endDt; set => _endDt = value; }
        public int? insertUserId { get => _insertUserId; set => _insertUserId = value; }
        public DateTime insertDt { get => _insertDt; set => _insertDt = value; }
        public int? updateUserId { get => _updateUserId; set => _updateUserId = value; }
        public DateTime? updateDt { get => _updateDt; set => _updateDt = value; }

    }

}
