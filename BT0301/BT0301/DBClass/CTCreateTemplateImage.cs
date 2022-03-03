using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT0301Batch
{
    class CTCreateTemplateImage
    {
        private int _createTemplateImageId;
        private int _createTemplateId;
        private int _similarCircuitSearchId;
        private int _imageForAddFlg;
        private int _useFlg;
        private string _createSvgFileName;
        private string _createPdfFileName;
        private int? _insertUserId;
        private DateTime _insertDt;
        private int? _updateUserId;
        private DateTime _updateDt;

        public int createTemplateImageId { get => _createTemplateImageId; set => _createTemplateImageId = value; }
        public int createTemplateId { get => _createTemplateId; set => _createTemplateId = value; }
        public int similarCircuitSearchId { get => _similarCircuitSearchId; set => _similarCircuitSearchId = value; }
        public int imageForAddFlg { get => _imageForAddFlg; set => _imageForAddFlg = value; }
        public int useFlg { get => _useFlg; set => _useFlg = value; }
        public string createSvgFileName { get => _createSvgFileName; set => _createSvgFileName = value; }
        public string createPdfFileName { get => _createPdfFileName; set => _createPdfFileName = value; }
        public int? insertUserId { get => _insertUserId; set => _insertUserId = value; }
        public DateTime insertDt { get => _insertDt; set => _insertDt = value; }
        public int? updateUserId { get => _updateUserId; set => _updateUserId = value; }
        public DateTime updateDt { get => _updateDt; set => _updateDt = value; }
    }
}
