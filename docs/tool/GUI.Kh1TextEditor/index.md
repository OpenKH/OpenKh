# KH1 Text Editor

`OpenKh.Tools.Kh1TextEditor` edits text from Kingdom Hearts 1.5 ReMIX `*.binl`,
`*.kmb`, text-table `*.bin`, `*.evdl`, and `*.ev` files. The KH1 character
table is compiled into the tool, so no external `*.tbl` file is required.

## Usage

1. Open one supported extracted file with **File > Open text file**, or open
   the extracted `remastered` folder with **Open remastered folder**.
2. When opening a folder, choose a language code such as `SP`, `UK`, or `US`.
   Loading a single language is faster and uses substantially less memory; an
   **All languages** option is also available. `US` is selected by default.
   Final Mix (`FM`) files are excluded because `FM` is a game version rather
   than an international text language for this encoding.
3. Use the `BINL`, `KMB`, `BIN`, `EVDL`, and `EV` tabs to work with one file
   type at a time. When folder mode finds the exact same text in several files
   of that type, it groups the occurrences into one entry and saves the edit to
   every listed location.
4. In folder mode, **Save** updates the affected files under `remastered`.
   **Save as** creates a ZIP containing only modified files and keeps their
   paths relative to `remastered`. In single-file mode, **Save** and **Save as**
   behave like normal file operations.

In folder mode, use **File language** in the main menu to switch languages
without selecting the folder again.

The built-in KH1 table uses `01` for a space, `02` for a line break, and `00`
for `{eol}`. A line break typed in the editor is saved as `02`.

The table is maintained in `OpenKh.Kh1/Kh1TextTable.cs`. Update
`CreateDefault()` there when a character mapping needs to change.

Tokens formatted as `{cmd:...}` are EvMsg control commands used by BINL and
the message sections of EV/EVDL files. Keep them intact unless you understand
the command bytecode. Unknown or ambiguous table values are shown losslessly
as `{0xNN}`.
The unmapped KMB control byte `0F` is displayed as `{ctrl:0F}`.

The editor supports EvMsg BINL, `Message v361` BINL, KMB message tables,
known remastered BIN text tables, and validated EvMsg sections embedded in EV
and EVDL containers. Files that do not contain those validated structures are
ignored. Data outside editable text ranges, container offsets, and padding are
preserved.
