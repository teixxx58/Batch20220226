using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        double LINE_INTERVAL = 60;
        //経線の長さ
        double LINE_LENGTH = 60;
        //パーツの高さ
        double PARTS_HEIGHT = 35;
        //パーツの左右マージン
        double PARTS_LEFT_RIGHT_MARGIN = 30;
        //フォント高さ
        double FONT_HEIGH = 6;
        //フォントマージン
        double FONT_MARGIN = 2;
        //結線幅
        double LINE_WEIDTH = 4.1;


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
            double pts;

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
            var whiteLineTemplate = (XmlElement)svgRoot.SelectSingleNode("//*[@name='whiteLineTemplate']");
            whiteLineTemplate.RemoveAttribute("name");
            var doubleLineTemplate = (XmlElement)svgRoot.SelectSingleNode("//*[@name='doubleLineTemplate']");
            doubleLineTemplate.RemoveAttribute("name");

            //SVGを空にする
            var svgChildren = svgRoot.SelectSingleNode("//*[@id='ewd_top']");
            svgChildren.RemoveAll();

            double X_LANE_POSITION = LEFT_MARGIN;
            double partsWidth;

            //経線図作成
            foreach (string key in dicWires.Keys)
            {
                //経線本数
                int lineCnt = dicWires[key].Count;
                //パーツ幅
                partsWidth = 2 * PARTS_LEFT_RIGHT_MARGIN + (lineCnt - 1) * LINE_INTERVAL;

                //テンプレートコピー
                XmlElement fromParts = (XmlElement)fromPartsTemplate.CloneNode(true);
                //FROMパーツ
                //Fromパーツコード
                fromParts.SetAttribute("ewd:code", dicWires[key][0]["from_parts_code"].ToString());
                //FromパーツHW名称
                fromParts.InnerXml = Regex.Replace(fromParts.InnerXml, "#from_hw_parts_name#", dicWires[key][0]["from_wh_parts_name"].ToString());
                pts = dicWires[key][0]["from_wh_parts_name"].ToString().Length;
                //FROMパーツ名称
                string fromPartsNamePosition = X_LANE_POSITION  + "  " + (TOP_MARGIN - FONT_MARGIN *2);
                fromParts.InnerXml = Regex.Replace(fromParts.InnerXml, "#fromPartsNamePosition#", fromPartsNamePosition);

                //Fromパーツ名称
                fromParts.InnerXml = Regex.Replace(fromParts.InnerXml, "#from_parts_name#", dicWires[key][0]["from_parts_name"].ToString());
                //Fromパーツ表示位置
                string fromDisplayPosition =  X_LANE_POSITION + ",  " + TOP_MARGIN + 
                    "  L  " + (X_LANE_POSITION + partsWidth) + ",  " +  TOP_MARGIN + 
                    "  L  " + (X_LANE_POSITION + partsWidth) + ",  " + (TOP_MARGIN + PARTS_HEIGHT) + 
                    "  L  " +  X_LANE_POSITION + ",  " + (TOP_MARGIN + PARTS_HEIGHT) + 
                    "  L  " + X_LANE_POSITION + ",  " + TOP_MARGIN;

                fromParts.InnerXml = Regex.Replace(fromParts.InnerXml, "#fromPartsDisplayPosition#", fromDisplayPosition);
                svgChildren.AppendChild(fromParts.CloneNode(true));

                //TOパーツ
                //テンプレートコピー
                XmlElement toParts = (XmlElement)toPartsTemplate.CloneNode(true);
                //Toパーツコード
                toParts.SetAttribute("ewd:code", dicWires[key][0]["to_parts_code"].ToString());
                //ToパーツHW名称
                toParts.InnerXml = Regex.Replace(toParts.InnerXml, "#to_hw_parts_name#",
                    dicWires[key][0]["to_wh_parts_name"].ToString());
                //Toパーツ名称
                pts = dicWires[key][0]["to_wh_parts_name"].ToString().Length;
                string toPartsNamePosition = X_LANE_POSITION + "  " + (TOP_MARGIN + 2*PARTS_HEIGHT + LINE_LENGTH + FONT_HEIGH + FONT_MARGIN);
                toParts.InnerXml = Regex.Replace(toParts.InnerXml, "#toPartsNamePosition#", toPartsNamePosition);

                //Toパーツ名称
                toParts.InnerXml = Regex.Replace(toParts.InnerXml, "#to_parts_name#", 
                    dicWires[key][0]["to_parts_name"].ToString());
                //Toパーツ表示位置
                string toDisplayPosition = X_LANE_POSITION + ",  " +   (TOP_MARGIN + PARTS_HEIGHT + LINE_LENGTH ) + 
                    "  L  " + (X_LANE_POSITION + partsWidth) + ",  " + (TOP_MARGIN + PARTS_HEIGHT + LINE_LENGTH) + 
                    "  L  " + (X_LANE_POSITION + partsWidth) + ",  " + (TOP_MARGIN + PARTS_HEIGHT + LINE_LENGTH + PARTS_HEIGHT) +
                    "  L  " + X_LANE_POSITION + ",  " +                (TOP_MARGIN + PARTS_HEIGHT + LINE_LENGTH + PARTS_HEIGHT) + 
                    "  L  " + X_LANE_POSITION + ",  " +                (TOP_MARGIN + PARTS_HEIGHT + LINE_LENGTH);

                toParts.InnerXml = Regex.Replace(toParts.InnerXml, "#toPartsDisplayPosition#", toDisplayPosition);
                svgChildren.AppendChild(toParts.CloneNode(true));

                //結線作成
                //X軸の移動
                X_LANE_POSITION = X_LANE_POSITION + PARTS_LEFT_RIGHT_MARGIN;
                foreach (Hashtable detailWire in dicWires[key])
                {

                    var colors = detailWire["wire_color"].ToString().Split('-');
                    if (colors.Length > 1)
                    {
                        //テンプレートコピー
                        var doubleLine = doubleLineTemplate.CloneNode(true);

                        doubleLine.InnerXml = Regex.Replace(doubleLine.InnerXml,
                            "#backColor#", ColorConst.GetColor(colors[1]));
                        doubleLine.InnerXml = Regex.Replace(doubleLine.InnerXml,
                            "#frontColor#", ColorConst.GetColor(colors[0]));

                        doubleLine.InnerXml = Regex.Replace(doubleLine.InnerXml,
                            "#startX#", X_LANE_POSITION.ToString());
                        doubleLine.InnerXml = Regex.Replace(doubleLine.InnerXml,
                            "#startY#", (TOP_MARGIN + PARTS_HEIGHT).ToString());
                        doubleLine.InnerXml = Regex.Replace(doubleLine.InnerXml,
                            "#endX#", X_LANE_POSITION.ToString());
                        doubleLine.InnerXml = Regex.Replace(doubleLine.InnerXml,
                            "#endY#", (TOP_MARGIN + + PARTS_HEIGHT + LINE_LENGTH).ToString());

                        //Wのみ場合
                        if (detailWire["wire_color"].ToString().Equals("W"))
                        {
                            doubleLine.RemoveChild(doubleLine.ChildNodes[2]);
                        }
                        svgChildren.AppendChild(doubleLine.CloneNode(true));

                    }
                    //白線
                    else if (colors.Length == 1 && detailWire["wire_color"].ToString().Equals("W"))
                    {
                        //テンプレートコピー
                        var whiteLine = whiteLineTemplate.CloneNode(true);

                        whiteLine.InnerXml = Regex.Replace(whiteLine.InnerXml,
                            "#backColor#", ColorConst.GetColor("B"));
                        whiteLine.InnerXml = Regex.Replace(whiteLine.InnerXml,
                             "#frontColor#", ColorConst.GetColor("W"));
                        whiteLine.InnerXml = Regex.Replace(whiteLine.InnerXml,
                            "#startX#", X_LANE_POSITION.ToString());
                        whiteLine.InnerXml = Regex.Replace(whiteLine.InnerXml,
                            "#startY#", (TOP_MARGIN + PARTS_HEIGHT).ToString());
                        whiteLine.InnerXml = Regex.Replace(whiteLine.InnerXml,
                            "#endX#", X_LANE_POSITION.ToString());
                        whiteLine.InnerXml = Regex.Replace(whiteLine.InnerXml,
                            "#endY#", (TOP_MARGIN + +PARTS_HEIGHT + LINE_LENGTH).ToString());
                       
                        svgChildren.AppendChild(whiteLine.CloneNode(true));

                    }
                    else
                    {
                        //テンプレートコピー
                        var singleLine = singleLineTemplate.CloneNode(true);
                        singleLine.InnerXml = Regex.Replace(singleLine.InnerXml,
                            "#frontColor#", ColorConst.GetColor(colors[0]));

                        singleLine.InnerXml = Regex.Replace(singleLine.InnerXml,
                            "#startX#", X_LANE_POSITION.ToString());
                        singleLine.InnerXml = Regex.Replace(singleLine.InnerXml,
                            "#startY#", (TOP_MARGIN + PARTS_HEIGHT).ToString());
                        singleLine.InnerXml = Regex.Replace(singleLine.InnerXml,
                            "#endX#", X_LANE_POSITION.ToString());
                        singleLine.InnerXml = Regex.Replace(singleLine.InnerXml,
                            "#endY#", (TOP_MARGIN + PARTS_HEIGHT  + LINE_LENGTH).ToString());

                        svgChildren.AppendChild(singleLine.CloneNode(true));
                    }


                    //端子名
                    //テンプレートコピー
                    var label = labelTemplate.CloneNode(true);
                    label.InnerXml = Regex.Replace(label.InnerXml, "#from_terminal_name#",
                        detailWire["from_terminal_name"].ToString());
                    
                    SizeF fromTerminalName_size = PDFUtil.MeasureFontSize(detailWire["from_terminal_name"].ToString());
                    label.InnerXml = Regex.Replace(label.InnerXml, "#fromTerminalNameX#",
                        (X_LANE_POSITION - fromTerminalName_size.Width/2 + LINE_WEIDTH/2).ToString());

                    label.InnerXml = Regex.Replace(label.InnerXml, "#fromTerminalNameY#",
                        (TOP_MARGIN + PARTS_HEIGHT - FONT_MARGIN).ToString());

                    label.InnerXml = Regex.Replace(label.InnerXml, "#to_terminal_name#",
                        detailWire["to_terminal_name"].ToString());
                    
                    SizeF toTerminalName_size = PDFUtil.MeasureFontSize(detailWire["to_terminal_name"].ToString());
                    label.InnerXml = Regex.Replace(label.InnerXml, "#toTerminalNameX#",
                        (X_LANE_POSITION - toTerminalName_size.Width/2 + LINE_WEIDTH / 2).ToString());

                    label.InnerXml = Regex.Replace(label.InnerXml, "#toTerminalNameY#",
                        (TOP_MARGIN + PARTS_HEIGHT + LINE_LENGTH + FONT_HEIGH + FONT_MARGIN).ToString());

                    //ピンNo.
                    label.InnerXml = Regex.Replace(label.InnerXml, "#from_pin_no#",
                        detailWire["from_pin_no"].ToString());

                    SizeF fromPinNo_size = PDFUtil.MeasureFontSize(detailWire["from_pin_no"].ToString());

                    label.InnerXml = Regex.Replace(label.InnerXml, "#fromPinNoX#",
                        (X_LANE_POSITION - fromPinNo_size.Width - FONT_MARGIN).ToString());

                    label.InnerXml = Regex.Replace(label.InnerXml, "#fromPinNoY#",
                        (TOP_MARGIN + PARTS_HEIGHT + FONT_HEIGH).ToString());

                    label.InnerXml = Regex.Replace(label.InnerXml, "#to_pin_no#",
                        detailWire["to_pin_no"].ToString());

                    SizeF toPinNo_size = PDFUtil.MeasureFontSize(detailWire["to_pin_no"].ToString());
                    label.InnerXml = Regex.Replace(label.InnerXml, "#toPinNoX#",
                        (X_LANE_POSITION - toPinNo_size.Width - FONT_MARGIN).ToString());
                    label.InnerXml = Regex.Replace(label.InnerXml, "#toPinNoY#",
                        (TOP_MARGIN + PARTS_HEIGHT + LINE_LENGTH - FONT_MARGIN).ToString());

                    //線色
                    label.InnerXml = Regex.Replace(label.InnerXml, "#wire_color#",
                        detailWire["wire_color"].ToString());

                    label.InnerXml = Regex.Replace(label.InnerXml, "#wireColorX#",
                        (X_LANE_POSITION - FONT_HEIGH).ToString());

                    SizeF color_size = PDFUtil.MeasureFontSize(detailWire["wire_color"].ToString());
                    label.InnerXml = Regex.Replace(label.InnerXml, "#wireColorY#",
                        (TOP_MARGIN + PARTS_HEIGHT + LINE_LENGTH/2 + color_size.Width / 2).ToString());

                    svgChildren.AppendChild(label.CloneNode(true));
                    //何本目結線
                    X_LANE_POSITION = X_LANE_POSITION + LINE_INTERVAL;
                }
                //最後の1本も、移動させる
                X_LANE_POSITION = X_LANE_POSITION + PARTS_LEFT_RIGHT_MARGIN;
            }
            ((XmlElement)svgRoot).SetAttribute("viewBox", "0 0 " + X_LANE_POSITION.ToString() + " 710");
            ((XmlElement)svgRoot).SetAttribute("width", X_LANE_POSITION.ToString());
            return;
        }

    }
}
