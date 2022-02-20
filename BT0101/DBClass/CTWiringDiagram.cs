using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT0101.DBClass
{
    class CTWiringDiagram
    {
        private int _wiringDiagramId;
        private int _pubNoId;                          // PUB_NOの登録で登録したレコードのPUB_NO_ID
        private string _figName;                       // fig名称
        private string _systemTitle;                   // システムタイトル
        private string _carModelSpecification;         // 車両仕様
        private string _wiringSpecificationsNo;        // 配線仕様書No
        private string _platform;                      // プラットフォーム
        private int? _insertUserId;
        private int? _updateUserId;

        public int wiringDiagramId
        {
            get { return _wiringDiagramId; }
            set { _wiringDiagramId = value; }
        }
        public int pubNoId
        {
            get { return _pubNoId; }
            set { _pubNoId = value; }
        }
        public string figName
        {
            get { return _figName; }
            set { _figName = value; }
        }
        public string systemTitle
        {
            get { return _systemTitle; }
            set { _systemTitle = value; }
        }
        public string carModelSpecification
        {
            get { return _carModelSpecification; }
            set { _carModelSpecification = value; }
        }
        public string wiringSpecificationsNo
        {
            get { return _wiringSpecificationsNo; }
            set { _wiringSpecificationsNo = value; }
        }
        public string platform
        {
            get { return _platform; }
            set { _platform = value; }
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
        public override string ToString()
        {
            return string.Format("pubNoId:[{0}]," +
                "figName:[{1}]," +
                "systemTitle:[{2}]," +
                "carModelSpecification:[{3}]," +
                "wiringSpecificationsNo:[{4}]," +
                "platform:[{5}],",
                pubNoId,
                figName,
                systemTitle,
                carModelSpecification,
                wiringSpecificationsNo,
                platform
                );
        }
    }
}
