using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDRA_Parser.Model
{
    public class ViolationItem
    {
        public string ViolationNumber { get; set; }
        public string Location { get; set; }
        public int idNumber { get; set; }
        public string MainLocation { get; set; } // 추가
        public string LineNumber { get; set; } // 추가

        public bool isDiff { get; set; } // 다른 항목에 하이라이트를 주기 위한 속성 default는 true

        // 기본 생성자
        public ViolationItem()
        {
            // isDiff의 기본값 설정
            isDiff = true;
        }

        // isDiff를 제외한 값들로 생성
        public ViolationItem(string violationNumber, string location, int idNumber, string mainLocation, string lineNumber)
        {
            ViolationNumber = violationNumber;
            Location = location;
            this.idNumber = idNumber;
            MainLocation = mainLocation;
            LineNumber = lineNumber;
            isDiff = true; // 기본값 설정
        }

        public bool IsSame(ViolationItem item)
        {

            if (this.ViolationNumber != item.ViolationNumber) return false;

            else if (this.Location != item.Location) return false;


            else if (this.MainLocation != item.MainLocation) return false;

            else if (this.LineNumber != item.LineNumber) return false;

            return true;

        }
    }

}