using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace BT0301Batch
{
    /// <summary>
    /// Double型Point
    /// </summary>
    struct BTPoint
    {
        public double X;
        public double Y;
    }
    struct BTLine
    {
        public BTPoint fromP;
        public BTPoint toP;
    }
    class Syugaki
    {
        //朱書きTEXT(端子)
        string TEXT_STYLE = "fill:#FF0000;font-size:" + BatchBase.SYUGAKI_FONT_SIZE + "px;font-family:'Arial';font-weight:bold;text-anchor:end;";
        //朱書きTEXT(接続先)
        string TEXT_STYLE_CENTER = "fill:#FF0000;font-size:" + BatchBase.SYUGAKI_FONT_SIZE + "px; font-family:'Arial';font-weight:bold;";
        //横書き Matrix
        const string HOR_MTX = "matrix(1.00 -0.00 0.00 1.00 @ )";
        //縦書き Matrix
        const string VER_MTX = "matrix(-0.00 -1.00 1.00 -0.00  @ )";
        //結線消し点線
        const string STROKE_DOT_LINE = "stroke-dasharray:5 2 ;";

        private float X_OFFSET = 1F;
        private float Y_OFFSET = 3F;
        private float FONT_SIZE = 6F;
        private double LINE_WIDTH = 5;

        private string SHIELDED = "shielded";

        private XmlDocument _xmlDoc;
        private string _fileName;
        public Syugaki(string fileName)
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
        public void SVGSave(string filePath)
        {
            if (_xmlDoc != null)
            {
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }
                XmlTextWriter writer = new XmlTextWriter(filePath, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                _xmlDoc.Save(writer);
                writer.Close();
            }
        }
        /// <summary>
        /// 結線朱書き
        /// </summary>
        /// <param name="wireInfoList"></param>
        public void RedDraws(List<Hashtable> wireInfoList)
        {
            try
            {
                double x, y;
                int direction;
                int pinNo;
                string terminalName, code;
                string partsCodeDiffFlag, pinNoDiffFlag, terminalNameDiffFlag;

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(_xmlDoc.NameTable);
                nsmgr.AddNamespace("ns", "http://www.w3.org/2000/svg");
                nsmgr.AddNamespace("ewd", "http://shcl.co.jp/ewd");
                nsmgr.AddNamespace("a", "http://ns.adobe.com/AdobeSVGViewerExtensions/3.0/");


                //XmlNodeList nodeList = _xmlDoc.SelectNodes("/ns:svg/ns:g/ns:g", nsmgr);
                XmlNodeList shieldNodeList = _xmlDoc.SelectNodes("/ns:svg/ns:g/ns:a", nsmgr);

                // 結線ごとに、SVGに朱書き内容（部品コード、端子、ピン番号）を記載する。
                foreach (Hashtable item in wireInfoList)
                {
                    // 朱書き条件判定
                    if (BatchBase.targetSyugakiMinPoint <= Convert.ToInt32(item["similar_point"].ToString()) &&
                                  Convert.ToInt32(item["similar_point"].ToString()) < BatchBase.similar100Point)
                    {
                        // *************************************
                        // 部品、端子、ピン番号の変更
                        // *************************************
                        //データチェック
                        XmlNode fromPartsNode, toPartsNode, fromLabelNode, toLabelNode;
                        fromPartsNode = _xmlDoc.SelectSingleNode("/ns:svg/ns:g/ns:g[@ewd:code='" + item["from_new_parts_cd"].ToString() + "']", nsmgr);
                        if (fromPartsNode == null)
                        {
                            CLogger.Logger("WNG_PARTS_LOSS",_fileName, item["from_new_parts_cd"].ToString());
                            BatchBase.AppendErrMsg("WNG_PARTS_LOSS", _fileName, item["from_new_parts_cd"].ToString());

                            toPartsNode = _xmlDoc.SelectSingleNode("/ns:svg/ns:g/ns:g[@ewd:code='" + item["to_new_parts_cd"].ToString() + "']", nsmgr);
                            if (toPartsNode == null)
                            {
                                CLogger.Logger("WNG_PARTS_LOSS", _fileName, item["to_new_parts_cd"].ToString());
                                BatchBase.AppendErrMsg("WNG_PARTS_LOSS", _fileName, item["to_new_parts_cd"].ToString());
                                //配線図に対象パーツがないため、スキップ
                                continue;
                            }
                            else
                            {
                                fromPartsNode = toPartsNode;
                            }
                        }
                        else
                        {
                            toPartsNode = _xmlDoc.SelectSingleNode("/ns:svg/ns:g/ns:g[@ewd:code='" + item["to_new_parts_cd"].ToString() + "']", nsmgr);
                            if (toPartsNode == null)
                            {
                                CLogger.Logger("WNG_PARTS_LOSS", _fileName, item["to_new_parts_cd"].ToString());
                                BatchBase.AppendErrMsg("WNG_PARTS_LOSS", _fileName, item["to_new_parts_cd"].ToString());
                                toPartsNode = fromPartsNode;
                            }
                        }
                        //結線取得
                        fromLabelNode = fromPartsNode.SelectSingleNode("ns:g[@ewd:lineID='" + item["svg_line_id"].ToString() + "']", nsmgr);
                        toLabelNode = toPartsNode.SelectSingleNode("ns:g[@ewd:lineID='" + item["svg_line_id"].ToString() + "']", nsmgr);
                        if (fromLabelNode == null)
                        {
                            CLogger.Logger("WNG_PARTS_LOSS", _fileName, item["svg_line_id"].ToString());
                            BatchBase.AppendErrMsg("WNG_PARTS_LOSS", _fileName, item["svg_line_id"].ToString());
                            if (toLabelNode == null)
                            {
                                CLogger.Logger("WNG_PARTS_LOSS", _fileName, item["svg_line_id"].ToString());
                                BatchBase.AppendErrMsg("WNG_PARTS_LOSS", _fileName, item["svg_line_id"].ToString());
                                //配線図に対象結線がないため、スキップ
                                continue;
                            }
                            else
                            {
                                fromLabelNode = toLabelNode;
                            }
                        }
                        else
                        {
                            if (toLabelNode == null)
                            {
                                CLogger.Logger("WNG_PARTS_LOSS", _fileName, item["svg_line_id"].ToString());
                                BatchBase.AppendErrMsg("WNG_PARTS_LOSS", _fileName, item["svg_line_id"].ToString());
                                toLabelNode = fromLabelNode;
                            }
                        }

                        ///test
/*                        fromPartsNode.AppendChild(createPathElement(item["from_point_x"].ToString(),
                                            item["from_point_y"].ToString(), item["to_point_x"].ToString(),
                                            item["to_point_y"].ToString()));*/
                        ////////////////////////////////////////
                        // FROM側記載
                        ////////////////////////////////////////
                        // T_SIMILAR_CALC.REVERSE_FLGが0の場合
                        if (item["reverse_flg"].ToString().Equals("0"))
                        {
                            x = Convert.ToDouble(item["from_point_x"].ToString());
                            y = Convert.ToDouble(item["from_point_y"].ToString());
                            direction = Convert.ToInt16(item["from_direction"].ToString());
                            pinNo = Convert.ToInt16(item["detail_from_pin_no"].ToString());
                            terminalName = item["detail_from_terminal_name"].ToString();
                            code = item["detail_from_parts_code"].ToString();

                        }
                        // T_SIMILAR_CALC.REVERSE_FLGが1の場合
                        else
                        {
                            x = Convert.ToDouble(item["to_point_x"].ToString());
                            y = Convert.ToDouble(item["to_point_y"].ToString());
                            direction = Convert.ToInt16(item["to_direction"].ToString());
                            pinNo = Convert.ToInt16(item["detail_to_pin_no"].ToString());
                            terminalName = item["detail_to_terminal_name"].ToString();
                            code = item["detail_to_parts_code"].ToString();
                        }

                        partsCodeDiffFlag = item["from_parts_code_diff_flg"].ToString();
                        pinNoDiffFlag = item["from_pin_no_diff_flg"].ToString();
                        terminalNameDiffFlag = item["from_terminal_name_diff_flg"].ToString();
                        //朱書き位置調整
                        x = x + BatchBase.SYUGAKI_OFFSET_SIZE_X;
                        y = y + BatchBase.SYUGAKI_OFFSET_SIZE_Y;
                        // 朱書き実施
                        RedDraw(fromLabelNode, x, y, direction,
                            pinNo, terminalName, code,
                            partsCodeDiffFlag, pinNoDiffFlag, terminalNameDiffFlag);


                        ////////////////////////////////////////
                        // TO側記載
                        ////////////////////////////////////////
                        // T_SIMILAR_CALC.REVERSE_FLGが1の場合
                        if (item["reverse_flg"].ToString().Equals("1"))
                        {
                            x = Convert.ToDouble(item["from_point_x"].ToString());
                            y = Convert.ToDouble(item["from_point_y"].ToString());
                            direction = Convert.ToInt16(item["from_direction"].ToString());
                            pinNo = Convert.ToInt16(item["detail_from_pin_no"].ToString());
                            terminalName = item["detail_from_terminal_name"].ToString();
                            code = item["detail_from_parts_code"].ToString();
                        }
                        // T_SIMILAR_CALC.REVERSE_FLGが0の場合
                        else
                        {
                            x = Convert.ToDouble(item["to_point_x"].ToString());
                            y = Convert.ToDouble(item["to_point_y"].ToString());
                            direction = Convert.ToInt16(item["to_direction"].ToString());
                            pinNo = Convert.ToInt16(item["detail_to_pin_no"].ToString());
                            terminalName = item["detail_to_terminal_name"].ToString();
                            code = item["detail_to_parts_code"].ToString();

                        }
                        partsCodeDiffFlag = item["to_parts_code_diff_flg"].ToString();
                        pinNoDiffFlag = item["to_pin_no_diff_flg"].ToString();
                        terminalNameDiffFlag = item["to_terminal_name_diff_flg"].ToString();

                        //朱書き位置調整
                        x = x + BatchBase.SYUGAKI_OFFSET_SIZE_X;
                        y = y + BatchBase.SYUGAKI_OFFSET_SIZE_Y;

                        // 朱書き実施
                        RedDraw(toLabelNode, x, y, direction,
                            pinNo, terminalName, code,
                            partsCodeDiffFlag, pinNoDiffFlag, terminalNameDiffFlag);

                        // *************************************
                        // 線色の変更、シールド表現
                        // *************************************
                        item["wire_color_diff_flg"] = 1;
                        //item["path"] = "test";
                        if (item["wire_color_diff_flg"].ToString().Equals("1"))
                        {
                            // 線色の朱書き
                            foreach (XmlNode shieldNode in shieldNodeList)
                            {
                                if (shieldNode.Attributes["ewd:lineID"] != null
                                    && shieldNode.Attributes["ewd:lineID"].Value.Equals(item["svg_line_id"].ToString()))
                                {
                                    XmlNodeList pathNodes = shieldNode.SelectNodes("ns:path", nsmgr);
                                    // 一つ目Pathの真ん中「shielded」書く
                                    // d="M x,y L x,y"の真ん中位置決め
                                    string dValue = pathNodes[0].Attributes["d"].Value;

                                    BTPoint centerXY = GetCenterXY(dValue);

                                    //シールド朱書き
                                    string str = item["detail_wire_color"].ToString();
                                    if (item["path"] != null) str += " " + SHIELDED;
                                    // 中心座標、方向、色
                                    ShieldRedDraw(shieldNode, centerXY, str,
                                        Convert.ToInt16(item["from_direction"].ToString()));

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                
                CLogger.Err(ex);
                BatchBase.AppendErrMsg("ERR_PROCESS_FAILED","朱書き処理");
                //ファイル操作の原因で処理中止
                throw ex;
            }
            return;
        }

        /// <summary>
        /// shield朱書き
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private XmlElement CreateTextElement(string text)
        {
            XmlNode root = _xmlDoc.GetElementsByTagName("svg")[0];
            // xmlns:""防止
            XmlElement elem = _xmlDoc.CreateElement("text", root.NamespaceURI);

            elem.SetAttribute("style", TEXT_STYLE_CENTER);
            elem.InnerText = text;
 
            return elem;
        }
        /// <summary>
        /// SVG画像の朱書き操作
        /// </summary>
        /// <param name="node"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="direction"></param>
        /// <param name="pinNo"></param>
        /// <param name="terminalName"></param>
        /// <param name="code"></param>
        /// <param name="partsCodeDiffFlag"></param>
        /// <param name="pinNoDiffFlag"></param>
        /// <param name="terminalNameDiffFlag"></param>
        private void RedDraw(XmlNode node,
            double X,
            double Y,
            int direction,
            int pinNo,
            string terminalName,
            string code,
            string partsCodeDiffFlag,
            string pinNoDiffFlag,
            string terminalNameDiffFlag)
        {
            XmlElement pinNoElement = CreateTextElement(pinNo.ToString());
            XmlElement terminalElement = CreateTextElement(terminalName);
            XmlElement codeElement = CreateTextElement(code);

            string mtx, transform;
            double x, y;
            SizeF pinNo_size = PDFUtil.MeasureFontSize(Convert.ToString(pinNo));
            SizeF terminalName_size = PDFUtil.MeasureFontSize(Convert.ToString(terminalName));
            SizeF code_size = PDFUtil.MeasureFontSize(Convert.ToString(code));

            //test
            partsCodeDiffFlag = "1";
            pinNoDiffFlag = "1";
            terminalNameDiffFlag = "1";
            switch (direction)
            {
                // 上側へ接続
                case 3:
                case 4:
                case 5:
                    x = X - X_OFFSET- pinNo_size.Width;
                    y = Y + pinNo_size.Height*2;
                    mtx = HOR_MTX;
                    transform = Regex.Replace(mtx, "@", string.Format("{0:0.0}, {1:0.0}", x, y));
                    pinNoElement.SetAttribute("transform", transform);

                    x = X - X_OFFSET - terminalName_size.Width;
                    y = Y + terminalName_size.Height * 3;
                    transform = Regex.Replace(mtx, "@", string.Format("{0:0.0}, {1:0.0}", x, y));
                    terminalElement.SetAttribute("transform", transform);

                    x = X + LINE_WIDTH;
                    y = Y + code_size.Height;
                    transform = Regex.Replace(mtx, "@", string.Format("{0:0.0}, {1:0.0}", x, y));
                    codeElement.SetAttribute("transform", transform);

                    break;
                case 2:
                    // 右側へ接続
                    x = X - X_OFFSET - pinNo_size.Width;
                    y = Y - Y_OFFSET - pinNo_size.Height;
                    
                    mtx = HOR_MTX;
                    transform = Regex.Replace(mtx, "@", string.Format("{0:0.0}, {1:0.0}", x, y));
                    pinNoElement.SetAttribute("transform", transform);

                    x = X - FONT_SIZE - X_OFFSET - terminalName_size.Width - pinNo_size.Width;
                    y = Y - Y_OFFSET - terminalName_size.Height;
                    transform = Regex.Replace(mtx, "@", string.Format("{0:0.0}, {1:0.0}", x, y));
                    terminalElement.SetAttribute("transform", transform);

                    x = X - X_OFFSET - code_size.Width;
                    y = Y + Y_OFFSET + code_size.Height;
                    transform = Regex.Replace(mtx, "@", string.Format("{0:0.0}, {1:0.0}", x, y));
                    codeElement.SetAttribute("transform", transform);

                    break;
                // 下側へ接続
                case 0:
                case 1:
                case 7:
                    x = X - X_OFFSET- pinNo_size.Width;
                    y = Y - pinNo_size.Height-1;
                    mtx = HOR_MTX;
                    transform = Regex.Replace(mtx, "@", string.Format("{0:0.0}, {1:0.0}", x, y));
                    pinNoElement.SetAttribute("transform", transform);

                    x = X - X_OFFSET - terminalName_size.Width;
                    y = Y - terminalName_size.Height * 2;
                    transform = Regex.Replace(mtx, "@", string.Format("{0:0.0}, {1:0.0}", x, y));
                    terminalElement.SetAttribute("transform", transform);

                    x = X + LINE_WIDTH;
                    y = Y - Y_OFFSET;
                    transform = Regex.Replace(mtx, "@", string.Format("{0:0.0}, {1:0.0}", x, y));
                    codeElement.SetAttribute("transform", transform);

                    break;
                // 左側へ接続
                case 6:
                    x = X + 3*X_OFFSET ;
                    y = Y - Y_OFFSET - pinNo_size.Height;
                    mtx = HOR_MTX;
                    transform = Regex.Replace(mtx, "@", string.Format("{0:0.0}, {1:0.0}", x, y));
                    pinNoElement.SetAttribute("transform", transform);

                    x = X + 3*X_OFFSET + FONT_SIZE + pinNo_size.Width;
                    y = Y - Y_OFFSET - terminalName_size.Height;
                    transform = Regex.Replace(mtx, "@", string.Format("{0:0.0}, {1:0.0}", x, y));
                    terminalElement.SetAttribute("transform", transform);

                    x = X + 3*X_OFFSET;
                    y = Y + Y_OFFSET + code_size.Height;
                    transform = Regex.Replace(mtx, "@", string.Format("{0:0.0}, {1:0.0}", x, y));
                    codeElement.SetAttribute("transform", transform);

                    break;
                default:

                    break;
            }
            if (pinNoDiffFlag.Equals("1"))
            {
                node.AppendChild(pinNoElement);
            }
            if (terminalNameDiffFlag.Equals("1"))
            {
                node.AppendChild(terminalElement);
            }
            if (partsCodeDiffFlag.Equals("1"))
            {
                node.AppendChild(codeElement);
            }

        }
        /// <summary>
        /// シールド朱書き
        /// </summary>
        /// <param name="node"></param>
        /// <param name="centerXY"></param>
        /// <param name="str"></param>
        private void ShieldRedDraw(XmlNode node, BTPoint centerXY, string str, int direc)
        {
            string mtx, transform;
            double x, y;
            XmlElement shieldElem = CreateTextElement(str);
            SizeF shield_size = PDFUtil.MeasureFontSize(Convert.ToString(str));
            switch (direc)
            {
                case 2:
                case 6:
                    x = centerXY.X - X_OFFSET - shield_size.Width/2;
                    y = centerXY.Y - Y_OFFSET;
                    mtx = HOR_MTX;
                    break;
                default:
                    x = centerXY.X - X_OFFSET - shield_size.Height;
                    y = centerXY.Y + shield_size.Width / 2;
                    mtx = VER_MTX;
                    break;        
            }

            transform = Regex.Replace(mtx, "@", string.Format("{0:0.0}, {1:0.0}", x, y));
            shieldElem.SetAttribute("transform", transform);

            node.AppendChild(shieldElem);
        }

        /// <summary>
        /// 結線座標取得
        /// </summary>
        /// <param name="dValue"></param>
        /// <returns></returns>        
        private BTLine GetLine(string dValue)
        {
            int posM = dValue.IndexOf('M');
            int posFirstL = dValue.IndexOf('L');
            int posSecondL = dValue.IndexOf('L', posFirstL + 1);
            if (posSecondL == -1)
                posSecondL = dValue.Length - 1;

            string fromXY = dValue.Substring(posM + 1, posFirstL - 1 - posM);
            // ⇒"zzz.zz,zzz.zz"空白除去
            fromXY = Regex.Replace(fromXY, @"\s", "");

            string toXY = dValue.Substring(posFirstL + 1, posSecondL - posFirstL - 1);
            // ⇒"zzz.zz,zzz.zz"
            toXY = Regex.Replace(toXY, @"\s", "");

            BTLine line = new BTLine();
            string[] temp = fromXY.Split(',');
            line.fromP.X = Convert.ToDouble(temp[0]);
            line.fromP.Y = Convert.ToDouble(temp[1]);

            temp = toXY.Split(',');
            line.toP.X = Convert.ToDouble(temp[0]);
            line.toP.Y = Convert.ToDouble(temp[1]);

            return line;

        }
        /// <summary>
        /// 結線真ん中座標取得
        /// </summary>
        /// <param name="dValue"></param>
        /// <returns></returns>
        private BTPoint GetCenterXY(string dValue)
        {
            BTLine line = GetLine(dValue);

            BTPoint centerXY = new BTPoint();
            centerXY.X = (line.fromP.X + line.toP.X) / 2;
            centerXY.Y = (line.fromP.Y + line.toP.Y) / 2;

            return centerXY;
        }
        /// <summary>
        /// 結線削除
        /// </summary>
        /// <param name="wireInfoList"></param>
        public void DeleteRedDraws(List<Hashtable> wireInfoList)
        {
            try
            {
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(_xmlDoc.NameTable);
                nsmgr.AddNamespace("ns", "http://www.w3.org/2000/svg");
                XmlNodeList delLineNodeList = _xmlDoc.SelectNodes("/ns:svg/ns:g/ns:a", nsmgr);

                foreach (Hashtable item in wireInfoList)
                {
                    // 削除の朱書き
                    foreach (XmlNode deleteNode in delLineNodeList)
                    {
                        if (deleteNode.Attributes["ewd:lineID"] != null
                            && deleteNode.Attributes["ewd:lineID"].Value.Equals(item["svg_line_id"].ToString()))
                        {
                            XmlNodeList pathNodes = deleteNode.SelectNodes("ns:path", nsmgr);
                            // Styleに点線追加
                            string dValue = pathNodes[0].Attributes["style"].Value + STROKE_DOT_LINE;
                            pathNodes[0].Attributes["style"].Value = dValue;
                        }
                    }
                }
            }catch(Exception ex)
            {
                CLogger.Err(ex);
                BatchBase.AppendErrMsg("ERR_PROCESS_FAILED", "結線削除処理");
                //ファイル操作の原因で処理中止
                throw ex;
            }
        }
        static int staCnt=0;
        private XmlElement createPathElement(string tX, string tY, string bX, string bY)
        {
            staCnt++;
            XmlNode root = _xmlDoc.GetElementsByTagName("svg")[0];
            // xmlns:""防止
            XmlElement elem = _xmlDoc.CreateElement("path", root.NamespaceURI);

            elem.SetAttribute("style", "stroke:#00FF00;stroke-width: 4.10 ;fill:none;stroke-linecap:butt; stroke-linejoin:round;");
            elem.SetAttribute("name", "testSVG" + staCnt);
            string d = " M  " + tX + ",  " + tY + "   L  " + bX + ",    " + bY + "  ";
            elem.SetAttribute("d", d);

            return elem;
        }
    }
}
