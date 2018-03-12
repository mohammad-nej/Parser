# Parser for learners
This is a parser that can get the grammer from input and can parse any input string. it comes with a default lexer (although you can change it).
This parser can also report many things including : 
 First and Follow sets
 LL,LR,SLR parse Tables
 LR pars DFA
 Parser stack (while parsing)
You can also see the detailed actions that parser takes while parsing. It can show you the pars proccess step by step.
# Error detection
 This application checks your input grammer to see if it's Left-Recursive or not. If it's Left-Recursive it will you a message and will
 explain why it's Left-Recursive so you can use it to check your grammer if it's Left-Recursive or not.
#Default types
Lexer will return : 
* "num" for numbers
* "string" for strings ( sorrounded by """)
* some aritmathic operations and stuff like that including : => <= == != = ; , ! ( ) [ ] types are equal to their texts
* "id" for everything else
NOTE: If you have a keyword like "While" in you grammer, in order to make it work you have to declare "While" as a keyword in "keyword.txt" file. The type of a keywords is equal to it's Text.

# Lexer Output feature
 In general Lexer should detect the Tokens and returns the Type of the Tokens to the parser. Default Type for every terminal is "id" unless you explicitly declare it as a keyword in "keyword.txt" file.
 Although this is the normal approach but there is another option in this application. You can define the Lexer output with 2 radioButtons in the UI. If you select the
 "Text" button then the lexer will simply returns the "Text" of the Token to the parser. for example consider this grammer:
 
  start-><a> world
  a->hello
  ********
  input string: hello world
  ****************************
  On Type-mod you need to define "hello" and "world" in the "keyword.txt" file to make the parser works because as I said before the default Type for lexer is "id" and this can cause you a lot of headaches when working with such a simple grammers.
  But on Text-mod since the lexer will return the text of the tokens (in this case "hello" and "world") then the parser will accept this input even without changing the "keyword.txt" file.
  IMPORTANT: Keep in mind that "Text-mod" is just for simple grammers so you may not be able to pars real world grammers on this mod since you dont have access to token types.

