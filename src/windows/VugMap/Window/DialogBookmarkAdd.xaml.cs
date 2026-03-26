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

using VugMap.Utility.Data;

namespace VugMap.Window
{
	/// <summary>
	/// Interaction logic for DialogBookmarkAdd.xaml
	/// </summary>
	public partial class DialogBookmarkAdd : System.Windows.Window
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

		public int Position
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
			DialogResult					= true;

			Close();
		}

		private void OnCancelClick( object obj, RoutedEventArgs ea )
		{
			Close();
		}

		private void OnLoaded( object obj, RoutedEventArgs ea )
		{
			m_tbTitle.Focus();
		}
	}
}
