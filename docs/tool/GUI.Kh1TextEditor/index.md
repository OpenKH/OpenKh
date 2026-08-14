# KH1 Text Editor

`OpenKh.Tools.Kh1TextEditor` edits text from Kingdom Hearts 1.5 ReMIX `*.binl`
and `*.kmb` files. The KH1 character table is compiled into the tool, so no
external `*.tbl` file is required.

## Usage

1. Open one extracted `*.binl` or `*.kmb` file with **File > Open BINL/KMB**,
   or open the extracted `remastered` folder with **Open remastered folder**.
2. When opening a folder, choose a language code such as `SP`, `UK`, or `US`.
   Loading a single language is faster and uses substantially less memory; an
   **All languages** option is also available. `US` is selected by default.
   Final Mix (`FM`) files are excluded because `FM` is a game version rather
   than an international text language for this encoding.
3. Search for an entry and edit its text. When folder mode finds the exact same
   text in several files, it groups the occurrences into one entry and saves the
   edit to every listed location.
4. Use **Save** to update the affected files. **Save as** is available in
   single-file mode.

The toolbar provides shortcuts for opening a file or folder. In folder mode,
use the **File language** button to switch languages without selecting the
folder again.

The built-in KH1 table uses `01` for a space, `02` for a line break, and `00`
for `{eol}`. A line break typed in the editor is saved as `02`.

The table is maintained in `OpenKh.Kh1/Kh1TextTable.cs`. Update
`CreateDefault()` there when a character mapping needs to change.

Tokens formatted as `{cmd:...}` are EvMsg BINL control commands. Keep them
intact unless you understand the command bytecode. Unknown or ambiguous table
values are shown losslessly as `{0xNN}`.
The unmapped KMB control byte `0F` is displayed as `{ctrl:0F}`.

The editor supports EvMsg BINL, `Message v361` BINL, and KMB message tables.
Unrecognized BINL data such as offset tables is ignored in folder mode. Data
outside editable text ranges and the file's padding style are preserved.
