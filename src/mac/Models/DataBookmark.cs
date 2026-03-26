using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MetaScope.Models
{
	public class DataBookmark
	{
		//			.								.								.
		private		string							m_strSequenceId					= null;
		private		string							m_strTitle						= null;
		private		int								m_nPosition						= 0;
		private		double							m_dZoom							= 0.0f;

		public DataBookmark()
		{
		}

		public DataBookmark( string strSeqId, string strTitle, int nPosition, double dZoom )
		{
			m_strSequenceId	= strSeqId;
			m_strTitle		= strTitle;
			m_nPosition		= nPosition;
			m_dZoom			= dZoom;
		}

		public string GetString()
		{
			string			str				= string.Format( "{0}, {1}, {2} ({3})", SequenceId, Position, Title, Zoom );

			return str;
		}

		[ XmlAttribute( AttributeName = "SequenceId", DataType = "string" ) ]
		public string SequenceId
		{
			get {	return m_strSequenceId; }
			set {	m_strSequenceId	= value; }
		}

		[ XmlAttribute( AttributeName = "Title", DataType = "string" ) ]
		public string Title
		{
			get {	return m_strTitle; }
			set {	m_strTitle		= value; }
		}

		[ XmlAttribute( AttributeName = "Position", DataType = "int" ) ]
		public int Position
		{
			get {	return m_nPosition; }
			set {	m_nPosition		= value; }
		}

		[ XmlAttribute( AttributeName = "Zoom", DataType = "double" ) ]
		public double Zoom
		{
			get {	return m_dZoom; }
			set {	m_dZoom			= value; }
		}
	}
}
