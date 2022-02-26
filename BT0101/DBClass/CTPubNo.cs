namespace BT0101.DBClass
{
    class CTPubNo
    {
        private int _pubNoId;               // PubNoId
        private string _pubNo;              // PubNo
        private int _importVersion;         // 取込バージョン
        private int _oldVersionFlg;         // 旧バージョンフラグ
        private string _carModel;           // 車両
        private string _destination;        // 仕向け
        private string _task;               // 残存課題
        private string _fiscalYear;         // 作成年度
        private string _brand;              // ブランド
        private string _engine;             // エンジン
        private string _mission;            // ミッション
        private int? _insertUserId;     
        private int? _updateUserId;     

        public int pubNoId
        {
            get { return _pubNoId; }
            set { _pubNoId = value; }
        }
        public string pubNo
        {
            get { return _pubNo; }
            set { _pubNo = value; }
        }
        public int importVersion
        {
            get { return _importVersion; }
            set { _importVersion = value; }
        }
        public int oldVersionFlg
        {
            get { return _oldVersionFlg; }
            set { _oldVersionFlg = value; }
        }
        public string carModel
        {
            get { return _carModel; }
            set { _carModel = value; }
        }
        public string destination
        {
            get { return _destination; }
            set { _destination = value; }
        }
        public string task
        {
            get { return _task; }
            set { _task = value; }
        }
        public string fiscalYear
        {
            get { return _fiscalYear; }
            set { _fiscalYear = value; }
        }
        public string brand
        {
            get { return _brand; }
            set { _brand = value; }
        }
        public string engine
        {
            get { return _engine; }
            set { _engine = value; }
        }
        public string mission
        {
            get { return _mission; }
            set { _mission = value; }
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
