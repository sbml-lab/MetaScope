using System;
using System.Collections;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace VugMap.Utility.Data
{
	using		DicAttribute					= Dictionary< string, string >;
	using		ListFeature						= List< DataFeature >;
	using		ListString						= List< string >;

	public class DataFeature : IComparable
	{
		public static DataFeature MakeFeatureByMerge( ListFeature lst )
		{
			DataFeature		dfFirst			= lst[ 0 ];

			string			strSource		= dfFirst.Source;
			int				nStart			= int.MaxValue;
			int				nEnd			= int.MinValue;
			double[]		dScoreA			= new double[ lst.Count ];
			string			strStrand		= dfFirst.Strand;
			string			strPhase		= dfFirst.Phase;
			StringBuilder	sbAttribute		= new StringBuilder();

			for( int i = 0; i < lst.Count; i++ )
			{
				DataFeature		df				= lst[ i ];

				nStart			= Math.Min( nStart, df.Start );
				nEnd			= Math.Max( nEnd, df.End );
				dScoreA[ i ]	= df.Score;

				if( df.Attribute == null || df.Attribute == "" )
				{
				}
				else
				{
					sbAttribute.Append( df.Attribute + ";" );
				}
			}

			double			dScore			= UtilityMath.GetAverage( dScoreA );
			string			strAttribute	= sbAttribute.ToString();

			DataFeature		dfNew			= new DataFeature( strSource, nStart, nEnd, dScore, strStrand, strPhase, "" );

			return dfNew;
		}

			//			.								.								.
		private static ushort FloatToHalf( float value )
		{
			int fbits = BitConverter.ToInt32( BitConverter.GetBytes( value ), 0 );
			int sign  = ( fbits >> 16 ) & 0x8000;
			if( ( fbits & 0x7FFFFFFF ) == 0 )		return ( ushort ) sign;						// zero
			int exp   = ( fbits >> 23 ) & 0xFF;
			int man   = fbits & 0x007FFFFF;
			if( exp == 255 )						return ( ushort )( sign | 0x7C00 | ( ( man != 0 ) ? 1 : 0 ) );	// Inf/NaN
			int hexp  = exp - 127 + 15;
			if( hexp <= 0 )							return ( ushort ) sign;						// underflow → zero
			if( hexp >= 31 )						return ( ushort )( sign | 0x7C00 );			// overflow → Inf
			return ( ushort )( sign | ( hexp << 10 ) | ( man >> 13 ) );
		}

		private static float HalfToFloat( ushort half )
		{
			int sign = ( half & 0x8000 ) << 16;
			int exp  = ( half >> 10 ) & 0x1F;
			int man  = half & 0x03FF;
			if( exp == 0 )
			{
				if( man == 0 )				return BitConverter.ToSingle( BitConverter.GetBytes( sign ), 0 );
				exp = 1;
				while( ( man & 0x0400 ) == 0 ) { man <<= 1; exp--; }
				man &= 0x03FF;
				int fbits = sign | ( ( exp + 127 - 15 ) << 23 ) | ( man << 13 );
				return BitConverter.ToSingle( BitConverter.GetBytes( fbits ), 0 );
			}
			else if( exp == 31 )
			{
				int fbits = sign | 0x7F800000 | ( man << 13 );
				return BitConverter.ToSingle( BitConverter.GetBytes( fbits ), 0 );
			}
			else
			{
				int fbits = sign | ( ( exp + 127 - 15 ) << 23 ) | ( man << 13 );
				return BitConverter.ToSingle( BitConverter.GetBytes( fbits ), 0 );
			}
		}

		private static bool HalfIsNaN( ushort half )
		{
			int exp = ( half >> 10 ) & 0x1F;
			int man = half & 0x03FF;
			return ( exp == 31 && man != 0 );
		}

		private		static Encoding					ASCII							= System.Text.ASCIIEncoding.Default;
		private		static bool						s_bSkipAttributeStorage			= false;

		public static bool SkipAttributeStorage
		{
			get {	return s_bSkipAttributeStorage; }
			set {	s_bSkipAttributeStorage = value; }
		}

		private		string							m_strSource						= null;
		private		int								m_nStart						= 0;
		private		int								m_nEnd							= 0;
		private		ushort							m_usScore						= 0;		// IEEE 754 half-precision float (2 bytes)
		private		byte							m_bStrand						= 0;			// '+' '−' '.' → byte
		private		byte							m_bPhase						= 0;			// '0' '1' '2' '.' → byte
		private		byte[]							m_bAttributeA					= null;
		private		Brush							m_bshColor						= null;

			public DataFeature( string strSource, int nStart, int nEnd, double dScore, string strStrand, string strPhase, string strAttribute )
		{
			DoSourceSet( strSource );
			m_nStart		= nStart;
			m_nEnd			= nEnd;
			m_usScore		= FloatToHalf( ( float ) dScore );
			Strand			= strStrand;
			Phase			= strPhase;

			if( s_bSkipAttributeStorage == false )
				DoAttributeSet( strAttribute );
			DoAttributeParse( strAttribute );
		}

		public DataFeature( DataFeature df )
		{
			DoSourceSet( df.Source );
			m_nStart		= df.Start;
			m_nEnd			= df.End;
			m_usScore		= df.m_usScore;
			m_bStrand		= df.m_bStrand;
			m_bPhase		= df.m_bPhase;

			if( s_bSkipAttributeStorage == false )
				DoAttributeSet( df.Attribute );
			DoAttributeParse( df.Attribute );
		}

		public bool DoCheckCompatible( DataFeature df )
		{
			if( Start != df.Start )
				return false;
			else if( End != df.End )
				return false;
			else if( Phase != df.Phase )
				return false;
			else if( Source != df.Source )
				return false;
			//else if( Strand != df.Strand )
			//	return false;

			return true;
		}

			public void DoSourceSet( string strSource )
		{
			if( strSource  == null )		return;

			m_strSource						= string.Intern( strSource );
		}

		public string DoSourceGet()
		{
			return m_strSource;
		}

		public void DoAttributeSet( string strAttribute )
		{
			if( strAttribute == null )		return;

			m_bAttributeA					= ASCII.GetBytes( strAttribute );
		}

		public string DoAttributeGet()
		{
			if( m_bAttributeA == null )		return null;

			string			str				= ASCII.GetString( m_bAttributeA );

			return str;
		}

		public bool DoCheckOverlap( int nPosition )
		{
			if( nPosition >= Start && nPosition <= End )
				return true;
			else
				return false;
		}

		public bool DoCheckOverlap( DataFeature df )
		{
			if( DoCheckOverlap( df.Start ) == true ||
				DoCheckOverlap( df.End ) == true ||
				df.DoCheckOverlap( Start ) == true ||
				df.DoCheckOverlap( End ) == true )
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool DoSearch( string strSearch, bool bCase )
		{
			if( bCase == true )
			{
				if( m_bAttributeA != null &&
					Attribute.IndexOf( strSearch ) > 0 )
					return true;
				else
					return false;
			}
			else
			{
				if( m_bAttributeA != null &&
					Attribute.ToLower().IndexOf( strSearch.ToLower() ) > 0 )
					return true;
				else
					return false;
			}
		}

		public Brush ColorBrush
		{
			get {	return m_bshColor; }
			set {	m_bshColor		= value; }
		}

		public string Source
		{
			get {	return DoSourceGet(); }
			set {	DoSourceSet( value ); }
		}

		public string ScoreString
		{
			get
			{
				if( HalfIsNaN( m_usScore ) == true )
				{
					return ".";
				}
				else
				{
					return HalfToFloat( m_usScore ).ToString();
				}
			}
		}

		public double ScoreReal
		{
			get {	return ( double ) HalfToFloat( m_usScore ); }
			set {	m_usScore = FloatToHalf( ( float ) value ); }
		}

		public double Score
		{
			get
			{
				if( HalfIsNaN( m_usScore ) == true )
				{
					double			dScore			= 0.0f;

					if( m_bStrand == (byte) '+' )
						dScore			= 1.0f;
					else if( m_bStrand == (byte) '-' )
						dScore			= -1.0f;

					return dScore;
				}
				else
				{
					return ( double ) HalfToFloat( m_usScore );
				}

			}

			set
			{
				m_usScore		= FloatToHalf( ( float ) value );
			}
		}

		public int CompareTo( Object obj )
		{
			DataFeature		df				= obj as DataFeature;

			if( m_nStart < df.Start )
			{
				return -1;
			}
			else if( m_nStart > df.Start )
			{
				return 1;
			}
			else
			{
				if( m_nEnd < df.End )		return -1;
				else if( m_nEnd > df.End )	return 1;
				else						return 0;
			}
		}

		public int Start
		{
			get {	return m_nStart; }
			set {	m_nStart = value; }
		}

		public int End
		{
			get {	return m_nEnd; }
			set {	m_nEnd = value; }
		}

			public string Strand
		{
			get {	return ( ( char ) m_bStrand ).ToString(); }
			set {	m_bStrand = ( value != null && value.Length > 0 ) ? ( byte ) value[ 0 ] : ( byte ) '.'; }
		}

		public string Phase
		{
			get {	return ( ( char ) m_bPhase ).ToString(); }
			set {	m_bPhase = ( value != null && value.Length > 0 ) ? ( byte ) value[ 0 ] : ( byte ) '.'; }
		}

		public string Attribute
		{
			get {	return DoAttributeGet(); }
			set {	DoAttributeSet( value ); DoAttributeParse(); }
		}

		public void SetBrushColor( string strColor )
		{
			m_bshColor		= ManagerBrush.GetManager().GetBrush( strColor );
		}

		public void DoAttributeAdd( string strAttr, string strValue )
		{
			DicAttribute	dic			= DoAttributeParse();

			if( dic.Keys.Contains( strAttr ) == true )
			{
				dic[ strAttr ]				= string.Format( "{0}, {1}", dic[ strAttr ], strValue );
			}
			else
			{
				dic.Add( strAttr, strValue );
			}

			DoAttributeSet( dic );
			DoAttributeParse();
		}

		public string DoAttributeGet( string strAttr )
		{
			DicAttribute	dic			= DoAttributeParse();

			if( dic.Keys.Contains( strAttr ) == true )
			{
				string			strValue		= dic[ strAttr ];

				return strValue;
			}
			else
			{
				return null;
			}
		}

		public void DoAttributeSet( string strAttr, string strValue )
		{
			DicAttribute	dic			= DoAttributeParse();

			if( dic.Keys.Contains( strAttr ) == true )
			{
				dic[ strAttr ]				= strValue;
			}
			else
			{
				dic.Add( strAttr, strValue );
			}

			DoAttributeSet( dic );
			DoAttributeParse();
		}

		private void DoAttributeSet( DicAttribute dic )
		{
			StringBuilder	sb				= new StringBuilder();

			foreach( KeyValuePair< string, string > kv in dic )
			{
				string			strPair		= null;

				if( kv.Value == null || kv.Value == "" )
					strPair			= string.Format( "{0};", kv.Key );
				else
					strPair			= string.Format( "{0}={1};", kv.Key, kv.Value );

				sb.Append( strPair );
			}

			string			strAttribute	= sb.ToString();

			Attribute						= strAttribute;
		}

		public DicAttribute DoAttributeParse()
		{
			DicAttribute	dic				= new DicAttribute();

			dic								= DoAttributeParse( Attribute );

			return dic;
		}

		public int DoComparePosition( DataFeature df )
		{
			if( Start < df.Start )
			{
				return -1;
			}
			else if( Start > df.Start )
			{
				return 1;
			}
			else
			{
				if( End < df.End )
				{
					return -1;
				}
				else if( End > df.End )
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}
		}

		private DicAttribute DoAttributeParse( string strAttribute )
		{
			if( strAttribute == null )
			{
				return null;
			}

			DicAttribute	dic				= new DicAttribute();

			string[]		strAttrA		= strAttribute.Split( ';' );
			foreach( string strEach in strAttrA )
			{
				if( strEach == "" )				continue;

				string[]		strEachA		= strEach.Trim().Split( '=' );
				string			strAttr			= strEachA[ 0 ];
				string			strValue		= null;

				if( strEachA.Length == 2 )
				{
					strValue		= strEachA[ 1 ];
				}

				if( dic.Keys.Contains( strAttr ) == true )
				{
					dic[ strAttr ]	= string.Format( "{0}, {1}", dic[ strAttr ], strValue );
				}
				else
				{
					dic[ strAttr ]	= strValue;
				}

				if( strAttr.ToLower() == "color" )
				{
					m_bshColor		= ManagerBrush.GetManager().GetBrush( strValue );
				}
			}

			return dic;
		}
	}

	public class ComparerFeatureScore : IComparer< DataFeature >
	{
		public int Compare( DataFeature df0, DataFeature df1 )
		{
			if( df0.Score > df1.Score )
			{
				return -1;
			}
			else if( df0.Score < df1.Score )
			{
				return 1;
			}
			else
			{
				return 0;
			}
		}
	}
}
