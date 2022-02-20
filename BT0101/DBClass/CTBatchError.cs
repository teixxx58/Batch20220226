using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT0101.DBClass
{
    public class CTBatchError
    {
        private string _batchCd;      // 固定値のため："BT0101"
        private string _errorKbn;       // エラーが場合01、警告場合02
        private string _errorTitle;     // エラー内容
        private string _errorDetail;    // エラー、警告内容
        private DateTime _startDt;      // バッチの実行開始日時
        private DateTime _endDt;        // バッチの実行終了日時
        private int? _insertUserId;
        private int? _updateUserId;

        // InsertとUpdate時の現在時刻はSQLステートメントに取得のため
        //private DateTime _insertDt;
        //private DateTime _updateDt;

        public string batchCd
        {
            get { return "BT0101"; }
            set { _batchCd = value; }
        }
       public string errorKbn
        {
            get { return _errorKbn; }
            set { _errorKbn = value; }
        }
        public string errorTitle
        {
            get { return _errorTitle; }
            set { _errorTitle = value; }
        }
        public string errorDetail
        {
            get { return _errorDetail; }
            set { _errorDetail = value; }
        }
        public DateTime startDt
        {
            get { return _startDt; }
            set { _startDt = value; }
        }
        public DateTime endDt
        {
            get { return _endDt; }
            set { _endDt = value; }
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
