using BT0101Batch;
using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;

namespace BT0101.DBClass
{
    class CTParts
    {
        private int _parts_id;
        private int _wiringDiagramId;             // 配線図ID
        private string _whPartsName;              // 部品名称(W/H図面)
        private string _partsName;                // 部品名称(正式名)
        private string _newPartsCd;               // 新記号
        private string _hinmeiCd;                 // 品名コード
        private string _hinban5;                  // 品番（頭5桁）
        private string _connectorHinban;          // コネクタ品番
        private string _wireNameList;             // ワイヤーネームリスト
        private int? _insertUserId;               // NULL
        private int? _updateUserId;               // NULL

        public int partsId
        {
            get { return _parts_id; }
            set { _parts_id = value; }
        }
        [Name("配線図ID")]
        [Required(ErrorMessageResourceName = "Validate_Required",
            ErrorMessageResourceType = typeof(Messages))]
        public int wiringDiagramId
        {
            get { return _wiringDiagramId; }
            set { _wiringDiagramId = value; }
        }
        [Name("部品名称(W/H図面)")]
        [Required(ErrorMessageResourceName = "Validate_Required", 
            ErrorMessageResourceType = typeof(Messages))]
        [MaxLength(255, ErrorMessageResourceName = "Validate_MaxLength", 
            ErrorMessageResourceType = typeof(Messages))]
        public string whPartsName
        {
            get { return _whPartsName; }
            set { _whPartsName = value; }
        }
        [Name("部品名称(正式名)")]
        [Required(ErrorMessageResourceName = "Validate_Required", 
            ErrorMessageResourceType = typeof(Messages))]
        [MaxLength(255, ErrorMessageResourceName = "Validate_MaxLength", 
            ErrorMessageResourceType = typeof(Messages))]
        public string partsName
        {
            get { return _partsName; }
            set { _partsName = value; }
        }
        [Name("新記号")]
        [Required(ErrorMessageResourceName = "Validate_Required",
            ErrorMessageResourceType = typeof(Messages))]
        [MaxLength(10, ErrorMessageResourceName = "Validate_MaxLength",
            ErrorMessageResourceType = typeof(Messages))]
        public string newPartsCd
        {
            get { return _newPartsCd; }
            set { _newPartsCd = value; }
        }
        [Name("品名コード")]
        [MaxLength(30, ErrorMessageResourceName = "Validate_MaxLength",
            ErrorMessageResourceType = typeof(Messages))]
       public string hinmeiCd
        {
            get { return _hinmeiCd; }
            set { _hinmeiCd = value; }
        }
        [Name("品番（頭5桁）")]
        [MaxLength(5, ErrorMessageResourceName = "Validate_MaxLength", 
            ErrorMessageResourceType = typeof(Messages))]
        public string hinban5
        {
            get { return _hinban5; }
            set { _hinban5 = value; }
        }
        [Name("コネクタ品番")]
        [MaxLength(30, ErrorMessageResourceName = "Validate_MaxLength",
            ErrorMessageResourceType = typeof(Messages))]
        public string connectorHinban
        {
            get { return _connectorHinban; }
            set { _connectorHinban = value; }
        }
        [Name("ワイヤーネームリスト")]
        [MaxLength(30, ErrorMessageResourceName = "Validate_MaxLength", 
            ErrorMessageResourceType = typeof(Messages))]
        public string wireNameList
        {
            get { return _wireNameList; }
            set { _wireNameList = value; }
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
