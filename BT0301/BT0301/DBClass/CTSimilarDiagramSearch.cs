using System;

namespace BT0301.DBClass
{
    class CTSimilarDiagramSearch
    {
        public int similarDiagramSearchId { get; set; }
        public int wireListPubNoId { get; set; }
        public int PUB_NO { get; set; }
        public int importVersion { get; set; }
        public string FigName { get; set; }
        public string statusCd { get; set; }
        public int similarPoint { get; set; }
        public float similarParcent { get; set; }
        public DateTime startDt { get; set; }
        public DateTime? endDt { get; set; }
        public int? insertUserId { get; set; }
        public DateTime insertDt { get; set; }
        public int? updateUserId { get; set; }
        public DateTime? updateDt { get; set; }
    }
}
