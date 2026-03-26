using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.IO;
using System.Text;
using Avalonia.Media;

using MetaScope.Controls;
using MetaScope.Services;
using MetaScope.Services.Error;

namespace MetaScope.Models
{
	using		DicDataType						= Dictionary< string, DataType >;
	using		ListString						= List< string >;
	using		DicDataFeature					= SortedDictionary< BigInteger, DataFeature >;
	using		ListFeature						= List< DataFeature >;

	/// <summary>
	/// Enums for filter/merge operations.
	/// Defined here temporarily until dialog classes are ported.
	/// </summary>
	public enum EFilterMethod
	{
		OUTSIDE						= 0x0,
		INSIDE,
	}

	public enum EMergeMethod
	{
		MEDIAN						= 0x0,
		AVERAGE,
	}

	public class DataFile
	{
		//			.								.								.
		private		ManagerData						m_mgrData						= null;
		private		string							m_strFile						= null;
		private		string							m_strHeader						= null;
		private		string							m_strSource						= null;
		private		DicDataType						m_dicDataType					= null;
		private		ListString						m_lstSequenceId					= null;
		private		ListString						m_lstType						= null;
		private		bool							m_bEdited						= false;
		private		bool							m_bReadOnly						= false;

		public DataFile( ManagerData mgrData, string strFile )
		{
			m_mgrData		= mgrData;
			m_strFile		= strFile;
			m_dicDataType	= new DicDataType();
			m_lstSequenceId	= new ListString();
			m_lstType		= new ListString();
			m_bEdited		= false;
		}

		public bool IsReadOnly
		{
			get {	return m_bReadOnly; }
			set {	m_bReadOnly = value; }
		}

		public void AddDataTypeByIntegrationPorf( DataType dtStart, DataType dtStop, DataType dtProteome, DelegateDoProgressSet delProgress, object dlg /* TODO: DialogIntegrationOperation */ )
		{
			if( dtStart.GetCount() == 0 || dtStop.GetCount() == 0 || dtProteome.GetCount() == 0 )
			{
				return;
			}

			string			strSeqId		= dtStart.SequenceId;
			string			strType			= string.Format( "pORF..{0}..{1}..w/..{2}", dtStart.Type, dtStop.Type, dtProteome.Type );
			string			strStrand		= dtStart.GetFeatureLinkFirst().Value.Strand;
			string			strSource		= dtProteome.GetFeatureLinkFirst().Value.Source;

			if( strStrand == "+" )
			{
				HashSet< DataFeature >			hshStart		= new HashSet< DataFeature >();
				HashSet< DataFeature >			hshStop			= new HashSet< DataFeature >();
				HashSet< DataFeature >			hshProteome		= new HashSet< DataFeature >();

				DicDataFeature[]				dicA			= new DicDataFeature[ 3 ];
				dicA[ 0 ]										= new DicDataFeature();	// 3
				dicA[ 1 ]										= new DicDataFeature();	// 1
				dicA[ 2 ]										= new DicDataFeature();	// 2

				LinkedListNode< DataFeature >	lStart			= dtStart.GetFeatureLinkFirst();
				LinkedListNode< DataFeature >	lStop			= dtStop.GetFeatureLinkFirst();
				LinkedListNode< DataFeature >	lProteome		= dtProteome.GetFeatureLinkFirst();

				int				nCount			= 1;
				int				nTotal			= dtStart.GetCount() + dtStop.GetCount() + dtProteome.GetCount();

				Stopwatch		sw				= new Stopwatch();
				sw.Start();

				while( lStart != null )
				{
					BigInteger		iKey			= ( ( BigInteger ) lStart.Value.Start ) * ( DataType.N_KEYRANGE ) + lStart.Value.End;

					dicA[ lStart.Value.Start % 3 ].Add( iKey, lStart.Value );

					hshStart.Add( lStart.Value );

					sw.Stop();

					if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
					{
						{ int c = nCount, t = nTotal; Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( c, t ) ); }
						sw.Restart();
					}
					else
					{
						sw.Start();
					}

					nCount++;

					lStart			= lStart.Next;
				}

				while( lStop != null )
				{
					BigInteger		iKey			= ( ( BigInteger ) lStop.Value.Start ) * ( DataType.N_KEYRANGE ) + lStop.Value.End;

					dicA[ lStop.Value.Start % 3 ].Add( iKey, lStop.Value );

					hshStop.Add( lStop.Value );

					sw.Stop();

					if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
					{
						{ int c = nCount, t = nTotal; Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( c, t ) ); }
						sw.Restart();
					}
					else
					{
						sw.Start();
					}

					nCount++;

					lStop			= lStop.Next;
				}

				while( lProteome != null )
				{
					BigInteger		iKey			= ( ( BigInteger ) lProteome.Value.Start ) * ( DataType.N_KEYRANGE ) + lProteome.Value.End;

					if( dicA[ lProteome.Value.Start % 3 ].Keys.Contains( iKey ) == false )
					{
						dicA[ lProteome.Value.Start % 3 ].Add( iKey, lProteome.Value );
					}

					hshProteome.Add( lProteome.Value );

					sw.Stop();

					if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
					{
						{ int c = nCount, t = nTotal; Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( c, t ) ); }
						sw.Restart();
					}
					else
					{
						sw.Start();
					}

					nCount++;

					lProteome		= lProteome.Next;
				}

				sw.Stop();

				nCount			= 1;
				nTotal			= dicA[ 0 ].Count + dicA[ 1 ].Count + dicA[ 2 ].Count;

				sw.Start();

