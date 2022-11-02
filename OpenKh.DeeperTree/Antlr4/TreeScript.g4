grammar TreeScript;

script: NL* statement* EOF;
statement: (property | array | block) NL*;

property: name = token value = token;
array: name = token NL* '[' NL* (element NL*)* NL* ']';
block: name = token NL* '{' NL* (statement NL*)* NL* '}';

element: token;
token: Bare | Quoted;

Bare: ~[ \r\n"{}\u005b\u005d]+;
Quoted: '"' ('\\' NL | '\\' . | ~["])* '"';

WS: [ \t] -> skip;
NL: '\r\n' | '\n' | '\r';

LINE_COMMENT: ';' ~[\r\n]* -> skip;
