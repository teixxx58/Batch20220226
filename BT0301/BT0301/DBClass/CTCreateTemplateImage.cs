using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT0301Batch
{
    class CTCreateTemplateImage
    {
        public int createTemplateImageId { get; set; }
        public int createTemplateId { get; set; }
        public int similarCircuitSearchId { get; set; }
        public int imageForAddFlg { get; set; }
        public int useFlg { get; set; }
        public string createSvgFileName { get; set; }
        public string createPdfFileName { get; set; }
        public int? insertUserId { get; set; }
        public DateTime insertDt { get; set; }
        public int? updateUserId { get; set; }
        public DateTime updateDt { get; set; }
    }
}
