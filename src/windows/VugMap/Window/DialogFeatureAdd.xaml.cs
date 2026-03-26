using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interop;

using VugMap.Utility;
using VugMap.Utility.Data;

namespace VugMap.Window
{
	/// <summary>
	/// Interaction logic for DialogFeatureAdd.xaml
	/// </summary>
	public partial class DialogFeatureAdd : System.Windows.Window
	{
		//			.								.								.
		private		PnlMapLane						m_pnlLane						= null;

		public DialogFeatureAdd( PnlMapLane pnlLane )
		{
			m_pnlLane		= pnlLane;

			InitializeComponent();
		}

		protected override void OnSourceInitialized( EventArgs ea )
		{
			base.OnSourceInitialized( ea );

			HwndSource		hwndSource		= PresentationSource.FromVisual( this ) as HwndSource;

			if( hwndSource != null )
			{
				hwndSource.AddHook( UtilityWindow.HwndSourceHook );
			}
		}

		public string Source
		{
			get {	return m_tbSource.Text; }
			set {	m_tbSource.Text = value; }
		}

		public int Start
		{
			get {	return int.Parse( m_tbStart.Text ); }
			set {	m_tbStart.Text = value.ToString(); }
		}

		public int End
		{
			get {	return int.Parse( m_tbEnd.Text ); }
			set {	m_tbEnd.Text = value.ToString(); }
		}

		public double Score
		{
			get 
			{	
				if( m_tbScore.Text == "." )
				{
					return double.NaN;
				}
				else
				{
					return double.Parse( m_tbScore.Text ); 
				}
			}
			set {	m_tbScore.Text = value.ToString(); }
		}

		public string Strand
		{
			get {	return m_tbStrand.Text; }
		}

		public string Phase
		{
			get {	return m_tbPhase.Text; }
		}

		public string Attribute
		{
			get {	return m_tbAttribute.Text; }
		}

		public DataFeature MakeFeatureAdded()
		{
			DataFeature		df				= new DataFeature( Source, Start, End, Score, Strand, Phase, Attribute );

			return df;
		}

		private void OnOkClick( object obj, RoutedEventArgs ea )
		{
			int				nStart;
			int				nEnd;

			if( int.TryParse( m_tbStart.Text, out nStart ) == false )
			{
				MessageBox.Show( "Start must be a valid integer.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning );
				m_tbStart.Focus();
				return;
			}

			if( int.TryParse( m_tbEnd.Text, out nEnd ) == false )
			{
				MessageBox.Show( "End must be a valid integer.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning );
				m_tbEnd.Focus();
				return;
			}

			if( m_tbScore.Text != "." )
			{
				double			dScore;
				if( double.TryParse( m_tbScore.Text, out dScore ) == false )
				{
					MessageBox.Show( "Score must be a number or '.' for missing.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning );
					m_tbScore.Focus();
					return;
				}
			}

			DialogResult					= true;

			DataFeature		df				= MakeFeatureAdded();
			df.ColorBrush					= m_pnlLane.DataTypeSelected.DoBrushGet();

			m_pnlLane.DoFeatureAdd( df );

			MainWindow		mw				= MainWindow.GetMainWindow();
			mw.DoAutoSaveImmediate();

			Close();
		}

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			Close();
		}

		private void OnSizeClick( object obj, RoutedEventArgs ea )
		{
			DialogFeatureAddSize
							dlg				= new DialogFeatureAddSize();

			int				nStart			= 0;
			int				nEnd			= 0;

			bool			bStart			= int.TryParse( m_tbStart.Text, out nStart );
			bool			bEnd			= int.TryParse( m_tbEnd.Text, out nEnd );

			if( bStart == true && bEnd == true )
			{
				int				nWidth			= nEnd - nStart + 1;
				dlg.SetWidth( nWidth );
			}

			dlg.Owner						= this;

			Nullable< bool >	b			= dlg.ShowDialog();
			if( b == true )
			{
				int				nWidth			= dlg.GetWidth();
				bool			bPosStart		= dlg.PositionStart;

				if( bPosStart == true )
				{					
					nEnd			= nStart + nWidth - 1;
					
					m_tbEnd.Text	= nEnd.ToString();
				}
				else
				{
					nStart			= nStart - nWidth / 2;
					nEnd			= nStart + nWidth - 1;

					m_tbStart.Text	= nStart.ToString();
					m_tbEnd.Text	= nEnd.ToString();
				}
			}
		}

		private void OnLoaded( object obj, RoutedEventArgs ea )
		{
			m_tbAttribute.Focus();
		}
	}
}
