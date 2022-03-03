using System;
using System.ComponentModel;

namespace BT0301Batch
{

    public class ColorConst
    {
        /// <summary>
        /// 線色定数
        /// </summary>
        enum ColorEnum
        {
            [Description("#F8B500")]
            AM,
            [Description("#000000")]
            B,
            [Description("#C3AF96")]
            BE,
            [Description("#992903")]
            BR,
            [Description("#666666")]
            DG,
            [Description("#008837")]
            G,
            [Description("#999999")]
            GR,
            [Description("#00A0C6")]
            L,
            [Description("#BFB1C5")]
            LA,
            [Description("#7FC97F")]
            LG,
            [Description("#FF7F00")]
            O,
            [Description("#F668B2")]
            P,
            [Description("#FF0000")]
            R,
            [Description("#99D9E8")]
            SB,
            [Description("#56077D")]
            V,
            [Description("#FFFFFF")]
            W,
            [Description("#FFE600")]
            Y,
            [Description("#000000")]
            DEFAULT,
        }
        // 拡張メソッド
        public static string GetColor(string col)
        {
            string description = null;
            ColorEnum color = new ColorEnum();
            try
            {
                var gm = color.GetType().GetMember(col);
                var attributes = gm[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                description = ((DescriptionAttribute)attributes[0]).Description;
            }
            catch(Exception ex)
            {
                CLogger.Err(ex);
                BatchBase.AppendErrMsg("ERR_COLOR_NOT_FUND", col);
                return "#000000";
            }

            return description;
        }

    }

}
