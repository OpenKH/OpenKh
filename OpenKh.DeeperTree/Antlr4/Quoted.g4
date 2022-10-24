lexer grammar Quoted;
Start: '"' -> skip, pushMode(QUOTE);

mode QUOTE;
End: '"' -> skip, popMode;
NL: '\\' ('\r\n' | '\n' | '\r');
Cr: '\\r';
Lf: '\\n';
Tab: '\\t';
EscapedChar: '\\' AnyChar;
AnyChar: .;
