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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OsuSqlTool
{
    /// <summary>
    /// Interaktionslogik für MapControl.xaml
    /// </summary>
    public partial class MapControl : UserControl
    {
        // Using a DependencyProperty as the backing store for Map.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MapProperty =
            DependencyProperty.Register("Map", typeof(SQLMap), typeof(MapControl), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for SQL.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SQLProperty =
            DependencyProperty.Register("SQL", typeof(SQLConnector), typeof(MapControl), new PropertyMetadata(null));

        public MapControl()
        {
            InitializeComponent();
        }

        public SQLMap Map
        {
            get { return (SQLMap)GetValue(MapProperty); }
            set
            {
                DataContext = value;
                SetValue(MapProperty, value);
            }
        }

        public SQLConnector SQL
        {
            get { return (SQLConnector)GetValue(SQLProperty); }
            set { SetValue(SQLProperty, value); }
        }

        private void Ban_Click(object sender, RoutedEventArgs e)
        {
            SQL.Ban(Map);
        }

        private void Pick_Click(object sender, RoutedEventArgs e)
        {
            SQL.Pick(Map);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == MapProperty)
            {
                if (Map.Category == SQLCategory.Tiebreaker)
                {
                    pickButton.Visibility = Visibility.Collapsed;
                    banButton.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
