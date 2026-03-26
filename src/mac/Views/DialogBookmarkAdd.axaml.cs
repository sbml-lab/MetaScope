using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

using MetaScope.Models;

namespace MetaScope.Views
{
	/// <summary>
	/// Interaction logic for DialogBookmarkAdd.axaml
	/// </summary>
	public partial class DialogBookmarkAdd : Window
	{
		//			.								.								.
		private		string							m_strSeqId						= null;

		public DialogBookmarkAdd()
		{
			InitializeComponent();
		}

		public string SequenceId
		{
			get {	return m_strSeqId; }
			set {	m_strSeqId		= value; }
		}

		public string BookmarkTitle
		{
			get {	return m_tbTitle.Text; }
			set {	m_tbTitle.Text = value; }
		}

		public int GetPosition()
		{
			int				nPosition		= int.Parse( m_tbPosition.Text );

			return nPosition;
		}

		public void SetPosition( int nPosition )
		{
			m_tbPosition.Text				= nPosition.ToString();
		}

		public new int Position
		{
			get {	return GetPosition(); }
			set {	SetPosition( value ); }
		}

		public double GetZoom()
		{
			double			dZoom			= double.Parse( m_tbZoom.Text );

			return dZoom;
		}

		public void SetZoom( double dZoom )
		{
			m_tbZoom.Text					= dZoom.ToString();
		}

		public double Zoom
		{
			get {	return GetZoom(); }
			set {	SetZoom( value ); }
		}

		public DataBookmark MakeBookmark()
		{
			string			strTitle		= m_tbTitle.Text;
			int				nPosition		= Position;
			double			dZoom			= Zoom;

			DataBookmark	db				= new DataBookmark( m_strSeqId, strTitle, nPosition, dZoom );

			return db;
		}

		private void OnOkClick( object obj, RoutedEventArgs ea )
		{
			Close( true );
		}

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			Close( false );
		}

		private void OnLoaded( object obj, RoutedEventArgs ea )
		{
			m_tbTitle.Focus();
		}
	}
}
