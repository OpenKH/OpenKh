#pragma warning disable CS4014 

using Avalonia.Controls;
using Avalonia.Threading;
using Octokit;
using OpenKh.Tools.ModManager.Classes;
using OpenKh.Tools.ModManager.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModManager.Services
{
    public static class DialogService
    {
        public async static Task<bool> ShowProgress(Window? owner, string title, string message, CancellationTokenSource cancelToken, SafePtr firstProgCurr, SafePtr firstProgMax, SafePtr? firstProgText = null, SafePtr? secondProgCurr = null, SafePtr? secondProgMax = null, SafePtr? secondProgText = null)
        {
            var _fetchDialog = new ProgressDialog 
            { 
                Title = title, 
                Message = message
            };

            Task.Run(async () =>
            {
                while (true)
                {
                    double? _fetchFirstCurrent = firstProgCurr;
                    double? _fetchFirstMaximum = firstProgMax;

                    if (_fetchFirstCurrent == null || _fetchFirstMaximum == null)
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            _fetchDialog.Close(_fetchDialog.Result = true);
                        });

                        break;
                    }

                    double? _fetchSecondCurrent = secondProgCurr != null ? secondProgCurr : null;
                    double? _fetchSecondMaximum = secondProgMax != null ? secondProgMax : null;

                    string? _fetchFirstString = firstProgText != null ? firstProgText : null;
                    string? _fetchSecondString = secondProgText != null ? secondProgText : null;

                    Dispatcher.UIThread.Post(() =>
                    {
                        _fetchDialog.MainProgress.IsIndeterminate = _fetchFirstMaximum.Value <= -1;

                        _fetchDialog.MainProgress.Maximum = _fetchFirstMaximum.Value;
                        _fetchDialog.MainProgress.Value = _fetchFirstCurrent.Value;

                        _fetchDialog.MainProgress.ShowProgressText = _fetchFirstString != null;
                        _fetchDialog.MainProgress.ProgressTextFormat = _fetchFirstString ?? "";

                        _fetchDialog.MiscProgress.IsIndeterminate = _fetchSecondMaximum != null ? _fetchSecondMaximum.Value <= -1 : false;

                        _fetchDialog.MiscProgress.Maximum = _fetchSecondMaximum ?? 0;
                        _fetchDialog.MiscProgress.Value = _fetchSecondCurrent ?? 0;

                        _fetchDialog.MiscProgress.ShowProgressText = _fetchSecondString != null;
                        _fetchDialog.MiscProgress.ProgressTextFormat = _fetchSecondString ?? "";

                        _fetchDialog.MiscProgress.IsVisible = _fetchSecondCurrent != null && _fetchSecondMaximum != null;
                    });

                    await Task.Delay(5, CancellationToken.None);
                }
            }, CancellationToken.None);

            if (owner != null)
            {
                var _fetchResult = await _fetchDialog.ShowDialog<bool>(owner);

                if (!_fetchResult)
                    await cancelToken.CancelAsync();

                return _fetchResult;
            }

            else
            {
                _fetchDialog.Show();

                await Task.Run(() =>
                {
                    while (_fetchDialog.Result == null)
                    { }
                }, CancellationToken.None);

                if (!_fetchDialog.Result.Value)
                    await cancelToken.CancelAsync();

                return _fetchDialog.Result.Value;
            }
        }

        public static async Task<bool> ShowQuestion(Window? owner, string title, string message)
        {
            var _fetchDialog = new QuestionDialog { Title = title };

            _fetchDialog.MainText.Text = message;

            if (owner != null)
                return await _fetchDialog.ShowDialog<bool>(owner);

            else
            {
                _fetchDialog.Show();

                await Task.Run(() =>
                {
                    while (_fetchDialog.Result == null)
                    { }
                }, CancellationToken.None);

                return _fetchDialog.Result.Value;
            }
        }

        public static async Task ShowMessage(Window? owner, string title, string message, MessageType type)
        {
            var _fetchDialog = new MessageDialog { Title = title, Type = type };

            _fetchDialog.MainText.Text = message;

            if (owner != null)
                await _fetchDialog.ShowDialog(owner);

            else
                _fetchDialog.Show();
        }

        public static async Task<string> ShowInput(Window? owner, string title, string message, string mainButtonText, string placeholderText, string? currentText = null, string? miscButtonMessage = null, Func<Window?, Task<string?>>? miscButtonCallback = null)
        {
            var _fetchDialog = new InputDialog { Title = title, MiscButtonCallback = miscButtonCallback, CurrentText = currentText, MiscButtonText = miscButtonMessage };

            _fetchDialog.MainText.Text = message;
            _fetchDialog.AcceptButton.Content = mainButtonText;
            _fetchDialog.InputText.PlaceholderText = placeholderText;

            if (owner != null)
                return await _fetchDialog.ShowDialog<string>(owner);

            else
            {
                _fetchDialog.Show();

                await Task.Run(() =>
                {
                    while (_fetchDialog.Result == null)
                    { }
                }, CancellationToken.None);

                return _fetchDialog.Result;
            }
        }

        public static async Task<string> ShowOptions(Window? owner, string title, string message, string mainButtonText, string[] options, string? miscButtonMessage = null, Func<Window?, Task<string?>>? miscButtonCallback = null)
        {
            var _fetchDialog = new OptionsDialog { Title = title, MiscButtonCallback = miscButtonCallback, Options = options, MiscButtonText = miscButtonMessage };

            _fetchDialog.MainText.Text = message;
            _fetchDialog.AcceptButton.Content = mainButtonText;

            if (miscButtonMessage != null)
                _fetchDialog.MiscButton.Content = miscButtonMessage;

            if (owner != null)
                return await _fetchDialog.ShowDialog<string>(owner);

            else
            {
                _fetchDialog.Show();

                await Task.Run(() =>
                {
                    while (_fetchDialog.Result == null)
                    { }
                }, CancellationToken.None);

                return _fetchDialog.Result;
            }
        }
    }
}

#pragma warning restore CS4014
