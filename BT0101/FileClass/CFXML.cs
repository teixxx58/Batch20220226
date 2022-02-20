using BT0101.DBClass;
using BT0101Batch;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace BT0101.FileClass
{
	class CFXML : CFBase
	{

		public CFXML(){
		}
		public override void ImportFiles() {
		}
		/// <summary>
		/// 配線図リストアップ
		/// </summary>
		/// <returns>配線図リスト</returns>
		public List<CTWiringDiagram> GetFigDiagramList()
		{
			List<CTWiringDiagram> figList = new List<CTWiringDiagram>();

			// ターゲットファイルは *.svg、title.xml
			string xmlTitlePath = FileManager.localDataDir + PUBNO + FileManager.titleXmlDir;
			string xmlFileTitle = @"title.xml";

			string xmlFigPath = FileManager.localDataDir + PUBNO + FileManager.svgDir;
			string xmlFileSvg = "*.svg";
			//Title.xml読み込み
			XElement xml = XElement.Load(xmlTitlePath + xmlFileTitle);

			string[] svgNames = Directory.GetFiles(xmlFigPath, xmlFileSvg);
			foreach (string svg in svgNames)
			{
				string svgFilenameWithoutExt = Path.GetFileNameWithoutExtension(svg);

				// System/figを取得
				IEnumerable<XElement> figs = from item in xml.Elements("System")
											 where item.Element("fig").Value == svgFilenameWithoutExt
											 select item;
				// システムタイトル
				if (figs != null && figs.Count() > 0)
				{
					CTWiringDiagram record = new CTWiringDiagram();
					string systemTitle = figs.First().Element("name").Value;
					// 配線図データ
					record.pubNoId = PUBNO_ID;                 /* 登録したPUBNOのID */
					record.figName = svgFilenameWithoutExt;
					record.systemTitle = systemTitle;
					record.carModelSpecification = "";
					record.wiringSpecificationsNo = "";
					record.platform = "";
					record.insertUserId = null;
					record.updateUserId = null;
					//リストアップ
					figList.Add(record);
				}
			}
			return figList;
		}

		/// <summary>
		/// 配線図の登録
		/// </summary>
		public void ImportWireDiagramData(CTWiringDiagram figDiagram)
		{
			//SQLの実行
			try
			{
				// 配線図データ登録
				mapper.Insert("InsertWiringDiagram", figDiagram);
				WIRING_DIAGRAM_ID = mapper.QueryForObject<int>("SelectMaxWiringDiagramId",null);
				FIG_NAME = figDiagram.figName;
			}
			catch (Exception ex)
			{
				BatchBase.AppendErrMsg("WNG_DB_FAILED", figDiagram.figName, "配線図");
				CLogger.Err(ex);
				throw (ex);
			}
		}

		/// <summary>
		/// PUB_NO登録
		/// </summary>
		public void ImportPubNoData()
		{
			// Pub.xmlのパス
			string xmlFilePath = FileManager.localDataDir + PUBNO + FileManager.pubXmlDir;
			string xmlFileName = @"Pub.xml";

			//Filtering.xmlのパス ミッションの抽出
			string filteringPath = FileManager.localDataDir + PUBNO + FileManager.filteringXmlDir;
			string filteringFileName = @"filtering.xml";

			CTPubNo record = new CTPubNo();

			// Pub.xml読み込み
			XElement xml = XElement.Load(xmlFilePath + xmlFileName);
			//Pubタグ内の要素を取得する
			IEnumerable<XElement> models = xml.Elements("pub").Elements("models").Elements("model");

			record.carModel = string.Join(",", models.Select(e => e.Value));
			record.fiscalYear = "";
			IEnumerable<XElement> entryDates = xml.Elements("pub").Elements("entry-date");
			if(entryDates != null && entryDates.Count() > 0)
            {
				record.fiscalYear = entryDates.First().Value.Substring(0, 4);
            }
            
			IEnumerable<XElement> brands = xml.Elements("pub").Elements("brand");
			record.brand = "";
			if (brands !=null && brands.Count()> 0)
            {
				record.brand = brands.FirstOrDefault().Value;
			}

			IEnumerable<XElement> engines = xml.Elements("pub").Elements("models").Elements("model").
				Elements("model-code").Elements("engine-code"); ;

			// カンマ区切り
			record.engine = string.Join(",", engines.Select(e => e.Value));

			// 残存課題 "あり""なし"判定
			string kadaiDirectoryPath = FileManager.kadaiFileDir + PUBNO;
			DirectoryInfo diSource = new DirectoryInfo(kadaiDirectoryPath);

			if (!diSource.Exists || diSource.EnumerateFiles(@"*").Count() < 1)
			{
				record.task = "なし";
			}
			else
			{
				record.task = "あり";
			}

			// ミッションの抽出
			xml = XElement.Load(filteringPath + filteringFileName);
			IEnumerable<XElement> filters = xml.Elements("pub").Elements("filter-list").Elements("filter").
				Elements("full-model-code").Elements("transmission");

			// カンマ区切り
			record.mission = string.Join(",", filters.Select(e =>e.Value));

			record.insertUserId = null;
			record.updateUserId = null;

			record.pubNo = PUBNO;

			// SQL実行
			try
			{
				// pub_noデータ追加
				int maxImportVersion = mapper.QueryForObject<int>("SelectPubnoMaxImportVersion", PUBNO);
				if (maxImportVersion > 0)
				{
					// t_pub_noテーブルの旧バージョンフラグ更新
					mapper.Update("UpdatePubNo", PUBNO);
				}
				//T_PUB_NOに登録されている同じPUB_NOのデータのMAX+1（既存データが存在しない場合1）
				record.importVersion = maxImportVersion + 1;

				mapper.Insert("InsertPubNo", record);
                PUBNO_ID = mapper.QueryForObject<int>("SelectMaxPubNoId", null);
				IMPORT_VERSION = record.importVersion;
			}
			catch (Exception ex)
			{
				CLogger.Err(ex);
				BatchBase.AppendErrMsg("ERR_FILE_READ", PUBNO, filteringFileName);
				throw (ex);
			}
		}

		/// <summary>
		/// parts.xmlからコードを搔き集める
		/// </summary>
		/// <returns>parts-ids,codeのDictionary</returns>
		private Dictionary<string, List<string>> GetPartsXMLCodes()
        {
			string xmlFilePath = FileManager.localDataDir + PUBNO + FileManager.partsXmlDir;
			string xmlFileName = xmlFilePath + @"parts.xml";
			string codepattern = @"(code=)([A-Za-z0-9\\s]*)$";

			Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();

			XElement xml = XElement.Load(xmlFileName);
			IEnumerable<XElement> parts = xml.Elements("pub").Elements("term").Elements("parts");
			foreach (XElement part in parts)
			{
				List<string> codeList = new List<string>();
				IEnumerable<XElement> elList = from el in part.Descendants("location")
											select el;
				foreach (XElement el in elList)
				{
					Match rlt =Regex.Match((el.Attribute("linkkey").Value), codepattern);
					if (rlt.Success)
					{
						//code=D16,D17,...集める
						codeList.AddRange((rlt.Groups[2].Value).Split(','));
					}
				}
				codeList.Distinct();
				dic.Add(part.Attribute("parts-id").Value,codeList);
			}

		return dic;
		}
		/// <summary>
		/// code/品名コードの搔き集め
		/// </summary>
		/// <param name="list"></param>
		/// <returns>code,[parts-id1,2...]</returns>
		private Dictionary<string, string> GetHinmeiCodes()
		{
			//parts.xmlからコードを搔き集める
			Dictionary<string, List<string>> PartsXMLcodes = GetPartsXMLCodes();

			//parts-id複数場合、連結する
			Dictionary<string, List<string>> res = new Dictionary<string, List<string>>();
			// Keys	は parts-id
			foreach (string item in PartsXMLcodes.Keys)      
			{

				foreach (string code in PartsXMLcodes[item])
                {
                    if (!res.Keys.Contains(code))
                    {
						List<string> val = new List<string>();
						res[code] = val;
					}
					res[code].Add(item);
					res[code].Distinct();
				}
			}

			//parts-id複数場合、連結する
			Dictionary<string, string> resDic = new Dictionary<string, string>();
			foreach (var code in res.Keys)
            {
				resDic[code]= string.Join(",", res[code].Distinct().ToArray());
			}
			return resDic;
		}

		/// <summary>
		/// 品名コードのセット
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public List<CTParts> SetHinmeiCodes(List<CTParts> list)
		{
			string xmlFilePath = FileManager.localDataDir + PUBNO + FileManager.partsXmlDir;
			string xmlFileName = xmlFilePath + @"parts.xml";

			Dictionary<string, string> HinmeiCodeDic = GetHinmeiCodes();

            //品名コードのセット
            foreach (CTParts rec in list)
            {
                if (HinmeiCodeDic.Keys.Contains(rec.newPartsCd))
                {
					rec.hinmeiCd = HinmeiCodeDic[rec.newPartsCd];
                }
                else
                {
					rec.hinmeiCd = "ダミー品名コード";
					BatchBase.AppendErrMsg("WNG_DATA_LOSS", "ファイル:" + xmlFileName + "  Code:" + rec.newPartsCd);
					CLogger.Logger("WNG_DATA_LOSS", "ファイル:" + xmlFileName + "   Code:" + rec.newPartsCd);
				}
            }
			return list;
		}
	}
	
}
