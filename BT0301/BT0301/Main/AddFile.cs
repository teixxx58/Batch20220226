using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace BT0301Batch
{

    class AddFile
    {
        //TOPマージン
        double TOP_MARGIN = 100;
        //LEFTマージン
        double LEFT_MARGIN = 150;
        //経線間間隔
        double LINE_INTERVAL = 30;
        //経線の長さ
        double LINE_LENGTH = 40;
        //パーツの高さ
        double PARTS_HEIGHT = 55;
        //パーツの左右マージン
        double PARTS_LEFT_RIGHT_MARGIN = 20;

        private XmlDocument _xmlDoc;
        private string _fileName;


        public AddFile(string fileName)
        {
            try
            {
                _fileName = fileName;
                _xmlDoc = new XmlDocument();
                if (_xmlDoc != null)
                {
                    _xmlDoc.XmlResolver = null;
                }
                _xmlDoc.Load(_fileName);

            }
            catch (Exception ex)
            {
                CLogger.Err(ex);
                BatchBase.AppendErrMsg("ERR_FILE_READ_FAILED", fileName);
                //対象イメージ作成処理中止
                throw ex;
            }
        }
        /// <summary>
        /// svgファイル保存
        /// </summary>
        /// <param name="filePath"></param>
        public void SVGSave(string filePath)
        {
            if (_xmlDoc != null)
            {
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }
                _xmlDoc.Save(filePath); 
            }
        }
        /// <summary>
        /// 追加ファイル作成
        /// </summary>
        /// <param name="dicWires"></param>
        public void GenerateAddFigDiagramFile( Dictionary<string, List<Hashtable>> dicWires)
        {
            XmlNode svgRoot = _xmlDoc.DocumentElement;
            //テンプレート取得
            var fromPartsTemplate = (XmlElement)svgRoot.SelectSingleNode("//*[@name='fromPartsTemplate']");
            fromPartsTemplate.RemoveAttribute("name");
            var toPartsTemplate = (XmlElement)svgRoot.SelectSingleNode("//*[@name='toPartsTemplate']");
            toPartsTemplate.RemoveAttribute("name");
            var labelTemplate = (XmlElement)svgRoot.SelectSingleNode("//*[@name='labelTemplate']");
            labelTemplate.RemoveAttribute("name");
            var singleLineTemplate = (XmlElement)svgRoot.SelectSingleNode("//*[@name='singleLineTemplate']");
            singleLineTemplate.RemoveAttribute("name");
            var doubleLineTemplate = (XmlElement)svgRoot.SelectSingleNode("//*[@name='doubleLineTemplate']");
            doubleLineTemplate.RemoveAttribute("name");

            //SVGを空にする
            var svgChildren = svgRoot.SelectSingleNode("//*[@id='ewd_top']");
            svgChildren.RemoveAll();

            double X_LANE_POSITION = LEFT_MARGIN;
            double partsWidth = 0;

            //経線図作成
            foreach (string key in dicWires.Keys)
            {
                //経線本数
                int lineCnt = dicWires[key].Count;
                //パーツ幅
                partsWidth = 2 * PARTS_LEFT_RIGHT_MARGIN + (lineCnt - 1) * LINE_INTERVAL;

                //FROMパーツ
                //Fromパーツコード
                fromPartsTemplate.InnerXml = Regex.Replace(fromPartsTemplate.InnerXml, "#from_parts_code#", dicWires[key][0]["from_parts_code"].ToString());
                //FromパーツHW名称
                fromPartsTemplate.InnerXml = Regex.Replace(fromPartsTemplate.InnerXml, "#from_hw_parts_name#", dicWires[key][0]["from_wh_parts_name"].ToString());
                //Fromパーツ名称
                fromPartsTemplate.InnerXml = Regex.Replace(fromPartsTemplate.InnerXml, "#from_parts_name#", dicWires[key][0]["from_parts_name"].ToString());
                //Fromパーツ表示位置
                string fromDisplayPosition = "\" M  " + X_LANE_POSITION + ",  " + TOP_MARGIN + " L  " + (X_LANE_POSITION + partsWidth) + ",  " +
                    (TOP_MARGIN) + "  L  " + (X_LANE_POSITION + partsWidth) + ",  " + (TOP_MARGIN + PARTS_HEIGHT) + " L  " +
                    X_LANE_POSITION + ",  " + (TOP_MARGIN + PARTS_HEIGHT) + " L  " + X_LANE_POSITION + ",  " + TOP_MARGIN + "\"";

                fromPartsTemplate.InnerXml = Regex.Replace(fromPartsTemplate.InnerXml, "#partsDisplayPosition#", fromDisplayPosition);
                svgChildren.AppendChild(fromPartsTemplate.CloneNode(true));



                //TOパーツ
                //Toパーツコード
                toPartsTemplate.InnerXml = Regex.Replace(toPartsTemplate.InnerXml,
                    "#to_parts_code#", dicWires[key][0]["to_parts_code"].ToString());
                //FromパーツHW名称
                toPartsTemplate.InnerXml = Regex.Replace(toPartsTemplate.InnerXml, "#to_hw_parts_name#",
                    dicWires[key][0]["to_wh_parts_name"].ToString());
                //Fromパーツ名称
                toPartsTemplate.InnerXml = Regex.Replace(toPartsTemplate.InnerXml, "#to_parts_name#", 
                    dicWires[key][0]["to_parts_name"].ToString());
                //Fromパーツ表示位置
                string toDisplayPosition = "\" M  " + X_LANE_POSITION + ",  " + (TOP_MARGIN + PARTS_HEIGHT + LINE_LENGTH) + " L  " + (X_LANE_POSITION + partsWidth) + ",  " +
                    (TOP_MARGIN + PARTS_HEIGHT + LINE_LENGTH) + "  L  " + (X_LANE_POSITION + partsWidth) + ",  " + (TOP_MARGIN + PARTS_HEIGHT + PARTS_HEIGHT + LINE_LENGTH) + " L  " +
                    X_LANE_POSITION + ",  " + (TOP_MARGIN + PARTS_HEIGHT + PARTS_HEIGHT + LINE_LENGTH) + " L  " + X_LANE_POSITION + ",  " + (TOP_MARGIN + PARTS_HEIGHT + LINE_LENGTH) + "\"";

                toPartsTemplate.InnerXml = Regex.Replace(toPartsTemplate.InnerXml, "#partsDisplayPosition#", toDisplayPosition);
                svgChildren.AppendChild(toPartsTemplate.CloneNode(true));

                //経線作成
                //X軸の移動
                X_LANE_POSITION += PARTS_LEFT_RIGHT_MARGIN;
                foreach (Hashtable detailWire in dicWires[key])
                {
                    detailWire["from_terminal_name"].ToString();
                    detailWire["from_pin_no"].ToString();
                    detailWire["to_terminal_name"].ToString();
                    detailWire["to_pin_no"].ToString();

                    var colors = detailWire["wire_color"].ToString().Split('-');
                    if (colors.Length > 1)
                    {
                        doubleLineTemplate.InnerXml = Regex.Replace(singleLineTemplate.InnerXml,
                            "#backColor#", ColorConst.GetColor(colors[0]));
                        doubleLineTemplate.InnerXml = Regex.Replace(singleLineTemplate.InnerXml,
                            "#frontColor#", ColorConst.GetColor(colors[1]));

                        doubleLineTemplate.InnerXml = Regex.Replace(singleLineTemplate.InnerXml,
                            "#startX#", X_LANE_POSITION.ToString());
                        doubleLineTemplate.InnerXml = Regex.Replace(singleLineTemplate.InnerXml,
                            "#startY", (TOP_MARGIN + PARTS_HEIGHT).ToString());
                        doubleLineTemplate.InnerXml = Regex.Replace(singleLineTemplate.InnerXml,
                            "#endX#", X_LANE_POSITION.ToString());
                        doubleLineTemplate.InnerXml = Regex.Replace(singleLineTemplate.InnerXml,
                            "#endY#", (TOP_MARGIN + PARTS_HEIGHT + PARTS_HEIGHT + LINE_LENGTH).ToString());

                    }
                    else
                    {
                        singleLineTemplate.InnerXml = Regex.Replace(singleLineTemplate.InnerXml,
                            "#frontColor#", ColorConst.GetColor(colors[0]));

                        singleLineTemplate.InnerXml = Regex.Replace(singleLineTemplate.InnerXml,
                            "#startX#", X_LANE_POSITION.ToString());
                        singleLineTemplate.InnerXml = Regex.Replace(singleLineTemplate.InnerXml,
                            "#startY", (TOP_MARGIN + PARTS_HEIGHT).ToString());
                        singleLineTemplate.InnerXml = Regex.Replace(singleLineTemplate.InnerXml,
                            "#endX#", X_LANE_POSITION.ToString());
                        singleLineTemplate.InnerXml = Regex.Replace(singleLineTemplate.InnerXml,
                            "#endY#", (TOP_MARGIN + PARTS_HEIGHT + PARTS_HEIGHT + LINE_LENGTH).ToString());
                    }
                    //端子名
                    double pts;
                    labelTemplate.InnerXml = Regex.Replace(labelTemplate.InnerXml, "#from_terminal_name#",
                        detailWire["from_terminal_name"].ToString());
                    pts = 6 * detailWire["from_terminal_name"].ToString().Length;
                    labelTemplate.InnerXml = Regex.Replace(labelTemplate.InnerXml, "#fromTerminalNameX#",
                        (X_LANE_POSITION - pts / 2).ToString());
                    labelTemplate.InnerXml = Regex.Replace(labelTemplate.InnerXml, "#fromTerminalNameY#",
                        (TOP_MARGIN + PARTS_HEIGHT - 6).ToString());

                    labelTemplate.InnerXml = Regex.Replace(labelTemplate.InnerXml, "#to_terminal_name#",
                        detailWire["to_terminal_name"].ToString());
                    pts = 6 * detailWire["to_terminal_name"].ToString().Length;
                    labelTemplate.InnerXml = Regex.Replace(labelTemplate.InnerXml, "#toTerminalNameX#",
                        (X_LANE_POSITION - pts / 2).ToString());
                    labelTemplate.InnerXml = Regex.Replace(labelTemplate.InnerXml, "#tomTerminalNameY#",
                        (TOP_MARGIN + PARTS_HEIGHT + LINE_LENGTH + 6).ToString());

                    //ピンNo.
                    labelTemplate.InnerXml = Regex.Replace(labelTemplate.InnerXml, "#from_pin_no#",
                        detailWire["from_pin_no"].ToString());
                    pts = 6 * detailWire["from_pin_no"].ToString().Length;
                    labelTemplate.InnerXml = Regex.Replace(labelTemplate.InnerXml, "#fromPinNoX#",
                        (X_LANE_POSITION - pts - 5).ToString());
                    labelTemplate.InnerXml = Regex.Replace(labelTemplate.InnerXml, "#fromPinNoY#",
                        (TOP_MARGIN + PARTS_HEIGHT + 6).ToString());

                    labelTemplate.InnerXml = Regex.Replace(labelTemplate.InnerXml, "#to_pin_no#",
                        detailWire["to_pin_no"].ToString());
                    pts = 6 * detailWire["to_pin_no"].ToString().Length;
                    labelTemplate.InnerXml = Regex.Replace(labelTemplate.InnerXml, "#toPinNoX#",
                        (X_LANE_POSITION - pts - 5).ToString());
                    labelTemplate.InnerXml = Regex.Replace(labelTemplate.InnerXml, "#toPinNoY#",
                        (TOP_MARGIN + PARTS_HEIGHT + LINE_LENGTH - 6 - 6).ToString());

                    //線色
                    labelTemplate.InnerXml = Regex.Replace(labelTemplate.InnerXml, "#wire_color#",
                        detailWire["wire_color"].ToString());
                    pts = 6 * detailWire["wire_color"].ToString().Length;
                    labelTemplate.InnerXml = Regex.Replace(labelTemplate.InnerXml, "#wireColorX#",
                        (X_LANE_POSITION - 10).ToString());
                    labelTemplate.InnerXml = Regex.Replace(labelTemplate.InnerXml, "#wireColorY#",
                        (TOP_MARGIN + PARTS_HEIGHT + LINE_LENGTH / 2 + pts / 2).ToString());

                    //何本目結線
                    X_LANE_POSITION += LINE_INTERVAL;
                }
                //最後の1本も、移動させる
                X_LANE_POSITION += PARTS_LEFT_RIGHT_MARGIN;
            }
            return;
        }

    }
}
