using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace MetaScope.Views
{
	public partial class DialogShortcuts : Window
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
				{ "← / →",					"Scroll left / right (small)" },
				{ "⇧← / ⇧→",				"Scroll left / right (large)" },
				{ "Home / End",				"Go to genome start / end" },
				{ "⌘G",					"Go to position" },
				{ "⇧+Scroll",				"Scroll left / right (mouse)" },
			});

			AddSection( "Zoom", new string[,]
			{
				{ "⌘+ / ⌘-",				"Zoom in / out" },
				{ "⌘0",					"Zoom to custom level" },
				{ "⌘+Scroll",				"Zoom in / out (mouse)" },
			});

			AddSection( "File", new string[,]
			{
				{ "⌘O",					"Open GFF file" },
				{ "⌘⇧O",					"Open workspace" },
				{ "⌘S",					"Save all data" },
				{ "⌘⇧S",					"Save workspace" },
				{ "⌘⇧E",					"Export image (PNG/SVG)" },
				{ "⌘⇧W",					"Close all" },
				{ "⌘Q",					"Quit" },
			});

			AddSection( "View", new string[,]
			{
				{ "⌘Tab / ⌘⇧Tab",			"Next / previous tab" },
				{ "⌘T",					"Split view" },
				{ "⌘F",					"Search" },
				{ "F5",					"Refresh view" },
				{ "⌘⇧+ / ⌘⇧-",			"Scale up / down" },
			});

			AddSection( "Track", new string[,]
			{
				{ "⌘⇧↑ / ⌘⇧↓",			"Move track up / down" },
				{ "⌘⇧C",					"Set track color" },
				{ "⌘⇧H",					"Set track height" },
				{ "⌘⇧B / P / L",			"Display as bar / point / line" },
				{ "⌘⇧G / ⌘⇧U",			"Group / ungroup tracks" },
				{ "⌘⇧A",					"Select all features" },
				{ "⌘⇧T",					"Change type" },
				{ "⌘⇧D",					"Hide lane" },
			});

			AddSection( "Feature Operations", new string[,]
			{
				{ "⌘U",					"Unite selected features" },
				{ "⌘M",					"Merge features" },
				{ "⌘V",					"Move features" },
				{ "⌘C",					"Copy features" },
				{ "⌘D",					"Delete features" },
				{ "⌘Z",					"Undo" },
			});

			AddSection( "Feature Adjust", new string[,]
			{
				{ "⌥← / ⌥→",				"Move feature left / right (keep length)" },
				{ "⌥↑",					"Increase start point" },
				{ "⌥↓",					"Increase end point" },
			});
		}

		private void AddSection( string strTitle, string[,] arrRows )
		{
			TextBlock		tbTitle			= new TextBlock();
			tbTitle.Text					= strTitle;
			tbTitle.FontSize				= 14;
			tbTitle.FontWeight				= FontWeight.Bold;
			tbTitle.Margin					= new Thickness( 0, m_spContent.Children.Count > 0 ? 14 : 0, 0, 6 );
			m_spContent.Children.Add( tbTitle );

			Grid			grid			= new Grid();
			grid.ColumnDefinitions.Add( new ColumnDefinition { Width = new GridLength( 190 ) } );
			grid.ColumnDefinitions.Add( new ColumnDefinition { Width = new GridLength( 1, GridUnitType.Star ) } );

			int				nRows			= arrRows.GetLength( 0 );

			for( int i = 0; i < nRows; i++ )
			{
				grid.RowDefinitions.Add( new RowDefinition { Height = GridLength.Auto } );

				IBrush		bshBg			= i % 2 == 0
											? new SolidColorBrush( Color.FromRgb( 245, 245, 245 ) )
											: Brushes.White;

				Border		bdrKey			= new Border();
				bdrKey.Background			= bshBg;
				bdrKey.Padding				= new Thickness( 8, 4, 8, 4 );
				Grid.SetRow( bdrKey, i );
				Grid.SetColumn( bdrKey, 0 );

				TextBlock	tbKey			= new TextBlock();
				tbKey.Text					= arrRows[ i, 0 ];
				tbKey.FontFamily			= new FontFamily( "Menlo, 'SF Mono', Consolas, monospace" );
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
