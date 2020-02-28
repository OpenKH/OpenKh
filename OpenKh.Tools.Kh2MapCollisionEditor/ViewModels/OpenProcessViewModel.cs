using OpenKh.Kh2;
using OpenKh.Tools.Common;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;

namespace OpenKh.Tools.Kh2MapCollisionEditor.ViewModels
{
    public class OpenProcessViewModel : BaseNotifyPropertyChanged
    {
        private const int ProcessSearchDelay = 1000;
        private const int MemoryAlignment = 0x10;
        private uint _address;
        private List<Process> _processes = new List<Process>();
        private Process _selectedProcess;
        private ProcessStream _processStream;

        public RelayCommand SearchNextCommand { get; }

        public IEnumerable<Process> Processes
        {
            get
            {
                lock (_processes)
                    return _processes.AsEnumerable();
            }
            private set
            {
                if (EqualTo(_processes, value))
                    return;

                lock (_processes)
                {
                    _processes = value.ToList();
                    OnPropertyChanged(nameof(Processes));

                    SelectedProcess = _processes.FirstOrDefault();
                    OnPropertyChanged(nameof(ShowPcsx2HelpLabel));
                }
            }
        }

        public Process SelectedProcess
        {
            get => _selectedProcess;
            set
            {
                if (value != null || !Processes.Any()) // stupid hack
                {
                    if (value != null && value.Id == _selectedProcess?.Id)
                        return;

                    _selectedProcess = value;
                    _processStream?.Dispose();

                    _processStream = ProcessService.OpenPcsx2ProcessStream(value);
                    
                    OnPropertyChanged(nameof(SelectedProcess));
                    OnPropertyChanged(nameof(IsProcessSelected));
                }
            }
        }

        public Visibility ShowPcsx2HelpLabel => Processes.Any() ? Visibility.Collapsed : Visibility.Visible;

        public bool IsProcessSelected => _selectedProcess != null;

        public uint Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged(nameof(Address));
                OnPropertyChanged(nameof(IsAddressCorrect));
            }
        }

        public bool IsAddressCorrect =>
            Address >= 0 && Address < _processStream?.Length;

        public OpenProcessViewModel()
        {
            SearchNextCommand = new RelayCommand(_ =>
            {
                if (_processStream == null)
                    return;

                _processStream.Position = Address + MemoryAlignment;
                var address = FindNextCoctAddress(_processStream);
                if (address == 0)
                    address = FindNextCoctAddress(_processStream);

                Address = address;
            });

            Task.Run(() =>
            {
                while (true)
                {
                    var processes = ProcessService.GetPcsx2Processes();
                    Application.Current?.Dispatcher.Invoke(() => Processes = processes);
                    Thread.Sleep(ProcessSearchDelay);
                }
            });
        }

        private static uint FindNextCoctAddress(Stream stream)
        {
            if (stream.Position >= stream.Length)
                stream.Position = 0;

            try
            {
                return ProcessService.FilterAddresses(stream, s => Coct.IsValid(s), MemoryAlignment)
                    .FirstOrDefault();
            }
            catch (EndOfStreamException)
            {
                return 0;
            }
        }

        private static bool EqualTo(IEnumerable<Process> a, IEnumerable<Process> b) =>
            a.All(x => b.Any(y => x.Id == y.Id)) &&
            a.Count() == b.Count();
    }
}
