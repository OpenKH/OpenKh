using OpenKh.Kh2.Ard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Models
{
    public class EventScriptModel
    {
        public string Name { get; set; }
        public List<Event.IEventEntry> EventEntries { get; set; }

        public string LastError { get; internal set; }
        public bool IsError { get; internal set; }
        public string Decompiled { get; internal set; }

        public EventScriptModel(string name, List<Event.IEventEntry> eventEntries)
        {
            Name = name;
            EventEntries = eventEntries;

            Decompiled = EventsXmlRoot.ToXml(
                new EventsXmlRoot
                {
                    Entries = eventEntries,
                }
            );
        }

        public void Compile()
        {
            IsError = false;
            LastError = null;

            try
            {
                EventEntries = EventsXmlRoot.FromXml(Decompiled).Entries;
                LastError = "OK";
            }
            catch (Exception ex)
            {
                IsError = true;
                LastError = ex.Message;
            }
        }

        public void CopyAll()
        {
            IsError = false;
            LastError = null;

            ClipboardService.SetText(
                Decompiled,
                () => LastError = "Copied",
                ex =>
                {
                    IsError = true;
                    LastError = ex.Message;
                }
            );
        }

        public void PasteReplace()
        {
            IsError = false;
            LastError = null;

            ClipboardService.GetText(
                text =>
                {
                    Decompiled = text;
                    LastError = "Pasted";
                },
                ex =>
                {
                    IsError = true;
                    LastError = ex.Message;
                }
            );
        }
    }
}
