using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT0301Batch.Main
{
    class Util
    {
        private static void GeneratePDF()
        {
            string filename = "../../ffffff.pdf";
            string imageLoc = "../../sample.png";

            PdfDocument document = new PdfDocument();
            //1191×842
            List<Image> imgs = GeneratePDF1(Image.FromFile(imageLoc), 1391, 842);

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
            // Save and start View
            document.Save(filename);
        }

        /// <summary>
        /// 指定したImageを分割してPDF作成
        /// </summary>
        /// <param name="originalImage">オリジナルImage</param>
        /// <param name="pageWidth">横幅</param>
        /// <param name="pageHeight">高さ</param>
        /// <returns>分割済Image</returns>
        /// <remarks></remarks>
        public static List<Image> GeneratePDF1(Image originalImage, double pageWidth, double pageHeight)
        {
            double leftMargin = 10.0;
            double rightMargin = 10.0;
            double topMargin = 10.0;
            double bottomMargin = 10.0;

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
