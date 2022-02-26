using BT0101.DBClass;
using BT0101Batch;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BT0101.FileClass
{
    class CFXLS : CFBase
	{

		/// <summary>
		/// 部品名称(W/H図面)・コネクタ品番の取得
		/// </summary>
		/// <param name="alist"></param>
		/// <returns>コード、部品名称(W/H図面)・コネクタ品番</returns>
		public List<CTParts> SetPartsNameHinnban(List<CTParts> alist)
		{
			string partsFilePath = FileManager.localDataDir + PUBNO + FileManager.pubNoPartsXlsDir;
			string partsFile = partsFilePath + PUBNO + @"-Parts.xls";

			if (!File.Exists(partsFile))
            {
				CLogger.Logger("ERR_NOTEXIST_FILE", partsFile);
				BatchBase.AppendErrMsg("ERR_NOTEXIST_FILE", partsFile);

				throw new Exception(partsFile + @"ファイルが見つかりませんでした。");
			}

			IWorkbook book = WorkbookFactory.Create(partsFile);
			ISheet sheet = book.GetSheet("Parts");

			// xlsファイルの中身を辞書化する。
			// 対象コラム：1,6,7
			Dictionary<string, string[]> dic = new Dictionary<string, string[]>();

			//B2から走査
			int idxRow = 0;
			while(true) 
			{
				idxRow++;
				if (sheet.GetRow(idxRow) == null) break;
				var row = sheet.GetRow(idxRow);
				var cell = row.GetCell(1);
				var cellValue = cell.StringCellValue;
				
				string[] dualItems = new string[2];

				dualItems[0] = row.GetCell(6).StringCellValue;
				dualItems[1] = row.GetCell(7).StringCellValue;
				if(!dic.Keys.Contains(cellValue) && !string.IsNullOrEmpty(cellValue))
					dic.Add(cellValue, dualItems);
			}

			//コードに紐付く部品名称(W/H図面)・コネクタ品番の取得
			foreach (CTParts rec in alist)
            {
				if(dic.ContainsKey(rec.newPartsCd))
                {
					rec.whPartsName = dic[rec.newPartsCd][0];
					rec.connectorHinban = dic[rec.newPartsCd][1];
                }
                else
                {
					rec.whPartsName = "";
					rec.connectorHinban = "";
					BatchBase.AppendErrMsg("WNG_DATA_LOSS", "ファイル:" + partsFile + "  Code:" + rec.newPartsCd);
					CLogger.Logger("WNG_DATA_LOSS", "ファイル:" + partsFile + "   Code:" + rec.newPartsCd);
                }
            }
			return alist;
        }
		/// <summary>
		/// ワイヤーネームリストの取得
		/// </summary>
		/// <param name="alist"></param>
		/// <returns>コード、ワイヤーネームリスト</returns>
		public List<CTParts> SetWireNameList(List<CTParts> alist)
		{
			string wireNameListFilePath = FileManager.localDataDir + PUBNO + FileManager.pubNoWireNameListDir;
			string wireNameListFile = wireNameListFilePath + PUBNO + @"-WireNameList.xls";

			if (!File.Exists(wireNameListFile))
			{
				CLogger.Logger("ERR_NOTEXIST_FILE", wireNameListFile);
				BatchBase.AppendErrMsg("ERR_NOTEXIST_FILE", wireNameListFile);
				throw new Exception(wireNameListFile + @"ファイルが見つかりませんでした。");
			}

			IWorkbook book = WorkbookFactory.Create(wireNameListFile);
			var sheet = book.GetSheet("Wire名称");

			// xlsファイルの中身を辞書化する。
			// 対象コラム：0,3
			Dictionary<string, string> dic = new Dictionary<string, string>();

			int idxRow = 0;
			while (true)
			{
				idxRow++;
				if (sheet.GetRow(idxRow) == null) break;

				var row = sheet.GetRow(idxRow);
				var cell = row.GetCell(0) ;
				var cellValue = cell.StringCellValue;
				if (string.IsNullOrEmpty(cellValue)) break;

				if (!dic.Keys.Contains(cellValue) && !string.IsNullOrEmpty(cellValue))
					dic.Add(cellValue, row.GetCell(3).StringCellValue);
			}

			foreach (CTParts rec in alist)
			{
				if (dic.ContainsKey(rec.newPartsCd.Substring(0, 1)))
				{
					rec.wireNameList = dic[rec.newPartsCd.Substring(0, 1)];
				}
				else
				{
					rec.wireNameList = "";
					BatchBase.AppendErrMsg("WNG_DATA_LOSS", "ファイル:" + wireNameListFile + "  Code:" + rec.newPartsCd);
					CLogger.Logger("WNG_DATA_LOSS", "ファイル:" + wireNameListFile + "   Code:" + rec.newPartsCd);
				}
			}
			return alist;
		}
		private void ImportRefNoTableData()
		{
            try
            {
				string refNoTablePath = FileManager.localDataDir + FileManager.sekkeiCheckFileDir;
				if(!Directory.Exists(refNoTablePath))
				{
					return;
				}
				string[] targetFiles = Directory.GetFiles(refNoTablePath, "*");

				foreach (string file in targetFiles)
				{
					// 設計チェック送付Ref_No管理表.xlsより、"開発No.(号口No.)"、"Pub. No." 抽出
					IWorkbook book = WorkbookFactory.Create(file);
					var sheet = book.GetSheet("EWD SH");
					if (sheet == null)
                    {
						BatchBase.AppendErrMsg("WNG_NOT_SHEET", file);
						CLogger.Logger("WNG_NOT_SHEET", file);
					}
						
					if(sheet.Header == null)
                    {
						BatchBase.AppendErrMsg("WNG_NOT_HEADER", file);
						CLogger.Logger("WNG_NOT_HEADER", file);
					}

					CTDevelopmentCd record = new CTDevelopmentCd();
					record.insertUserId = null;
					record.updateUserId = null;

					int idxRow = 0;
					while (true)
					{
						idxRow++;
						if (sheet.GetRow(idxRow) == null) break;
						var row = sheet.GetRow(idxRow);
						var cell_2 = row.GetCell(2);    // 開発No.(号口No.)
						var cell_3 = row.GetCell(3);    // Pub. No.
						string cellValue = cell_2.StringCellValue;
						if (string.IsNullOrEmpty(cellValue)) break;

						record.pubNo = cell_3.StringCellValue;	// Pub. No.

						cellValue = Regex.Replace(cell_2.StringCellValue, @"[\s]+", "");   // 開発No.(号口No.)
						var regex = new Regex(@"[\(（].*?[\)）]");
						record.developmentCd = regex.Replace(cellValue, "");

						var partsValidator = ValidationFactory.CreateValidator<CTDevelopmentCd>();

						var partsValidateResult = partsValidator.Validate(record);
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

						// T_DEVELOPMENT_CD 更新
						int cnt = mapper.QueryForObject<int>("SelectDevelopmentCd", record);
						if (cnt == 0)
						{
							// T_DEVELOPMENT_CD 登録
							mapper.Insert("InsertDevelopmentCd", record);
                        }
                        else
                        {
							mapper.Update("UpdateDevelopmentCd", record);
						}
					}
				}
			}
			catch (Exception ex)
			{
				BatchBase.AppendErrMsg("WNG_NOT_HEADER", "*設計チェック送付Ref_No管理表.xls");
				CLogger.Err(ex);
				//throw ex;
			}
		}
		public override void ImportFiles()
		{
			ImportRefNoTableData();
		}
	}
}
