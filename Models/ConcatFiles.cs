using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Reporting.WebForms.Internal.Soap.ReportingServices2005.Execution;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using System.Linq;
using iText.Layout.Element;
using iText.IO.Image;
using iText.Layout;
using iText.Kernel.Geom;
using iText.Layout.Properties;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Layout.Borders;
using Org.BouncyCastle.Crypto.General;
using Org.BouncyCastle.Crypto;


namespace MergePdfWebApp.Models
{
    public class ConcatFiles
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const string HEBREW_FONT = @"C:\WINDOWS\Fonts\ARIAL.ttf";

        public void MergePDF(string File1, List<ParameterValue> parameters, string env, string outputPdfPath)
        {
            File1 = File1.Trim();
            logger.Info($"ConcatFiles::MergePDF trim({File1}, {outputPdfPath})");

            DbService db = DbService.Instance;
            if (db == null)
            {
                logger.Info("ConcatFiles::MergePDF:dbService == null ");
            }

            Dictionary<string, string> pm = GetParams(parameters);
            pm.TryGetValue("GoremYozem", out string goremYozem);
            pm.TryGetValue("Shana", out string shana);
            pm.TryGetValue("Numerator", out string numerator);
            pm.TryGetValue("MsMahadura", out string msMahadura);

            List<string> imgs = db.GetImagesOfProducts(goremYozem, shana, numerator, msMahadura, env);
            List<string> mifratim = db.GetMifratim(goremYozem, shana, numerator, msMahadura, env);

            List<string> imgAndMifrat = ConcatLists(imgs, mifratim);
            string[] fileArray = new string[imgAndMifrat.Count + 1];
            fileArray[0] = File1;

            if (imgAndMifrat.Count > 0)
            {
                for (int i = 1, j = 0; i <= imgAndMifrat.Count; i++, j++)
                {
                    fileArray[i] = imgAndMifrat[j];

                }
            }
            for (int i = 0; i < fileArray.Length; i++)
            {
                logger.Info($"ConcatFiles::MergePDF:Files to merge: {fileArray[i]}");
            }

            var finalPDF = outputPdfPath + "rsAndMifrat-" + goremYozem + "-" + shana + "-" + numerator + ".pdf";
            logger.Info("ConcatFiles::outputPdfPath: " + finalPDF);

            Byte[] finalFilesBytes;
            try
            {
                using (var finalFile = new MemoryStream())
                {
                    using (var doc = new PdfDocument(new PdfWriter(finalFile)))
                    {
                        PdfMerger merger = new PdfMerger(doc);

                        for (var i = 0; i < fileArray.Length; i++)
                        {
                            if (fileArray[i].ToLower().EndsWith(".jpg") || fileArray[i].ToLower().EndsWith(".png") || fileArray[i].ToLower().EndsWith(".jpeg"))
                            {
                                Byte[] imageBytes;
                                // Convert img to PDF and merge
                                using (var img = new MemoryStream())
                                {
                                    using (var pdfDoc = new PdfDocument(new PdfWriter(img)))
                                    {
                                        FontProgramFactory.RegisterFont(HEBREW_FONT, "MyFont");
                                        PdfFont font = PdfFontFactory.CreateRegisteredFont("MyFont", PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED, true);
                                        using (var d = new Document(pdfDoc, PageSize.A4))
                                        {
                                            d.SetMargins(36, 36, 36, 36);
                                            Table table = new Table(1);
                                            table.SetWidth(PageSize.A4.GetWidth() - 72);

                                            Cell headerCell = new Cell();
                                            Cell cell = new Cell();

                                            // Fetch shem parit
                                            string makat = fileArray[i].Substring(fileArray[i].LastIndexOf(@"\") + 1, fileArray[i].IndexOf('-') - fileArray[i].LastIndexOf('\\') - 1);
                                            string makatFormat = makat.Substring(0, 2) + "-" + makat.Substring(2, 2) + "-" + makat.Substring(4, 4);

                                            string shemParit = db.GetShemParit(makat, env);

                                            headerCell.SetHeight(36);
                                            headerCell.Add(new Paragraph()
                                                .SetFont(font)
                                                .Add(shemParit + " :" + makatFormat)
                                                .SetTextAlignment(TextAlignment.CENTER));
                                            headerCell.SetBorder(Border.NO_BORDER);
                                            table.AddHeaderCell(headerCell);

                                            Image image = new Image(ImageDataFactory.Create(fileArray[i]));
                                            image.SetAutoScale(true);
                                            cell.SetHeight(PageSize.A4.GetWidth() - 72);
                                            cell.SetNextRenderer(new ImageAndPositionRenderer(cell, 0.5f, 0.5f, image, "", TextAlignment.CENTER));
                                            cell.SetBorder(Border.NO_BORDER);
                                            table.AddCell(cell);

                                            d.Add(table);
                                            logger.Info($"ConcatFiles::MergePDF: Image has been added");
                                            d.Close();
                                        }
                                    }
                                    imageBytes = img.ToArray();
                                }
                                Stream stream = new MemoryStream(imageBytes); // Convert Byte to Stream
                                MergeToPdf(merger, stream);
                            }

                            // Merge existing PDF files
                            if (fileArray[i].ToLower().EndsWith(".pdf"))
                            {
                                MergeToPdf(merger, fileArray[i]);
                            }
                        }
                    }
                    finalFilesBytes = finalFile.ToArray();
                }
                File.WriteAllBytes(finalPDF, finalFilesBytes);
                logger.Info($"ConcatFiles::MergePDF: Wrote as expected");
            }
            catch (Exception ex)
            {
                logger.Error($"EXCEPTION: {ex}");
            }
        }

        private void MergeToPdf(PdfMerger merger, string fullPath)
        {
            using (var reader = new PdfReader(fullPath))
            {
                reader.SetUnethicalReading(true);
                using (var pdf2merge = new PdfDocument(reader))
                {
                    merger.Merge(pdf2merge, 1, pdf2merge.GetNumberOfPages());
                    pdf2merge.Close();
                }
                reader.Close();
            }
        }

        private void MergeToPdf(PdfMerger merger, Stream fullPath)
        {
            using (var reader1 = new PdfReader(fullPath))
            {
                reader1.SetUnethicalReading(true);
                using (var pdf2merge = new PdfDocument(reader1))
                {
                    merger.Merge(pdf2merge, 1, pdf2merge.GetNumberOfPages());
                    pdf2merge.Close();
                }
                reader1.Close();
            }
        }

        private Dictionary<string, string> GetParams(List<ParameterValue> parameters)
        {
            Dictionary<string, string> pm = new Dictionary<string, string>();

            int count = 0;
            foreach (ParameterValue pr in parameters)
            {
                if (pr.Name.Equals("GoremYozem") || pr.Name.Equals("Shana") || pr.Name.Equals("Numerator") || pr.Name.Equals("MsMahadura"))
                {
                    pm[pr.Name] = pr.Value;
                    count++;
                }
            }
            return pm;
        }

        // Maybe need to change algorithm
        private List<string> ConcatLists(List<string> firstList, List<string> secondList)
        {
            if (firstList.Count == 0 && secondList.Count > 0)
            {
                logger.Info($"ConcatFiles::MergePDF:ConcatLists: Mifratim list is return");
                return secondList;
            }
            if (firstList.Count == 0)
            {
                logger.Info($"ConcatFiles::MergePDF:ConcatLists: Pictures list is {firstList.Count}");
                return new List<string>();
            }
            if (secondList.Count == 0)
            {
                logger.Info($"ConcatFiles::MergePDF:ConcatLists: Mifratim list is {secondList.Count}");
                return firstList;
            }

            List<string> list = new List<string>()
            {
                firstList[0]
            };

            string s = firstList[0].Substring(0, firstList[0].IndexOf('-'));
            for (int i = 1; i < firstList.Count; i++)
            {
                if (s.Equals(firstList[i].Substring(0, firstList[i].IndexOf('-'))))
                {
                    list.Add(firstList[i]);
                    s = firstList[i].Substring(0, firstList[i].IndexOf('-'));
                }
                else
                {
                    for (int j = 0; j < secondList.Count; j++)
                    {
                        if (s.Equals(secondList[j].Substring(0, secondList[j].IndexOf('-'))))
                        {
                            list.Add(secondList[j]);
                            secondList.Remove(secondList[j]);
                        }
                    }
                    list.Add(firstList[i]);
                    s = firstList[i].Substring(0, firstList[i].IndexOf('-'));
                }
            }

            // If left adding remaining mifratim
            if (secondList.Count > 0)
            {
                foreach (string mifrat in secondList)
                {
                    list.Add(mifrat);
                }
            }

            return list;
        }
    }//class ConcatFiles
}