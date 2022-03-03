using System;

namespace BT0301Batch
{
    class Program
    {
        /// <summary>
        /// 過去データ取込インポート
        /// </summary>
        static void Main(string[] args)
        {
            CLogger.Logger("INFO_BATCHSTART");

            try
            {
                BatchMain batch = new BatchMain();
                batch.Run();
            }
            catch (Exception ex)
            {

                CLogger.Logger(ex.Message);
            }

            CLogger.Logger("INFO_BATCHEND");
        }
    }
}
