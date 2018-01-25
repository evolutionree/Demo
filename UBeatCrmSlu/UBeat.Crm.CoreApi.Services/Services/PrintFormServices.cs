using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UBeat.Crm.CoreApi.Services.Models.PrintForm;
using UBeat.Crm.CoreApi.Services.Utility.OpenXMLUtility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public  class PrintFormServices:BaseServices
    {
        public PrintFormServices() {
        }
        public void TetSaveDoc(string myname) {
            WordprocessingDocument doc = WordprocessingDocument.Open("c:/tmp/template.docx", true);
            //doc.Close();
            //doc.Dispose();
            //doc = WordprocessingDocument.Open("c:/tmp/a.docx", true);
            IEnumerable<Paragraph> ps = doc.MainDocumentPart.Document.Body.Elements<Paragraph>();
            int totalCount = ps.Count();
            for (int i = 0; i < totalCount; i++) {
                Paragraph p = ps.ElementAt(i);
                int topPos = p.InnerText.IndexOf("【#我的名字#】");
                if (topPos >= 0) {
                    ReplaceInParagraph(p, "【#我的名字#】", myname);
                }
              
                
            }


            doc.SaveAs("c:/tmp/a.pdf");
            doc.Close();
            doc.Dispose();
            
        }
        private class TmpClass {
            public Text text { get; set; }
            public Run run { get; set; }
        }
        private void ReplaceInParagraph(Paragraph p, string scheme, string replace) {
            List<TmpClass> removeList = new List<TmpClass>();
            bool isStart = false;
            bool isFound = false;
            Text firstText = null;
            string firstMath = "";
            foreach (Run r in p.Elements<Run>()) {
                foreach (Text t in r.Elements<Text>()) {
                    if (t.Text == null) continue;
                    string mat = matchText(t.Text, scheme);
                    if (mat != null) {
                        if (isStart == false)
                        {
                            isStart = true;
                            firstText = t;
                            firstMath = mat;
                        }
                        else {
                            TmpClass tempClass = new TmpClass()
                            {
                                run = r,
                                text = t
                            };
                            removeList.Add(tempClass);
                        }
                        scheme = scheme.Substring(mat.Length);
                        if (scheme.Length == 0)
                        {
                            isFound = true;
                            break;
                        }
                    }
                    
                    
                }
                if (isFound) break;
            }
            if (isFound) {
                if (firstText != null) {
                    firstText.Text = firstText.Text.Replace(firstMath, replace);
                }
                foreach (TmpClass item in removeList) {
                    item.run.RemoveChild(item.text);
                }
            }
        }
        private string matchText(string t1, string t2) {
            if (t1 == null || t2 == null) return null;
            int curIndex = 0;
            bool isStart = false;
            string matchText = "";
            int t1len = t1.Length;
            for (int i = 0; i < t1len; i++) {
                if (t1[i] == t2[curIndex])
                {
                    matchText = matchText + t1[i];
                    if (isStart == false) {
                        isStart = true;
                    }
                    curIndex++;
                    if (curIndex == t2.Length) break;
                }
                else {
                    if (isStart) return null;
                }
            }
            if (isStart) return matchText;
            else return null;

        }
        private void ReplaceInRun(Run r, string scheme, string replace) {
            List<Text> removeList = new List<Text>();
            bool isStart = false;
            Text firstText = null;
            foreach (Text t in r.Elements<Text>()) {
                if (t.Text == null) continue;
                if (scheme.StartsWith(t.Text)) {
                    if (isStart == false)
                    {
                        isStart = true;
                        firstText = t;
                    }
                    else {
                        removeList.Add(t);
                    }
                    scheme = scheme.Substring(t.Text.Length);
                    if (scheme.Length == 0) break;
                }

            }
            if (removeList.Count > 0) {
                foreach (Text t in removeList) {
                    r.RemoveChild<Text>(t);
                }
            }
            if (firstText != null) firstText.Text = replace;
        }



        public byte[] GetOutputDocument(OutputDocumentParameter formData,out string fileName)
        {
            fileName = null;
            if (formData.Data != null)
            {
                if (formData.Data.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    throw new Exception("please upload a valid excel file of version 2007 and above");
                }
                var excelData= ExcelHelper.ReadExcel(formData.Data.OpenReadStream());
                if (excelData == null || excelData.Sheets == null || excelData.Sheets.Count == 0)
                    throw new Exception("error");
                //excelData.Sheets.FirstOrDefault().Rows.Add()

                foreach (var sheet in excelData.Sheets)
                {
                    //sheet.Rows.Add(sheet.Rows.Last())
                }

            }

            //ExcelHelper
            return null;
        }

    }
}
