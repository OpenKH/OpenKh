for %%f in (*.ksy) do "C:\Program Files (x86)\kaitai-struct-compiler\bin\kaitai-struct-compiler.bat" -t csharp --dotnet-namespace OpenKh.Research.GenGhidraComments.Ksy %%f
