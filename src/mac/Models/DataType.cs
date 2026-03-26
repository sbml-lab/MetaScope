using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Avalonia.Media;

using MetaScope.Services;

namespace MetaScope.Models
{
	using		ListFeature						= List< DataFeature >;
	using		ListListFeature					= List< List< DataFeature > >;
	using		DicDataFeature					= SortedDictionary< BigInteger, DataFeature >;
	using		ListDataFeature					= LinkedList< DataFeature >;
	using		ListDataFeatureNode				= LinkedListNode< DataFeature >;
	using		ListDataFeatureIndex			= List< LinkedListNode< DataFeature > >;

	/// <summary>
	/// Delegate for progress reporting during long operations.
	/// Defined here temporarily until dialog classes are ported.
	/// </summary>
	public delegate void DelegateDoProgressSet( int nCurrent, int nTotal );

	/// <summary>
	/// Delegate for search progress reporting.
	/// Defined here temporarily until DialogSearch is ported.
	/// </summary>
	public delegate void DelegateDoSearchProgressUpdate( int nCurrent, int nTotal );

	public class DataType
	{
		//			.								.								.
		public		static int						N_INDEXSPAN						= 1000;
		public		static int						N_INDEXSIZE						= ( 15 * 1000 * 1000 ) / N_INDEXSPAN;		// 15Mbp (bacteria max genome cover)
		public		static int						N_KEYRANGE						= 300 * 1000 * 1000;						// BigInteger key calculation (do not change)

		private		DataFile						m_dfFile						= null;
		private		string							m_strSequenceId					= null;
		private		string							m_strType						= null;
		private		DicDataFeature					m_dicFeature					= null;
		private		ListDataFeature					m_lstFeature					= null;
		private		ListDataFeatureIndex			m_lstIndexStart					= null;
		private		ListDataFeatureIndex			m_lstIndexEnd					= null;
		private		int								m_nPositionMin					= int.MaxValue;
		private		int								m_nPositionMax					= int.MinValue;
		private		double							m_dScoreMin						= double.MaxValue;
		private		double							m_dScoreMax						= double.MinValue;
		private		double							m_dScaleMax						= 0.0f;
		private		double							m_dScaleMin						= 0.0f;
		private		bool							m_bScale						= false;
		private		IBrush							m_bshFeature					= null;
		private		bool							m_bEdited						= false;
		private		EDataTypeDisplay				m_eDisplay						= EDataTypeDisplay.BAR;
		private		int								m_nSeed							= 0;
		private		int								m_nStackLayer					= 0;
		private		bool							m_bReadOnly						= false;
		private		DataFeature[]					m_arrFeature					= null;				// read-only: sorted array

		public DataType( DataFile df, string strType, string strSequenceId )
			: this( df, strType, strSequenceId, false )
		{
		}

		public DataType( DataFile df, string strType, string strSequenceId, bool bReadOnly )
		{
			m_dfFile		= df;
			m_strSequenceId	= strSequenceId;
			m_strType		= strType;
			m_bReadOnly		= bReadOnly;
			m_dicFeature	= new DicDataFeature();
			m_lstFeature	= new ListDataFeature();
			m_bshFeature	= ManagerBrush.GetManager().GetBrushRandom();

			if( bReadOnly == false )
			{
				m_lstIndexStart	= new ListDataFeatureIndex( N_INDEXSIZE );
				m_lstIndexEnd	= new ListDataFeatureIndex( N_INDEXSIZE );

				for( int i = 0; i < N_INDEXSIZE; i++ )
				{
					m_lstIndexStart.Add( new ListDataFeatureNode( null ) );
					m_lstIndexEnd.Add( new ListDataFeatureNode( null ) );
				}
			}
		}

		public bool IsReadOnly
		{
			get {	return m_bReadOnly; }
			set {	m_bReadOnly = value; }
		}

		public void DoScale( double dScaleMax, double dScaleMin )
		{
			m_bScale		= true;
			m_dScaleMax		= dScaleMax;
			m_dScaleMin		= dScaleMin;
		}

		public void DoScaleAuto()
		{
			m_bScale		= true;
			m_dScaleMax		= m_dScoreMax;
			m_dScaleMin		= m_dScoreMin;
		}

		public bool Scale
		{
			get {	return m_bScale; }
			set {	m_bScale = value; }
		}

