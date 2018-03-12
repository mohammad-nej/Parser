using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;//baraye estefade az TextPointer
using System.Windows.Media;
using System.IO;
namespace WpfApplication1
{
    class Visualizer
    {

        private System.Windows.Controls.RichTextBox workingBox { get; set; }
        private Brush keyWordColor { get; set; }
        private Brush commentColor { get; set; }
        private Brush LexerColor { get; set; }
        private Brush textColor { get; set; }
        private string commentCharacter { get; set; }
        public  bool  FirstTime { get; set; }
        
        public Visualizer (System.Windows.Controls.RichTextBox temp) {
            workingBox = temp;
            keyWordColor = Brushes.DarkBlue;
            textColor = Brushes.Black;
            FirstTime = true;
            commentColor=Brushes.DarkGreen;
            LexerColor = Brushes.DarkRed;
        }
        

   
        public void ColorText (WordPointer word , WordType type,TextPointer entered){

            FirstTime = false;
            if (word.EndingPosition == null||word.StartingPoistion==null)
                return;
            workingBox.Selection.Select(word.StartingPoistion,word.EndingPosition);
            switch (type.type) { 
                    
                case WordType.boundType.TEXT :
                    
                    workingBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, textColor);
                    
                    break;
                case WordType.boundType.KEYWORD:
                    workingBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, keyWordColor);
                    break;
                case WordType.boundType.LEXERROR:
                    workingBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, LexerColor);
                    break;
                case WordType.boundType.COMMENT:
                    workingBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkGreen);
                    break;
                case WordType.boundType.STRING:
                    workingBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.DarkOrange);
                    break;
            }
            workingBox.Selection.Select(entered, entered);//in khat bayad bashe ta bad az in ke karemun tamum shod selection
            //bardashte beshe 
            FirstTime = true;
        }
    }
}
