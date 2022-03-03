using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace BT0301Batch
{
    class PDFUtil
    {

        private const double PAGE_WIDTH = 1391;
        private const double PAGE_HEIGHT = 842;

        static double leftMargin = 10.0;
        static double rightMargin = 10.0;
        static double topMargin = 10.0;
        static double bottomMargin = 10.0;

        /// <summary>
        /// PDFファイル生成
        /// </summary>
        /// <param name="filePath"></param>
        public static void GeneratePDF(string svgFilePath, string outPdfPath)
        {
            PdfDocument document = new PdfDocument();

            //SVG画像
            SvgDocument svgDoc = SvgDocument.Open(svgFilePath);
            try
            {
                //1191×842 A3用紙サイズ
                List<Image> imgs = GeneratePDF(svgDoc.Draw(), PAGE_WIDTH, PAGE_HEIGHT);

                foreach (Image img in imgs)
                {
                    // Create an empty page or load existing
                    PdfPage page = document.AddPage();
                    page.Orientation = PageOrientation.Landscape;
                    page.Size = PdfSharp.PageSize.A3;

                    // Get an XGraphics object for drawing
                    XGraphics gfx = XGraphics.FromPdfPage(page);

                    MemoryStream strm = new MemoryStream();
                    img.Save(strm, System.Drawing.Imaging.ImageFormat.Png);

                    gfx.DrawImage(XImage.FromStream(strm), 50, 10);

                }
                //PDFファイル保存
                if (!Directory.Exists(Path.GetDirectoryName(outPdfPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outPdfPath));
                }

                document.Save(outPdfPath);
            }
            catch(Exception ex)
            {
                CLogger.Err(ex);
                BatchBase.AppendErrMsg("ERR_FILE_WRITE_FAILED", outPdfPath);
            }
        }

        /// <summary>
        /// 指定したImageを分割してPDF作成
        /// </summary>
        /// <param name="originalImage">オリジナルImage</param>
        /// <param name="pageWidth">横幅</param>
        /// <param name="pageHeight">高さ</param>
        /// <returns>分割済Image</returns>
        /// <remarks></remarks>
        private static List<Image> GeneratePDF(Image originalImage, double pageWidth, double pageHeight)
        {
            double Width = pageWidth - leftMargin - rightMargin;
            double Height = pageHeight - topMargin - bottomMargin;

            int horizontalNum = (int)Math.Ceiling(originalImage.Width / Width);
            int verticalNum = (int)Math.Ceiling(originalImage.Height / Height);

            List<Image> rltImage = new List<Image>();
            for (int hCnt = 0; hCnt < horizontalNum; hCnt++)
            {
                Rectangle rect;
                if ((hCnt + 1) * Width > originalImage.Width)
                {
                    rect = new Rectangle((int)(hCnt * Width), 0,
                        originalImage.Width - (int)(hCnt * Width), originalImage.Height);
                }
                else
                {
                    rect = new Rectangle((int)(hCnt * Width), 0,
                               (int)Width, originalImage.Height);
                }
                //オリジナルImageの内容を描画
                Bitmap bmp = new Bitmap(rect.Width, rect.Height);
                Graphics g = Graphics.FromImage(bmp);
                g.DrawImage(originalImage, new Rectangle(0, 0, rect.Width, rect.Height),
                    rect, GraphicsUnit.Pixel);
                g.Dispose();

                rltImage.Add(bmp);
            }
            return rltImage;
        }
    }
}
