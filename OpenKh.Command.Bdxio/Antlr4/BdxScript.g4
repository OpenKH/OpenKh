grammar BdxScript;

prog: NL* (statement NL*)* EOF;
statement:
	section
	| equ
	| label ':' NL
	| label ':' order
	| order;

label: name = id;

equ: name = id 'equ' numberdata;

order: db | resb | dw | resw | instruction | include;

dw: 'dw' worddata (',' worddata)* NL;
db: 'db' bytedata (',' bytedata)* NL;
resb: 'resb' (numberdata | id) NL;
resw: 'resw' (numberdata | id) NL;

instruction: id (arg (',' arg)*)? NL;

arg: (numberdata | id);
offset: 'offset' id;
bytedata: numberdata | id | StringData;
worddata: numberdata | id;
numberdata:
	decimalnumber = DEC
	| '0x' hexnumber = (DEC | HEX)
	| floatnumber = FLOATNUM;
hexbody: DEC | HEX;
id: DEC | HEX | ID;
include: '%include' FileName NL;
section: 'section' section_id NL;
section_id: '.text' | '.bss';

StringData: '\'' ~[']* '\'';
DEC: [-+]? [0-9]+;
HEX: [0-9a-fA-F]+;
FLOATNUM:
	[-+]? ([0-9]* '.' [0-9]+)
	| ([0-9]+ [eE] [-+]? [0-9]+);
ID: [a-zA-Z_][a-zA-Z0-9_]* ('.' [a-zA-Z0-9_]+)*;
WS: [ \t] -> skip;
NL: '\r'? '\n' | '\r';
FileName: '"' ~["]* '"';

LINE_COMMENT: ';' ~[\r\n]* -> skip;
