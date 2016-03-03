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
    /// Interaktionslogik für QueueElement.xaml
    /// </summary>
    public partial class QueueElement : UserControl
    {
        // Using a DependencyProperty as the backing store for SQL.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SQLProperty =
            DependencyProperty.Register("SQL", typeof(SQLConnector), typeof(QueueElement), new PropertyMetadata(null));

        private State currentState = State.Unqueued;

        public QueueElement()
        {
            InitializeComponent();
        }

        private enum State
        {
            Queued,
            Unqueued,
            MatchFound,
        }

        public SQLConnector SQL
        {
            get { return (SQLConnector)GetValue(SQLProperty); }
            set { SetValue(SQLProperty, value); }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == SQLProperty)
            {
                SQL.MatchFound += SQL_MatchFound;
                SQL.Queued += SQL_Queued;
                SQL.Unqueued += SQL_Unqueued;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (currentState)
            {
                case State.Queued:
                    SQL.Unqueue();
                    break;
                case State.Unqueued:
                    SQL.Queue(Settings.Instance.Ladder);
                    break;
                case State.MatchFound:
                    SQL.Ready();
                    break;
            }
        }

        private void SQL_MatchFound(object sender, EventArgs e)
        {
            currentState = State.MatchFound;
            Dispatcher.Invoke(() =>
            {
                //text.Text = "Ready";
            });
        }

        private void SQL_Queued(object sender, EventArgs e)
        {
            currentState = State.Queued;
            Dispatcher.Invoke(() =>
            {
                //text.Text = "Unqueue";
            });
        }

        private void SQL_Unqueued(object sender, EventArgs e)
        {
            currentState = State.Unqueued;
            Dispatcher.Invoke(() =>
            {
                //text.Text = "Queue";
            });
        }
    }
}
