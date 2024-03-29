//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.11.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from TreeScript.g4 by ANTLR 4.11.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.11.1")]
[System.CLSCompliant(false)]
public partial class TreeScriptParser : Parser {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		T__0=1, T__1=2, T__2=3, T__3=4, Bare=5, Quoted=6, WS=7, NL=8, LINE_COMMENT=9;
	public const int
		RULE_script = 0, RULE_statement = 1, RULE_property = 2, RULE_array = 3, 
		RULE_block = 4, RULE_element = 5, RULE_token = 6;
	public static readonly string[] ruleNames = {
		"script", "statement", "property", "array", "block", "element", "token"
	};

	private static readonly string[] _LiteralNames = {
		null, "'['", "']'", "'{'", "'}'"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, null, null, "Bare", "Quoted", "WS", "NL", "LINE_COMMENT"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "TreeScript.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static TreeScriptParser() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}

		public TreeScriptParser(ITokenStream input) : this(input, Console.Out, Console.Error) { }

		public TreeScriptParser(ITokenStream input, TextWriter output, TextWriter errorOutput)
		: base(input, output, errorOutput)
	{
		Interpreter = new ParserATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	public partial class ScriptContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode Eof() { return GetToken(TreeScriptParser.Eof, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode[] NL() { return GetTokens(TreeScriptParser.NL); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode NL(int i) {
			return GetToken(TreeScriptParser.NL, i);
		}
		[System.Diagnostics.DebuggerNonUserCode] public StatementContext[] statement() {
			return GetRuleContexts<StatementContext>();
		}
		[System.Diagnostics.DebuggerNonUserCode] public StatementContext statement(int i) {
			return GetRuleContext<StatementContext>(i);
		}
		public ScriptContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_script; } }
	}

	[RuleVersion(0)]
	public ScriptContext script() {
		ScriptContext _localctx = new ScriptContext(Context, State);
		EnterRule(_localctx, 0, RULE_script);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 17;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==NL) {
				{
				{
				State = 14;
				Match(NL);
				}
				}
				State = 19;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 23;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==Bare || _la==Quoted) {
				{
				{
				State = 20;
				statement();
				}
				}
				State = 25;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 26;
			Match(Eof);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class StatementContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public PropertyContext property() {
			return GetRuleContext<PropertyContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public ArrayContext array() {
			return GetRuleContext<ArrayContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public BlockContext block() {
			return GetRuleContext<BlockContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode[] NL() { return GetTokens(TreeScriptParser.NL); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode NL(int i) {
			return GetToken(TreeScriptParser.NL, i);
		}
		public StatementContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_statement; } }
	}

	[RuleVersion(0)]
	public StatementContext statement() {
		StatementContext _localctx = new StatementContext(Context, State);
		EnterRule(_localctx, 2, RULE_statement);
		try {
			int _alt;
			EnterOuterAlt(_localctx, 1);
			{
			State = 31;
			ErrorHandler.Sync(this);
			switch ( Interpreter.AdaptivePredict(TokenStream,2,Context) ) {
			case 1:
				{
				State = 28;
				property();
				}
				break;
			case 2:
				{
				State = 29;
				array();
				}
				break;
			case 3:
				{
				State = 30;
				block();
				}
				break;
			}
			State = 36;
			ErrorHandler.Sync(this);
			_alt = Interpreter.AdaptivePredict(TokenStream,3,Context);
			while ( _alt!=2 && _alt!=global::Antlr4.Runtime.Atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					State = 33;
					Match(NL);
					}
					} 
				}
				State = 38;
				ErrorHandler.Sync(this);
				_alt = Interpreter.AdaptivePredict(TokenStream,3,Context);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class PropertyContext : ParserRuleContext {
		public TokenContext name;
		public TokenContext value;
		[System.Diagnostics.DebuggerNonUserCode] public TokenContext[] token() {
			return GetRuleContexts<TokenContext>();
		}
		[System.Diagnostics.DebuggerNonUserCode] public TokenContext token(int i) {
			return GetRuleContext<TokenContext>(i);
		}
		public PropertyContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_property; } }
	}

	[RuleVersion(0)]
	public PropertyContext property() {
		PropertyContext _localctx = new PropertyContext(Context, State);
		EnterRule(_localctx, 4, RULE_property);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 39;
			_localctx.name = token();
			State = 40;
			_localctx.value = token();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class ArrayContext : ParserRuleContext {
		public TokenContext name;
		[System.Diagnostics.DebuggerNonUserCode] public TokenContext token() {
			return GetRuleContext<TokenContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode[] NL() { return GetTokens(TreeScriptParser.NL); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode NL(int i) {
			return GetToken(TreeScriptParser.NL, i);
		}
		[System.Diagnostics.DebuggerNonUserCode] public ElementContext[] element() {
			return GetRuleContexts<ElementContext>();
		}
		[System.Diagnostics.DebuggerNonUserCode] public ElementContext element(int i) {
			return GetRuleContext<ElementContext>(i);
		}
		public ArrayContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_array; } }
	}

	[RuleVersion(0)]
	public ArrayContext array() {
		ArrayContext _localctx = new ArrayContext(Context, State);
		EnterRule(_localctx, 6, RULE_array);
		int _la;
		try {
			int _alt;
			EnterOuterAlt(_localctx, 1);
			{
			State = 42;
			_localctx.name = token();
			State = 46;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==NL) {
				{
				{
				State = 43;
				Match(NL);
				}
				}
				State = 48;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 49;
			Match(T__0);
			State = 53;
			ErrorHandler.Sync(this);
			_alt = Interpreter.AdaptivePredict(TokenStream,5,Context);
			while ( _alt!=2 && _alt!=global::Antlr4.Runtime.Atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					State = 50;
					Match(NL);
					}
					} 
				}
				State = 55;
				ErrorHandler.Sync(this);
				_alt = Interpreter.AdaptivePredict(TokenStream,5,Context);
			}
			State = 65;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==Bare || _la==Quoted) {
				{
				{
				State = 56;
				element();
				State = 60;
				ErrorHandler.Sync(this);
				_alt = Interpreter.AdaptivePredict(TokenStream,6,Context);
				while ( _alt!=2 && _alt!=global::Antlr4.Runtime.Atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						State = 57;
						Match(NL);
						}
						} 
					}
					State = 62;
					ErrorHandler.Sync(this);
					_alt = Interpreter.AdaptivePredict(TokenStream,6,Context);
				}
				}
				}
				State = 67;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 71;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==NL) {
				{
				{
				State = 68;
				Match(NL);
				}
				}
				State = 73;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 74;
			Match(T__1);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class BlockContext : ParserRuleContext {
		public TokenContext name;
		[System.Diagnostics.DebuggerNonUserCode] public TokenContext token() {
			return GetRuleContext<TokenContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode[] NL() { return GetTokens(TreeScriptParser.NL); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode NL(int i) {
			return GetToken(TreeScriptParser.NL, i);
		}
		[System.Diagnostics.DebuggerNonUserCode] public StatementContext[] statement() {
			return GetRuleContexts<StatementContext>();
		}
		[System.Diagnostics.DebuggerNonUserCode] public StatementContext statement(int i) {
			return GetRuleContext<StatementContext>(i);
		}
		public BlockContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_block; } }
	}

	[RuleVersion(0)]
	public BlockContext block() {
		BlockContext _localctx = new BlockContext(Context, State);
		EnterRule(_localctx, 8, RULE_block);
		int _la;
		try {
			int _alt;
			EnterOuterAlt(_localctx, 1);
			{
			State = 76;
			_localctx.name = token();
			State = 80;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==NL) {
				{
				{
				State = 77;
				Match(NL);
				}
				}
				State = 82;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 83;
			Match(T__2);
			State = 87;
			ErrorHandler.Sync(this);
			_alt = Interpreter.AdaptivePredict(TokenStream,10,Context);
			while ( _alt!=2 && _alt!=global::Antlr4.Runtime.Atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					State = 84;
					Match(NL);
					}
					} 
				}
				State = 89;
				ErrorHandler.Sync(this);
				_alt = Interpreter.AdaptivePredict(TokenStream,10,Context);
			}
			State = 99;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==Bare || _la==Quoted) {
				{
				{
				State = 90;
				statement();
				State = 94;
				ErrorHandler.Sync(this);
				_alt = Interpreter.AdaptivePredict(TokenStream,11,Context);
				while ( _alt!=2 && _alt!=global::Antlr4.Runtime.Atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						State = 91;
						Match(NL);
						}
						} 
					}
					State = 96;
					ErrorHandler.Sync(this);
					_alt = Interpreter.AdaptivePredict(TokenStream,11,Context);
				}
				}
				}
				State = 101;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 105;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==NL) {
				{
				{
				State = 102;
				Match(NL);
				}
				}
				State = 107;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 108;
			Match(T__3);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class ElementContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public TokenContext token() {
			return GetRuleContext<TokenContext>(0);
		}
		public ElementContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_element; } }
	}

	[RuleVersion(0)]
	public ElementContext element() {
		ElementContext _localctx = new ElementContext(Context, State);
		EnterRule(_localctx, 10, RULE_element);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 110;
			token();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class TokenContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode Bare() { return GetToken(TreeScriptParser.Bare, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode Quoted() { return GetToken(TreeScriptParser.Quoted, 0); }
		public TokenContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_token; } }
	}

	[RuleVersion(0)]
	public TokenContext token() {
		TokenContext _localctx = new TokenContext(Context, State);
		EnterRule(_localctx, 12, RULE_token);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 112;
			_la = TokenStream.LA(1);
			if ( !(_la==Bare || _la==Quoted) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
				ErrorHandler.ReportMatch(this);
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	private static int[] _serializedATN = {
		4,1,9,115,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,7,6,1,0,
		5,0,16,8,0,10,0,12,0,19,9,0,1,0,5,0,22,8,0,10,0,12,0,25,9,0,1,0,1,0,1,
		1,1,1,1,1,3,1,32,8,1,1,1,5,1,35,8,1,10,1,12,1,38,9,1,1,2,1,2,1,2,1,3,1,
		3,5,3,45,8,3,10,3,12,3,48,9,3,1,3,1,3,5,3,52,8,3,10,3,12,3,55,9,3,1,3,
		1,3,5,3,59,8,3,10,3,12,3,62,9,3,5,3,64,8,3,10,3,12,3,67,9,3,1,3,5,3,70,
		8,3,10,3,12,3,73,9,3,1,3,1,3,1,4,1,4,5,4,79,8,4,10,4,12,4,82,9,4,1,4,1,
		4,5,4,86,8,4,10,4,12,4,89,9,4,1,4,1,4,5,4,93,8,4,10,4,12,4,96,9,4,5,4,
		98,8,4,10,4,12,4,101,9,4,1,4,5,4,104,8,4,10,4,12,4,107,9,4,1,4,1,4,1,5,
		1,5,1,6,1,6,1,6,0,0,7,0,2,4,6,8,10,12,0,1,1,0,5,6,122,0,17,1,0,0,0,2,31,
		1,0,0,0,4,39,1,0,0,0,6,42,1,0,0,0,8,76,1,0,0,0,10,110,1,0,0,0,12,112,1,
		0,0,0,14,16,5,8,0,0,15,14,1,0,0,0,16,19,1,0,0,0,17,15,1,0,0,0,17,18,1,
		0,0,0,18,23,1,0,0,0,19,17,1,0,0,0,20,22,3,2,1,0,21,20,1,0,0,0,22,25,1,
		0,0,0,23,21,1,0,0,0,23,24,1,0,0,0,24,26,1,0,0,0,25,23,1,0,0,0,26,27,5,
		0,0,1,27,1,1,0,0,0,28,32,3,4,2,0,29,32,3,6,3,0,30,32,3,8,4,0,31,28,1,0,
		0,0,31,29,1,0,0,0,31,30,1,0,0,0,32,36,1,0,0,0,33,35,5,8,0,0,34,33,1,0,
		0,0,35,38,1,0,0,0,36,34,1,0,0,0,36,37,1,0,0,0,37,3,1,0,0,0,38,36,1,0,0,
		0,39,40,3,12,6,0,40,41,3,12,6,0,41,5,1,0,0,0,42,46,3,12,6,0,43,45,5,8,
		0,0,44,43,1,0,0,0,45,48,1,0,0,0,46,44,1,0,0,0,46,47,1,0,0,0,47,49,1,0,
		0,0,48,46,1,0,0,0,49,53,5,1,0,0,50,52,5,8,0,0,51,50,1,0,0,0,52,55,1,0,
		0,0,53,51,1,0,0,0,53,54,1,0,0,0,54,65,1,0,0,0,55,53,1,0,0,0,56,60,3,10,
		5,0,57,59,5,8,0,0,58,57,1,0,0,0,59,62,1,0,0,0,60,58,1,0,0,0,60,61,1,0,
		0,0,61,64,1,0,0,0,62,60,1,0,0,0,63,56,1,0,0,0,64,67,1,0,0,0,65,63,1,0,
		0,0,65,66,1,0,0,0,66,71,1,0,0,0,67,65,1,0,0,0,68,70,5,8,0,0,69,68,1,0,
		0,0,70,73,1,0,0,0,71,69,1,0,0,0,71,72,1,0,0,0,72,74,1,0,0,0,73,71,1,0,
		0,0,74,75,5,2,0,0,75,7,1,0,0,0,76,80,3,12,6,0,77,79,5,8,0,0,78,77,1,0,
		0,0,79,82,1,0,0,0,80,78,1,0,0,0,80,81,1,0,0,0,81,83,1,0,0,0,82,80,1,0,
		0,0,83,87,5,3,0,0,84,86,5,8,0,0,85,84,1,0,0,0,86,89,1,0,0,0,87,85,1,0,
		0,0,87,88,1,0,0,0,88,99,1,0,0,0,89,87,1,0,0,0,90,94,3,2,1,0,91,93,5,8,
		0,0,92,91,1,0,0,0,93,96,1,0,0,0,94,92,1,0,0,0,94,95,1,0,0,0,95,98,1,0,
		0,0,96,94,1,0,0,0,97,90,1,0,0,0,98,101,1,0,0,0,99,97,1,0,0,0,99,100,1,
		0,0,0,100,105,1,0,0,0,101,99,1,0,0,0,102,104,5,8,0,0,103,102,1,0,0,0,104,
		107,1,0,0,0,105,103,1,0,0,0,105,106,1,0,0,0,106,108,1,0,0,0,107,105,1,
		0,0,0,108,109,5,4,0,0,109,9,1,0,0,0,110,111,3,12,6,0,111,11,1,0,0,0,112,
		113,7,0,0,0,113,13,1,0,0,0,14,17,23,31,36,46,53,60,65,71,80,87,94,99,105
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
