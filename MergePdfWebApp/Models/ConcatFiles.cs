using iTextSharp.text;
using iTextSharp.text.pdf;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace MergePdfWebApp.Models
{
    public class ConcatFiles
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public void MergePDF(string File1, string File2, string outputPdfPath)
        {
            logger.Info($"ConcatFiles::MergePDF({File1}, {File2}, {outputPdfPath})");
            string[] fileArray = new string[3];
            fileArray[0] = File1;
            fileArray[1] = File2;
            PdfReader reader = null;
            Document sourceDocument = null;
            PdfCopy pdfCopyProvider = null;
            PdfImportedPage importedPage;

            //string outputPdfPath = @"C:/temp/rsAndMifrat.pdf";
            outputPdfPath = outputPdfPath + "rsAndMifrat.pdf";
            logger.Info("ConcatFiles::outputPdfPath: " + outputPdfPath);
            sourceDocument = new Document();
            pdfCopyProvider = new PdfCopy(sourceDocument, new System.IO.FileStream(outputPdfPath, System.IO.FileMode.Create));
            if (null == pdfCopyProvider)
            {
                logger.Error("ConcatFiles::pdfCopyProvider == null");

            }
            else
            {
                logger.Info("ConcatFiles::pdfCopyProvider;");
            }

            //output file Open  
            sourceDocument.Open();

            logger.Info("ConcatFiles::files list wise Loop");
            try
            {
                logger.Info($"ConcatFiles::fileArray.Length: {fileArray.Length}");
            }
            catch (Exception e)
            {
                logger.Error("ConcatFiles::Message: " + e.Message + "; StackTrace:" + e.StackTrace);
            }
            //files list wise Loop  
            for (int f = 0; f < fileArray.Length - 1; f++)
            {
                int pages = TotalPageCount(fileArray[f]);
                logger.Info("ConcatFiles::pages:" + pages);
                reader = new PdfReader(fileArray[f]);
                //Add pages in new file  
                for (int i = 1; i <= pages; i++)
                {
                    importedPage = pdfCopyProvider.GetImportedPage(reader, i);
                    pdfCopyProvider.AddPage(importedPage);
                }
                reader.Close();
            }
            //save the output file  
            logger.Info("ConcatFiles::sourceDocument.Close(); ");
            sourceDocument.Close();
        }
        private static int TotalPageCount(string file)
        {
            logger.Info("TotalPageCount start");
            using (StreamReader sr = new StreamReader(System.IO.File.OpenRead(file)))
            {
                Regex regex = new Regex(@"/Type\s*/Page[^s]");
                MatchCollection matches = regex.Matches(sr.ReadToEnd());
                try
                {
                    logger.Info($"TotalPageCount:{matches.Count} end");
                }
                catch (Exception e)
                {
                    logger.Error("TotalPageCount::Message: " + e.Message + "; StackTrace:" + e.StackTrace);
                }
                return matches.Count;
            }
        }
    }
}