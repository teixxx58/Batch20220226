namespace BT0301Batch
{
    public struct CTWireInfo
    {
        private int _wireID;
        private int _fromTerminalID;
        private int _toTerminalID;
        private int _svgLineID;                       // SVG結線ID

        private string _reverseFlag;
        private int _similarPoint;
        private string _wireColorDiffFlag;
        private string _wireColor;

        private string _fromTerminalName;             // 端子名称(from)
        private int _fromPinNo;
        private float _fromPointX;
        private float _fromPointY;
        private int _fromDirection;
        private string _fromPartsCode;                // 新記号(from)
        private string _fromPartsCodeDiffFlag;
        private string _fromPinNoDiffFlag;
        private string _fromTerminalNameDiffFlag;


        private string _toTerminalName;               // 端子名称(to)
        private int _toPinNo;
        private float _toPointX;
        private float _toPointY;
        private int _toDirection;
        private string _toPartsCode;                  // 新記号(to)
        private string _toPartsCodeDiffFlag;
        private string _toPinNoDiffFlag;
        private string _toTerminalNameDiffFlag;

        private string _path;

        public int wireID { get => _wireID; set => _wireID = value; }
        public int fromTerminalID { get => _fromTerminalID; set => _fromTerminalID = value; }
        public int toTerminalID { get => _toTerminalID; set => _toTerminalID = value; }
        public int svgLineID { get => _svgLineID; set => _svgLineID = value; }
        public string reverseFlag { get => _reverseFlag; set => _reverseFlag = value; }
        public int similarPoint { get => _similarPoint; set => _similarPoint = value; }
        public string wireColorDiffFlag { get => _wireColorDiffFlag; set => _wireColorDiffFlag = value; }
        public string wireColor { get => _wireColor; set => _wireColor = value; }
        public string fromTerminalName { get => _fromTerminalName; set => _fromTerminalName = value; }
        public int fromPinNo { get => _fromPinNo; set => _fromPinNo = value; }
        public float fromPointX { get => _fromPointX; set => _fromPointX = value; }
        public float fromPointY { get => _fromPointY; set => _fromPointY = value; }
        public int fromDirection { get => _fromDirection; set => _fromDirection = value; }
        public string fromPartsCode { get => _fromPartsCode; set => _fromPartsCode = value; }
        public string fromPartsCodeDiffFlag { get => _fromPartsCodeDiffFlag; set => _fromPartsCodeDiffFlag = value; }
        public string fromPinNoDiffFlag { get => _fromPinNoDiffFlag; set => _fromPinNoDiffFlag = value; }
        public string fromTerminalNameDiffFlag { get => _fromTerminalNameDiffFlag; set => _fromTerminalNameDiffFlag = value; }
        public string toTerminalName { get => _toTerminalName; set => _toTerminalName = value; }
        public int toPinNo { get => _toPinNo; set => _toPinNo = value; }
        public float toPointX { get => _toPointX; set => _toPointX = value; }
        public float toPointY { get => _toPointY; set => _toPointY = value; }
        public int toDirection { get => _toDirection; set => _toDirection = value; }
        public string toPartsCode { get => _toPartsCode; set => _toPartsCode = value; }
        public string toPartsCodeDiffFlag { get => _toPartsCodeDiffFlag; set => _toPartsCodeDiffFlag = value; }
        public string toPinNoDiffFlag { get => _toPinNoDiffFlag; set => _toPinNoDiffFlag = value; }
        public string toTerminalNameDiffFlag { get => _toTerminalNameDiffFlag; set => _toTerminalNameDiffFlag = value; }
        public string path { get => _path; set => _path = value; }
    }
}
