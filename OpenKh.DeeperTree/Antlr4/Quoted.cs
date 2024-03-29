//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.11.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from Quoted.g4 by ANTLR 4.11.1

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
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.11.1")]
[System.CLSCompliant(false)]
public partial class Quoted : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		Start=1, End=2, NL=3, Cr=4, Lf=5, Tab=6, EscapedChar=7, AnyChar=8;
	public const int
		QUOTE=1;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE", "QUOTE"
	};

	public static readonly string[] ruleNames = {
		"Start", "End", "NL", "Cr", "Lf", "Tab", "EscapedChar", "AnyChar"
	};


	public Quoted(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public Quoted(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, null, null, null, "'\\r'", "'\\n'", "'\\t'"
	};
	private static readonly string[] _SymbolicNames = {
		null, "Start", "End", "NL", "Cr", "Lf", "Tab", "EscapedChar", "AnyChar"
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

	public override string GrammarFileName { get { return "Quoted.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static Quoted() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static int[] _serializedATN = {
		4,0,8,48,6,-1,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,
		7,6,2,7,7,7,1,0,1,0,1,0,1,0,1,0,1,1,1,1,1,1,1,1,1,1,1,2,1,2,1,2,1,2,3,
		2,33,8,2,1,3,1,3,1,3,1,4,1,4,1,4,1,5,1,5,1,5,1,6,1,6,1,6,1,7,1,7,0,0,8,
		2,1,4,2,6,3,8,4,10,5,12,6,14,7,16,8,2,0,1,1,2,0,10,10,13,13,47,0,2,1,0,
		0,0,1,4,1,0,0,0,1,6,1,0,0,0,1,8,1,0,0,0,1,10,1,0,0,0,1,12,1,0,0,0,1,14,
		1,0,0,0,1,16,1,0,0,0,2,18,1,0,0,0,4,23,1,0,0,0,6,28,1,0,0,0,8,34,1,0,0,
		0,10,37,1,0,0,0,12,40,1,0,0,0,14,43,1,0,0,0,16,46,1,0,0,0,18,19,5,34,0,
		0,19,20,1,0,0,0,20,21,6,0,0,0,21,22,6,0,1,0,22,3,1,0,0,0,23,24,5,34,0,
		0,24,25,1,0,0,0,25,26,6,1,0,0,26,27,6,1,2,0,27,5,1,0,0,0,28,32,5,92,0,
		0,29,30,5,13,0,0,30,33,5,10,0,0,31,33,7,0,0,0,32,29,1,0,0,0,32,31,1,0,
		0,0,33,7,1,0,0,0,34,35,5,92,0,0,35,36,5,114,0,0,36,9,1,0,0,0,37,38,5,92,
		0,0,38,39,5,110,0,0,39,11,1,0,0,0,40,41,5,92,0,0,41,42,5,116,0,0,42,13,
		1,0,0,0,43,44,5,92,0,0,44,45,3,16,7,0,45,15,1,0,0,0,46,47,9,0,0,0,47,17,
		1,0,0,0,3,0,1,32,3,6,0,0,5,1,0,4,0,0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
