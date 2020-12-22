@rem "H:\Proj\kaitai_struct\compiler\jvm\target\universal\stage\bin\kaitai-struct-compiler.bat"
@rem "C:\Program Files (x86)\kaitai-struct-compiler\bin\kaitai-struct-compiler.bat"
for %%f in (*.ksy) do "H:\Proj\kaitai_struct\compiler\jvm\target\universal\stage\bin\kaitai-struct-compiler.bat" -t csharp --dotnet-namespace OpenKh.Research.GenGhidraComments.Ksy --read-pos %%f
