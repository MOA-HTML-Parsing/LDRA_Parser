
namespace LDRA_Parser.Model
{
    /**
     *  PopUp(상세에러) 을 담기 위한 클래스
     */
    public class ViolationItem
    {
        public string ViolationNumber { get; set; } // Number_of_Violations 클릭 시 연결되는 세부 문서의 해당하는 데이터를 담는 변수
        public string Location { get; set; } // Number_of_Violations 클릭 시 연결되는 세부 문서의 해당하는 데이터를 담는 변수
        public int idNumber { get; set; } // Number_of_Violations 클릭 시 연결되는 세부 문서의 해당하는 데이터를 담는 변수
        public string MainLocation { get; set; } // Number_of_Violations 클릭 시 연결되는 세부 문서의 해당하는 데이터를 담는 변수
        public string LineNumber { get; set; } // Number_of_Violations 클릭 시 연결되는 세부 문서의 해당하는 데이터를 담는 변수

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

        public ViolationItem(string violationNumber, string location, int idNumber, string mainLocation, string lineNumber, bool isDiff)
        {
            ViolationNumber = violationNumber;
            Location = location;
            this.idNumber = idNumber;
            MainLocation = mainLocation;
            LineNumber = lineNumber;
            this.isDiff = isDiff; // 기본값 설정
        }

        /**
         * 같은지 여부를 비교할때 사용하는 함수
         * ViolationNumber, Location, MainLocation, LineNumber가 동일한지 확인한다.
         */
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