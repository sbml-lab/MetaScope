using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

using MetaScope.Controls;
using MetaScope.Models;
using MetaScope.Services.Error;

namespace MetaScope.Views
{
	public partial class DialogFeatureEdit : Window
	{
		//			.								.								.
		private		PnlMapLane						m_pnlLane						= null;

		public DialogFeatureEdit( PnlMapLane pnlLane )
		{
			m_pnlLane		= pnlLane;

			InitializeComponent();
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
				ErrorMessage.ShowError( "Start must be a valid integer." );
				m_tbStart.Focus();
				return;
			}

			if( int.TryParse( m_tbEnd.Text, out nEnd ) == false )
			{
				ErrorMessage.ShowError( "End must be a valid integer." );
				m_tbEnd.Focus();
				return;
			}

			if( m_tbScore.Text != "." )
			{
				float			fScore;
				if( float.TryParse( m_tbScore.Text, out fScore ) == false )
				{
					ErrorMessage.ShowError( "Score must be a number or '.' for missing." );
					m_tbScore.Focus();
					return;
				}
			}

			this.Close( true );
		}

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			this.Close( false );
		}

		protected override void OnOpened( EventArgs ea )
		{
			base.OnOpened( ea );

			m_tbAttribute.Focus();
		}
	}
}
