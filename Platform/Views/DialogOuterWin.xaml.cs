using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Platform.Views
{
    /// <summary>
    /// DialogOuterWin.xaml 的交互逻辑
    /// </summary>
    public partial class DialogOuterWin : Window, IDialogWindow
    {
        public DialogOuterWin()
        {
            InitializeComponent();
        }

        public IDialogResult Result { get; set; }
    }
}
