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
        public static readonly RoutedEvent PickedEvent = EventManager.RegisterRoutedEvent(
            "Picked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MapControl));
        public static readonly RoutedEvent BannedEvent = EventManager.RegisterRoutedEvent(
            "Banned", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MapControl));

        public MapControl()
        {
            InitializeComponent();
        }

        public MapControl(SQLMap map)
            : this()
        {
            Map = map;
            DataContext = map;

            if (map.Category == SQLCategory.Tiebreaker)
            {
                pickButton.Visibility = Visibility.Collapsed;
                banButton.Visibility = Visibility.Collapsed;
            }
        }

        public event RoutedEventHandler Picked
        {
            add
            {
                AddHandler(PickedEvent, value);
            }
            remove
            {
                RemoveHandler(PickedEvent, value);
            }
        }

        public event RoutedEventHandler Banned
        {
            add
            {
                AddHandler(BannedEvent, value);
            }
            remove
            {
                RemoveHandler(BannedEvent, value);
            }
        }

        public SQLMap Map { get; private set; }

        private void Pick_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(PickedEvent));
        }

        private void Ban_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(BannedEvent));
        }
    }
}