				foreach( DicDataFeature dic in dicA )
				{
					DataFeature		dfStart			= null;
					bool			bProteome		= false;

					foreach( KeyValuePair< BigInteger, DataFeature > kv in dic )
					{
						if( hshStart.Contains( kv.Value ) == true )
						{
							if( dfStart == null )
							{
								dfStart			= kv.Value;
							}
						}
						else if( hshStop.Contains( kv.Value ) == true )
						{
							if( dfStart != null && bProteome == true )
							{
								DataFeature		dfNew			= new DataFeature( strSource, dfStart.Start, kv.Value.End, double.NaN, "+", ".", "" );

								AddFeature( strSeqId, strType, dfNew );
							}

							dfStart			= null;
							bProteome		= false;
						}
						else
						{
							bProteome		= true;
						}

						sw.Stop();

						if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
						{
							{ int c = nCount, t = nTotal; Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( c, t ) ); }
							sw.Restart();
						}
						else
						{
							sw.Start();
						}

						nCount++;
					}
				}

				hshStart.Clear();
				hshStop.Clear();
				hshProteome.Clear();
				dicA[ 0 ].Clear();
				dicA[ 1 ].Clear();
				dicA[ 2 ].Clear();
			}
			else
			{
				HashSet< DataFeature >			hshStart		= new HashSet< DataFeature >();
				HashSet< DataFeature >			hshStop			= new HashSet< DataFeature >();
				HashSet< DataFeature >			hshProteome		= new HashSet< DataFeature >();

				DicDataFeature[]				dicA			= new DicDataFeature[ 3 ];
				dicA[ 0 ]										= new DicDataFeature();	// 3
				dicA[ 1 ]										= new DicDataFeature();	// 1
				dicA[ 2 ]										= new DicDataFeature();	// 2

				LinkedListNode< DataFeature >	lStart			= dtStart.GetFeatureLinkFirst();
				LinkedListNode< DataFeature >	lStop			= dtStop.GetFeatureLinkFirst();
				LinkedListNode< DataFeature >	lProteome		= dtProteome.GetFeatureLinkFirst();

				int				nCount			= 1;
				int				nTotal			= dtStart.GetCount() + dtStop.GetCount() + dtProteome.GetCount();

				Stopwatch		sw				= new Stopwatch();
				sw.Start();

				while( lStart != null )
				{
					int				nStart			= DataType.N_KEYRANGE - lStart.Value.Start;
					int				nEnd			= DataType.N_KEYRANGE - lStart.Value.End;

					BigInteger		iKey			= ( ( BigInteger ) nEnd ) * ( DataType.N_KEYRANGE ) + nStart;

					dicA[ lStart.Value.Start % 3 ].Add( iKey, lStart.Value );

					hshStart.Add( lStart.Value );

					sw.Stop();

					if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
					{
						{ int c = nCount, t = nTotal; Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( c, t ) ); }
						sw.Restart();
					}
					else
					{
						sw.Start();
					}

					nCount++;

					lStart			= lStart.Next;
				}

				sw.Stop();

				nCount			= 1;
				nTotal			= dicA[ 0 ].Count + dicA[ 1 ].Count + dicA[ 2 ].Count;

				sw.Start();

				while( lStop != null )
				{
					int				nStart			= DataType.N_KEYRANGE - lStop.Value.Start;
					int				nEnd			= DataType.N_KEYRANGE - lStop.Value.End;

					BigInteger		iKey			= ( ( BigInteger ) nEnd ) * ( DataType.N_KEYRANGE ) + nStart;

					dicA[ lStop.Value.Start % 3 ].Add( iKey, lStop.Value );

					hshStop.Add( lStop.Value );

					sw.Stop();

					if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
					{
						{ int c = nCount, t = nTotal; Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( c, t ) ); }
						sw.Restart();
					}
					else
					{
						sw.Start();
					}

					nCount++;

					lStop			= lStop.Next;
				}

				while( lProteome != null )
				{
					int				nStart			= DataType.N_KEYRANGE - lProteome.Value.Start;
					int				nEnd			= DataType.N_KEYRANGE - lProteome.Value.End;

					BigInteger		iKey			= ( ( BigInteger ) nEnd ) * ( DataType.N_KEYRANGE ) + nStart;

					if( dicA[ lProteome.Value.Start % 3 ].Keys.Contains( iKey ) == false )
					{
						dicA[ lProteome.Value.Start % 3 ].Add( iKey, lProteome.Value );
					}

					hshProteome.Add( lProteome.Value );

					sw.Stop();

					if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
					{
						{ int c = nCount, t = nTotal; Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( c, t ) ); }
						sw.Restart();
					}
					else
					{
						sw.Start();
					}

					nCount++;

					lProteome		= lProteome.Next;
				}

				foreach( DicDataFeature dic in dicA )
				{
					DataFeature		dfStart			= null;
					bool			bProteome		= false;

					foreach( KeyValuePair< BigInteger, DataFeature > kv in dic )
					{
						if( hshStart.Contains( kv.Value ) == true )
						{
							if( dfStart == null )
							{
								dfStart			= kv.Value;
							}
						}
						else if( hshStop.Contains( kv.Value ) == true )
						{
							if( dfStart != null && bProteome == true )
							{
								DataFeature		dfNew			= new DataFeature( strSource, kv.Value.Start, dfStart.End, double.NaN, "-", ".", "" );

								AddFeature( strSeqId, strType, dfNew );
							}

							dfStart			= null;
							bProteome		= false;
						}
						else
						{
							bProteome		= true;
						}

						sw.Stop();

						if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
						{
							{ int c = nCount, t = nTotal; Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( c, t ) ); }
							sw.Restart();
						}
						else
						{
							sw.Start();
						}

						nCount++;
					}
				}

				hshStart.Clear();
				hshStop.Clear();
				hshProteome.Clear();
				dicA[ 0 ].Clear();
				dicA[ 1 ].Clear();
				dicA[ 2 ].Clear();
			}
		}

		public void AddDataTypeByIntegrationRts( DataType dtRbr, DataType dtTd, DelegateDoProgressSet delProgress, object dlg /* TODO: DialogIntegrationOperation */ )
		{
			if( dtRbr.GetCount() == 0 || dtTd.GetCount() == 0 )
			{
				return;
			}

			string			strType			= string.Format( "RTS..{0}..w/rbr..{1}", dtTd.Type, dtRbr.Type );
			string			strStrand		= dtRbr.GetFeatureLinkFirst().Value.Strand;

			if( strStrand == "+" )
			{
				LinkedList< DataFeature >		lstChunk		= new LinkedList< DataFeature >();
				LinkedListNode< DataFeature >	lRbr			= dtRbr.GetFeatureLinkFirst();
				LinkedListNode< DataFeature >	lTd				= dtTd.GetFeatureLinkFirst();

				DataFeature		dfFirst			= null;
				DataFeature		dfLast			= null;

				while( lTd != null )
				{
					if( lTd.Value.Score == 0 )
					{
					}
					else
					{
						if( lRbr != null && lRbr.Value.Start < lTd.Value.Start )
						{
							if( dfFirst != null )
							{
								dfFirst.End		= dfLast.End;

								lstChunk.AddLast( dfFirst );

								dfFirst			= new DataFeature( lTd.Value );
								dfLast			= dfFirst;
							}

							lRbr			= lRbr.Next;
						}
						else if( dfFirst == null )
						{
							dfFirst			= new DataFeature( lTd.Value );
							dfLast			= dfFirst;
						}
						else
						{
							int				nLast			= dfLast.End;
							int				nPos			= lTd.Value.Start;

							if( nPos - nLast <= 25 )
							{
								dfLast			= lTd.Value;
							}
							else
							{
								dfFirst.End		= dfLast.End;

								lstChunk.AddLast( dfFirst );

								dfFirst			= new DataFeature( lTd.Value );
								dfLast			= dfFirst;
							}
						}
					}

					lTd				= lTd.Next;

					if( lTd == null )
					{
						dfFirst.End		= dfLast.End;

						lstChunk.AddLast( dfFirst );

						dfFirst			= null;
						dfLast			= null;
					}
				}

				/*
				foreach( DataFeature df in lstChunk )
				{
					DataFeature		dfNew			= new DataFeature( df );
					dfNew.Strand					= "+";

					AddFeature( dtRbr.SequenceId, strType + "_temp", dfNew );
				}
				 */

				lRbr											= dtRbr.GetFeatureLinkFirst();
				LinkedListNode< DataFeature >	lRbrNext		= null;
				LinkedListNode< DataFeature >	lChunk			= lstChunk.First;

				while( lRbr != null )
				{
					ListFeature		lst0			= new ListFeature();

					lRbrNext		= lRbr.Next;

					if( lRbrNext == null )
					{
						while( lChunk != null )
						{
							lst0.Add( lChunk.Value );

							lChunk			= lChunk.Next;
						}
					}
					else
					{
						while( lChunk != null )
						{
							if( lChunk.Value.Start < lRbrNext.Value.Start )
							{
								if( lChunk.Value.Start >= lRbr.Value.Start )
								{
									lst0.Add( lChunk.Value );
								}
							}
							else
							{
								break;
							}

							lChunk			= lChunk.Next;
						}
					}

					if( lst0.Count > 0 )
					{
						DataFeature		df0				= new DataFeature( lst0[ 0 ] );

						for( int i = 1; i < lst0.Count; i++ )
						{
							if( lst0[ i ].Start - df0.End <= 500 )
							{
								df0.End			= lst0[ i ].End;
							}
							else
							{
								break;
							}
						}

						if( df0.Start - lRbr.Value.Start < 400 )
						{
							AddFeature( dtRbr.SequenceId, strType, df0 );
						}
					}

					lRbr			= lRbr.Next;
				}
			}
			else
			{
				LinkedList< DataFeature >		lstChunk		= new LinkedList< DataFeature >();
				LinkedListNode< DataFeature >	lRbr			= dtRbr.GetFeatureLinkLast();
				LinkedListNode< DataFeature >	lTd				= dtTd.GetFeatureLinkLast();

				DataFeature		dfFirst			= null;
				DataFeature		dfLast			= null;

				while( lTd != null )
				{
					if( lTd.Value.Score == 0 )
					{
					}
					else
					{
						if( lRbr != null && lRbr.Value.Start > lTd.Value.Start )
						{
							if( dfFirst != null )
							{
								dfFirst.Start	= dfLast.Start;

								lstChunk.AddLast( dfFirst );

								dfFirst			= new DataFeature( lTd.Value );
								dfLast			= dfFirst;
							}

							lRbr			= lRbr.Previous;
						}
						else if( dfFirst == null )
						{
							dfFirst			= new DataFeature( lTd.Value );
							dfLast			= dfFirst;
						}
						else
						{
							int				nLast			= dfLast.Start;
							int				nPos			= lTd.Value.Start;

							if( nLast - nPos <= 25 )
							{
								dfLast			= lTd.Value;
							}
							else
							{
								dfFirst.Start	= dfLast.Start;

								lstChunk.AddLast( dfFirst );

								dfFirst			= new DataFeature( lTd.Value );
								dfLast			= dfFirst;
							}
						}
					}

					lTd				= lTd.Previous;

					if( lTd == null )
					{
						dfFirst.Start	= dfLast.Start;

						lstChunk.AddLast( dfFirst );

						dfFirst			= null;
						dfLast			= null;
					}
				}

				/*
				foreach( DataFeature df in lstChunk )
				{
					DataFeature		dfNew			= new DataFeature( df );
					dfNew.Strand					= "-";

					AddFeature( dtRbr.SequenceId, strType + "_temp", dfNew );
				}
				 */

				lRbr											= dtRbr.GetFeatureLinkLast();
				LinkedListNode< DataFeature >	lRbrNext		= null;
				LinkedListNode< DataFeature >	lChunk			= lstChunk.First;

				while( lRbr != null )
				{
					ListFeature		lst0			= new ListFeature();

					lRbrNext		= lRbr.Previous;

					if( lRbrNext == null )
					{
						while( lChunk != null )
						{
							lst0.Add( lChunk.Value );

							lChunk			= lChunk.Next;
						}
					}
					else
					{
						while( lChunk != null )
						{
							if( lChunk.Value.End > lRbrNext.Value.Start )
							{
								if( lChunk.Value.End <= lRbr.Value.Start )
								{
									lst0.Add( lChunk.Value );
								}
							}
							else
							{
								break;
							}

							lChunk			= lChunk.Next;
						}
					}

					if( lst0.Count > 0 )
					{
						DataFeature		df0				= new DataFeature( lst0[ 0 ] );

						for( int i = 1; i < lst0.Count; i++ )
						{
							if( df0.Start - lst0[ i ].End <= 500 )
							{
								df0.Start		= lst0[ i ].Start;
							}
							else
							{
								break;
							}
						}

						if( lRbr.Value.Start - df0.End < 400 )
						{
							AddFeature( dtRbr.SequenceId, strType, df0 );
						}
					}

					lRbr			= lRbr.Previous;
				}
			}
		}

		public void AddDataTypeByIntegrationTu( DataType dtTss, DataType dtRts, DataType dtPorf, DelegateDoProgressSet delProgress, object dlg /* TODO: DialogIntegrationOperation */ )
		{
			string			strTypeTu		= string.Format( "TU..{0}..w/tss..{1}", dtRts.Type, dtTss.Type );
			string			strTypeTss		= string.Format( "TSS..{0}..w/rts..{1}", dtTss.Type, dtRts.Type );
			string			strTypePorf		= string.Format( "PORF..{0}..w/rts..{1}", dtPorf.Type, dtRts.Type );

			LinkedListNode< DataFeature >	lTss			= dtTss.GetFeatureLinkFirst();
			ListFeature						lstTu			= new ListFeature();

			int				nCount			= 1;
			int				nTotal			= dtTss.GetCount();

			Stopwatch		sw				= new Stopwatch();
			sw.Start();

			while( lTss != null )
			{
				int				nTss			= lTss.Value.Start;

				LinkedListNode< DataFeature >	lRts			= dtRts.GetFeatureLinkFirst();
				LinkedListNode< DataFeature >	lRtsClosest		= null;
				int								nDistMin		= int.MaxValue;

				while( lRts != null )
				{
					if( lRts.Value.Strand == "-" )
					{
						// Reverse
						int				nEnd			= lRts.Value.End;
						int				nDist			= Math.Abs( nTss - nEnd );

						if( nDist < nDistMin )
						{
							nDistMin		= nDist;
							lRtsClosest		= lRts;
						}
					}
					else
					{
						// Forward or else
						int				nStart			= lRts.Value.Start;
						int				nDist			= Math.Abs( nTss - nStart );

						if( nDist < nDistMin )
						{
							nDistMin		= nDist;
							lRtsClosest		= lRts;
						}
					}

					lRts			= lRts.Next;
				}

				DataFeature		df				= new DataFeature( lRtsClosest.Value );
				df.DoAttributeSet( "ID", string.Format( "TU_{0}", nCount ) );
				df.DoAttributeSet( "TSS", nTss.ToString() );

				AddFeature( dtRts.SequenceId, strTypeTu, df );
				lstTu.Add( df );

				df								= new DataFeature( lTss.Value );
				df.DoAttributeSet( "ID", string.Format( "TSS_TU_{0}", nCount ) );
				df.DoAttributeSet( "Parent", string.Format( "TU_{0}", nCount ) );

				AddFeature( dtRts.SequenceId, strTypeTss, df );

				sw.Stop();

				if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
				{
					{ int c = nCount, t = nTotal; Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( c, t ) ); }
					sw.Restart();
				}
				else
				{
					sw.Start();
				}

				nCount++;
				lTss			= lTss.Next;
			}

			if( delProgress != null )
			{
				Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( 100, 100 ) );
			}

			sw.Stop();

			sw.Start();
			nCount			= 1;
			nTotal			= dtPorf.GetCount();

			LinkedListNode< DataFeature >	lPorf			= dtPorf.GetFeatureLinkFirst();

			while( lPorf != null )
			{
				string			strIdPorf		= string.Format( "PORF_{0}", nCount );
				DataFeature		dfPorf			= new DataFeature( lPorf.Value );
				dfPorf.DoAttributeSet( "ID", strIdPorf );

				AddFeature( dtRts.SequenceId, strTypePorf, dfPorf );

				foreach( DataFeature df in lstTu )
				{
					if( df.DoCheckOverlap( lPorf.Value ) == true )
					{
						int				nOverlapStart			= Math.Max( df.Start, lPorf.Value.Start );
						int				nOverlapEnd				= Math.Min( df.End, lPorf.Value.End );
						int				nOverlap				= nOverlapEnd - nOverlapStart + 1;
						int				nPorfWidth				= lPorf.Value.End - lPorf.Value.Start + 1;
						double			dOverlap				= ( ( double ) nOverlap ) / nPorfWidth;

						if( dOverlap > 0.5 )
						{
							df.DoAttributeSet( "pORF", strIdPorf );
							dfPorf.DoAttributeAdd( "Parent", df.DoAttributeGet( "ID" ) );
						}
					}
				}

				sw.Stop();

				if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
				{
					{ int c = nCount, t = nTotal; Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( c, t ) ); }
					sw.Restart();
				}
				else
				{
					sw.Start();
				}

				nCount++;
				lPorf			= lPorf.Next;
			}

			if( delProgress != null )
			{
				Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( 100, 100 ) );
			}

			sw.Stop();
		}

		public void AddDataTypeByDiff( List< PnlMapLane > lst, PnlMapLane pnlDiff, DelegateDoProgressSet delProgress, object dlg )
		{
				foreach( PnlMapLane pml in lst )
			{
				string			strSeqId		= pnlDiff.DataTypeSelected.SequenceId;
				string			strType			= string.Format( "{0}..-..{1}", pml.DataTypeSelected.Type, pnlDiff.DataTypeSelected.Type );

				int				nCount			= 1;
				int				nTotal			= pml.DataTypeSelected.GetCount();

				LinkedListNode< DataFeature >	lLane				= pml.DataTypeSelected.GetFeatureLinkFirst();
				LinkedListNode< DataFeature >	lDiff				= pnlDiff.DataTypeSelected.GetFeatureLinkFirst();

				Stopwatch		sw				= new Stopwatch();
				sw.Start();

				while( lLane != null && lDiff != null )
				{
					double			dScore			= lLane.Value.ScoreReal - lDiff.Value.ScoreReal;

					DataFeature		df				= new DataFeature(
														lLane.Value.Source, lLane.Value.Start, lLane.Value.End,
														dScore, lLane.Value.Strand, lLane.Value.Phase, lLane.Value.Attribute );

					AddFeature( pml.DataTypeSelected.SequenceId, strType, df );

					lLane							= lLane.Next;
					lDiff							= lDiff.Next;

					sw.Stop();

					if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
					{
						{ int c = nCount, t = nTotal; Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( c, t ) ); }
						sw.Restart();
					}
					else
					{
						sw.Start();
					}

					nCount++;
				}

				if( delProgress != null )
				{
					Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( 100, 100 ) );
				}

				sw.Stop();
			}
		}

		public void AddDataTypeByFilter( List< PnlMapLane > lst, PnlMapLane pnlFilter, EFilterMethod eMethod, DelegateDoProgressSet delProgress, object dlg )
		{
				StringBuilder	sbType			= new StringBuilder();
			int				nCount			= 1;
			int				nTotal			= 0;

			for( int i = 0; i < lst.Count; i++ )
			{
				if( i == 0 )
				{
					sbType.Append( lst[ i ].DataTypeSelected.Type );
				}
				else
				{
					sbType.Append( "..U.." );
					sbType.Append( lst[ i ].DataTypeSelected.Type );
				}

				nTotal			+= lst[ i ].DataTypeSelected.GetCount();
			}

			sbType.Append( string.Format( "..|..{0}", pnlFilter.DataTypeSelected.Type ) );

			string			strType			= sbType.ToString();
			string			strSeqId		= pnlFilter.DataTypeSelected.SequenceId;

			foreach( PnlMapLane pnl in lst )
			{
				ListFeature		lstInside		= new ListFeature();

				DataType		dt				= pnl.DataTypeSelected;

				Stopwatch		sw				= new Stopwatch();
				sw.Start();

				LinkedListNode< DataFeature >	lnk				= pnl.DataTypeSelected.GetFeatureLinkFirst();

				while( lnk != null )
				{
					DataFeature			df				= lnk.Value;
					int					nStart			= df.Start;

					LinkedListNode< DataFeature >
										lnkFilter		= pnlFilter.DataTypeSelected.GetFeatureLinkByEnd( nStart );

					while( lnkFilter != null && lnkFilter.Value.Start <= df.End )
					{
						if( lnkFilter.Value.DoCheckOverlap( df ) == true )
						{
							lstInside.Add( df );
						}

						lnkFilter			= lnkFilter.Next;
					}

					sw.Stop();

					if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
					{
						{ int c = nCount, t = nTotal; Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( c, t ) ); }
						sw.Restart();
					}
					else
					{
						sw.Start();
					}

					nCount++;

					lnk				= lnk.Next;
				}

				sw.Stop();

				if( eMethod == EFilterMethod.OUTSIDE )
				{
					foreach( DataFeature df in lstInside )
					{
						DataFeature		dfNew			= new DataFeature( df );

						AddFeature( strSeqId, strType, dfNew );
					}
				}
				else
				{
					LinkedListNode< DataFeature >
									lFeature			= pnl.DataTypeSelected.GetFeatureLinkFirst();

					while( lFeature != null )
					{
						if( lstInside.Contains( lFeature.Value ) == false )
						{
							DataFeature		dfNew			= new DataFeature( lFeature.Value );

							AddFeature( strSeqId, strType, dfNew );
						}

						lFeature		= lFeature.Next;
					}
				}
			}

			if( delProgress != null )
			{
				Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( 100, 100 ) );
			}
		}

		public List<DataFeature> AddDataTypeByCopyFeature( PnlMapLane pnlFrom, PnlMapLane pnlTo, DelegateDoProgressSet delProgress, object dlg )
		{
				int				nCount			= 1;
			int				nTotal			= pnlFrom.ListFeatureSelected.Count;
			string			strType			= pnlTo.DataTypeSelected.Type;

			ListFeature		lstTo			= new ListFeature();

			Stopwatch		sw				= new Stopwatch();
			sw.Start();

			foreach( DataFeature df in pnlFrom.ListFeatureSelected )
			{
				DataFeature			dfTo			= new DataFeature( df );
				lstTo.Add( dfTo );

				sw.Stop();

				if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
				{
					{ int c = nCount, t = nTotal; Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( c, t ) ); }
					sw.Restart();
				}
				else
				{
					sw.Start();
				}

				nCount++;
			}

			pnlTo.DataTypeSelected.DoFeatureAdd( lstTo );

			if( delProgress != null )
			{
				Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( 100, 100 ) );
			}

			sw.Stop();

			return lstTo;
		}

		public List<DataFeature> AddDataTypeByMoveFeature( PnlMapLane pnlFrom, PnlMapLane pnlTo, DelegateDoProgressSet delProgress, object dlg )
		{
				int				nCount			= 1;
			int				nTotal			= pnlFrom.ListFeatureSelected.Count;
			string			strType			= pnlTo.DataTypeSelected.Type;

			ListFeature		lstTo			= new ListFeature();

			Stopwatch		sw				= new Stopwatch();
			sw.Start();

			foreach( DataFeature df in pnlFrom.ListFeatureSelected )
			{
				DataFeature			dfTo			= new DataFeature( df );
				lstTo.Add( dfTo );

				sw.Stop();

				if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
				{
					{ int c = nCount, t = nTotal; Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( c, t ) ); }
					sw.Restart();
				}
				else
				{
					sw.Start();
				}

				nCount++;
			}

			pnlFrom.DataTypeSelected.DoFeatureRemove( pnlFrom.ListFeatureSelected );
			pnlFrom.ListFeatureSelected.Clear();
			pnlTo.DataTypeSelected.DoFeatureAdd( lstTo );

			if( delProgress != null )
			{
				Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( 100, 100 ) );
			}

			sw.Stop();

			return lstTo;
		}

		public void AddDataTypeByMergeFeature( List< PnlMapLane > lst, EMergeMethod eMethod, DelegateDoProgressSet delProgress, object dlg )
		{
				StringBuilder		sbType			= new StringBuilder();
			int					nTotal			= 0;

			for( int i = 0; i < lst.Count; i++ )
			{
				if( i == 0 )
				{
					sbType.Append( lst[ i ].DataTypeSelected.Type );
				}
				else
				{
					sbType.Append( "..#`.." );
					sbType.Append( lst[ i ].DataTypeSelected.Type );
				}

				nTotal			+= lst[ i ].DataTypeSelected.GetCount();
			}

			string			strType			= sbType.ToString();
			DicDataFeature	dicFeature		= new DicDataFeature();

			int				nOffset			= 0;
			int				nCount			= 1;

			Stopwatch		sw				= new Stopwatch();
			sw.Start();

			foreach( PnlMapLane pnl in lst )
			{
				foreach( DataFeature df in pnl.ListFeatureSelected )
				{
					BigInteger		iKey			= ( ( ( BigInteger ) df.Start ) * ( DataType.N_KEYRANGE ) + df.End ) * 10 + nOffset;

					dicFeature.Add( iKey, df );

					sw.Stop();

					if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
					{
						{ int c = nCount, t = nTotal; Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( c, t ) ); }
						sw.Restart();
					}
					else
					{
						sw.Start();
					}

					nCount++;
				}

				nOffset++;
			}

			if( delProgress != null )
			{
				Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( 100, 100 ) );
			}
			sw.Stop();

			nCount							= 0;
			DataFeature		dfCurrent		= new DataFeature( null, 0, 0, 0, null, null, null );
			ListFeature		lstFeature		= new ListFeature();

			sw.Start();

			foreach( KeyValuePair< BigInteger, DataFeature > kv in dicFeature )
			{
				if( lstFeature.Count == 0 )
				{
					dfCurrent.Start					= kv.Value.Start;
					dfCurrent.End					= kv.Value.End;

					lstFeature.Add( kv.Value );
				}
				else if( dfCurrent.DoCheckOverlap( kv.Value ) == true )
				{
					dfCurrent.Start					= Math.Min( dfCurrent.Start, kv.Value.Start );
					dfCurrent.End					= Math.Max( dfCurrent.End, kv.Value.End );

					lstFeature.Add( kv.Value );
				}
				else
				{
					double[]		dStartA			= new double[ lstFeature.Count ];
					double[]		dEndA			= new double[ lstFeature.Count ];
					double[]		dScoreA			= new double[ lstFeature.Count ];

					for( int i = 0; i < lstFeature.Count; i++ )
					{
						dStartA[ i ]	= lstFeature[ i ].Start;
						dEndA[ i ]		= lstFeature[ i ].End;
						dScoreA[ i ]	= lstFeature[ i ].Score;
					}

					double			dStart			= 0;
					double			dEnd			= 0;

					if( eMethod == EMergeMethod.MEDIAN )
					{
						dStart			= UtilityMath.GetMedian( dStartA );
						dEnd			= UtilityMath.GetMedian( dEndA );
					}
					else if( eMethod == EMergeMethod.AVERAGE )
					{
						dStart			= UtilityMath.GetAverage( dStartA );
						dEnd			= UtilityMath.GetAverage( dEndA );
					}

					double			dScore			= UtilityMath.GetAverage( dScoreA );

					int				nStart			= UtilityMath.DoRound( dStart );
					int				nEnd			= UtilityMath.DoRound( dEnd );

					string			strAttribute	= string.Format( "Count={0}", lstFeature.Count );

					DataFeature		dfNew			= new DataFeature(
														lstFeature[ 0 ].Source, nStart, nEnd, dScore,
														lstFeature[ 0 ].Strand, lstFeature[ 0 ].Phase, strAttribute );

					AddFeature( lst[ 0 ].DataTypeSelected.SequenceId, strType, dfNew );

					lstFeature.Clear();

					// Add a new one
					dfCurrent.Start					= kv.Value.Start;
					dfCurrent.End					= kv.Value.End;

					lstFeature.Add( kv.Value );
				}

				sw.Stop();

				if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
				{
					{ int c = nCount, t = nTotal; Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( c, t ) ); }
					sw.Restart();
				}
				else
				{
					sw.Start();
				}

				nCount++;
			}

			if( lstFeature.Count != 0 )
			{
				double[]		dStartA			= new double[ lstFeature.Count ];
				double[]		dEndA			= new double[ lstFeature.Count ];
				double[]		dScoreA			= new double[ lstFeature.Count ];

				for( int i = 0; i < lstFeature.Count; i++ )
				{
					dStartA[ i ]	= lstFeature[ i ].Start;
					dEndA[ i ]		= lstFeature[ i ].End;
					dScoreA[ i ]	= lstFeature[ i ].Score;
				}

				double			dStart			= UtilityMath.GetMedian( dStartA );
				double			dEnd			= UtilityMath.GetMedian( dEndA );
				double			dScore			= UtilityMath.GetAverage( dScoreA );

				int				nStart			= UtilityMath.DoRound( dStart );
				int				nEnd			= UtilityMath.DoRound( dEnd );

				string			strAttribute	= string.Format( "Count={0}", lstFeature.Count );

				DataFeature		dfNew			= new DataFeature(
													lstFeature[ 0 ].Source, nStart, nEnd, dScore,
													lstFeature[ 0 ].Strand, lstFeature[ 0 ].Phase, strAttribute );

				AddFeature( lst[ 0 ].DataTypeSelected.SequenceId, strType, dfNew );

				lstFeature.Clear();
			}

			if( delProgress != null )
			{
				Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( 100, 100 ) );
				sw.Stop();
			}
		}

		public void AddDataTypeByMerge( List< PnlMapLane > lst, EMergeMethod eMethod, DelegateDoProgressSet delProgress, object dlg )
		{
				StringBuilder		sbType			= new StringBuilder();
			int					nTotal			= 0;

			for( int i = 0; i < lst.Count; i++ )
			{
				if( i == 0 )
				{
					sbType.Append( lst[ i ].DataTypeSelected.Type );
				}
				else
				{
					sbType.Append( "..#.." );
					sbType.Append( lst[ i ].DataTypeSelected.Type );
				}

				nTotal			+= lst[ i ].DataTypeSelected.GetCount();
			}

			string			strType			= sbType.ToString();
			DicDataFeature	dicFeature		= new DicDataFeature();

			int				nOffset			= 0;
			int				nCount			= 1;

			Stopwatch		sw				= new Stopwatch();
			sw.Start();

			foreach( PnlMapLane pnl in lst )
			{
				LinkedListNode< DataFeature >	lnk				= pnl.DataTypeSelected.GetFeatureLinkFirst();

				while( lnk != null )
				{
					DataFeature		df				= lnk.Value;
					BigInteger		iKey			= ( ( ( BigInteger ) df.Start ) * ( DataType.N_KEYRANGE ) + df.End ) * 10 + nOffset;

					dicFeature.Add( iKey, df );

					sw.Stop();

					if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
					{
						{ int c = nCount, t = nTotal; Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( c, t ) ); }
						sw.Restart();
					}
					else
					{
						sw.Start();
					}

					nCount++;
					lnk				= lnk.Next;
				}

				nOffset++;
			}

			if( delProgress != null )
			{
				Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( 100, 100 ) );
			}
			sw.Stop();

			nCount							= 0;
			DataFeature		dfCurrent		= new DataFeature( null, 0, 0, 0, null, null, null );
			ListFeature		lstFeature		= new ListFeature();

			sw.Start();

			foreach( KeyValuePair< BigInteger, DataFeature > kv in dicFeature )
			{
				if( lstFeature.Count == 0 )
				{
					dfCurrent.Start					= kv.Value.Start;
					dfCurrent.End					= kv.Value.End;

					lstFeature.Add( kv.Value );
				}
				else if( dfCurrent.DoCheckOverlap( kv.Value ) == true )
				{
					dfCurrent.Start					= Math.Min( dfCurrent.Start, kv.Value.Start );
					dfCurrent.End					= Math.Max( dfCurrent.End, kv.Value.End );

					lstFeature.Add( kv.Value );
				}
				else
				{
					double[]		dStartA			= new double[ lstFeature.Count ];
					double[]		dEndA			= new double[ lstFeature.Count ];
					double[]		dScoreA			= new double[ lstFeature.Count ];

					StringBuilder	sbAttr			= new StringBuilder();

					for( int i = 0; i < lstFeature.Count; i++ )
					{
						dStartA[ i ]	= lstFeature[ i ].Start;
						dEndA[ i ]		= lstFeature[ i ].End;
						dScoreA[ i ]	= lstFeature[ i ].Score;

						foreach( PnlMapLane pml in lst )
						{
							if( pml.DataTypeSelected.DoFeatureContaining( lstFeature[ i ] ) == true )
							{
								sbAttr.Append( string.Format( "MergedType.{0}={1};", i, pml.DataTypeSelected.Type ) );
							}
						}
					}

					double			dStart			= 0;
					double			dEnd			= 0;
					string			strAttrType		= sbAttr.ToString();

					if( eMethod == EMergeMethod.MEDIAN )
					{
						dStart			= UtilityMath.GetMedian( dStartA );
						dEnd			= UtilityMath.GetMedian( dEndA );
					}
					else if( eMethod == EMergeMethod.AVERAGE )
					{
						dStart			= UtilityMath.GetAverage( dStartA );
						dEnd			= UtilityMath.GetAverage( dEndA );
					}

					double			dScore			= UtilityMath.GetAverage( dScoreA );

					int				nStart			= UtilityMath.DoRound( dStart );
					int				nEnd			= UtilityMath.DoRound( dEnd );

					string			strAttribute	= string.Format( "Count={0};{1}", lstFeature.Count, strAttrType );

					DataFeature		dfNew			= new DataFeature(
														lstFeature[ 0 ].Source, nStart, nEnd, dScore,
														lstFeature[ 0 ].Strand, lstFeature[ 0 ].Phase, strAttribute );

					AddFeature( lst[ 0 ].DataTypeSelected.SequenceId, strType, dfNew );

					lstFeature.Clear();

					// Add a new one
					dfCurrent.Start					= kv.Value.Start;
					dfCurrent.End					= kv.Value.End;

					lstFeature.Add( kv.Value );
				}

				sw.Stop();

				if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
				{
					{ int c = nCount, t = nTotal; Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( c, t ) ); }
					sw.Restart();
				}
				else
				{
					sw.Start();
				}

				nCount++;
			}

			if( lstFeature.Count != 0 )
			{
				double[]		dStartA			= new double[ lstFeature.Count ];
				double[]		dEndA			= new double[ lstFeature.Count ];
				double[]		dScoreA			= new double[ lstFeature.Count ];

				for( int i = 0; i < lstFeature.Count; i++ )
				{
					dStartA[ i ]	= lstFeature[ i ].Start;
					dEndA[ i ]		= lstFeature[ i ].End;
					dScoreA[ i ]	= lstFeature[ i ].Score;
				}

				double			dStart			= UtilityMath.GetMedian( dStartA );
				double			dEnd			= UtilityMath.GetMedian( dEndA );
				double			dScore			= UtilityMath.GetAverage( dScoreA );

				int				nStart			= UtilityMath.DoRound( dStart );
				int				nEnd			= UtilityMath.DoRound( dEnd );

				string			strAttribute	= string.Format( "Count={0}", lstFeature.Count );

				DataFeature		dfNew			= new DataFeature(
													lstFeature[ 0 ].Source, nStart, nEnd, dScore,
													lstFeature[ 0 ].Strand, lstFeature[ 0 ].Phase, strAttribute );

				AddFeature( lst[ 0 ].DataTypeSelected.SequenceId, strType, dfNew );

				lstFeature.Clear();
			}

			if( delProgress != null )
			{
				Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( 100, 100 ) );
				sw.Stop();
			}
		}

		public void AddDataTypeByAverage( List< PnlMapLane > lst, DelegateDoProgressSet delProgress, object dlg, bool bByMedian )
		{
				LinkedListNode< DataFeature >[]	lA				= new LinkedListNode< DataFeature >[ lst.Count ];
			StringBuilder					sbType			= new StringBuilder();

			for( int i = 0; i < lst.Count; i++ )
			{
				lA[ i ]			= lst[ i ].DataTypeSelected.GetFeatureLinkFirst();

				if( i == 0 )
				{
					sbType.Append( lst[ i ].DataTypeSelected.Type );
				}
				else
				{
					sbType.Append( "..!.." );
					sbType.Append( lst[ i ].DataTypeSelected.Type );
				}
			}

			string			strType			= sbType.ToString();

			int				nCount			= 1;
			int				nTotal			= lst[ 0 ].DataTypeSelected.GetCount();

			Stopwatch		sw				= new Stopwatch();
			sw.Start();

			double			dScore			= 0.0f;
			double[]		dScoreA			= new double[ lst.Count ];

			while( true )
			{
				bool			bRun			= true;

				for( int i = 0; i < lst.Count; i++ )
				{
					if( lA[ i ] == null )
					{
						bRun			= false;
						break;
					}
				}

				if( bRun == false )
					break;

				bool			bMatch			= true;

				for( int i = 0; i < lst.Count - 1; i++ )
				{
					for( int j = i + 1; j < lst.Count; j++ )
					{
						int				nCompare		= lA[ i ].Value.DoComparePosition( lA[ j ].Value );

						if( nCompare == 0 )
						{
						}
						else if( nCompare < 0 )
						{
							bMatch			= false;
							lA[ i ]			= lA[ i ].Next;
							break;
						}
						else
						{
							bMatch			= false;
							lA[ j ]			= lA[ j ].Next;
							break;
						}
					}

					if( bMatch == false )
					{
						break;;
					}
				}

				if( bMatch == false )
					continue;

				for( int i = 0; i < lst.Count; i++ )
				{
					dScoreA[ i ]	= lA[ i ].Value.ScoreReal;
				}

				if( bByMedian == true )
				{
					dScore			= UtilityMath.GetMedian( dScoreA );
				}
				else
				{
					dScore			= UtilityMath.GetAverage( dScoreA );
				}

				DataFeature		df				= new DataFeature(
													lA[ 0 ].Value.Source, lA[ 0 ].Value.Start, lA[ 0 ].Value.End,
													dScore, lA[ 0 ].Value.Strand, lA[ 0 ].Value.Phase, lA[ 0 ].Value.Attribute );

				AddFeature( lst[ 0 ].DataTypeSelected.SequenceId, strType, df );

				for( int i = 0; i < lst.Count; i++ )
				{
					lA[ i ]			= lA[ i ].Next;
				}

				sw.Stop();

				if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
				{
					{ int c = nCount, t = nTotal; Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( c, t ) ); }
					sw.Restart();
				}
				else
				{
					sw.Start();
				}

				nCount++;
			}

			if( delProgress != null )
			{
				Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( 100, 100 ) );
				sw.Stop();
			}
		}

		public void AddDataTypeBySum( List< PnlMapLane > lst, DelegateDoProgressSet delProgress, object dlg )
		{
				LinkedListNode< DataFeature >[]	lA				= new LinkedListNode< DataFeature >[ lst.Count ];
			StringBuilder					sbType			= new StringBuilder();

			for( int i = 0; i < lst.Count; i++ )
			{
				lA[ i ]			= lst[ i ].DataTypeSelected.GetFeatureLinkFirst();

				if( i == 0 )
				{
					sbType.Append( lst[ i ].DataTypeSelected.Type );
				}
				else
				{
					sbType.Append( "..+.." );
					sbType.Append( lst[ i ].DataTypeSelected.Type );
				}
			}

			string			strType			= sbType.ToString();

			int				nCount			= 1;
			int				nTotal			= lst[ 0 ].DataTypeSelected.GetCount();

			Stopwatch		sw				= new Stopwatch();
			sw.Start();

			double			dScore			= 0.0f;

			while( true )
			{
				bool			bRun			= true;

				for( int i = 0; i < lst.Count; i++ )
				{
					if( lA[ i ] == null )
					{
						bRun			= false;
						break;
					}
				}

				if( bRun == false )
					break;

				bool			bMatch			= true;

				for( int i = 0; i < lst.Count - 1; i++ )
				{
					for( int j = i + 1; j < lst.Count; j++ )
					{
						int				nCompare		= lA[ i ].Value.DoComparePosition( lA[ j ].Value );

						if( nCompare == 0 )
						{
						}
						else if( nCompare < 0 )
						{
							bMatch			= false;
							lA[ i ]			= lA[ i ].Next;
							break;
						}
						else
						{
							bMatch			= false;
							lA[ j ]			= lA[ j ].Next;
							break;
						}
					}

					if( bMatch == false )
					{
						break;;
					}
				}

				if( bMatch == false )
					continue;

				dScore			= 0.0f;

				for( int i = 0; i < lst.Count; i++ )
				{
					dScore			+= lA[ i ].Value.ScoreReal;
				}

				DataFeature		df				= new DataFeature(
													lA[ 0 ].Value.Source, lA[ 0 ].Value.Start, lA[ 0 ].Value.End,
													dScore, lA[ 0 ].Value.Strand, lA[ 0 ].Value.Phase, lA[ 0 ].Value.Attribute );

				AddFeature( lst[ 0 ].DataTypeSelected.SequenceId, strType, df );

				for( int i = 0; i < lst.Count; i++ )
				{
					lA[ i ]			= lA[ i ].Next;
				}

				sw.Stop();

				if( delProgress != null && sw.ElapsedMilliseconds >= 100 )
				{
					{ int c = nCount, t = nTotal; Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( c, t ) ); }
					sw.Restart();
				}
				else
				{
					sw.Start();
				}

				nCount++;
			}

			if( delProgress != null )
			{
				Avalonia.Threading.Dispatcher.UIThread.Post( () => delProgress( 100, 100 ) );
				sw.Stop();
			}
		}

		public void DoDispose()
		{
			foreach( DataType dt in m_dicDataType.Values )
			{
				dt.DoDispose();
			}
		}

		public void DoClose()
		{
			m_mgrData.DoFileClose( m_strFile );
		}

		public void DoSave()
		{
			DoSave( m_strFile );

			SetEdited( false );
		}

		public void DoSave( string strFile )
		{
			StreamWriter	sw				= null;

			try
			{
				sw				= new StreamWriter( strFile );

				if( m_strHeader != null )
				{
					sw.Write( m_strHeader );
				}

				foreach( KeyValuePair< string, DataType > kv in m_dicDataType )
				{
					DataType		dt				= kv.Value;

					dt.DoSave( sw );
				}

				sw.Flush();
			}
			catch( Exception e )
			{
				Logger.PrintLine( "# ERROR, DataFile:DoSave - {0}", e.ToString() );
				ErrorMessage.ShowError( string.Format( "Failed to save file \"{0}\".\r\n\r\n{1}", strFile, e.Message ) );
			}
			finally
			{
				if( sw != null )		sw.Close();
			}
		}

		public int GetCountSequenceId()
		{
			int				nCount			= m_lstSequenceId.Count;

			return nCount;
		}

		public string GetSequenceId( int nIndex )
		{
			string			strSequenceId	= m_lstSequenceId[ nIndex ];

			return strSequenceId;
		}

		public int GetCountType()
		{
			int				nCount			= m_lstType.Count;

			return nCount;
		}

		public string GetType( int nIndex )
		{
			string			strType			= m_lstType[ nIndex ];

			return strType;
		}

		public string Source
		{
			get {	return m_strSource; }
			set {	m_strSource		= value; }
		}

		public ManagerData ManagerData
		{
			get
			{
				return m_mgrData;
			}

			set
			{
				m_mgrData		= value;
			}
		}

		public int GetPositionMin( string strSequenceId )
		{
			int				nPositionMin	= int.MaxValue;

			foreach( KeyValuePair< string, DataType > kv in m_dicDataType )
			{
				if( kv.Key.StartsWith( strSequenceId ) == true )
				{
					nPositionMin	= Math.Min( nPositionMin, kv.Value.PositionMin );
				}
			}

			return nPositionMin;
		}

		public int GetPositionMax( string strSequenceId )
		{
			int				nPositionMax	= int.MinValue;

			foreach( KeyValuePair< string, DataType > kv in m_dicDataType )
			{
				if( kv.Key.StartsWith( strSequenceId ) == true )
				{
					nPositionMax	= Math.Max( nPositionMax, kv.Value.PositionMax );
				}
			}

			return nPositionMax;
		}

		public void BuildIndex()
		{
			foreach( KeyValuePair< string, DataType > kv in m_dicDataType )
			{
				kv.Value.BuildIndex();
			}
		}

		public string FileName
		{
			get
			{
				FileInfo		fi				= new FileInfo( m_strFile );
				string			strName			= string.Format( "{0}", fi.Name );

				return strName;
			}
		}

		public string File
		{
			get {	return m_strFile; }
			set {	m_strFile		= value; }
		}

		public int GetCountDataType()
		{
			int				nCount			= m_dicDataType.Count;

			return nCount;
		}

		public int GetCountDataType( string strSequenceId )
		{
			int				nCount			= 0;

			foreach( KeyValuePair< string, DataType > kv in m_dicDataType )
			{
				if( kv.Key.StartsWith( strSequenceId ) == true )
				{
					nCount++;
				}
			}

			return nCount;
		}

		public DataType GetDataType( string strSequenceId, int nIndex )
		{
			int				nIndex0			= 0;

			foreach( KeyValuePair< string, DataType > kv in m_dicDataType )
			{
				if( kv.Key.StartsWith( strSequenceId ) == true )
				{
					if( nIndex == nIndex0 )
					{
						return kv.Value;
					}
					else
					{
						nIndex0++;
					}
				}
			}

			return null;
		}

		public DataType GetDataType( int nIndex )
		{
			int				nIndex0			= 0;

			foreach( KeyValuePair< string, DataType > kv in m_dicDataType )
			{
				if( nIndex == nIndex0 )
				{
					return kv.Value;
				}
				else
				{
					nIndex0++;
				}
			}

			return null;
		}

		public int GetCountFeature()
		{
			int				nCount			= 0;

			foreach( string strKey in m_dicDataType.Keys )
			{
				DataType		dt				= m_dicDataType[ strKey ];

				nCount			+= dt.GetCount();
			}

			return nCount;
		}

		public string Header
		{
			get {	return m_strHeader; }
			set {	m_strHeader = value; }
		}

		public void SetEdited( bool bEdited )
		{
			m_bEdited			= bEdited;

			if( m_bEdited == true )
			{
				m_mgrData.SetEdited( m_bEdited );
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

		public DataType GetDataType( string strSequenceId, string strType )
		{
			DataType		dt				= null;

			string			strKey			= string.Format( "{0}:{1}", strSequenceId, strType );

			if( m_dicDataType.Keys.Contains( strKey ) == false )
			{
				dt				= new DataType( this, strType, strSequenceId, m_bReadOnly );

				if( m_dicDataType.Count > 0 )
				{
					IBrush	bsh				= m_dicDataType.Values.First().DoBrushGet();
					dt.DoBrushSet( bsh );
				}

				m_dicDataType.Add( strKey, dt );
			}
			else
			{
				dt				= m_dicDataType[ strKey ];
			}

			return dt;
		}

		public void AddFeature( string strSequenceId, string strType, DataFeature dfAdd )
		{
			if( m_lstSequenceId.Contains( strSequenceId ) == false )
				m_lstSequenceId.Add( strSequenceId );

			if( m_lstType.Contains( strType ) == false )
				m_lstType.Add( strType );

			DataType		dt				= GetDataType( strSequenceId, strType );

			dt.DoFeatureAddBeforeIndex( dfAdd );
		}
	}
}