		public double ScaleMax
		{
			get {	return m_dScaleMax; }
			set {	m_dScaleMax		= value; }
		}

		public double ScaleMin
		{
			get {	return m_dScaleMin; }
			set {	m_dScaleMin		= value; }
		}

		public void DoAdjust( double dMultiply, double dShift, int nWidth, DelegateDoProgressSet delProgress, object dlg /* TODO: DialogLaneOperation */ )
		{
			if( m_bReadOnly )		return;

			int				nCount			= 1;
			int				nTotal			= GetCount();

			ListDataFeatureNode	lnkFirst	= m_lstFeature.First;
			ListDataFeatureNode	lnkEnd		= m_lstFeature.Last;
			ListDataFeatureNode	lnk			= lnkFirst;

			Stopwatch		sw				= new Stopwatch();
			sw.Start();

			while( lnk != null && lnk != lnkEnd )
			{
				if( double.IsNaN( dMultiply ) == false )
				{
					lnk.Value.ScoreReal				= lnk.Value.ScoreReal * dMultiply;
				}
				else if( double.IsNaN( dShift ) == false )
				{
					lnk.Value.ScoreReal				= lnk.Value.ScoreReal + dShift;
				}
				else
				{
					int				nStart			= lnk.Value.Start;
					int				nEnd			= lnk.Value.End;
					double			dCenter			= ( ( double ) nStart + nEnd ) / 2;

					nStart							= ( int ) ( dCenter - ( ( double ) nWidth ) / 2 );
					nEnd							= nStart + nWidth;

					lnk.Value.Start					= nStart;
					lnk.Value.End					= nEnd;
				}

				if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
				{
					Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( nCount, nTotal ) );
					sw.Restart();
				}
				else
				{
					sw.Start();
				}

				nCount++;
				lnk				= lnk.Next;
			}

			if( delProgress != null )
			{
				Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( 100, 100 ) );
			}

