using Microsoft.Win32;
using OpenKh.Bbs;
using OpenKh.Ddd;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace KHBBS_BON_Viewer;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var copyBinding = new CommandBinding(ApplicationCommands.Copy, CopyCellCommand_Executed);
        CommandBindings.Add(copyBinding);

        var copyRowBinding = new KeyBinding(
            new RelayCommand(CopyRowCommand_Executed),
            Key.C,
            ModifierKeys.Control | ModifierKeys.Shift);
        InputBindings.Add(copyRowBinding);
    }

    private void Window_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0 && files[0].EndsWith(".pmo", StringComparison.OrdinalIgnoreCase))
            {
                e.Effects = DragDropEffects.Copy;
                DragDropOverlay.Visibility = Visibility.Visible;
                return;
            }
        }
        e.Effects = DragDropEffects.None;
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0 && files[0].EndsWith(".pmo", StringComparison.OrdinalIgnoreCase))
            {
                e.Effects = DragDropEffects.Copy;
                DragDropOverlay.Visibility = Visibility.Visible;
                e.Handled = true;
                return;
            }
        }
        e.Effects = DragDropEffects.None;
        DragDropOverlay.Visibility = Visibility.Collapsed;
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        DragDropOverlay.Visibility = Visibility.Collapsed;

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                var filePath = files[0];
                if (filePath.EndsWith(".pmo", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        LoadPMOFile(filePath);
                        FileNameText.Text = Path.GetFileName(filePath);
                        StatusText.Text = $"Loaded: {filePath}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading file: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        StatusText.Text = "Error loading file";
                    }
                }
                else
                {
                    MessageBox.Show("Please drop a .pmo file.", "Invalid File Type",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }

    private void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "PMO Files (*.pmo)|*.pmo|All Files (*.*)|*.*",
            Title = "Select a PMO File"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                LoadPMOFile(openFileDialog.FileName);
                FileNameText.Text = Path.GetFileName(openFileDialog.FileName);
                StatusText.Text = $"Loaded: {openFileDialog.FileName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error loading file";
            }
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void CopyCell_Click(object sender, RoutedEventArgs e)
    {
        CopyCellCommand_Executed(sender, null!);
    }

    private void CopyRow_Click(object sender, RoutedEventArgs e)
    {
        CopyRowCommand_Executed(null);
    }

    private void CopyCellCommand_Executed(object? sender, ExecutedRoutedEventArgs? e)
    {
        if (BoneDataGrid.SelectedCells.Count > 0)
        {
            var selectedCell = BoneDataGrid.SelectedCells[0];

            if (selectedCell.Item is BoneDisplayData bone)
            {
                string columnName = selectedCell.Column.Header.ToString() ?? "";
                string value = GetCellValueAsString(bone, columnName);
                Clipboard.SetText(value);
                StatusText.Text = $"📋 Copied {columnName}: {value} (Bone: {bone.BoneName})";
            }
        }
    }

    private static string GetCellValueAsString(BoneDisplayData bone, string columnName)
    {
        return columnName switch
        {
            "Index" => bone.Index.ToString(CultureInfo.InvariantCulture),
            "Sibling" => bone.SiblingIndex.ToString(CultureInfo.InvariantCulture),
            "Parent" => bone.ParentIndex.ToString(CultureInfo.InvariantCulture),
            "Child" => bone.ChildIndex.ToString(CultureInfo.InvariantCulture),
            "ScaleX" => bone.ScaleXStr,
            "ScaleY" => bone.ScaleYStr,
            "ScaleZ" => bone.ScaleZStr,
            "ScaleW" => bone.ScaleWStr,
            "RotationX" => bone.RotationXStr,
            "RotationY" => bone.RotationYStr,
            "RotationZ" => bone.RotationZStr,
            "RotationW" => bone.RotationWStr,
            "TranslationX" => bone.TranslationXStr,
            "TranslationY" => bone.TranslationYStr,
            "TranslationZ" => bone.TranslationZStr,
            "Bone Name" => bone.BoneName,
            _ => ""
        };
    }

    private void CopyRowCommand_Executed(object? parameter)
    {
        if (BoneDataGrid.SelectedCells.Count > 0)
        {
            var selectedCell = BoneDataGrid.SelectedCells[0];
            if (selectedCell.Item is BoneDisplayData bone)
            {
                var sb = new StringBuilder();

                sb.Append($"{bone.Index}\t");
                sb.Append($"{bone.SiblingIndex}\t");
                sb.Append($"{bone.ParentIndex}\t");
                sb.Append($"{bone.ChildIndex}\t");
                sb.Append($"{bone.ScaleXStr}\t");
                sb.Append($"{bone.ScaleYStr}\t");
                sb.Append($"{bone.ScaleZStr}\t");
                sb.Append($"{bone.ScaleWStr}\t");
                sb.Append($"{bone.RotationXStr}\t");
                sb.Append($"{bone.RotationYStr}\t");
                sb.Append($"{bone.RotationZStr}\t");
                sb.Append($"{bone.RotationWStr}\t");
                sb.Append($"{bone.TranslationXStr}\t");
                sb.Append($"{bone.TranslationYStr}\t");
                sb.Append($"{bone.TranslationZStr}\t");
                sb.Append($"{bone.BoneName}");

                Clipboard.SetText(sb.ToString());
                StatusText.Text = $"📋 Copied entire row for bone: {bone.BoneName} (Index {bone.Index})";
            }
        }
    }

    private void LoadPMOFile(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        var initialPosition = fs.Position;
        Exception? bbsException = null;
        Exception? dddV4Exception = null;
        Exception? dddV4_2Exception = null;

        // Try BBS first
        try
        {
            LoadBBSPMO(fs);
            return;
        }
        catch (Exception ex)
        {
            bbsException = ex;
            fs.Position = initialPosition;
        }

        // Try DDD PmoV4
        try
        {
            LoadDDDPMOV4(fs);
            return;
        }
        catch (Exception ex)
        {
            dddV4Exception = ex;
            fs.Position = initialPosition;
        }

        // Try DDD PmoV4_2
        try
        {
            LoadDDDPMOV4_2(fs);
            return;
        }
        catch (Exception ex)
        {
            dddV4_2Exception = ex;
        }

        // If all fail, show all errors (If you see this error, you are stoopid!)
        var errorMessage = "Failed to load PMO file with any supported format:\n\n";
        errorMessage += $"BBS PMO: {bbsException?.Message}\n\n";
        errorMessage += $"DDD PMO v4: {dddV4Exception?.Message}\n\n";
        errorMessage += $"DDD PMO v4.2: {dddV4_2Exception?.Message}";
        throw new Exception(errorMessage);
    }

    private void LoadBBSPMO(FileStream fs)
    {
        var pmo = Pmo.Read(fs);

        if (pmo.boneList == null || pmo.boneList.Length == 0)
        {
            MessageBox.Show("This PMO file does not contain skeleton data.",
                "No Skeleton", MessageBoxButton.OK, MessageBoxImage.Information);
            BoneCountText.Text = "0";
            SkinnedBoneCountText.Text = "0";
            BoneDataGrid.ItemsSource = null;
            return;
        }

        BoneCountText.Text = pmo.skeletonHeader.BoneCount.ToString();
        SkinnedBoneCountText.Text = pmo.skeletonHeader.SkinnedBoneCount.ToString();

        var boneData = new List<BoneDisplayData>();
        for (int i = 0; i < pmo.boneList.Length; i++)
        {
            boneData.Add(CreateDisplayDataFromBBS(pmo.boneList[i], i, pmo.boneList));
        }

        BoneDataGrid.ItemsSource = boneData;
    }

    private void LoadDDDPMOV4(FileStream fs)
    {
        var pmo = PmoV4.Read(fs);

        if (pmo.boneList == null || pmo.boneList.Length == 0)
        {
            MessageBox.Show("This PMO file does not contain skeleton data.",
                "No Skeleton", MessageBoxButton.OK, MessageBoxImage.Information);
            BoneCountText.Text = "0";
            SkinnedBoneCountText.Text = "0";
            BoneDataGrid.ItemsSource = null;
            return;
        }

        BoneCountText.Text = pmo.skeletonHeader.BoneCount.ToString();
        SkinnedBoneCountText.Text = pmo.skeletonHeader.SkinnedBoneCount.ToString();

        var boneData = new List<BoneDisplayData>();
        for (int i = 0; i < pmo.boneList.Length; i++)
        {
            boneData.Add(CreateDisplayDataFromDDDV4(pmo.boneList[i], i, pmo.boneList));
        }

        BoneDataGrid.ItemsSource = boneData;
    }

    private void LoadDDDPMOV4_2(FileStream fs)
    {
        var pmo = PmoV4_2.Read(fs, skipTextures: true);

        if (pmo.Skel == null || pmo.Skel.Bones == null || pmo.Skel.Bones.Count == 0)
        {
            MessageBox.Show("This PMO file does not contain skeleton data.",
                "No Skeleton", MessageBoxButton.OK, MessageBoxImage.Information);
            BoneCountText.Text = "0";
            SkinnedBoneCountText.Text = "0";
            BoneDataGrid.ItemsSource = null;
            return;
        }

        BoneCountText.Text = pmo.Skel.Header.BoneCount.ToString();
        SkinnedBoneCountText.Text = pmo.Skel.Header.SkinnedBoneCount.ToString();

        var boneData = new List<BoneDisplayData>();
        for (int i = 0; i < pmo.Skel.Bones.Count; i++)
        {
            boneData.Add(CreateDisplayDataFromDDDV4_2(pmo.Skel.Bones[i], i, pmo.Skel.Bones));
        }

        BoneDataGrid.ItemsSource = boneData;
    }

    private static BoneDisplayData CreateDisplayDataFromBBS(
        Pmo.BoneData bone,
        int index,
        Pmo.BoneData[] allBones)
    {
        int siblingIndex = 0;
        int parentIdx = bone.ParentBoneIndex == 0xFFFF ? -1 : bone.ParentBoneIndex;

        if (parentIdx >= 0)
        {
            for (int i = index + 1; i < allBones.Length; i++)
            {
                if (allBones[i].ParentBoneIndex == bone.ParentBoneIndex)
                {
                    siblingIndex = i;
                    break;
                }
            }
        }

        int childIndex = 0;
        for (int i = 0; i < allBones.Length; i++)
        {
            if (allBones[i].ParentBoneIndex == bone.BoneIndex)
            {
                childIndex = i;
                break;
            }
        }

        Matrix4x4.Decompose(bone.Transform, out Vector3 scale, out Quaternion rotation, out Vector3 translation);

        return new BoneDisplayData
        {
            Index = index,
            SiblingIndex = siblingIndex,
            ParentIndex = parentIdx,
            ChildIndex = childIndex,
            ScaleX = scale.X,
            ScaleY = scale.Y,
            ScaleZ = scale.Z,
            ScaleW = 0,
            RotationX = rotation.X,
            RotationY = rotation.Y,
            RotationZ = rotation.Z,
            RotationW = rotation.W,
            TranslationX = translation.X,
            TranslationY = translation.Y,
            TranslationZ = translation.Z,
            BoneName = bone.JointName ?? ""
        };
    }

    private static BoneDisplayData CreateDisplayDataFromDDDV4(
        PmoV4.BoneData bone,
        int index,
        PmoV4.BoneData[] allBones)
    {
        int siblingIndex = 0;
        int parentIdx = bone.ParentBoneIndex == 0xFFFF ? -1 : bone.ParentBoneIndex;

        if (parentIdx >= 0)
        {
            for (int i = index + 1; i < allBones.Length; i++)
            {
                if (allBones[i].ParentBoneIndex == bone.ParentBoneIndex)
                {
                    siblingIndex = i;
                    break;
                }
            }
        }

        int childIndex = 0;
        for (int i = 0; i < allBones.Length; i++)
        {
            if (allBones[i].ParentBoneIndex == bone.BoneIndex)
            {
                childIndex = i;
                break;
            }
        }

        Matrix4x4.Decompose(bone.Transform, out Vector3 scale, out Quaternion rotation, out Vector3 translation);

        return new BoneDisplayData
        {
            Index = index,
            SiblingIndex = siblingIndex,
            ParentIndex = parentIdx,
            ChildIndex = childIndex,
            ScaleX = scale.X,
            ScaleY = scale.Y,
            ScaleZ = scale.Z,
            ScaleW = 0,
            RotationX = rotation.X,
            RotationY = rotation.Y,
            RotationZ = rotation.Z,
            RotationW = rotation.W,
            TranslationX = translation.X,
            TranslationY = translation.Y,
            TranslationZ = translation.Z,
            BoneName = bone.JointName ?? ""
        };
    }

    private static BoneDisplayData CreateDisplayDataFromDDDV4_2(
        PmoV4_2.BoneData bone,
        int index,
        List<PmoV4_2.BoneData> allBones)
    {
        int siblingIndex = 0;
        int parentIdx = bone.ParentBoneIndex == 0xFFFF ? -1 : bone.ParentBoneIndex;

        if (parentIdx >= 0)
        {
            for (int i = index + 1; i < allBones.Count; i++)
            {
                if (allBones[i].ParentBoneIndex == bone.ParentBoneIndex)
                {
                    siblingIndex = i;
                    break;
                }
            }
        }

        int childIndex = 0;
        for (int i = 0; i < allBones.Count; i++)
        {
            if (allBones[i].ParentBoneIndex == bone.BoneIndex)
            {
                childIndex = i;
                break;
            }
        }

        Matrix4x4.Decompose(bone.Transform, out Vector3 scale, out Quaternion rotation, out Vector3 translation);

        return new BoneDisplayData
        {
            Index = index,
            SiblingIndex = siblingIndex,
            ParentIndex = parentIdx,
            ChildIndex = childIndex,
            ScaleX = scale.X,
            ScaleY = scale.Y,
            ScaleZ = scale.Z,
            ScaleW = 0,
            RotationX = rotation.X,
            RotationY = rotation.Y,
            RotationZ = rotation.Z,
            RotationW = rotation.W,
            TranslationX = translation.X,
            TranslationY = translation.Y,
            TranslationZ = translation.Z,
            BoneName = bone.JointName ?? ""
        };
    }

    private void BoneDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {

    }

    private void BoneDataGrid_SelectionChanged_1(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {

    }
}

