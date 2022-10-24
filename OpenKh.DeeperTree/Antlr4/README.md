# Using Antlr4

Use Visual Studio Code.

Install vscode plugin `mike-lischke.vscode-antlr4`.

Edit `BdxScript.g4`.

After edit, run this command:

```bat
java.exe -jar antlr-4.11.1-complete.jar TreeScript.g4 -Dlanguage=CSharp -no-listener -no-visitor
java.exe -jar antlr-4.11.1-complete.jar Quoted.g4 -Dlanguage=CSharp -no-listener -no-visitor
```

Only `TreeScriptLexer.cs` and `TreeScriptParser.cs` are needed. Others are not needed.
