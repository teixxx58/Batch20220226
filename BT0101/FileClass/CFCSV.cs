using BT0101.DBClass;
using BT0101Batch;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BT0101.FileClass
{
    class CFCSV : CFBase
    {
        // 端子・結線データ対象ファイル
        private List<string> codeFromList = new List<string>();
        private List<string> codeToList = new List<string>();
        private List<string> codeFromToList = new List<string>();
        private List<CsvFields> csvContents = new List<CsvFields>();

         /// <summary>
        /// ファイル読み込み
        /// </summary>
        /// <returns>CSVレコード</returns>
        private List<CsvFields> ReadCsvFile(string csvPath)
        {
            // ファイルから読み込む
            List<CsvFields> list = new List<CsvFields>();

            try
            {
                using (var reader = new StreamReader(csvPath, Encoding.GetEncoding("Shift_JIS")))
                {
                    const string comma = "(,)?";
                    const string equal = "(=)";
                    const string value = "(\".*?\")(,)?";
                    string pattern = comma + equal + value;
                    string seikei = Regex.Replace(reader.ReadToEnd(), pattern, "$1$3$4");
                    TextReader txtReader = new StringReader(seikei);
                    

                    using (var csv = new CsvReader(txtReader, new CultureInfo("ja-JP", false)))
                    {
                        csv.Read();
                        csv.ReadHeader();
                        // csv データが行毎に CFCSV クラスに格納され、IEnumerable<CFCSV> として
                        // records に割り当てられます。
                        var records = csv.GetRecords<CsvFields>();
                        var partsValidator = ValidationFactory.CreateValidator<CsvFields>();
                        foreach (var item in records)
                        {
                            var partsValidateResult = partsValidator.Validate(item);
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
                            list.Add(item);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                BatchBase.AppendErrMsg("ERR_FILE_READ_FAILED", csvPath);      // CSVファイルは　=""　が混ざっている。
                CLogger.Err(ex);
            }
            return list;
        }
        /// <summary>
        /// ファイル単位のコード搔き集める
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,  List<string>> GetCsvFigCodes(string figName)
        {
            string csvFilePath = FileManager.localDataDir + PUBNO + FileManager.syugakiCsvDir;
            string path = csvFilePath + figName + ".csv";

            // ファイルから読み込む
            List<CsvFields> records = ReadCsvFile(path);

            foreach (var item in records)
            {
                codeFromList.Add(item.codeFrom);
                codeToList.Add(item.codeTo);
                codeFromToList.Add(item.codeFrom);
                codeFromToList.Add(item.codeTo);

            }
            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
            dic.Add("FROM", codeFromList.Distinct().ToList());
            dic.Add("TO", codeToList.Distinct().ToList());
            dic.Add("CSVCODE", codeFromToList.Distinct().ToList());

            return dic;
        }
        /// <summary>
        /// T_TERMINAL&T_WIREの更新
        /// </summary>
        public void ImportTerminalWire(string figName)
        {
            string csvFilePath = FileManager.localDataDir + PUBNO + FileManager.syugakiCsvDir;
            string path = csvFilePath + figName + ".csv";

            List<CsvFields> reacords = ReadCsvFile(path);
            // 端子データ登録準備
            foreach (CsvFields csv in reacords)
            {
                int toId = -1;
                int fromId = -1;
                try
                {
                    //FROM
                    CTTerminal terminal = new CTTerminal();
                    if (!string.IsNullOrEmpty(csv.codeFrom))
                    {
                        Hashtable partIdFrom = GetPartsId(PUBNO, figName, PUBNO_ID, csv.codeFrom);
                        if (partIdFrom != null && !string.IsNullOrEmpty(partIdFrom["parts_id"].ToString()))
                        {
                            terminal.partsId = (int)partIdFrom["parts_id"];
                            terminal.terminalName = csv.terminalName;
                            terminal.pinNo = csv.pin;
                            terminal.pointX = string.IsNullOrEmpty(csv.potX) ? float.Parse("0.0") : float.Parse(csv.potX);
                            terminal.pointY = string.IsNullOrEmpty(csv.potY) ? float.Parse("0.0") : float.Parse(csv.potY);
                            terminal.direction = csv.direction;
                            terminal.insertUserId = null;
                            terminal.updateUserId = null;

                            Hashtable diag = mapper.QueryForObject<Hashtable>("SelectDiagCd", terminal.partsId);
                            if (diag != null)
                            {
                                terminal.diagCd = diag["diag_cd"].ToString();
                                terminal.controlName = diag["control_name"].ToString();
                                terminal.signalName = diag["signal_name"].ToString();
                            }
                            // FROM端子データ登録
                            mapper.Insert("InsertTerminal", terminal);
                            fromId =  mapper.QueryForObject<int>("SelectTeminalId", null);
                        }
                        //TO
                        CTTerminal terminalTo = new CTTerminal();
                        if (!string.IsNullOrEmpty(csv.codeTo))
                        {
                            Hashtable partIdTo = GetPartsId(PUBNO, figName, PUBNO_ID, csv.codeTo);
                            if (partIdTo != null && !string.IsNullOrEmpty(partIdTo["parts_id"].ToString()))
                            {
                                Hashtable partId = GetPartsId(PUBNO, figName, PUBNO_ID, csv.codeTo);

                                terminalTo.terminalName = csv.terminalNameTo;
                                terminalTo.pinNo = csv.pinTo;
                                terminalTo.pointX = string.IsNullOrEmpty(csv.potXTo) ? float.Parse("0.0") : float.Parse(csv.potXTo);
                                terminalTo.pointY = string.IsNullOrEmpty(csv.potYTo) ? float.Parse("0.0") : float.Parse(csv.potYTo);
                                terminalTo.direction = csv.directionTo;
                                terminalTo.insertUserId = null;
                                terminalTo.updateUserId = null;

                                Hashtable diag = mapper.QueryForObject<Hashtable>("SelectDiagCd", terminalTo.partsId);
                                if (diag != null)
                                {
                                    terminalTo.diagCd = diag["diag_cd"].ToString();
                                    terminalTo.controlName = diag["control_name"].ToString();
                                    terminalTo.signalName = diag["signal_name"].ToString();
                                }
                                mapper.Insert("InsertTerminal", terminalTo);
                                toId = mapper.QueryForObject<int>("SelectTeminalId",null);
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    BatchBase.AppendErrMsg("WNG_DB_FAILED", "コード（From："+ csv.codeFrom + ",To:" + csv.codeTo + csv.wireId, "端子");
                    CLogger.Err(ex);
                    //throw ex;
                }
                try
                    {

                    // 結線登録データ準備
                    if (toId != -1 && fromId != -1)
                    {
                        CTWire wire = new CTWire();
                        wire.fromTerminalId = fromId;
                        wire.toTerminalId = toId;
                        wire.svgLineId = csv.wireId;                     // 結線ID
                        wire.wireColor = csv.lineColor;                  // 結線色
                        wire.insertUserId = null;
                        wire.updateUserId = null;
                        // 結線データ登録
                        mapper.Insert("InsertWire", wire);
                        //int wireId = mapper.QueryForObject<int>("SelectWireId", null);
                    }

                }
                catch (Exception ex)
                {
                    BatchBase.AppendErrMsg("WNG_DB_FAILED", "結線ID:" + csv.wireId, "結線"); 
                    CLogger.Err(ex);
                    //throw ex;
                }
            }

        }

        /// <summary>
        /// 図面、コードからパーツIDを取得する
        /// </summary>
        /// <param name="pubNo"></param>
        /// <param name="figName"></param>
        /// <param name="pubNoId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        private Hashtable GetPartsId(string pubNo,string figName, int pubNoId, string code)
        {
            Hashtable selectParams = new Hashtable();
            selectParams.Add("pubNo" ,pubNo);
            selectParams.Add("figName", figName);
            selectParams.Add("pubNoId", pubNoId);
            selectParams.Add("code", code);

            Hashtable rslt = mapper.QueryForObject<Hashtable>("SelectPartsId", selectParams);
            return rslt;

        }


        public override void ImportFiles()
        {

        }

        /// <summary>
        /// 取得項目のマッピング
        /// </summary>
        public class CsvFields
        {
            [Index(0)]
            public int wireId { get; set; } //結線ID
            [Index(1)]
            public string syubetsu { get; set; }   // 種別
            [Index(2)]
            public string seibetsu { get; set; }   // 性別
            [Index(3)]
            [StringLength(10, MinimumLength = 1, ErrorMessageResourceName = "Validate_RangeLength", 
                ErrorMessageResourceType = typeof(Messages))]
            public string codeFrom { get; set; }   // コード
            [Index(4)]
            public string subcode { get; set; }    // サブコード
            [Index(5)]
            public string pin { get; set; }    // ピン
            [Index(6)]
            public string pinSpecification { get; set; }   // ピン仕様
            [Index(7)]
            public string jcPin { get; set; }  // JCピン
            [Index(8)]
            public string JcErea { get; set; } // JC領域
            [Index(9)]
            public string connectorSpecification { get; set; } // コネクタ仕様
            [Index(10)]
            public string terminalName { get; set; }   // 端子名称
            [Index(11)]
            [StringLength(10, MinimumLength = 1, ErrorMessageResourceName = "Validate_RangeLength", 
                ErrorMessageResourceType = typeof(Messages))]
            [RegularExpression("^[0-9]+.[0-9]+$", ErrorMessageResourceName = "Validate_Regx", 
                ErrorMessageResourceType = typeof(Messages))]
            public string potX { get; set; }   // 接続点X
            [Index(12)]
            [StringLength(10, MinimumLength = 1, ErrorMessageResourceName = "Validate_RangeLength", 
                ErrorMessageResourceType = typeof(Messages))]
            [RegularExpression("^[0-9]+.[0-9]+$", ErrorMessageResourceName = "Validate_Regx", 
                ErrorMessageResourceType = typeof(Messages))]
            public string potY { get; set; }   // 接続点Y
            [Index(13)]
            public string direction { get; set; }  // 接続方向
            [Index(14)]
            public string syubetsuTo { get; set; }   // 種別
            [Index(15)]
            public string seibetsuTo { get; set; }   // 性別
            [Index(16)]
            [StringLength(10, MinimumLength = 1, ErrorMessageResourceName = "Validate_RangeLength",
                ErrorMessageResourceType = typeof(Messages))]
            public string codeTo { get; set; }   // コード
            [Index(17)]
            public string subcodeTo { get; set; }    // サブコード
            [Index(18)]
            public string pinTo { get; set; }    // ピン
            [Index(19)]
            public string pinSpecificationTo { get; set; }   // ピン仕様
            [Index(20)]
            public string jcPinTo { get; set; }  // JCピン
            [Index(21)]
            public string JcEreaTo { get; set; } // JC領域
            [Index(22)]
            public string connectorSpecificationTo { get; set; } // コネクタ仕様
            [Index(23)]
            public string terminalNameTo { get; set; }   // 端子名称
            [Index(24)]
            [StringLength(10, MinimumLength = 1, ErrorMessageResourceName = "Validate_RangeLength", 
                ErrorMessageResourceType = typeof(Messages))]
            [RegularExpression("^[0-9]+.[0-9]+$", ErrorMessageResourceName = "Validate_Regx", 
                ErrorMessageResourceType = typeof(Messages))]
            public string potXTo { get; set; }   // 接続点X
            [Index(25)]
            [StringLength(10, MinimumLength = 1, ErrorMessageResourceName = "Validate_RangeLength",
                ErrorMessageResourceType = typeof(Messages))]
            [RegularExpression("^[0-9]+.[0-9]+$", ErrorMessageResourceName = "Validate_Regx",
                ErrorMessageResourceType = typeof(Messages))]
            public string potYTo { get; set; }   // 接続点Y
            [Index(26)]
            public string directionTo { get; set; }  // 接続方向
            [Index(27)]
            public string lineColor { get; set; }    // 線色
            [Index(28)]
            public string specifination { get; set; }   // 仕様
            [Index(29)]
            public string dupFlag { get; set; } // DupFlag
        }

    }
}
