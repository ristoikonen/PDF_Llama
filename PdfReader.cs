using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
using UglyToad.PdfPig.Tokens;

namespace PdfReader;




public class Reader
{
    public string? SourceDirectory { get; set; }
    public List<string>? AllPDFs { get; set; }
    public Reader() { }

    public Reader(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            SourceDirectory = directoryPath;
            AllPDFs = Directory.GetFiles(directoryPath, "*.pdf", SearchOption.AllDirectories).ToList();
        }
    }

    public List<string> Read_PDF_Blocks_AsJSON(string filePath)
    {
        List<string> jsonList = new List<string>();
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            return jsonList;
        }
        // read PDF file into PDFText 
        var pdftext = ReadPdfParagraphs(filePath);
        if (pdftext is not null && pdftext?.Pages.Count > 0)
        {
            // serialise PDFText to JSON string
            string json_pdftext = JsonSerializer.Serialize<PDFText>(pdftext, new JsonSerializerOptions { WriteIndented = true });
            jsonList.Add(json_pdftext);
        }
        return jsonList;
    }


    // Get blocks as json 
    public List<string> ReadJSON(List<PDFText> pdfList)
    {
        List<string> jsonList= new List<string>();

        if (pdfList is not null || pdfList?.Count >  0)
        {
            foreach (var pdf in pdfList)
            {
                pdf.Pages.ForEach(page =>
                    page.BlockText.ForEach(block =>
                        jsonList.Add(block)
                    )
                );
            }
        }
        return jsonList;
    }

    public List<string> ReadDirectorysPdfsAsJSON()
    {
        List<string> pdf_json_list = new List<string>();

        // read all PDFs in the SourceDirectory into PDFText objects and serialize them to JSON strings
        if (string.IsNullOrEmpty(SourceDirectory) || AllPDFs is not null || AllPDFs?.Count > 0)
        {
            foreach (var filePath in AllPDFs!)
            {
                if (File.Exists(filePath))
                {
                    // read PDF file into PDFText 
                    var pdftext = ReadPdfParagraphs(filePath);
                    if (pdftext != null && pdftext.Pages.Count > 0)
                    {
                        // serialise PDFText to JSON string
                        string json_pdftext = JsonSerializer.Serialize<PDFText>(pdftext, new JsonSerializerOptions { WriteIndented = true });
                        pdf_json_list.Add(json_pdftext);
                        // name file as PDFs_name.json
                        File.WriteAllText(Path.GetFileNameWithoutExtension(pdftext.FileName!) + @".json", json_pdftext);
                    }
                }
            }
        }
        return pdf_json_list;
    }
    // get text and blocks
    public string ReadPdf(string filePath)
    {
        StringBuilder text = new StringBuilder();
        using (var document = UglyToad.PdfPig.PdfDocument.Open(filePath))
        {
            foreach (var page in document.GetPages())
            {
                var pageText = ContentOrderTextExtractor.GetText(page);
                text.AppendLine(pageText);
            }
        }
        return text.ToString();
    }

    // C:\Users\risto\Downloads\Vacancy Notice EEA-AD-2024-17.pdf
    // VACANCY NOTICE EEA-AD-2024-17.pdf
    //_OceanofPDF.com_TypeScript_Cookbook_-_Stefan_Baumgartner
    public PDFText? ReadPdfParagraphs(string filePath)
    {
        StringBuilder text = new StringBuilder();
        StringBuilder pagetext = new StringBuilder();

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            return null;
        }

        PDFText pdftext = new PDFText(filePath);

        using (var document = UglyToad.PdfPig.PdfDocument.Open(filePath))
        {
            for (int i = 1; i < document.NumberOfPages; i++)
            {
                PDFTextpage txtpage = new PDFTextpage(i);
                var page = document.GetPage(i);
                var letters = page.Letters;
                // 1. Extract words
                var wordExtractor = NearestNeighbourWordExtractor.Instance;

                var words = wordExtractor.GetWords(letters);

                //words.ToList().ForEach(word => Console.WriteLine(word.Text));

                // Add pages words
                txtpage.Words.AddRange(words.Select(w => w.Text));

                // 2. Segment page
                var pageSegmenter = DocstrumBoundingBoxes.Instance;

                // Read blocks from words
                var textblocks = pageSegmenter.GetBlocks(words);
                txtpage.BlockText = new List<string>();
                txtpage.BlockText.AddRange(textblocks.Select(b => b.Text));

                // 3. Images
                foreach (var pdfImage in page.GetImages())
                {
                    //pdfImage.TryGetPng(out var bytes);
                    var rawbytes = pdfImage.RawMemory;
                    byte[]? bytearr = new byte[rawbytes.Length];
                    rawbytes.CopyTo(bytearr);
                    /*
                    pdfImage.ImageDictionary.TryGet(NameToken.Create("WidthInSamples"), out var widthToken);
                    pdfImage.ImageDictionary.TryGet(NameToken.Create("HeightInSamples"), out var heightToken);
                    // Convert tokens to appropriate types if needed.
                    var width = widthToken is NumericToken widthNumeric ? (int)widthNumeric.Int : 0;
                    var height = heightToken is NumericToken heightNumeric ? (int)heightNumeric.Int : 0;
                    */

                    if (bytearr is not null && bytearr.Length>0)
                    {
                        txtpage.Images.Add(bytearr);
                    }

                    //File.WriteAllBytes($"image_{i++}.jpeg", bytes);
                    // public bool TryGetPng([NotNullWhen(true)] out byte[]? bytes) => PngFromPdfImageFactory.TryGenerate(this, out bytes);

                }   


                // Add page 
                pdftext.Pages.Add(txtpage);

                /*
                foreach (var block in textblocks)
                {
                    // 3. Extract text from each block
                    // Fix: Use the `Text` property of `TextBlock` instead of passing `TextBlock` directly to `ContentOrderTextExtractor.GetText`
                    var blockText = block.Text;
                    //(blockText);
                    var lines = block?.TextLines;
                    if (lines != null || lines?.Count > 0)
                    {
                        foreach (var line in lines)
                        {
                            //Console.WriteLine(line);
                        }
                    }
                    text.AppendLine(blockText);
                }
                */

                }
            }
            return pdftext;
    }

    public List<string>? ReadPdfBlocks(string filePath)
    {
        List<string> blocks = new List<string>();
        StringBuilder text = new StringBuilder();
        StringBuilder pagetext = new StringBuilder();

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            return null;
        }

        PDFText pdftext = new PDFText(filePath);

        using (var document = UglyToad.PdfPig.PdfDocument.Open(filePath))
        {
            for (int i = 1; i < document.NumberOfPages; i++)
            {
                PDFTextpage txtpage = new PDFTextpage(i);
                var page = document.GetPage(i);
                var letters = page.Letters;

                // Extract words
                var wordExtractor = NearestNeighbourWordExtractor.Instance;
                var words = wordExtractor.GetWords(letters);

                // Segment page
                var pageSegmenter = DocstrumBoundingBoxes.Instance;
                
                // Read blocks from words
                var textblocks = pageSegmenter.GetBlocks(words);
                blocks.AddRange(textblocks.Select(b => b.Text));
                //txtpage.BlockText = new List<string>();
                //txtpage.BlockText.AddRange(textblocks.Select(b => b.Text));
                
                //pdftext.Pages.Add(txtpage);
            }
        }
        return blocks;
    }


}
