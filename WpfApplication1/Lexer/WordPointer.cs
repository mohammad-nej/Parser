using System;
//using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
namespace WpfApplication1
{
    class WordPointer
    {
        //har token ke ma darim dar vaghe ye object az class WordPointere 
        //ke mokhtaraste shuru,payan, matn token,type token,ok budanesh az nazar lexer va ... moshakhas shode
        
        public TextPointer StartingPoistion { get; set; }
        public TextPointer EndingPosition{get;set;}
        public string text { get; set; }
        public bool isNull { set; get; }
        public bool lexingOK { get; set; }
        public bool isComment { get; set; }
        public bool isString { get; set; }
        public string Type { get; set; }
        public WordPointer(string dollar) { // in yeki faghat baraye $ moghe estefade az tabe seeToken() estefade mishe
            text = dollar;
            isComment = false;
            lexingOK=true;
            isString=false;
            Type = dollar;
        }
        public WordPointer(TextPointer Start, TextPointer End) {
            if(End !=null)
            if (Start != null )
            {
                StartingPoistion = Start;
                EndingPosition = End;
                text = new TextRange(StartingPoistion, EndingPosition).Text;
                isNull = false;
            }
            else {
                text = "";
                isNull = true;
            }
            
        }
        
    }
}
