using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace PdfReader;

[Serializable()]
public class PDFText
{
    public string? FileName { get; set; }
    [JsonInclude]
    public List<PDFTextpage> Pages = new List<PDFTextpage>();

    public PDFText(string fileName)
    {
        if(fileName is not null && File.Exists(fileName))
            this.FileName = fileName;
    }
}

[Serializable()]
public class PDFTextpage
{
    public int PageNumber { get; set; }
    [JsonInclude]
    public List<string> BlockText = new List<string>();
    [JsonInclude]
    public List<string> Words = new List<string>();
    [JsonIgnore]
    public List<byte[]> Images = new List<byte[]> ();

    public PDFTextpage(int pagenumber)
    {
        this.PageNumber = pagenumber;
    }
}








/*
string pdffilename = "TypeScript_Cookbook__Stefan_Baumgartner"; // "VN"; // "TypeScript_Cookbook__Stefan_Baumgartner.pdf";

//list_of_pdftext.ForEach(json => File.WriteAllText(json, json))

// "C:\\Users\\risto\\Downloads\\Vacancy Notice EEA-AD-2024-17.pdf");
// C:\Users\risto\source\repos\PdfReader\PDFs
// C:\Users\risto\source\repos\PdfReader\PDFs\TypeScript_Cookbook__Stefan_Baumgartner.pdf
//"C://Temp//VN.pdf";
//var pdfFileNamePath = "C:\\Users\\risto\\Downloads\\VN.pdf";


var pdfFileNamePath = @"PDFs//" + pdffilename + @".pdf"; // TypeScript_Cookbook__Stefan_Baumgartner.pdf";
var pdfFileNamePath2 = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
var pdfFileNamePath3 = Environment.SpecialFolder.MyDocuments.ToString();


Reader reader = new Reader(); 
reader.SourceDirectory = "C:\\Users\\risto\\Downloads\\";
var pdftext = reader.ReadPdfParagraphs(pdfFileNamePath);
//Console.WriteLine(txt);

string fileName = pdffilename + @".json";

var options = new JsonSerializerOptions { WriteIndented = true };
if (pdftext is not null && pdftext.Pages.Count > 0)
{
    string jsonString = JsonSerializer.Serialize<PDFText>(pdftext, options);
    File.WriteAllText(fileName, jsonString);

}
*/


//Person anotherPerson = (Person)BinaryFormatter.Deserialize(serializedPerson);
//byte[] serializedPerson = BinaryFormatter.Serialize(nestedTestClass);
