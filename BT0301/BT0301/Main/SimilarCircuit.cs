using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT0301Batch
{
    class SimilarCircuit
    {
        private IList<Hashtable> assignedWires = new List<Hashtable>();
        public const string ASSING_FLG_TRUE = "1";
        public const string ASSING_FLG_FALSE = "0";

        /// <summary>
        /// 経線の割当
        /// </summary>
        /// <param name="wireList"></param>
        /// <returns></returns>
        private IList<Hashtable> AssignWireId(IList<Hashtable> wireList)
        {
            foreach (Hashtable item in wireList)
            {
                item.Add("assingFlg", "");
                if (CompareWireId(item))
                {
                    item["assingFlg"]= ASSING_FLG_TRUE;
                }
                else
                {
                    item["assingFlg"] = ASSING_FLG_FALSE;
                }
                //割り当て完了分
                assignedWires.Add(item);
            }
            return wireList;
        }

        /// <summary>
        /// 割当可否の判定
        /// </summary>
        /// <param name="wire"></param>
        /// <returns></returns>
        private bool CompareWireId(Hashtable wire)
        {

            foreach (Hashtable item in assignedWires)
            {
                //詳細リスト経線IDと旧経線IDのいずれかすでに割当った場合、再割当不可
                //類似度高い順で順次に割り当ていく
                if ((item["wire_list_detail_id"].ToString().Equals(wire["wire_list_detail_id"].ToString()) ||
                         item["wire_id"].ToString().Equals(wire["wire_id"].ToString()))
                         && ASSING_FLG_TRUE.Equals(item["assingFlg"]))
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 割り当てた朱書き・追加経線情報の取得
        /// </summary>
        /// <param name="wireList"></param>
        /// <returns></returns>
        public Dictionary<string, Dictionary<string, List<Hashtable>>> GetSyugakiWireids(List<Hashtable> wireList)
        {
            IList<Hashtable> assignWire = AssignWireId(wireList);

            Dictionary<string, Dictionary<string, List<Hashtable>>> figs = new Dictionary<string, Dictionary<string, List<Hashtable>>>();
            foreach (Hashtable wire in assignWire)
            {
                /// 朱書き経線              
                Dictionary<string, List<Hashtable>> dicSyugaki = new Dictionary<string, List<Hashtable>>();
                //追加結線
                Dictionary<string, List<Hashtable>> dicAddWires = new Dictionary<string, List<Hashtable>>();
                if (ASSING_FLG_TRUE.Equals(wire["assingFlg"].ToString()))
                {
                    if (!dicSyugaki.Keys.Contains(wire["fig_name"].ToString()))
                    {
                        List<Hashtable> syugaki = new List<Hashtable>();
                        syugaki.Add(wire);
                        dicSyugaki.Add(wire["fig_name"].ToString(), syugaki);
                    }
                    else
                    {
                        dicSyugaki[wire["fig_name"].ToString()].Add(wire);
                    }
                }
                /// 追加結線のファイルを作成する
                else
                {
                    if (!dicAddWires.Keys.Contains(wire["fig_name"].ToString()))
                    {
                        List<Hashtable> addwires = new List<Hashtable>();
                        addwires.Add(wire);
                        dicAddWires.Add(wire["fig_name"].ToString(), addwires);
                    }
                    else
                    {
                        dicAddWires[wire["fig_name"].ToString()].Add(wire);
                    }
                }
                figs.Add("SYUGAKI", dicSyugaki);
                figs.Add("ADDWIRES", dicAddWires);
            }
            return figs;
        }
        /// <summary>
        /// 配線図ごとに朱書き
        /// </summary>
        /// <param name="fig"></param>
        public void EditSyugaki(Dictionary<string, List<Hashtable>> fig)
        {


        }

        /// <summary>
        /// 追加配線図(1ファイルに出力)
        /// </summary>
        /// <param name="fig"></param>
        public void AddFigDiagram(Dictionary<string, List<Hashtable>> fig)
        {


        }
        /// <summary>
        /// PDFファイル作成
        /// </summary>
        /// <param name="fig"></param>
        public void MakePDF(string svgPath)
        {


        }

    }
}
