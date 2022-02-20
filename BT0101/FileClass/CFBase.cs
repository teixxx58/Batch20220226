using BT0101.DBClass;
using BT0101Batch;
using System;
using System.Text;

namespace BT0101.FileClass
{
    abstract class CFBase
	{
		protected static DatabaseHelper mapper;

		public static string PUBNO;                   // 新規登録したPUBNO
		public static int PUBNO_ID;                    // 新規登録したPUBNO_ID
		public static int IMPORT_VERSION;              // 新規登録したIMPORT_VERSION
		public static int WIRING_DIAGRAM_ID;            // 新規登録したWIRING_DIAGRAM_ID
		public static string FIG_NAME;                 // 新規登録したFIG_NAME
		public static int PARTS_ID;                    // 新規登録したPARTS_ID
		public static int TERMINAL_ID;                 // 新規登録したTERMINAL_ID
		public static int WIRE_ID;                     // 新規登録したWIRE_ID

		public static void SetMapper(DatabaseHelper db)
		{
			mapper = db;
		}
		/////////////////////////////////////////////////////
		/// <summary>
		/// アブストラクトメソッド
		/// </summary>
		/// <param/>
		/////////////////////////////////////////////////////
		public abstract void ImportFiles();

		public static void DeletePubNo()
        {
			// T_PUB_NOにデータが存在し、T_SIMILAR_DIAGRAM_SEARCHにデータが存在しない場合、
			// T_PUB_NOとそれにつながるデータを削除
			//結線テーブル削除
			mapper.Delete("DeleteWire", CFBase.PUBNO);
			//端子テーブル削除
			mapper.Delete("DeleteTerminal", CFBase.PUBNO);
			//部品テーブル削除
			mapper.Delete("DeleteParts", CFBase.PUBNO);
			//配線図テーブル削除
			mapper.Delete("DeleteWiringDiagram", CFBase.PUBNO);
			//PUB_NOテーブル削除
			mapper.Delete("DeletePubNo", CFBase.PUBNO);
		}
	}
}
