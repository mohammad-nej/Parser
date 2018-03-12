using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;//baraye background worker

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    
    public partial class MainWindow : Window
    {
         public MainWindow()
        {
            InitializeComponent();
            
            rtxMain.FontSize = 18;
            //rtxMain.FontWeight = FontWeights.Bold;
            
            
        }
        private LexerOutPut lexer_output;
        private Parser pars;
        private Visualizer visual;
        private Lexer lex;
        private TextPointer beforeEnter;
        private Report reporter;

        private void richTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            //This part is for coloring the text while typing. You can uncomment the code below
            //if you want this feature.it may reduce the speed and it's unstable.

            /*if(visual!=null && lex!=null)    
            if (visual.FirstTime == true && lex.FirstTime == true)
            {
                

                lex.getLastChar(beforeEnter);
            }*/
           
            
        }

        private void btnLexer_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnRead_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Defaults
            lexer_output = new LexerOutPut();
            txtGrammer.Text = "SLRgrammer.txt";
            txtLexer.Text = "DFA.txt";
            txtKeywords.Text = "KeyWords.txt";

            //Combo Box <=> Grid1
            comboBox.Items.Add("Grammer");
            comboBox.Items.Add("First(s)");
            comboBox.Items.Add("Follow(s)");
            comboBox.Items.Add("SLR DFA");
            comboBox.Items.Add("LR Table");
            comboBox.Items.Add("SLR Table");
            comboBox.Items.Add("LL Table");


            //Combo Box2 <=> Grid2
            comboBox2.Items.Add("Grammer");
            comboBox2.Items.Add("First(s)");
            comboBox2.Items.Add("Follow(s)");
            comboBox2.Items.Add("SLR DFA");
            comboBox2.Items.Add("LR Table");
            comboBox2.Items.Add("SLR Table");
            comboBox2.Items.Add("LL Table");
            

            comboBox1.Items.Add("LL Parser");
            comboBox1.Items.Add("LR Parser");
            comboBox1.Items.Add("SLR Parser");
            comboBox1.SelectedIndex = 1;

            rtxMain.IsEnabled = false;
            comboBox.IsEnabled = false;
            comboBox2.IsEnabled = false;
            button1.IsEnabled = false;
            comboBox1.IsEnabled = false;
            btnNext.IsEnabled = false;
            btnStop.IsEnabled = false;
            chkDetail.IsEnabled = false;
            chkFinish.IsEnabled = false;

            rdbText.IsChecked = true;//Lexer output default

            chkDetail.Click += chkDetail_CheckedChanged;

            btnGrammerOpenFile.Click -= btnKeyWordsOpen_Click;
            btnGrammerOpenFile.Click += btnGrammerOpenFileClick;

            
            btnDFAOpenFile.Click += btnDFAOpenFileClick;
            btnDFAOpenFile.Click -= btnKeyWordsOpen_Click;

            btnKeyWordsOpenFile.Click += btnKeywordsOpenFileClick;
            btnKeyWordsOpenFile.Click -= btnKeyWordsOpen_Click;
        }
        private void btnGrammerOpenFileClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(txtGrammer.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnKeywordsOpenFileClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(txtKeywords.Text);
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnDFAOpenFileClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(txtLexer.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void rtxMain_SelectionChanged(object sender, RoutedEventArgs e)
        {
         
        }

        private void rtxMain_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

           string text = rtxMain.Selection.Text;
            beforeEnter = rtxMain.Selection.Start;
        }

        private void rtxMain_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            
        }
        
        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            //Start button
            reporter = new Report(pars,lex);
            string reason = "";
            button.IsEnabled = false;
            rtxMain.IsEnabled = false;

            rdbText.IsEnabled = false;
            rdbType.IsEnabled = false;
            if (!lex.staticLexer()){
                  if (comboBox1.SelectedIndex == 0)
                {
                    
                    if (!pars.isLL)
                        MessageBox.Show("This Grammer is not LL!\r\nReason(s):"+pars.notLLReason,"Not LL",MessageBoxButton.OK,MessageBoxImage.Warning);
                     reason= pars.parsWithTable();
                    
                }
                  else if (comboBox1.SelectedIndex == 1)
                {
                    if(!pars.isLR)
                        MessageBox.Show("This grammer is not LR! Reason(s):\r\n" + pars.notLRReason, "Not LR", MessageBoxButton.OK, MessageBoxImage.Warning);
                    reason = pars.SLRParser(pars.LRTable);
                }
                      
                  else if(comboBox1.SelectedIndex== 2) {
                    if (!pars.isSLR)                    
                        MessageBox.Show("This grammer is not SLR! Reason(s):\r\n"+pars.notSLRReason, "Not SLR", MessageBoxButton.OK, MessageBoxImage.Warning);
                    reason = pars.SLRParser(pars.SLRTable);

                }
                MessageBox.Show(reason);

            }
             
            else
                MessageBox.Show("Lexer Error","Error",MessageBoxButton.OK,MessageBoxImage.Error);
            rtxMain.IsEnabled = true;
            button.IsEnabled = true;

            rdbType.IsEnabled = true;
            rdbText.IsEnabled = true;
        }



        private void lvUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //Load button
            rtxMain.IsEnabled = false;
            comboBox.IsEnabled = false;
            comboBox1.IsEnabled = true;
            comboBox2.IsEnabled = false;
            button1.IsEnabled = false;
            btnNext.IsEnabled = false;
            btnStop.IsEnabled = false;
            chkFinish.IsEnabled = false;
            chkDetail.IsEnabled = false;

            try
            {
                visual = new Visualizer(rtxMain);
                lex = new Lexer(rtxMain, txtLexer.Text, txtKeywords.Text);
                pars = new Parser(lex,lexer_output, txtGrammer.Text, chkFinish, gridStack);
                if (pars.grammerFormatOk)
                {
                    reporter = new Report(pars, lex);
                    
                    //Enableing controls
                    rtxMain.IsEnabled = true;
                    comboBox.IsEnabled = true;
                    comboBox2.IsEnabled = true;
                    button1.IsEnabled = true;
                    btnStop.IsEnabled = true;
                    btnNext.IsEnabled = true;
                    chkDetail.IsEnabled = true;
                    chkFinish.IsEnabled = true;
                    if (pars.isLeftRecursive)
                    {
                        MessageBox.Show("This grammer is Left-Recursive! Parser may not work correctly!\r\nFinish checkbox is disabled to avoid infinit loop!\r\nReason:" + pars.LeftRecursiveReason,"Warning",MessageBoxButton.OK,MessageBoxImage.Warning);

                        chkFinish.IsChecked = false;
                        chkFinish.IsEnabled = false;
                    }
                }
                else
                {
                    //If the grammer input format is incorrect, it will open the grammer file
                    System.Diagnostics.Process.Start(txtGrammer.Text);
                }
            }catch(Exception er)
            {
               MessageBox.Show(er.Message);
            }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void btnDFAOpen_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog of = new System.Windows.Forms.OpenFileDialog();
            
            var result = of.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK)
            { 
                try
                {
                   string file = of.FileName;
                   txtLexer.Text = file;
                }catch(Exception f)
                {
                    MessageBox.Show(f.Message);
                }


            }
        }

        private void btnKeyWordsOpen_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog of = new System.Windows.Forms.OpenFileDialog();

            var result = of.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            { 
                try
                {
                    string file = of.FileName;
                    txtKeywords.Text = file;
                }
                catch (Exception f)
                {
                    MessageBox.Show(f.Message);
                }


            }
        }

        private void btnGrammerOpen_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog of = new System.Windows.Forms.OpenFileDialog();

            var result = of.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {//agar ok shod 
                try
                {
                    string file = of.FileName;
                    txtGrammer.Text = file;
                }
                catch (Exception f)
                {
                    MessageBox.Show(f.Message);
                }


            }
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                for (int i = 0; i < reporter.dataSet.Tables.Count; i++)
                {
                    if (comboBox.SelectedValue.ToString() == reporter.dataSet.Tables[i].TableName)
                    {
                        Grid1.ItemsSource = reporter.dataSet.Tables[i].DefaultView;
                        break;
                    }
                }
            }catch(Exception f)
            {
                MessageBox.Show(f.Message);
            }
        }

        private void comboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                for (int i = 0; i < reporter.dataSet.Tables.Count; i++)
                {
                    if (comboBox2.SelectedValue.ToString() == reporter.dataSet.Tables[i].TableName)
                    {
                        Grid2.ItemsSource = reporter.dataSet.Tables[i].DefaultView;
                        break;
                    }
                }
            }catch(Exception f)
            {
                MessageBox.Show(f.Message);
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
                pars.Resume = true;
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            pars.Resume = true;
            pars.ParserStop = true;
        }

        private void chkDetail_Checked(object sender, RoutedEventArgs e)
        {

        }
        private void chkDetail_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (chkDetail.IsChecked == true)
                pars.DetailedDescription = true;
            else
                pars.DetailedDescription = false;
        }

        private void radioButton_Checked(object sender, RoutedEventArgs e)
        {
            //rdbType
            lexer_output.output = LexerOutPut.OutPut.TYPE;
        }

        private void rdbText_Checked(object sender, RoutedEventArgs e)
        {
            //rdbText
            lexer_output.output = LexerOutPut.OutPut.TEXT;
        }
    }
}
