using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernContextMenuManager.ViewModels
{
    public class ViewModelLocator
    {
        private MainWindowViewModel? mainWindowViewModel;

        public MainWindowViewModel MainWindowViewModel => mainWindowViewModel ??= new MainWindowViewModel();

        public static ViewModelLocator Instance => (ViewModelLocator)App.Current!.Resources["Locator"]!;
    }
}
