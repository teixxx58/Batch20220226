using BT0101.DBClass;
using BT0101Batch;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BT0101.FileClass
{
    class CFSVG : CFBase
    {
        public override void ImportFiles()
        {

        }
         /// <summary>
        /// 配線図コードの搔き集め
        /// </summary>
        /// <param name="svgFn"></param>
        /// <returns>配線図のコード、名称</returns>
        private Dictionary<string, string> GetSvgFigCodes(string svgFn)
        {
            const string ns = @"http://www.w3.org/2000/svg";
            const string gEl = @"{" + ns + "}g";
            const string aEl = @"{" + ns + "}a";
            const string titleEl = @"{" + ns + "}title";

            // ファイルから読み込む
            string svg = FileManager.localDataDir + PUBNO + FileManager.svgDir + svgFn + ".svg";
            Dictionary<string, string> svgCodeDic = new Dictionary<string, string>();
            try
            {
                XDocument xdoc = XDocument.Load(svg);
                XElement rootElements = xdoc.Root;
                // "svg/g/a/title"
                IEnumerable<XElement> nodes = from el in rootElements.Elements(gEl) select el;
                foreach (var node in nodes)
                {
                    IEnumerable<XElement> childNodes = from el in node.Elements(aEl) select el;
                    foreach (var childNode in childNodes)
                    {
                        // タイトル取得
                        IEnumerable<XElement> titleNodes = from el in childNode.Elements(titleEl) select el;
                        string title = null;
                        foreach (var titleNode in titleNodes)
                        {
                            title = titleNode.Value;
                        }

                        // childNodeの属性取得
                        IEnumerable<XAttribute> attList = from at in childNode.Attributes() select at;
                        foreach (XAttribute att in attList)
                        {
                            if (att.Name.ToString().Contains("code"))
                            {
                                //コード搔き集める
                                char[] delimiterChars = { ',' };
                                string[] codes = att.Value.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);

                                foreach (string code in codes)
                                {
                                    //重複判定
                                    if (!svgCodeDic.Keys.Contains(code))
                                    {
                                        svgCodeDic.Add(code, title);
                                    }

                                }
                            }
                        }
                    }
                }
                // 一つsvgファイルのコード抽出処理終了
                // コードリストを重複抜き、辞書に追加する
                svgCodeDic.Distinct();
                return svgCodeDic;
            }
            catch (Exception ex)
            {
                CLogger.Err(ex);
                BatchBase.AppendErrMsg("ERR_FILE_READ_FAILED", svg);
                throw ex;
            }
        }

        public void ImportPartsData(string figName)
        {

            /// 部品データ抽出
            /// T_PARTSのデータの追加を実施

            /// コード(パターン1)：svg
            /// （カンマ区切りアリの場合、それぞれ）
            Dictionary<string, string> svgFigCodes = GetSvgFigCodes(figName);

            /// コード(パターン2)：csv
            /// （二つ列があるため、それぞれ）
            CFCSV csvObj = new CFCSV();
            Dictionary<string, List<string>> csvCodeDicDic = csvObj.GetCsvFigCodes(figName);

            /// コード(パターン1、２)SVGのコードにマージる
            foreach (string code in csvCodeDicDic["CSVCODE"])
            {
                if (!svgFigCodes.Keys.Contains(code))
                {
                    //csv集めたコードの名称は？？仕様確認する？？
                    svgFigCodes.Add(code, "ダミー名称");
                }
            }

            // svgCodeListDic と csvCodeDicDic コードごとに
            /// 部品名称(W/H図面)：{PUB_NO}-Parts.xls
            /// 品名コード：parts.xml
            /// コネクタ品番：{PUB_NO}-Parts.xls
            /// ワイヤーネームリスト：{PUB_NO}-WireNameList.xls
            /// を取得する。
            /// 
            CFXLS xls = new CFXLS();
            CFXML xml = new CFXML();


            //コードを軸にParts取得。(パターン1,2の両方で取得)
            List<CTParts> partsList = new List<CTParts>();
            foreach (string code in svgFigCodes.Keys)
            {
                CTParts partsRec = new CTParts();
                // 配線図ID
                partsRec.wiringDiagramId = WIRING_DIAGRAM_ID;
                // <コード,部品名称(正式名)>
                partsRec.newPartsCd = code;
                partsRec.partsName = svgFigCodes[code];

                partsList.Add(partsRec);
            }
            // <コード,ワイヤーネームリスト>
            xls.SetWireNameList(partsList);
            // <コード,コネクタ品番>
            xls.SetPartsNameHinnban(partsList);
            //<品名コード>
            xml.SetHinmeiCodes(partsList);


            // DB登録
            executeImportPartsData(partsList);

        }
        /// <summary>
        /// パーツ更新
        /// </summary>
        /// <param name="parts"></param>
        /// <returns></returns>
        private int executeImportPartsData(List<CTParts> parts)
        {
            var partsValidator = ValidationFactory.CreateValidator<CTParts>();
            foreach (CTParts rec in parts)
            {
                try
                {
                    // 画像名称に応じる配線図ID取得
/*                    Hashtable selectParams = new Hashtable
                    {
                        { "pubNoId", CFBase.PUBNO_ID },
                        { "figName", CFBase.FIG_NAME },
                    };*/
                    //var id = mapper.QueryForObject<int>("SelectWiringDiagramId", selectParams);

                    rec.wiringDiagramId = CFBase.WIRING_DIAGRAM_ID;
                    var partsValidateResult = partsValidator.Validate(rec);
                    if (!partsValidateResult.IsValid)
                    {
                        foreach (var partsValidationResult in partsValidateResult)
                        {
                            //ログ出力
                            CLogger.Logger(partsValidationResult.Message);
                            BatchBase.AppendErrMsg("Validate_Hannyou", partsValidationResult.Message);
                        }
                        continue;
                    }

                    //SQLの実行
                    mapper.Insert("InsertParts", rec);

                }
                catch (Exception ex)
                {
                    BatchBase.AppendErrMsg("WNG_DB_FAILED", "パーツコード:" + rec.newPartsCd);
                    CLogger.Err(ex);
                }
            }
            return 0;
        }
    }
}
