using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using OpenKh.Tools.ModsManager.Avalonia.Services;

namespace OpenKh.Tools.ModsManager.Avalonia.Views;

public sealed partial class ControllerKeyboardWindow : EmbeddedDialogControl
{
    private static readonly string[][] CharacterRows =
    [
        ["1", "2", "3", "4", "5", "6", "7", "8", "9", "0"],
        ["q", "w", "e", "r", "t", "y", "u", "i", "o", "p"],
        ["a", "s", "d", "f", "g", "h", "j", "k", "l"],
        ["z", "x", "c", "v", "b", "n", "m"],
        ["-", "_", ".", "/", "\\", ":", "@"]
    ];

    private readonly TextBox _target;
    private readonly List<Button> _letterButtons = [];
    private Button? _firstButton;
    private bool _shift;

    public ControllerKeyboardWindow()
        : this(new TextBox())
    {
    }

    public ControllerKeyboardWindow(TextBox target)
    {
        _target = target;
        InitializeComponent();
        BuildKeyboard();
        RefreshPreview();
        Opened += (_, _) => _firstButton?.Focus(NavigationMethod.Directional);
    }

    public void HandleControllerAction(ControllerAction action)
    {
        if (action == ControllerAction.Secondary)
        {
            Backspace();
            return;
        }

        if (action == ControllerAction.MoveTop)
        {
            ToggleShift();
            return;
        }

        if (ControllerWindowNavigator.TryMoveFocus(this, action))
            return;

        if (action is ControllerAction.PreviousControl or ControllerAction.PreviousItem or ControllerAction.PreviousGame)
            ControllerWindowNavigator.MoveFocus(this, -1);
        else if (action is ControllerAction.NextControl or ControllerAction.NextItem or ControllerAction.NextGame)
            ControllerWindowNavigator.MoveFocus(this, 1);
        else if (action == ControllerAction.Cancel)
            Close();
        else if (action == ControllerAction.Confirm && FocusManager?.GetFocusedElement() is Button button)
            Activate(button);
    }

    private void BuildKeyboard()
    {
        foreach (var row in CharacterRows)
            KeyboardRows.Children.Add(CreateCharacterRow(row));

        KeyboardRows.Children.Add(CreateActionRow());
    }

    private Control CreateCharacterRow(IEnumerable<string> characters)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        foreach (var character in characters)
        {
            var button = CreateButton(character, $"Character:{character}", 56);
            if (char.IsLetter(character[0]))
                _letterButtons.Add(button);
            _firstButton ??= button;
            panel.Children.Add(button);
        }

        return panel;
    }

    private Control CreateActionRow()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        panel.Children.Add(CreateButton("Shift", "Shift", 90, "ShiftButton"));
        panel.Children.Add(CreateButton("Space", "Space", 190, "SpaceButton"));
        panel.Children.Add(CreateButton("Backspace", "Backspace", 120, "BackspaceButton"));
        panel.Children.Add(CreateButton("Clear", "Clear", 90, "ClearButton"));
        var done = CreateButton("Done", "Done", 110, "DoneButton");
        done.Classes.Add("primary");
        panel.Children.Add(done);
        return panel;
    }

    private Button CreateButton(string content, string action, double minWidth, string? name = null)
    {
        var button = new Button
        {
            Content = content,
            Tag = action,
            MinWidth = minWidth,
            Name = name
        };
        button.Click += Key_OnClick;
        return button;
    }

    private void Key_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (sender is Button button)
            Activate(button);
    }

    private void Activate(Button button)
    {
        if (button.Tag is not string action)
            return;

        if (action.StartsWith("Character:", StringComparison.Ordinal))
        {
            var value = action["Character:".Length..];
            Insert(_shift ? value.ToUpperInvariant() : value);
            return;
        }

        switch (action)
        {
            case "Shift":
                ToggleShift();
                break;
            case "Space":
                Insert(" ");
                break;
            case "Backspace":
                Backspace();
                break;
            case "Clear":
                _target.Text = string.Empty;
                _target.CaretIndex = 0;
                RefreshPreview();
                break;
            case "Done":
                Close(true);
                break;
        }
    }

    private void Insert(string value)
    {
        var current = _target.Text ?? string.Empty;
        var start = Math.Clamp(Math.Min(_target.SelectionStart, _target.SelectionEnd), 0, current.Length);
        var end = Math.Clamp(Math.Max(_target.SelectionStart, _target.SelectionEnd), start, current.Length);
        _target.Text = current[..start] + value + current[end..];
        _target.CaretIndex = start + value.Length;
        _target.SelectionStart = _target.CaretIndex;
        _target.SelectionEnd = _target.CaretIndex;
        RefreshPreview();
    }

    private void Backspace()
    {
        var current = _target.Text ?? string.Empty;
        var start = Math.Clamp(Math.Min(_target.SelectionStart, _target.SelectionEnd), 0, current.Length);
        var end = Math.Clamp(Math.Max(_target.SelectionStart, _target.SelectionEnd), start, current.Length);
        if (start == end && start > 0)
            start--;
        if (start == end)
            return;

        _target.Text = current.Remove(start, end - start);
        _target.CaretIndex = start;
        _target.SelectionStart = start;
        _target.SelectionEnd = start;
        RefreshPreview();
    }

    private void ToggleShift()
    {
        _shift = !_shift;
        foreach (var button in _letterButtons)
        {
            if (button.Tag is not string action)
                continue;
            var value = action["Character:".Length..];
            button.Content = _shift ? value.ToUpperInvariant() : value;
        }
    }

    private void RefreshPreview()
    {
        InputPreview.Text = string.IsNullOrEmpty(_target.Text) ? " " : _target.Text;
    }
}
