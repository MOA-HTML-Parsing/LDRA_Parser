using LDRA_Parser.Model;
using System.IO;
using System.Text.RegularExpressions;

namespace LDRA_Parser.Parser
{
    class HtmlParser // 세부 문서 HTML 파싱 클래스
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
                string violationNumber = match.Groups[1].Value; // 추출한 데이터를 저장
                string location = match.Groups[2].Value; // 추출한 데이터를 저장
                string mainLocation = match.Groups[3].Value; // 추출한 데이터를 저장 
                string lineNumber = match.Groups[4].Value; // 추출한 데이터를 저장 

                string result = $"Violation Number : {violationNumber}     Location : {location}";
                violations.Add(new ViolationItem { ViolationNumber = violationNumber, Location = location, MainLocation = mainLocation, LineNumber = lineNumber, idNumber = id });
            }
            Console.WriteLine("----------");
            Console.WriteLine(violations.Count);
            return violations;
        }
    }
}
