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
	/// Interaction logic for DialogFeatureEdit.xaml
	/// </summary>
	public partial class DialogFeatureEdit : System.Windows.Window
	{
		//			.								.								.
		private		PnlMapLane						m_pnlLane						= null;

		public DialogFeatureEdit( PnlMapLane pnlLane )
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
		}

		public int Start
		{
			get {	return int.Parse( m_tbStart.Text ); }
		}

		public int End
		{
			get {	return int.Parse( m_tbEnd.Text ); }
		}

		public float Score
		{
			get 
			{	
				if( m_tbScore.Text == "." )
				{
					return float.NaN;
				}
				else
				{
					return float.Parse( m_tbScore.Text ); 
				}
			}
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

		public DataFeature MakeFeatureEdited()
		{
			DataFeature		df				= new DataFeature( Source, Start, End, Score, Strand, Phase, Attribute );

			return df;
		}

		public void SetFeature( DataFeature df )
		{
			m_tbSource.Text					= df.Source;
			m_tbStart.Text					= df.Start.ToString();
			m_tbEnd.Text					= df.End.ToString();
			m_tbScore.Text					= df.ScoreString;
			m_tbStrand.Text					= df.Strand;
			m_tbPhase.Text					= df.Phase.ToString();
			m_tbAttribute.Text				= df.Attribute;											
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
				float			fScore;
				if( float.TryParse( m_tbScore.Text, out fScore ) == false )
				{
					MessageBox.Show( "Score must be a number or '.' for missing.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning );
					m_tbScore.Focus();
					return;
				}
			}

			DialogResult					= true;

			Close();
		}

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			Close();
		}

		private void OnLoaded( object obj, RoutedEventArgs ea )
		{
			m_tbAttribute.Focus();
		}
	}
}
