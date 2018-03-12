using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
namespace WpfApplication1
{
    class GroupWordPointer
    {
        //vaghean chera ino ye class joda tarif kardam nemidunam :D vali in class serfan ye List<WordPointer> dorost mikone
        //hamin o bas ! mishod kolan hazfesh kard vali hesesh nabud :D
        public TextPointer currentPositionInContext;
       
      //  public WordPointer[] GroupOfWords{get;set;}
        public List<WordPointer> GroupOfWords = new List<WordPointer>();
        public bool isEmpty;
        public short arrayIndex;
        public GroupWordPointer(TextPointer current){
        isEmpty=true;
         arrayIndex=0;
         //GroupOfWords = new WordPointer[100];
         currentPositionInContext = current;
         
        }
        public void addWord(WordPointer left){
            GroupOfWords.Add(left);
            arrayIndex++;
            isEmpty = false;
        }
    }
}