			sw.Stop();
		}

		public void DoAssignId( string strIdPattern, DelegateDoProgressSet delProgress, object dlg /* TODO: DialogLaneOperation */ )
		{
			if( m_bReadOnly )		return;

			int				nCount			= 1;
			int				nTotal			= GetCount();

			ListDataFeatureNode	lnkFirst	= m_lstFeature.First;
			ListDataFeatureNode	lnkEnd		= m_lstFeature.Last;
			ListDataFeatureNode	lnk			= lnkFirst;

			Stopwatch		sw				= new Stopwatch();
			sw.Start();

			while( lnk != null && lnk != lnkEnd )
			{
				string			strId			= strIdPattern;
				strId							= strId.Replace( "%COUNT%", nCount.ToString() );
				strId							= strId.Replace( "%POSITION%", lnk.Value.Start.ToString() );

				lnk.Value.DoAttributeSet( "ID", strId );

				if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
				{
					Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( nCount, nTotal ) );
					sw.Restart();
				}
				else
				{
					sw.Start();
				}

				nCount++;
				lnk				= lnk.Next;
			}

			if( delProgress != null )
			{
				Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( 100, 100 ) );
			}

			sw.Stop();
		}

		public int StackLayer
		{
			get {	return m_nStackLayer; }
		}

		private int DoSeedGet()
		{
			int				nSeed			= m_nSeed;

			DoSeedIncrease();

			return nSeed;
		}

		private void DoSeedIncrease()
		{
			m_nSeed++;
		}

		public EDataTypeDisplay Display
		{
			get {	return m_eDisplay; }
			set {	m_eDisplay = value; }
		}

		public void SetDisplay( EDataTypeDisplay eDisplay )
		{
			m_eDisplay		= eDisplay;
		}

		public LinkedListNode< DataFeature > GetFeatureLinkFirst()
		{
			if( m_lstFeature != null )		return m_lstFeature.First;

			// read-only: return standalone node from array (.Value accessible, use GetFeatureArray() for iteration)
			if( m_arrFeature != null && m_arrFeature.Length > 0 )
			{
				ListDataFeature		tmp			= new ListDataFeature();
				tmp.AddLast( m_arrFeature[ 0 ] );
				return tmp.First;
			}

			return null;
		}

		public LinkedListNode< DataFeature > GetFeatureLinkLast()
		{
			if( m_lstFeature != null )		return m_lstFeature.Last;

			// read-only: return standalone node from array (.Value accessible, use GetFeatureArray() for iteration)
			if( m_arrFeature != null && m_arrFeature.Length > 0 )
			{
				ListDataFeature		tmp			= new ListDataFeature();
				tmp.AddLast( m_arrFeature[ m_arrFeature.Length - 1 ] );
				return tmp.First;
			}

			return null;
		}

		public bool DoCheckCompatible( DataType dt )
		{
			if( GetCount() != dt.GetCount() )
			{
				return false;
			}
			else if( SequenceId != dt.SequenceId )
			{
				return false;
			}

			DataFeature[]	arr0			= GetFeatureArray();
			DataFeature[]	arr1			= dt.GetFeatureArray();

			if( arr0 != null && arr1 != null )
			{
				for( int i = 0; i < arr0.Length; i++ )
				{
					if( arr0[ i ].DoCheckCompatible( arr1[ i ] ) == false )
						return false;
				}
				return true;
			}

			LinkedListNode< DataFeature >	l0				= GetFeatureLinkFirst();
			LinkedListNode< DataFeature >	l1				= dt.GetFeatureLinkFirst();

			while( l0 != null )
			{
				bool			b				= l0.Value.DoCheckCompatible( l1.Value );
				if( b == false )
					return false;

				l0			= l0.Next;
				l1			= l1.Next;
			}

			return true;
		}

		public DataFile DataFile
		{
			get {	return m_dfFile; }
		}

		public IBrush DoBrushGet()
		{
			return m_bshFeature;
		}

		public void DoBrushSet( IBrush bsh )
		{
			m_bshFeature	= bsh;
		}

		public void DoBrushSet( string strColor )
		{
			m_bshFeature	= ManagerBrush.GetManager().GetBrush( strColor );
		}

		public string GetColorString()
		{
			ISolidColorBrush	scb				= m_bshFeature as ISolidColorBrush;
			string			strColor		= string.Format( "{0:X2}{1:X2}{2:X2}", scb.Color.R, scb.Color.G, scb.Color.B );

			return strColor;
		}

		public void DoSearch( string strSearch, bool bCase, ListFeature lstReturn, DelegateDoSearchProgressUpdate del )
		{
			IEnumerable<DataFeature>	enumFeature		= ( m_arrFeature != null ) ? (IEnumerable<DataFeature>) m_arrFeature : (IEnumerable<DataFeature>) m_lstFeature;
			int				nCountTotal		= GetCount();
			int				nCount			= 0;
			Stopwatch		sw				= new Stopwatch();
			sw.Start();

			foreach( DataFeature df in enumFeature )
			{
				if( df.DoSearch( strSearch, bCase ) == true )
					lstReturn.Add( df );

				nCount++;

				sw.Stop();
				if( sw.ElapsedMilliseconds >= 100 )
				{
					del( nCount, nCountTotal );
					sw.Restart();
				}
				else
				{
					sw.Start();
				}
			}

			del( nCountTotal, nCountTotal );
		}

		public void DoColorSet( string strColor )
		{
			m_bshFeature					= ManagerBrush.GetManager().GetBrush( strColor );
		}

		public void DoColorSet( Color clr )
		{
			m_bshFeature					= ManagerBrush.GetManager().GetBrush( clr );
		}

		public void DoDispose()
		{
		}

		public void DoClose()
		{
			m_dfFile.DoClose();
		}

		public void DoSave( StreamWriter sw )
		{
			IEnumerable<DataFeature>	enumFeature		= ( m_arrFeature != null ) ? (IEnumerable<DataFeature>) m_arrFeature : (IEnumerable<DataFeature>) m_lstFeature;

			foreach( DataFeature df in enumFeature )
			{
				string			strAttr			= df.Attribute ?? "";
				string			str				= string.Format( "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}",
													m_strSequenceId, df.Source, m_strType, df.Start, df.End, df.ScoreString, df.Strand, df.Phase, strAttr );

				sw.WriteLine( str );
			}

			sw.Flush();

			SetEdited( false );
		}

		public double ScoreMin
		{
			get
			{
				return m_dScoreMin;
			}
		}

		public double ScoreMax
		{
			get
			{
				return m_dScoreMax;
			}
		}

		public int PositionMin
		{
			get
			{
				return m_nPositionMin;
			}
		}

		public int PositionMax
		{
			get
			{
				return m_nPositionMax;
			}
		}

		public int GetCount()
		{
			if( m_arrFeature != null )		return m_arrFeature.Length;
			if( m_lstFeature != null )		return m_lstFeature.Count;
			return 0;
		}

		public DataFeature[] GetFeatureArray()
		{
			return m_arrFeature;
		}

		public int GetFeatureIndexByStart( int nPosStart )
		{
			return BinarySearchByStart( nPosStart );
		}

		// read-only: return first index where End >= nPos (Start-sorted, so linear scan)
		public int GetFeatureIndexByEnd( int nPos )
		{
			if( m_arrFeature == null || m_arrFeature.Length == 0 )		return -1;

			for( int i = 0; i < m_arrFeature.Length; i++ )
			{
				if( m_arrFeature[ i ].End >= nPos )
					return i;
			}

			return -1;
		}

		// read-only mode: binary search for first index where Start >= nPosStart
		private int BinarySearchByStart( int nPosStart )
		{
			if( m_arrFeature == null || m_arrFeature.Length == 0 )		return -1;

			int				lo				= 0;
			int				hi				= m_arrFeature.Length - 1;
			int				result			= -1;

			while( lo <= hi )
			{
				int			mid				= lo + ( hi - lo ) / 2;

				if( m_arrFeature[ mid ].Start >= nPosStart )
				{
					result		= mid;
					hi			= mid - 1;
				}
				else
				{
					lo			= mid + 1;
				}
			}

			return result;
		}

			public ListDataFeatureNode GetFeatureLinkByStart( int nPosStart )
		{
			if( m_bReadOnly )
			{
				return null;	// read-only: caller uses GetFeatureIndexByStart()
			}

			int				nIndexStart		= nPosStart / N_INDEXSPAN;
			Debug.Assert( nIndexStart >= 0 );
			if( nIndexStart >= N_INDEXSIZE )		return null;

			ListDataFeatureNode		node	= ( ListDataFeatureNode ) m_lstIndexStart[ nIndexStart ];

			while( node != null )
			{
				if( node.Value == null )
				{
					break;
				}

				if( node.Value.Start >= nPosStart )
				{
					return node;
				}
				else if( node.Value.Start < nPosStart )
				{
					node		= node.Next;
				}
				else
				{
					Debug.Assert( false );
				}
			}

			return null;
		}

			public ListDataFeatureNode GetFeatureLinkByEnd( int nPos )
		{
			if( m_bReadOnly )
			{
				return null;	// read-only: caller uses GetFeatureIndexByEnd()
			}

			int				nIndex			= nPos / N_INDEXSPAN;
			Debug.Assert( nIndex >= 0 );
			if( nIndex >= N_INDEXSIZE )		return null;

			ListDataFeatureNode		node2	= ( ListDataFeatureNode ) m_lstIndexEnd[ nIndex ];

			while( node2 != null )
			{
				if( node2.Value == null )
				{
					break;
				}

				if( node2.Value.End >= nPos )
				{
					return node2;
				}
				else if( node2.Value.End < nPos )
				{
					node2		= node2.Next;
				}
				else
				{
					Debug.Assert( false );
				}
			}

			return null;
		}

		public DataFeature GetFeatureByStart( int nPosStart )
		{
			if( m_bReadOnly )
			{
				int			idx				= BinarySearchByStart( nPosStart );
				if( idx < 0 )				return null;
				return m_arrFeature[ idx ];
			}

			ListDataFeatureNode	node		= GetFeatureLinkByStart( nPosStart );

			if( node == null )
			{
				return null;
			}
			else
			{
				return node.Value;
			}
		}

		public DataFeature GetFeatureContaining( int nPos, double dScore )
		{
			if( m_bReadOnly )
			{
				for( int i = 0; i < m_arrFeature.Length; i++ )
				{
					DataFeature		df			= m_arrFeature[ i ];
					if( df.Start > nPos )		break;
					if( df.DoCheckOverlap( nPos ) == true &&
						Math.Abs( dScore ) <= Math.Abs( df.Score ) )
					{
						return df;
					}
				}
				return null;
			}

			ListDataFeatureNode node		= GetFeatureLinkFirst();

			if( node == null )
			{
				return null;
			}
			else
			{
				while( nPos > node.Value.Start )
				{
					if( node.Value.DoCheckOverlap( nPos ) == true &&
						Math.Abs( dScore ) <= Math.Abs( node.Value.Score ) )
					{
						return node.Value;
					}

					node			= node.Next;

					if( node == null )
					{
						break;
					}
				}

				return null;
			}
		}

		public DataFeature GetFeatureContaining( int nPos )
		{
			if( m_bReadOnly )
			{
				int			idx				= GetFeatureIndexByEnd( nPos );
				if( idx < 0 )				return null;

				for( int i = idx; i >= 0; i-- )
				{
					DataFeature		df			= m_arrFeature[ i ];
					if( df.DoCheckOverlap( nPos ) == true )
						return df;
					if( df.End < nPos )
						break;
				}
				return null;
			}

			ListDataFeatureNode	node		= GetFeatureLinkByEnd( nPos );

			if( node == null )
			{
				return null;
			}
			else
			{
				while( nPos < node.Value.End )
				{
					if( node.Value.DoCheckOverlap( nPos ) == true )
					{
						return node.Value;
					}

					node			= node.Previous;

					if( node == null )
					{
						break;
					}
				}

				return null;
			}
		}
		/*
		public DataFeature GetFeatureContaining( int nPos )
		{
			ListDataFeatureNode	node		= GetFeatureLinkByStart( nPos );

			//Logger.Logger.PrintLine( "# DataType:GetFeatureContaining() {0}->{1},{2}", nPos, node.Value.Start, node.Value.End );

			while( node != null )
			{
				if( node.Value.DoCheckOverlap( nPos ) == true )
					return node.Value;
				else if( node.Value.End >= nPos )
					return null;
				else
					node			= node.Next;

			}

			return null;
		}*/

		public void BuildStack()
		{
			// Stack Layer
			ListListFeature		lstList		= new ListListFeature();
			ListFeature			lstLeft		= new ListFeature();

			if( m_arrFeature != null )
			{
				for( int i = 0; i < m_arrFeature.Length; i++ )
					lstLeft.Add( m_arrFeature[ i ] );
			}
			else
			{
				ListDataFeatureNode	lnkFirst	= m_lstFeature.First;
				ListDataFeatureNode	lnkEnd		= m_lstFeature.Last;
				ListDataFeatureNode	lnk			= lnkFirst;

				while( lnk != null && lnk != lnkEnd )
				{
					lstLeft.Add( lnk.Value );
					lnk				= lnk.Next;
				}
			}

			while( lstLeft.Count > 0 )
			{
				ListFeature		lstCurr			= new ListFeature();
				ListFeature		lstLeft0		= new ListFeature();
				DataFeature		dfCurr			= null;

				foreach( DataFeature df in lstLeft )
				{
					if( dfCurr != null && dfCurr.DoCheckOverlap( df ) == true )
					{
						lstLeft0.Add( df );
					}
					else
					{
						lstCurr.Add( df );

						dfCurr			= df;
					}
				}

				lstList.Add( lstCurr );
				lstLeft			= lstLeft0;
			}

			m_nStackLayer						= lstList.Count;
		}

		public void BuildIndex()
		{
			if( m_dicFeature != null )
			{
				m_lstFeature.Clear();
				m_lstFeature				= new ListDataFeature();

				foreach( KeyValuePair< BigInteger, DataFeature > kv in m_dicFeature )
				{
					m_lstFeature.AddLast( kv.Value );
				}

				m_dicFeature.Clear();
				m_dicFeature	= null;
			}

			// read-only mode: build array then free LinkedList
			if( m_bReadOnly )
			{
				int				nCount			= m_lstFeature.Count;
				m_arrFeature					= new DataFeature[ nCount ];

				int				idx				= 0;
				ListDataFeatureNode	node		= m_lstFeature.First;

				while( node != null )
				{
					m_arrFeature[ idx ]			= node.Value;
					idx++;
					node						= node.Next;
				}

				m_lstFeature.Clear();
				m_lstFeature					= null;

				return;
			}

			for( int i = 0; i < N_INDEXSIZE; i++ )
			{
				m_lstIndexStart[ i ]		= null;
				m_lstIndexEnd[ i ]			= null;
			}

			// Index for starting positions
			ListDataFeatureNode	ndIndex		= m_lstFeature.First;

			for( int i = 0; i < N_INDEXSIZE; i++ )
			{
				if( ndIndex == null )
				{
					break;
				}

				if( ndIndex.Value.Start >= i * N_INDEXSPAN )
				{
					m_lstIndexStart[ i ]		= ndIndex;
				}
				else
				{
					ndIndex			= ndIndex.Next;
					i--;
				}
			}

			// Index for ending positions
			ndIndex			= m_lstFeature.First;

			for( int i = 0; i < N_INDEXSIZE; i++ )
			{
				if( ndIndex == null )
				{
					break;
				}

				if( ndIndex.Value.End >= i * N_INDEXSPAN )
				{
					m_lstIndexEnd[ i ]			= ndIndex;
				}
				else
				{
					ndIndex			= ndIndex.Next;
					i--;
				}
			}
		}

		public bool DoFeatureContaining( DataFeature df )
		{
			if( m_arrFeature != null )
			{
				return Array.IndexOf( m_arrFeature, df ) >= 0;
			}

			bool			b				= m_lstFeature.Contains( df );

			return b;
		}

		public void DoFeatureAdd( ListFeature lst )
		{
			if( m_bReadOnly )		return;

			if( lst == null || lst.Count == 0 )
				return;

			m_dicFeature					= new DicDataFeature();

			foreach( DataFeature df in m_lstFeature )
			{
				DoFeatureAddBeforeIndex( df );
			}

			foreach( DataFeature df in lst )
			{
				DoFeatureAddBeforeIndex( df );
			}

			BuildIndex();
		}

		public void DoFeatureAdd( DataFeature df )
		{
			if( m_bReadOnly )		return;

			int				nStart			= df.Start;
			ListDataFeatureNode	node		= GetFeatureLinkByStart( nStart );

			if( node == null )
			{
				m_lstFeature.AddLast( df );
			}
			else if( node.Value.Start == df.Start )
			{
				if( node.Value.End > df.End )
				{
					m_lstFeature.AddBefore( node, df );
				}
				else if( node.Value.End < df.End )
				{
					m_lstFeature.AddAfter( node, df );
				}
				else
				{
					Debug.Assert( false );
				}
			}
			else if( node.Value.Start > df.Start )
			{
				m_lstFeature.AddBefore( node, df );
			}
			else
			{
				Debug.Assert( false );
			}

			BuildIndex();

			SetEdited( true );
		}

		public void DoFeatureRemove( ListFeature lst )
		{
			if( m_bReadOnly )		return;

			if( lst == null || lst.Count == 0 )
				return;

			// lst is sorted

			// O(m)
			foreach( DataFeature df in lst )
			{
				// O(n)
				m_lstFeature.Remove( df );
			}

			BuildIndex();

			SetEdited( true );
		}

		public void DoFeatureRemove( DataFeature df )
		{
			if( m_bReadOnly )		return;

			int				nStart			= df.Start;

			// O(m)
			m_lstFeature.Remove( df );

			BuildIndex();

			SetEdited( true );
		}

		public void DoFeatureAddBeforeIndex( DataFeature dfAdd )
		{
				BigInteger		iKey			= ( ( BigInteger ) dfAdd.Start ) * N_KEYRANGE + dfAdd.End;
			iKey							= iKey * ( N_INDEXSPAN ) + DoSeedGet();

			if( m_dicFeature.Keys.Contains( iKey ) == false )
			{
				m_dicFeature.Add( iKey, dfAdd );

				m_nPositionMin	= Math.Min( m_nPositionMin, dfAdd.Start );
				m_nPositionMax	= Math.Max( m_nPositionMax, dfAdd.End );
				m_dScoreMax		= Math.Max( m_dScoreMax, dfAdd.Score );
				m_dScoreMin		= Math.Min( m_dScoreMin, dfAdd.Score );

				m_nPositionMin	= Math.Min( m_nPositionMin, 0 );
			}
			else
			{
				int				nCount			= m_dicFeature.Count;
			}
		}

		public void SetEdited( bool bEdited )
		{
			if( m_bReadOnly )		return;

			m_bEdited			= bEdited;

			if( m_bEdited == true )
			{
				m_dfFile.SetEdited( m_bEdited );
			}
		}

		public bool IsEdited
		{
			get
			{
				return m_bEdited;
			}

			set
			{
				SetEdited( value );
			}
		}

		public string Type
		{
			get {	return m_strType; }
			set {	m_strType		= value; }
		}

		public string SequenceId
		{
			set
			{
				m_strSequenceId		= value;
			}

			get
			{
				return m_strSequenceId;
			}
		}
	}

	public enum EDataTypeDisplay
	{
		BAR				= 0x0,
		POINT,
		LINE,
		STACK,
	}
}
