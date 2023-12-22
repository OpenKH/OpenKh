# Using Antlr4

Use Visual Studio Code.

Install vscode plugin `mike-lischke.vscode-antlr4`.

Edit `BdxScript.g4`.

After edit, run this command:

```bat
java.exe -jar antlr-4.11.1-complete.jar BdxScript.g4 -Dlanguage=CSharp -no-listener -no-visitor
```

Only `BdxScriptLexer.cs` and `BdxScriptParser.cs` are needed. Others are not needed.

## How bdx text is parsed to tree

Use Visual Studio Code `mike-lischke.vscode-antlr4` plugin.

And write this at `OpenKh/OpenKh.Command.Bdxio/Antlr4/.vscode/launch.json`

```
{
    "version": "1.0.0",
    "configurations": [
        {
            "name": "Debug ANTLR4 grammar",
            "type": "antlr-debug",
            "request": "launch",
            "input": "test.bdx",
            "grammar": "BdxScript.g4",
            "printParseTree": true,
            "visualParseTree": true
        },
    ]
}
```

Write something to `test.bdx` and press `F5` key.
