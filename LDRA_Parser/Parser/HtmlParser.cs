using LDRA_Parser.Model;
using OpenQA.Selenium.DevTools.V124.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LDRA_Parser.Parser
{
    class HtmlParser
    {
        private List<ViolationItem> violations;
        public List<ViolationItem> popupHTMLPasing(string htmlPath) 
        {
            violations = new List<ViolationItem>();
            // HTML 내용을 문자열로 읽어옴
            string HtmlContent = File.ReadAllText(htmlPath);

            // 정규 표현식 패턴을 정의
            string pattern = @"<b>Violation Number</b> : (\d+ - .+?) &nbsp;&nbsp;&nbsp; <b>Location</b>  : <a href = '(.+?)'.*?>(.+?)</a> - <a href=.*?>(\d+)</a>";
            // 정규 표현식을 사용하여 데이터를 추출
            MatchCollection Matches = Regex.Matches(HtmlContent, pattern);


            violations.Clear();
            int id = 0; // 고유의 id번호 
            foreach (Match match in Matches)
            {
                id++;
                string violationNumber = match.Groups[1].Value;
                string location = match.Groups[2].Value;
                string mainLocation = match.Groups[3].Value; // main
                string lineNumber = match.Groups[4].Value; // 6

                string result = $"Violation Number : {violationNumber}     Location : {location}";
                violations.Add(new ViolationItem { ViolationNumber = violationNumber, Location = location, MainLocation = mainLocation, LineNumber = lineNumber, idNumber = id });
            }
            Console.WriteLine("----------");
            Console.WriteLine(violations.Count);
            return violations;
        }
    }
}
