using System;

namespace BT0101.DBClass
{
    public class CTTerminal
    {
        private int _terminalId;
        private int _partsId;                   // 部品ID
        private string _terminalName;           // 端子名称
        private string _pinNo;                  // ピン番号
        private Single _pointX;                 // 接続点X
        private Single _pointY;                 // 接続点Y
        private string _direction;              // 接続方向
        private string _diagCd;                 // ダイアグコード
        private string _controlName;            // 制御名
        private string _signalName;             // 信号名
        private int? _insertUserId;
        private int? _updateUserId;

        public int terminalId
        {
            get { return _terminalId; }
            set { _terminalId = value; }
        }
        public int partsId
        {
            get { return _partsId; }
            set { _partsId = value; }
        }
       public string terminalName
        {
            get { return _terminalName; }
            set { _terminalName = value; }
        }
       public string pinNo
        {
            get { return _pinNo; }
            set { _pinNo = value; }
        }
        public Single pointX
        {
            get { return _pointX; }
            set { _pointX = value; }
        }
        public Single pointY
        {
            get { return _pointY; }
            set { _pointY = value; }
        }
       public string direction
        {
            get { return _direction; }
            set { _direction = value; }
        }
       public string diagCd
        {
            get { return _diagCd; }
            set { _diagCd = value; }
        }
       public string controlName
        {
            get { return _controlName; }
            set { _controlName = value; }
        }
       public string signalName
        {
            get { return _signalName; }
            set { _signalName = value; }
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
