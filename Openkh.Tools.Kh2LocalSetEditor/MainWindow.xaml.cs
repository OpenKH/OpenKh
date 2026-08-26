using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace KH2FM_Localset_Editor
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string currentFilePath;
        private byte[] fileData;
        private ObservableCollection<WorldData> worlds;
        private WorldData selectedWorld;
        private int originalDataStartOffset; 

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsFileLoaded => !string.IsNullOrEmpty(currentFilePath);
        public WorldData SelectedWorld
        {
            get => selectedWorld;
            set
            {
                selectedWorld = value;
                OnPropertyChanged(nameof(SelectedWorld));
            }
        }

        private readonly string[] WorldNames = {
            "ZZ",
            "End of Sea",
            "Twilight Town",
            "Destiny Island",
            "Hollow Bastion",
            "Beast's Castle",
            "Olympus Coliseum",
            "Agrabah",
            "The Land of Dragons",
            "100 Acre Wood",
            "Pride Lands",
            "Atlantica",
            "Disney Castle",
            "Timeless River",
            "Halloween Town",
            "World Map",
            "Port Royal",
            "Space Paranoids",
            "The World That Never Was"
        };

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            worlds = new ObservableCollection<WorldData>();
            WorldListBox.ItemsSource = worlds;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    LoadFile(files[0]);
                }
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "BIN files (*.bin)|*.bin|All files (*.*)|*.*",
                Title = "Open 07localset.bin"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LoadFile(openFileDialog.FileName);
            }
        }

        private void LoadFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("File does not exist!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                fileData = File.ReadAllBytes(filePath);

                // Validate BAR header
                if (fileData.Length < 8 ||
                    fileData[0] != 0x42 || fileData[1] != 0x41 || fileData[2] != 0x52 || fileData[3] != 0x01)
                {
                    MessageBox.Show("Invalid BAR header! This doesn't appear to be a valid localset file.",
                                  "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Validate entry count (should be 0x13 = 19)
                uint entryCount = BitConverter.ToUInt32(fileData, 4);
                if (entryCount != 0x13)
                {
                    MessageBox.Show($"Unexpected entry count: {entryCount}. Expected 19 (0x13).",
                                  "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                currentFilePath = filePath;
                FilePathTextBlock.Text = Path.GetFileName(filePath);

                FindDataStartOffset();

                ParseLocalsetData();

                NoDataTextBlock.Visibility = Visibility.Collapsed;
                StatusTextBlock.Text = "File loaded sucessfully";
                OnPropertyChanged(nameof(IsFileLoaded));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FindDataStartOffset()
        {

            int barHeaderSize = 8; // BAR header
            int barEntriesSize = 19 * 16; 
            int searchStart = barHeaderSize + barEntriesSize;

            for (int i = searchStart; i < fileData.Length - 4; i++)
            {
                if (BitConverter.ToUInt32(fileData, i) == 0x01)
                {
                    originalDataStartOffset = i;
                    return;
                }
            }

            originalDataStartOffset = 140;
        }

        private void ParseLocalsetData()
        {
            worlds.Clear();
            int offset = originalDataStartOffset;

            for (int worldIndex = 0; worldIndex < 19 && offset < fileData.Length - 8; worldIndex++)
            {
                if (offset + 8 > fileData.Length)
                    break;

                uint listHeader = BitConverter.ToUInt32(fileData, offset);
                if (listHeader != 0x01)
                {
                    bool found = false;
                    for (int searchOffset = offset; searchOffset < fileData.Length - 8; searchOffset += 4)
                    {
                        if (BitConverter.ToUInt32(fileData, searchOffset) == 0x01)
                        {
                            offset = searchOffset;
                            found = true;
                            break;
                        }
                    }
                    if (!found) break;
                }

                offset += 4;

                // Get entry count
                uint entryCount = BitConverter.ToUInt32(fileData, offset);
                offset += 4;

                // Create world data
                WorldData world = new WorldData
                {
                    Index = worldIndex + 1,
                    Name = worldIndex < WorldNames.Length ? WorldNames[worldIndex] : $"Unknown World {worldIndex + 1}",
                    EntryCount = (int)entryCount,
                    Entries = new ObservableCollection<LocalsetEntry>()
                };

                // Parse entries
                for (int i = 0; i < entryCount && offset + 4 <= fileData.Length; i++)
                {
                    uint rawValue = BitConverter.ToUInt32(fileData, offset);
                    ushort id = (ushort)(rawValue & 0xFFFF);
                    ushort roomId = (ushort)((rawValue >> 16) & 0xFFFF);

                    LocalsetEntry entry = new LocalsetEntry
                    {
                        Index = i,
                        ID = id,
                        RoomID = roomId,
                        RawValue = rawValue,
                        Offset = offset
                    };

                    world.Entries.Add(entry);
                    offset += 4;
                }

                worlds.Add(world);
            }
        }

        private void WorldListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorldListBox.SelectedItem is WorldData selected)
            {
                SelectedWorld = selected;
                EntryDataGrid.ItemsSource = selected.Entries;
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "BIN files (*.bin)|*.bin|All files (*.*)|*.*",
                Title = "Save 07localset.bin",
                FileName = Path.GetFileName(currentFilePath) ?? "07localset.bin"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveFile(saveFileDialog.FileName);
            }
        }

        private void SaveFile(string filePath)
        {
            try
            {
                foreach (var world in worlds)
                {
                    foreach (var entry in world.Entries)
                    {
                        if (entry.Offset >= 0 && entry.Offset + 4 <= fileData.Length)
                        {
                            uint newValue = (uint)((entry.RoomID << 16) | entry.ID);
                            byte[] bytes = BitConverter.GetBytes(newValue);
                            Array.Copy(bytes, 0, fileData, entry.Offset, 4);
                        }
                    }
                }

                File.WriteAllBytes(filePath, fileData);
                StatusTextBlock.Text = $"Saved: {Path.GetFileName(filePath)}";
                MessageBox.Show("File saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateBarHeaderFileSizes()
        {
            for (int i = 0; i < Math.Min(19, worlds.Count); i++)
            {
                int barEntryOffset = 0x10 + (i * 0x10); 
                int fileSizeOffset = barEntryOffset + 0x0C; 

                if (fileSizeOffset + 2 <= fileData.Length)
                {
                    int currentEntryCount = worlds[i].Entries.Count;
                    ushort newFileSize = (ushort)(8 + (currentEntryCount * 4));

                    fileData[fileSizeOffset] = (byte)(newFileSize & 0xFF);
                    fileData[fileSizeOffset + 1] = (byte)((newFileSize >> 8) & 0xFF);
                }
            }
        }

        private void AddEntry_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedWorld == null)
            {
                MessageBox.Show("Please select a world first.", "No World Selected",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            LocalsetEntry newEntry = new LocalsetEntry
            {
                Index = SelectedWorld.Entries.Count,
                ID = 0,
                RoomID = 0,
                RawValue = 0,
                Offset = -1 
            };

            SelectedWorld.Entries.Add(newEntry);
            SelectedWorld.EntryCount = SelectedWorld.Entries.Count;

            UpdateEntryCountInFileData();

            var selectedIndex = WorldListBox.SelectedIndex;
            WorldListBox.ItemsSource = null;
            WorldListBox.ItemsSource = worlds;
            WorldListBox.SelectedIndex = selectedIndex;

            StatusTextBlock.Text = $"Added new entry to {SelectedWorld.Name}";
        }

        private void UpdateEntryCountInFileData()
        {
            RebuildFileData();
        }

        private void RebuildFileData()
        {
            try
            {
                int originalLocalDataEnd = FindOriginalLocalDataEnd();

                int newLocalDataSize = 0;
                foreach (var world in worlds)
                {
                    newLocalDataSize += 8; 
                    newLocalDataSize += world.Entries.Count * 4;
                }

                int originalLocalDataSize = originalLocalDataEnd - originalDataStartOffset;
                int sizeDifference = newLocalDataSize - originalLocalDataSize;

                int newSize = fileData.Length + sizeDifference;
                byte[] newFileData = new byte[newSize];

                Array.Copy(fileData, 0, newFileData, 0, originalDataStartOffset);

                int offset = originalDataStartOffset;

                for (int worldIndex = 0; worldIndex < worlds.Count; worldIndex++)
                {
                    var world = worlds[worldIndex];

                    byte[] listHeader = BitConverter.GetBytes((uint)0x01);
                    Array.Copy(listHeader, 0, newFileData, offset, 4);
                    offset += 4;

                    byte[] entryCount = BitConverter.GetBytes((uint)world.Entries.Count);
                    Array.Copy(entryCount, 0, newFileData, offset, 4);
                    offset += 4;

                    for (int entryIndex = 0; entryIndex < world.Entries.Count; entryIndex++)
                    {
                        var entry = world.Entries[entryIndex];
                        entry.Offset = offset;
                        entry.Index = entryIndex;

                        uint value = (uint)((entry.RoomID << 16) | entry.ID);
                        byte[] entryBytes = BitConverter.GetBytes(value);
                        Array.Copy(entryBytes, 0, newFileData, offset, 4);
                        offset += 4;
                    }

                    world.EntryCount = world.Entries.Count;
                }

                if (originalLocalDataEnd < fileData.Length)
                {
                    int trailingSize = fileData.Length - originalLocalDataEnd;
                    int newTrailingStart = originalDataStartOffset + newLocalDataSize;

                    Array.Copy(fileData, originalLocalDataEnd, newFileData, newTrailingStart, trailingSize);
                }

                fileData = newFileData;

                UpdateBarHeaderFileSizes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error rebuilding file data: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int FindOriginalLocalDataEnd()
        {
            int tempOffset = originalDataStartOffset;

            for (int worldIndex = 0; worldIndex < 19 && tempOffset < fileData.Length - 8; worldIndex++)
            {
                if (tempOffset + 8 > fileData.Length) break;

                uint listHeader = BitConverter.ToUInt32(fileData, tempOffset);
                if (listHeader != 0x01)
                {
                    bool found = false;
                    for (int searchOffset = tempOffset; searchOffset < fileData.Length - 8; searchOffset += 4)
                    {
                        if (BitConverter.ToUInt32(fileData, searchOffset) == 0x01)
                        {
                            tempOffset = searchOffset;
                            found = true;
                            break;
                        }
                    }
                    if (!found) break;
                }

                tempOffset += 4; 
                uint entryCount = BitConverter.ToUInt32(fileData, tempOffset);
                tempOffset += 4; 
                tempOffset += (int)entryCount * 4; 
            }

            return tempOffset;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class WorldData
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public int EntryCount { get; set; }
        public ObservableCollection<LocalsetEntry> Entries { get; set; }
    }

    public class LocalsetEntry : INotifyPropertyChanged
    {
        private ushort id;
        private ushort roomId;

        public int Index { get; set; }
        public uint RawValue { get; set; }
        public int Offset { get; set; }

        public ushort ID
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged();
            }
        }

        public ushort RoomID
        {
            get => roomId;
            set
            {
                roomId = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
