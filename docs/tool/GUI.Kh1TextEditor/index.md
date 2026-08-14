# KH1 Text Editor

`OpenKh.Tools.Kh1TextEditor` edits event-message text from Kingdom Hearts 1.5
ReMIX `*.binl` files. The KH1 character table is compiled into the tool, so no
external `*.tbl` file is required.

## Usage

1. Open the extracted `*.binl` file with **File > Open BINL**.
2. Search for an entry, edit its text, and use **Save as** to keep the original
   file as a backup.

The built-in KH1 table uses `01` for a space, `02` for a line break, and `00`
for `{eol}`. A line break typed in the editor is saved as `02`.

The table is maintained in `OpenKh.Kh1/Kh1TextTable.cs`. Update
`CreateDefault()` there when a character mapping needs to change.

Tokens formatted as `{cmd:...}` are BINL control commands. Keep them intact
unless you understand the command bytecode. Unknown or ambiguous table values
are shown losslessly as `{0xNN}`.

The editor preserves all data outside the editable text ranges and restores the
file's 16-byte alignment with `CD` padding when saving.