public class BoneDisplayData
{
    private float _scaleX, _scaleY, _scaleZ, _scaleW;
    private float _rotationX, _rotationY, _rotationZ, _rotationW;
    private float _translationX, _translationY, _translationZ;

    public int Index { get; set; }
    public int SiblingIndex { get; set; }
    public int ParentIndex { get; set; }
    public int ChildIndex { get; set; }

    public float ScaleX { get => _scaleX; set => _scaleX = value; }
    public float ScaleY { get => _scaleY; set => _scaleY = value; }
    public float ScaleZ { get => _scaleZ; set => _scaleZ = value; }
    public float ScaleW { get => _scaleW; set => _scaleW = value; }
    public float RotationX { get => _rotationX; set => _rotationX = value; }
    public float RotationY { get => _rotationY; set => _rotationY = value; }
    public float RotationZ { get => _rotationZ; set => _rotationZ = value; }
    public float RotationW { get => _rotationW; set => _rotationW = value; }
    public float TranslationX { get => _translationX; set => _translationX = value; }
    public float TranslationY { get => _translationY; set => _translationY = value; }
    public float TranslationZ { get => _translationZ; set => _translationZ = value; }

    public string ScaleXStr => _scaleX.ToString("G9", CultureInfo.InvariantCulture);
    public string ScaleYStr => _scaleY.ToString("G9", CultureInfo.InvariantCulture);
    public string ScaleZStr => _scaleZ.ToString("G9", CultureInfo.InvariantCulture);
    public string ScaleWStr => _scaleW.ToString("G9", CultureInfo.InvariantCulture);
    public string RotationXStr => _rotationX.ToString("G9", CultureInfo.InvariantCulture);
    public string RotationYStr => _rotationY.ToString("G9", CultureInfo.InvariantCulture);
    public string RotationZStr => _rotationZ.ToString("G9", CultureInfo.InvariantCulture);
    public string RotationWStr => _rotationW.ToString("G9", CultureInfo.InvariantCulture);
    public string TranslationXStr => _translationX.ToString("G9", CultureInfo.InvariantCulture);
    public string TranslationYStr => _translationY.ToString("G9", CultureInfo.InvariantCulture);
    public string TranslationZStr => _translationZ.ToString("G9", CultureInfo.InvariantCulture);

    public string BoneName { get; set; } = "";
}

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    public void Execute(object? parameter)
    {
        _execute(parameter);
    }
}
