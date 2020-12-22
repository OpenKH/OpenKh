@rem We need modded version of kaitai-struct-compiler:
@rem - install sbt: https://www.scala-sbt.org/1.x/docs/Installing-sbt-on-Windows.html
@rem - git clone from https://github.com/kenjiuno/kaitai_struct_compiler/tree/dev
@rem - proceed compile with such a way written in ".travis.yml"
@rem   e.g. `cd compile` and `sbt compile compilerJVM/stage compilerJVM/universal:packageBin`
@rem
@rem "H:\Proj\kaitai_struct\compiler\jvm\target\universal\stage\bin\kaitai-struct-compiler.bat"
@rem "C:\Program Files (x86)\kaitai-struct-compiler\bin\kaitai-struct-compiler.bat"
@rem don't forget to disable installed ksc:
@set KAITAI_STRUCT_COMPILER_HOME=
for %%f in (*.ksy) do "H:\Proj\kaitai_struct\compiler\jvm\target\universal\stage\bin\kaitai-struct-compiler.bat" -t csharp --dotnet-namespace OpenKh.Research.GenGhidraComments.Ksy --read-pos %%f
