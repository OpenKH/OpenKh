using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenKh.Tools.ModsManager.Services
{
    public class QueryApplyPatchService
    {
        public async Task<bool> QueryAsync()
        {
            var source = new TaskCompletionSource<bool>();

            source.SetResult(MessageBox.Show("Do you apply the result of output file?", "ModsManager", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation) == MessageBoxResult.Yes);

            return await source.Task;
        }
    }
}
