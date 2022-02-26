using System;
using BT0101Batch;
using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;

namespace BT0101.DBClass
{
    class CTDevelopmentCd
    {
        private int         _developmentCdId;
        private string      _developmentCd;        // 開発No.(号口No.)
        private string      _pubNo;                // Pub. No.
        private int?        _insertUserId;         // NULL
        private DateTime    _insertDt;             // 現在日時 ※INSERTの場合のみ
        private int?        _updateUserId;         // NULL
        private DateTime    _updateDt;             // 現在日時

        public int developmentCdId
        {
            get { return _developmentCdId; }
            set { _developmentCdId = value; }
        }

        [Name("開発No.(号口No.)")]
        [Required(ErrorMessageResourceName = "Validate_Required",
                               ErrorMessageResourceType = typeof(Messages))]
        [MaxLength(10, ErrorMessageResourceName = "Validate_MaxLength",
                              ErrorMessageResourceType = typeof(Messages))]
        public string developmentCd
        {
            get { return _developmentCd; }
            set { _developmentCd = value; }
        }
        [Name("Pub.No.")]
        [Required(ErrorMessageResourceName = "Validate_Required",
                              ErrorMessageResourceType = typeof(Messages))]
        public string pubNo
        {
            get { return _pubNo; }
            set { _pubNo = value; }
        }
        public DateTime InsertDt
        {
            get { return _insertDt; }
            set { _insertDt = value; }
        }
        public DateTime UpdateDt
        {
            get { return _updateDt; }
            set { _updateDt = value; }
        }
        public int? insertUserId
        {
            get { return _insertUserId; }
            set { _insertUserId = value; }
        }
        public int? updateUserId
        {
            get { return _updateUserId; }
            set { _updateUserId = value; }
        }
    }
}
