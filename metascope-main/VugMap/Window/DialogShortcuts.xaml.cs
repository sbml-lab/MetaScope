using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace VugMap.Window
{
	public partial class DialogShortcuts : System.Windows.Window
	{
		public DialogShortcuts()
		{
			InitializeComponent();
			BuildContent();
		}

		private void BuildContent()
		{
			AddSection( "Navigation", new string[,]
			{
				{ "Left / Right",			"Scroll left / right (small)" },
				{ "Shift+Left / Right",		"Scroll left / right (large)" },
				{ "Home / End",				"Go to genome start / end" },
				{ "Ctrl+G",					"Go to position" },
			});

			AddSection( "Zoom", new string[,]
			{
				{ "Ctrl++ / Ctrl+-",		"Zoom in / out" },
				{ "Ctrl+0",				"Zoom to custom level" },
				{ "Ctrl+Scroll",			"Zoom in / out (mouse)" },
			});

			AddSection( "File and View", new string[,]
			{
				{ "Ctrl+O",				"Open file" },
				{ "Ctrl+S",				"Save" },
				{ "Ctrl+Tab / Shift+Tab",	"Next / previous tab" },
				{ "Ctrl+T",				"Split view" },
				{ "Ctrl+F",				"Search" },
				{ "F5",						"Refresh view" },
				{ "Ctrl+Shift+E",			"Export image (PNG/SVG)" },
			});

			AddSection( "Track", new string[,]
			{
				{ "Ctrl+Shift+Up / Down",	"Move track up / down" },
				{ "Ctrl+Shift+C",			"Set track color" },
				{ "Ctrl+Shift+H",			"Set track height" },
				{ "Ctrl+Shift+B / P / L",	"Display as bar / point / line" },
			});

			AddSection( "Feature Editing", new string[,]
			{
				{ "NumPad 1 / Alt+Left",	"Move feature left" },
				{ "NumPad 2 / Alt+Right",	"Move feature right" },
				{ "NumPad 4 / Alt+Down",	"Shrink start" },
				{ "NumPad 5 / Alt+Up",		"Expand end" },
			});
		}

		private void AddSection( string strTitle, string[,] arrRows )
		{
			TextBlock		tbTitle			= new TextBlock();
			tbTitle.Text					= strTitle;
			tbTitle.FontSize				= 14;
			tbTitle.FontWeight				= FontWeights.Bold;
			tbTitle.Margin					= new Thickness( 0, m_spContent.Children.Count > 0 ? 14 : 0, 0, 6 );
			m_spContent.Children.Add( tbTitle );

			Grid			grid			= new Grid();
			grid.ColumnDefinitions.Add( new ColumnDefinition { Width = new GridLength( 190 ) } );
			grid.ColumnDefinitions.Add( new ColumnDefinition { Width = new GridLength( 1, GridUnitType.Star ) } );

			int				nRows			= arrRows.GetLength( 0 );

			for( int i = 0; i < nRows; i++ )
			{
				grid.RowDefinitions.Add( new RowDefinition { Height = GridLength.Auto } );

				Brush		bshBg			= i % 2 == 0
											? new SolidColorBrush( Color.FromRgb( 245, 245, 245 ) )
											: Brushes.White;

				Border		bdrKey			= new Border();
				bdrKey.Background			= bshBg;
				bdrKey.Padding				= new Thickness( 8, 4, 8, 4 );
				Grid.SetRow( bdrKey, i );
				Grid.SetColumn( bdrKey, 0 );

				TextBlock	tbKey			= new TextBlock();
				tbKey.Text					= arrRows[ i, 0 ];
				tbKey.FontFamily			= new FontFamily( "Consolas" );
				tbKey.FontSize				= 12;
				bdrKey.Child				= tbKey;
				grid.Children.Add( bdrKey );

				Border		bdrDesc			= new Border();
				bdrDesc.Background			= bshBg;
				bdrDesc.Padding				= new Thickness( 8, 4, 8, 4 );
				Grid.SetRow( bdrDesc, i );
				Grid.SetColumn( bdrDesc, 1 );

				TextBlock	tbDesc			= new TextBlock();
				tbDesc.Text					= arrRows[ i, 1 ];
				tbDesc.FontSize				= 12;
				bdrDesc.Child				= tbDesc;
				grid.Children.Add( bdrDesc );
			}

			m_spContent.Children.Add( grid );
		}

		private void OnOkClick( object obj, RoutedEventArgs ea )
		{
			Close();
		}
	}
}
