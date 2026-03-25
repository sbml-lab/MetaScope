using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using VugMap.Utility;
using VugMap.Utility.Command;
using VugMap.Utility.Data;
using VugMap.Utility.Logger;
using VugMap.Window.ColorPicker;

namespace VugMap.Window
{
	using			ListRect						= List< Rect >;
	using			ListFeature						= List< DataFeature >;
	using			ListListFeature					= List< List< DataFeature > >;
	using			DicRectFeature					= Dictionary< Rect, DataFeature >;
	using			ListDataType					= List< DataType >;
	using			ListLabel						= List< Label >;
	using			ListDataFeatureNode				= LinkedListNode< DataFeature >;	

	public class PnlMapLane : Panel
	{
		//			.								.								.
		public		static double					N_SCOREVERTICAL_GAP				= 10.0f;
		public		static double					N_LANE_MARGIN					= 10.0f;
		public		static int						N_FEATURE_COUNT					= 1024 * 2;
		public		static double					N_FEATURE_MINIMALGAP			= 1.0f;
		public		static double					N_FEATURE_MINIMALWIDTH			= 0.5f;
		public		static double					N_FEATURE_MINIMALHEIGHT			= 0.5f;
		public		static double					N_LANE_WIDTHMINIMUM				= 200.0f;
		public		static double					N_LANE_HEIGHTMINIMUM			= 50.0f;

		public		static Color					CLR_BACK						= Color.FromRgb( 255, 255, 255 );
		public		static Color					CLR_BACKSELECTED				= Color.FromRgb( 64, 64, 64 );
		public		static Color					CLR_SELECTED					= Color.FromRgb( 0, 10, 23 );		
		public		static Color					CLR_HEADSELECTED				= Color.FromRgb( 139, 178, 255 );
		public		static Color					CLR_TYPEBACK					= Color.FromArgb( 255, 237, 247, 255 );
		public		static Color					CLR_TYPEBACKSELECTED			= Color.FromArgb( 225, 209, 235, 255 );

		private		int								m_nPositionMax					= 0;
		private		int								m_nPositionMin					= 0;
		private		int								m_nPosDispMax					= 0;
		private		int								m_nPosDispMin					= 0;		
		
		private		double							m_dLaneWidth					= 0.0f;
		private		double							m_dLaneHeight					= 0.0f;
		private		double							m_dLaneHeightActual				= 0.0f;
		private		ListDataType					m_lstDataType					= null;
		private		DataType						m_dtSelected					= null;
		private		bool							m_bEditable						= false;
		private		bool							m_bSelected						= false;		

		private		RenderTargetBitmap				m_bmpLane						= null;
		private		RectangleGeometry				m_gmClip						= null;
		private		Brush							m_bshBack						= null;
		private		Pen								m_penBack						= null;		
		private		Brush							m_bshSelect						= null;
		private		Brush							m_bshHeadSelected				= null;
		private		ContextMenu						m_cmHead						= null;
		private		ContextMenu						m_cmBack						= null;
		private		ContextMenu						m_cmFeature						= null;
		private		Pen								m_penLine						= null;
		private		Pen								m_penScoreSub					= null;
		private		Typeface						m_tfScore						= null;
		
		private		StackPanel						m_splInfo						= null;
		private		ListLabel						m_lstType						= null;
		private		Brush							m_brsTypeBack					= null;
		private		Brush							m_brsTypeBackSelected			= null;
		private		Label							m_lblMouse						= null;
		private		Point							m_ptRrightClick;

		private		ListFeature						m_lstFeatSelected				= null;
		private		DicRectFeature					m_dicRectFeature				= null;
		private		DataFeature						m_dfToolTip						= null;

		public PnlMapLane()
		{
			m_lstDataType					= new ListDataType();
			
			BuildElementMenu();
			BuildElementBack();
			BuildElementLine();
			BuildElementType();
			BuildElementSelect();
			BuildElementFeature( N_FEATURE_COUNT );

			MouseMove		+= new MouseEventHandler( OnMouseMove );
			MouseDown		+= new MouseButtonEventHandler( OnMouseDown );
			MouseUp			+= new MouseButtonEventHandler( OnMouseUp );
		}

		public PnlMapLane( DataType dtLane )
			: this()
		{	
			m_lstDataType.Add( dtLane );
			
			m_dtSelected	= dtLane;
		}

		public ListFeature ListFeatureSelected
		{
			get {	return m_lstFeatSelected; }
		}

		public bool IsContainingDataType( DataType dt )
		{
			bool			b				= m_lstDataType.Contains( dt );

			return b;
		}

		public DataType DoDataTypeGet( int nIndex )
		{
			DataType		dt				= m_lstDataType[ nIndex ];

			return dt;
		}

		public int GetCountDataType()
		{
			int				nCount			= m_lstDataType.Count;

			return nCount;
		}

		public void DoDataTypeRemove( DataType dt )
		{
			m_lstDataType.Remove( dt );

			if( dt == m_dtSelected )
			{
				if( m_lstDataType.Count == 0 )
					m_dtSelected	= null;
				else
					m_dtSelected	= m_lstDataType.Last();
			}
		}

		public void DoDataTypeAdd( DataType dt )
		{
			m_lstDataType.Add( dt );
			
			m_dtSelected	= dt;
		}

		public void DoDataTypeSelect( string strType )
		{
			foreach( DataType dt in m_lstDataType )
			{
				if( dt.Type == strType )
				{
					DoDataTypeSelect( dt );
				}
			}
		}

		public void DoDataTypeSelect( DataType dt )
		{
			m_dtSelected	= dt;
		}

		public void SetLaneHeightActual( double dHeight )
		{
			dHeight			= Math.Max( dHeight, N_LANE_HEIGHTMINIMUM );
			
			LaneHeightActual				= dHeight;
		}

		public bool IsSelected
		{
			get {	return m_bSelected; }
			set {	m_bSelected = value; }
		}
		
		public bool GetIsLayoutUpdated()
		{
			if( m_bmpLane == null )
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		public void DoClose()
		{
			m_lstDataType.Clear();
			m_lstFeatSelected.Clear();
			m_dicRectFeature.Clear();
		}
				
		public void DoFeatureSelect( DataFeature df )
		{
			if( df != null && m_dtSelected != null && m_dtSelected.IsReadOnly )
				return;

			m_lstFeatSelected.Clear();

			if( df == null )
			{
			}
			else
			{
				m_lstFeatSelected.Add( df );
			}

			MainWindow		mw				= MainWindow.GetMainWindow();
			mw.DoFeatureSelectedSet( df );

			DoLayoutUpdate();
		}

		public int GetCountFeatureSelected()
		{
			int				nCount			= m_lstFeatSelected.Count;

			return nCount;
		}

		public void DoFeatureSelect( int nPosStart, int nPosEnd )
		{
			DoFeatureSelect( nPosStart, nPosEnd, double.MaxValue, double.MinValue );
		}

		public void DoFeatureSelect( int nPosStart, int nPosEnd, double dScoreMax, double dScoreMin )
		{
			if( m_dtSelected != null && m_dtSelected.IsReadOnly )
				return;

			nPosStart		= Math.Max( nPosStart, m_nPosDispMin );
			nPosEnd			= Math.Min( nPosEnd, m_nPosDispMax );

			m_lstFeatSelected.Clear();

			if( m_dtSelected.IsReadOnly )
			{
				DataFeature[]	arr				= m_dtSelected.GetFeatureArray();
				if( arr != null )
				{
					int				iStart			= m_dtSelected.GetFeatureIndexByEnd( nPosStart );
					if( iStart < 0 )		iStart	= 0;

					for( int i = iStart; i < arr.Length; i++ )
					{
						DataFeature		df			= arr[ i ];
						if( df.Start > nPosEnd )	break;
						if( df.Score >= dScoreMin && df.Score <= dScoreMax )
						{
							m_lstFeatSelected.Add( df );
						}
					}
				}
			}
			else
			{
				LinkedListNode< DataFeature >
								node			= m_dtSelected.GetFeatureLinkByEnd( nPosStart );

				while( node != null && node.Value.Start <= nPosEnd )
				{
					if( node.Value.Score >= dScoreMin && node.Value.Score <= dScoreMax )
					{
						m_lstFeatSelected.Add( node.Value );
					}

					node				= node.Next;
				}
			}
		
			MainWindow		mw				= MainWindow.GetMainWindow();
			mw.DoFeatureSelectedSet( m_lstFeatSelected );

			DoLayoutUpdate();
		}
		
		private void BuildElementSelect()
		{
			m_bshSelect		= new SolidColorBrush( CLR_SELECTED );
			m_bshSelect.Freeze();
		}

		public void DoScale( double dScaleMax, double dScaleMin )
		{
			DataTypeSelected.DoScale( dScaleMax, dScaleMin );

			InvalidateVisual();
		}

		public void DoScaleAuto()
		{
			DataTypeSelected.DoScaleAuto();

			DoLayoutUpdate();
		}
				
		private void OnMouseDown( object obj, MouseButtonEventArgs ea )
		{
			if( ea.LeftButton == MouseButtonState.Pressed )
			{
				OnMouseDownLeft( obj, ea );
			}
			else if( ea.RightButton == MouseButtonState.Pressed )
			{				
			}			
		}	

		private void OnMouseDownLeft( object obj, MouseButtonEventArgs ea )
		{
			Rect			rtHead			= new Rect( 0.0f, 0.0f, PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, ActualHeight );					
			Point			pt				= ea.GetPosition( this );

			if( ea.ClickCount == 1 )
			{
				if( Keyboard.IsKeyDown( Key.LeftShift ) == true || Keyboard.IsKeyDown( Key.RightShift ) == true )
				{
					if( rtHead.Contains( pt ) == true )
					{						
					}
					else
					{
						// 선택 표시
						MainWindow		mw				= MainWindow.GetMainWindow();						

						PnlMap			pnlMap			= Parent as PnlMap;
						int				nPosition		= UtilityMath.DoRound( pnlMap.GetPositionFromPixel( pt.X + PnlMap.N_LANE_VERTICALGAP ) );
						double			dScore			= GetScoreFromYOffset( pt.Y );
			
						Nullable<Rect>	rt				= GetRectangleHit( pt );
						if( rt == null )
						{						
							pnlMap.DoSelection();
						}
						else
						{			
							pnlMap.DoSelection( rt.Value.X + PnlMap.N_LANE_VERTICALGAP, rt.Value.Width );
						}
					}
				}
				else
				{
					if( rtHead.Contains( pt ) == true )
					{						
					}
					else
					{
						// 그냥 선택
						MainWindow		mw				= MainWindow.GetMainWindow();						

						PnlMap			pnlMap			= Parent as PnlMap;

						int				nPosition		= UtilityMath.DoRound( pnlMap.GetPositionFromPixel( pt.X + PnlMap.N_LANE_VERTICALGAP ) );
						double			dScore			= GetScoreFromYOffset( pt.Y );
			
						Nullable<Rect>	rt				= GetRectangleHit( pt );
						if( rt == null )
						{
							DoFeatureSelect( null );
							mw.DoFeatureSelectedSet( null as DataFeature );
							pnlMap.DoFeatureHighlightClear();
						}
						else
						{							
							DataFeature		df				= m_dtSelected.GetFeatureContaining( nPosition, dScore );
				
							DoFeatureSelect( df );
							if( df != null )
								pnlMap.DoFeatureHighlightSet( df.Start, df.End );
							else
								pnlMap.DoFeatureHighlightClear();
						}
					}
				}
			}
			else if( ea.ClickCount == 2 )
			{
				if( m_dtSelected != null && m_dtSelected.IsReadOnly )
					return;

				if( rtHead.Contains( pt ) == true )
				{
					PnlMap			pnlMap				= Parent as PnlMap;

					if( m_bEditable == true )
					{
						pnlMap.DoLaneSetEditable( this );
					}
					else
					{
						pnlMap.DoLaneSetEditable( this );
					}
				}
				else
				{
					PnlMap			pnlMap				= Parent as PnlMap;
										
					pnlMap.DoLaneSetEditable( this );					
				}
			}
		}

		private void OnMouseUpRight( object obj, MouseButtonEventArgs ea )
		{	
			Rect			rtHead			= new Rect( 0.0f, 0.0f, PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, ActualHeight );
			Rect			rtBody			= new Rect( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, 0.0f, 
												ActualWidth - (PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP ), ActualHeight );

			Point			pt				= ea.GetPosition( this );
			m_ptRrightClick					= pt;

			if( rtHead.Contains( pt ) == true )
			{
				PnlMap			pnlMap				= Parent as PnlMap;

				if( pnlMap.LaneSelected.Contains( this ) == false )
					pnlMap.DoLaneSelected( this );

				m_cmHead.HorizontalOffset		= pt.X;
				m_cmHead.VerticalOffset			= pt.Y;
				m_cmHead.PlacementTarget		= this;
				m_cmHead.Placement				= PlacementMode.RelativePoint;
				m_cmHead.IsOpen					= true;				
			}
			else if( rtBody.Contains( pt ) == true )
			{
				Nullable<Rect>	rt				= GetRectangleHit( pt );
				if( rt == null )
				{
					m_cmBack.HorizontalOffset		= pt.X;
					m_cmBack.VerticalOffset			= pt.Y;
					m_cmBack.PlacementTarget		= this;
					m_cmBack.Placement				= PlacementMode.RelativePoint;
					m_cmBack.IsOpen					= true;	
				}
				else
				{
					m_cmFeature.HorizontalOffset	= pt.X;
					m_cmFeature.VerticalOffset		= pt.Y;
					m_cmFeature.PlacementTarget		= this;
					m_cmFeature.Placement			= PlacementMode.RelativePoint;
					m_cmFeature.IsOpen				= true;	
				}
			}
		}

		private void OnMouseUpLeft( object obj, MouseButtonEventArgs ea )
		{
			Rect			rtHead			= new Rect( 0.0f, 0.0f, PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, ActualHeight );					
			Point			pt				= ea.GetPosition( this );

			if( Keyboard.IsKeyDown( Key.LeftShift ) == true || Keyboard.IsKeyDown( Key.RightShift ) == true )
			{
				if( rtHead.Contains( pt ) == true )
				{
					PnlMap			pnlMap				= Parent as PnlMap;

					pnlMap.DoLaneSelectedAdd( this );
				}
				else
				{					
				}
			}
			else
			{
				if( rtHead.Contains( pt ) == true )
				{
					PnlMap			pnlMap				= Parent as PnlMap;

					pnlMap.DoLaneSelected( this );
				}
				else
				{					
				}
			}			
		}

		private void OnMouseUp( object obj, MouseButtonEventArgs ea )
		{
			switch( ea.ChangedButton )
			{
				case MouseButton.Left :
				{
					OnMouseUpLeft( obj, ea );
					break;
				};

				case MouseButton.Right :
				{
					OnMouseUpRight( obj, ea );

					break;
				}

			}			
		}

		private void OnMouseMove( object obj, MouseEventArgs ea )
		{
			MainWindow		mw				= MainWindow.GetMainWindow();
			Point			pt				= ea.GetPosition( this );

			PnlMap			pnlMap			= Parent as PnlMap;
			int				nPosition		= UtilityMath.DoRound( pnlMap.GetPositionFromPixel( pt.X + PnlMap.N_LANE_VERTICALGAP ) );
			double			dScore			= GetScoreFromYOffset( pt.Y );

			if( m_lblMouse.Visibility == Visibility.Visible )
				m_lblMouse.Content				= string.Format( "ʚϊɞ {0} -> {1:N0}", pt.X, nPosition );

			Nullable<Rect>	rt				= GetRectangleHit( pt );
			if( rt == null )
			{
				mw.DoFeatureSet( null );

				if( m_dfToolTip != null )
				{
					m_dfToolTip					= null;
					ToolTip						= null;
				}
			}
			else
			{
				DataFeature		df				= m_dtSelected.GetFeatureContaining( nPosition, dScore );

				mw.DoFeatureSet( df );

				if( df != null && df != m_dfToolTip )
				{
					m_dfToolTip					= df;
					ToolTip						= DoToolTipMake( df );
					ToolTipService.SetInitialShowDelay( this, 200 );
				}
			}
		}

		private Nullable< Rect > GetRectangleHit( Point pt )
		{
			foreach( KeyValuePair<Rect, DataFeature> pv in m_dicRectFeature )
			{				
				bool			bContain		= pv.Key.Contains( pt );

				if( bContain == true )
				{
					return pv.Key;
				}
			}
			
			return null;
		}

		private string DoToolTipMake( DataFeature df )
		{
			StringBuilder	sb				= new StringBuilder();
			sb.AppendFormat( "{0:N0} .. {1:N0}  ({2:N0} bp)", df.Start, df.End, df.End - df.Start + 1 );
			sb.AppendLine();
			sb.AppendFormat( "Score: {0}  Strand: {1}", df.ScoreString, df.Strand );

			string			strAttr			= df.Attribute;
			if( strAttr != null )
			{
				string		strName			= df.DoAttributeGet( "Name" );
				if( strName == null )	strName	= df.DoAttributeGet( "ID" );
				if( strName == null )	strName	= df.DoAttributeGet( "locus_tag" );
				if( strName != null )
				{
					sb.AppendLine();
					sb.Append( strName );
				}
			}

			return sb.ToString();
		}

		public void SetEditable( bool bEditable )
		{
			if( m_bEditable != bEditable )
			{
				m_bEditable		= bEditable;

				DoLayoutUpdate();
			}			
		}

		public string GetTypeText()
		{
			StringBuilder	sb				= new StringBuilder();
						
			foreach( DataType dt in m_lstDataType )
			{
				string			str				= null;

				if( dt == m_dtSelected )
				{
					str				= string.Format( "{0}* ", dt.Type );
				}
				else
				{
					str				= string.Format( "{0} ", dt.Type );
				}

				if( dt == m_lstDataType.Last() )
				{
					sb.Append( string.Format( "{0}", str ) );
				}
				else
				{
					sb.Append( string.Format( "{0}, ", str ) );
				}
			}

			string			strType			= sb.ToString();

			return strType;
		}

		public int GetPositionMax()
		{
			int				nMax			= int.MinValue;

			foreach( DataType dt in m_lstDataType )
			{
				nMax			= Math.Max( nMax, dt.PositionMax );
			}

			return nMax;
		}

		public int GetPositionMin()
		{
			int				nMin			= int.MaxValue;

			foreach( DataType dt in m_lstDataType )
			{
				nMin			= Math.Min( nMin, dt.PositionMin );
			}

			return nMin;
		}

		public double GetScoreMax()
		{
			double			dMax			= double.MinValue;

			foreach( DataType dt in m_lstDataType )
			{
				dMax			= Math.Max( dMax, dt.ScoreMax );
			}

			return dMax;
		}

		public double GetScoreMin()
		{
			double			dMin			= double.MaxValue;

			foreach( DataType dt in m_lstDataType )
			{
				dMin			= Math.Min( dMin, dt.ScoreMin );
			}

			return dMin;
		}
		
		protected override void OnRender( DrawingContext dc )
		{
			/*
			if( m_bmpLane == null )
			{
				Logger.PrintLine( "# PnlMapLane:OnRender() - {0}, null", m_dtLane.Type );
				return;			
			}
			 */

			m_nPositionMax					= GetPositionMax();
			m_nPositionMin					= GetPositionMin();			

			base.OnRender(dc);
						
			if( DataTypeSelected.Display == EDataTypeDisplay.BAR || DataTypeSelected.Display == EDataTypeDisplay.LINE ||
				DataTypeSelected.Display == EDataTypeDisplay.POINT )
			{
				OnRenderLane( dc );
			}
			else if( DataTypeSelected.Display == EDataTypeDisplay.STACK )
			{
				if( DataTypeSelected.StackLayer == 0 )
				{
					DataTypeSelected.BuildStack();
				}

				OnRenderLaneStack( dc );
			}			

			foreach( DataType dt in m_lstDataType )
			{
				if( dt == m_lstDataType.First() )
				{
					OnRenderFeature( dc, dt );
				}
				else
				{
					dc.PushOpacity( 0.8 );
					OnRenderFeature( dc, dt );
					dc.Pop();
				}				
			}			
			
			/*
			Rect			rt				= new Rect( 0.0f, 0.0f, ActualWidth, ActualHeight );
			dc.DrawImage( m_bmpLane, rt );	
			 */
		}

		private void OnRenderLaneDrawRectDark( DrawingContext dc, Brush bsh, Pen pen, Rect rt )
		{
			Brush			bshDark			= ManagerBrush.GetManager().GetBrushDark( bsh as SolidColorBrush );
			
			dc.DrawRectangle( bshDark, pen, rt );
		}

		private void OnRenderLaneDrawRectSolid( DrawingContext dc, Brush bsh, Pen pen, Rect rt )
		{
			GuidelineSet	gs				= new GuidelineSet();
			gs.GuidelinesX.Add( rt.X + 0.5 );
			gs.GuidelinesX.Add( rt.X + rt.Width + 0.5 );

			gs.GuidelinesY.Add( rt.Y + 0.5 );
			gs.GuidelinesY.Add( rt.Y + rt.Height + 0.5 );
			dc.PushGuidelineSet( gs );
			
			dc.DrawRectangle( bsh, pen, rt );
			
			dc.Pop();			
		}

		private void OnRenderLaneDrawRect( DrawingContext dc, Brush bsh, Pen pen, Rect rt )
		{
			/*
			if( rt.Width < 1 )
			{
				GuidelineSet	gs				= new GuidelineSet();
				gs.GuidelinesX.Add( rt.X + 0.5 );
				gs.GuidelinesX.Add( rt.X + rt.Width + 0.5 );

				gs.GuidelinesY.Add( rt.Y + 0.5 );
				gs.GuidelinesY.Add( rt.Y + rt.Height + 0.5 );
				dc.PushGuidelineSet( gs );
			}*/						
			
			dc.DrawRectangle( bsh, pen, rt );

			/*
			if( rt.Width < 1 )
			{
				dc.Pop();
			}*/
		}

		private void OnRenderFeature( DrawingContext dc, DataType dt )
		{
			PnlMap			pnlMap			= Parent as PnlMap;			
						
			double			dLeft			= PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP;
			double			dDispWidth		= ActualWidth - N_LANE_MARGIN - dLeft;
			double			dDispHeight		= ActualHeight - 2 * N_LANE_MARGIN;
			int				nDispWidth		= UtilityMath.DoRound( dDispWidth );
			int				nPositionWidth	= m_nPosDispMax - m_nPosDispMin;
			double			dPosPerPix		= nPositionWidth / dDispWidth;
			double			dFeatHeight		= m_dLaneHeightActual - 2 * N_LANE_MARGIN;

			double			dScaleMax		= dt.ScaleMax;
			double			dScaleMin		= dt.ScaleMin;			
			if( dt.Scale == false )
			{
				dScaleMax		= dt.ScoreMax;
				dScaleMin		= dt.ScoreMin;

				dScaleMin						= Math.Min( dScaleMin, 0 );
				dScaleMax						= Math.Max( dScaleMax, 0 );
			}			

			double			dScaleBase		= Math.Max( Math.Min( dScaleMax, 0 ), dScaleMin );

			double			dHeightBase		= GetYOffsetScoreBase();
			double			dHeightPos		= GetYOffsetScoreBase() - N_LANE_MARGIN;
			double			dHeightNeg		= m_dLaneHeightActual - 2 * N_LANE_MARGIN - dHeightPos;
			Rect			rtPrevPos		= new Rect( double.MinValue, double.MinValue, 0.0f, 0.0f );
			Rect			rtPrevNeg		= new Rect( double.MinValue, double.MinValue, 0.0f, 0.0f );
			Nullable< Point >	ptPrevious	= null;
			
			if( m_dtSelected == dt )
			{
				m_dicRectFeature.Clear();
			}

			m_gmClip.Rect					= new Rect( dLeft, N_LANE_MARGIN, dDispWidth, dDispHeight );
			dc.PushClip( m_gmClip );
		
			if( dt.Display == EDataTypeDisplay.STACK )
			{
				ListListFeature		lstList		= new ListListFeature();
				ListFeature			lstLeft		= new ListFeature();

				if( dt.IsReadOnly )
				{
					DataFeature[]	arr				= dt.GetFeatureArray();
					if( arr != null )
					{
						int				iStart			= dt.GetFeatureIndexByEnd( m_nPosDispMin );
						if( iStart < 0 )	iStart		= 0;

						for( int i = iStart; i < arr.Length; i++ )
						{
							DataFeature		df			= arr[ i ];
							if( df.Start > m_nPosDispMax )	break;
							lstLeft.Add( df );
						}
					}
				}
				else
				{
					ListDataFeatureNode	lnkFirst	= dt.GetFeatureLinkByEnd( m_nPosDispMin );
					ListDataFeatureNode	lnkEnd		= dt.GetFeatureLinkByStart( m_nPosDispMax );
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

				int			nLayerCount		= dt.Scale == true ? ( int ) ( dt.ScaleMax - dt.ScaleMin + 1 ) : dt.StackLayer;
				int			nLayer			= 0;
				double		dHeightStack	= dDispHeight / nLayerCount;

				foreach( ListFeature lst in lstList )
				{
					if( dt.Scale == true && ( ( nLayer + 1 ) < dScaleMin || ( nLayer + 1 ) > dScaleMax ) )
					{
						continue;
					}

					Nullable< double >	dOffXLast	= null;
					bool				bPrevSelected	= false;

					foreach( DataFeature df in lst )
					{					
						double			dOffX			= dDispWidth * ( df.Start - m_nPosDispMin ) / nPositionWidth + dLeft;				
						bool			bCurrSelected	= m_lstFeatSelected.Contains( df );
				
						if( dOffXLast == null )
						{
							dOffXLast		= dOffX;
							bPrevSelected	= bCurrSelected;
						}
						else if( dOffX - dOffXLast < 0.5f && !bCurrSelected && !bPrevSelected )
						{
							continue;
						}
						else
						{
							dOffXLast		= dOffX;
							bPrevSelected	= bCurrSelected;
						}

						if( dOffX > dDispWidth + dLeft )
						{					
							continue;
						}

						double			dWidth			= ( df.End - df.Start + 1 ) * dDispWidth / nPositionWidth;
						double			dHeight			= dHeightStack - 1.0f;
						double			dOffY			= dHeightStack * ( nLayerCount - nLayer - 1 ) + N_LANE_MARGIN;

						if( dOffX + dWidth > dDispWidth + dLeft )
						{
							dWidth			= dDispWidth + dLeft - dOffX;
						}
				
						dWidth			= Math.Max( N_FEATURE_MINIMALWIDTH, dWidth );
						dHeight			= Math.Max( N_FEATURE_MINIMALHEIGHT, dHeight );

						Rect			rt				= new Rect( dOffX, dOffY, dWidth, dHeight );

						if( m_dtSelected == dt && m_dicRectFeature.ContainsKey( rt ) == false )
						{
							m_dicRectFeature.Add( rt, df );	
						}

						double			dXPrev			= rtPrevPos.X + rtPrevPos.Width;
						if( dOffX - dXPrev < N_FEATURE_MINIMALGAP && pnlMap.Zoom >= pnlMap.ZoomThreshould )
						{							
							rt.X			+= N_FEATURE_MINIMALGAP;
						}
					
						rtPrevPos		= rt;

						Brush		bshFeature		= df.ColorBrush;

						if( bshFeature == null )
							bshFeature					= dt.DoBrushGet();

						Rect		rtDraw			= rt;

						if( m_lstFeatSelected.Contains( df ) == true )
						{
							// Selected
							OnRenderLaneDrawRectDark( dc, bshFeature, null, rtDraw );
						}
						else
						{
							OnRenderLaneDrawRect( dc, bshFeature, null, rtDraw );
						}
					}

					nLayer++;
				}
			}
			else if( dt.Display == EDataTypeDisplay.POINT || dt.Display == EDataTypeDisplay.BAR ||
					 dt.Display == EDataTypeDisplay.LINE )
			{
				// read-only / edit 공통: 표시 범위 내 feature를 수집
				ListFeature			lstDisp		= new ListFeature();

				if( dt.IsReadOnly )
				{
					DataFeature[]	arr				= dt.GetFeatureArray();
					if( arr != null )
					{
						int				iStart			= dt.GetFeatureIndexByEnd( m_nPosDispMin );
						if( iStart < 0 )	iStart		= 0;

						for( int i = iStart; i < arr.Length; i++ )
						{
							if( arr[ i ].Start > m_nPosDispMax )	break;
							lstDisp.Add( arr[ i ] );
						}
					}
				}
				else
				{
					ListDataFeatureNode	lnkFirst	= dt.GetFeatureLinkByEnd( m_nPosDispMin );
					ListDataFeatureNode	lnkEnd2		= dt.GetFeatureLinkByStart( m_nPosDispMax );
					ListDataFeatureNode	lnk			= lnkFirst;

					while( lnk != null && lnk != lnkEnd2 )
					{
						lstDisp.Add( lnk.Value );
						lnk							= lnk.Next;
					}
				}

				Nullable< double >	dOffXLast	= null;
				bool				bPrevSelected	= false;

				for( int fi = 0; fi < lstDisp.Count; fi++ )
				{
					DataFeature		df				= lstDisp[ fi ];

					if( df == null )				continue;
				
					double			dOffX			= dDispWidth * ( df.Start - m_nPosDispMin ) / nPositionWidth + dLeft;				
					bool			bCurrSelected	= m_lstFeatSelected.Contains( df );
				
					if( dOffXLast == null )
					{
						dOffXLast		= dOffX;
						bPrevSelected	= bCurrSelected;
					}
					else if( dOffX - dOffXLast < 0.5f && !bCurrSelected && !bPrevSelected )
					{
						continue;
					}
					else
					{
						dOffXLast		= dOffX;
						bPrevSelected	= bCurrSelected;
					}

					if( dOffX > dDispWidth + dLeft )
					{					
						continue;
					}

					double			dScore			= Math.Min( Math.Max( df.Score, dScaleMin ), dScaleMax );

					double			dWidth			= ( df.End - df.Start + 1 ) * dDispWidth / nPositionWidth;
					double			dHeight			= ( dScore >= dScaleBase ) ?
														dHeightPos / ( dScaleMax - dScaleBase ) * ( dScore - dScaleBase ) :
														dHeightNeg / ( dScaleBase - dScaleMin ) * ( dScaleBase - dScore );
					double			dOffY			= ( dScore >= dScaleBase ) ?
														dHeightBase - dHeight : dHeightBase;

					if( dOffX + dWidth > dDispWidth + dLeft )
					{
						dWidth			= dDispWidth + dLeft - dOffX;
					}
				
					dWidth			= Math.Max( N_FEATURE_MINIMALWIDTH, dWidth );
					dHeight			= Math.Max( N_FEATURE_MINIMALHEIGHT, dHeight );

					Rect			rt			= new Rect( dOffX, dOffY, dWidth, dHeight );							
				
					if( m_dtSelected == dt && m_dicRectFeature.ContainsKey( rt ) == false )
					{
						m_dicRectFeature.Add( rt, df );	
					}

					if( dScore >= dScaleBase )
					{					
						double			dXPrev			= rtPrevPos.X + rtPrevPos.Width;
						if( dOffX - dXPrev < N_FEATURE_MINIMALGAP &&
							pnlMap.Zoom >= pnlMap.ZoomThreshould )
						{							
							rt.X			+= N_FEATURE_MINIMALGAP;
						}
					
						rtPrevPos		= rt;
					}
					else
					{
						double			dXPrev			= rtPrevNeg.X + rtPrevNeg.Width;
						if( dOffX - dXPrev < N_FEATURE_MINIMALGAP &&
							pnlMap.Zoom >= pnlMap.ZoomThreshould )
						{						
							rt.X			+= N_FEATURE_MINIMALGAP;
						}					

						rtPrevNeg		= rt;
					}

					Brush		bshFeature		= df.ColorBrush;

					if( bshFeature == null )
						bshFeature					= dt.DoBrushGet();

					Rect		rtDraw			= rt;

					if( dt.Display == EDataTypeDisplay.POINT || dt.Display == EDataTypeDisplay.LINE )
					{
						if( dScore >= dScaleBase )
						{
							rtDraw		= new Rect( rt.X, rt.Y - 1.0f, rt.Width, 2.0f );
						}
						else
						{
							rtDraw		= new Rect( rt.X, rt.Y + rt.Height - 1.0f, rt.Width, 2.0f );
						}					
					}

					if( dt.Display == EDataTypeDisplay.LINE )
					{
						if( ptPrevious == null )
						{
							if( dScore >= dScaleBase )
							{
								ptPrevious		= new Point( rt.X + rt.Width / 2, rt.Y );
							}						
							else
							{
								ptPrevious		= new Point( rt.X + rt.Width / 2, rt.Y + rt.Height );
							}
						}
						else
						{
							Nullable< Point >	ptCurr		= null;
						
							if( dScore >= dScaleBase )
							{
								ptCurr			= new Point( rt.X + rt.Width / 2, rt.Y );
							}						
							else
							{
								ptCurr			= new Point( rt.X + rt.Width / 2, rt.Y + rt.Height );
							}

							if( m_lstFeatSelected.Contains( df ) == true )
							{
								Pen				penDark			= ManagerPen.GetManager().GetPen( 
																ManagerBrush.GetManager().GetBrushDark( bshFeature as SolidColorBrush ) );

								OnRenderLaneDrawLine( dc, penDark, ptPrevious.Value, ptCurr.Value );
							}
							else
							{
								Pen				pen				= ManagerPen.GetManager().GetPen( bshFeature );

								OnRenderLaneDrawLine( dc, pen, ptPrevious.Value, ptCurr.Value );
							}

							ptPrevious		= ptCurr;
						}
					}
				
					if( m_lstFeatSelected.Contains( df ) == true )
					{
						// Selected
						OnRenderLaneDrawRectDark( dc, bshFeature, null, rtDraw );
					}
					else
					{
						OnRenderLaneDrawRect( dc, bshFeature, null, rtDraw );
					}
				}	
			}

			dc.Pop();
		}

		private void OnRenderLaneDrawLineSolid( DrawingContext dc, Pen pen, Point pt0, Point pt1 )
		{
			GuidelineSet	gs				= new GuidelineSet();
			gs.GuidelinesX.Add( pt0.X + 0.5 );
			gs.GuidelinesX.Add( pt1.X + 0.5 );

			gs.GuidelinesY.Add( pt0.Y + 0.5 );
			gs.GuidelinesY.Add( pt1.Y + 0.5 );
						
			dc.PushGuidelineSet( gs );
			dc.DrawLine( pen, pt0, pt1 );
			dc.Pop();
		}

		private void OnRenderLaneDrawLine( DrawingContext dc, Pen pen, Point pt0, Point pt1 )
		{
			dc.DrawLine( pen, pt0, pt1 );			
		}

		private void OnRenderLaneStack( DrawingContext dc )
		{
			PnlMap			pnlMap			= Parent as PnlMap;

			Rect			rtBack			= new Rect( 0.0f, 0.0f, ActualWidth, ActualHeight );
			Rect			rtHead			= new Rect( 1.0f, PnlMap.N_LANE_VERTICALGAP, 
												PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP - 1, ActualHeight - 2 * PnlMap.N_LANE_VERTICALGAP );
			
			if( m_bEditable == true )
			{
				OnRenderLaneDrawRectSolid( dc, m_bshBack, m_penBack, rtBack );
			}
			else
			{
				OnRenderLaneDrawRectSolid( dc, m_bshBack, null, rtBack );				
			}

			Point			ptUpper0		= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, N_LANE_MARGIN );
			Point			ptUpper1		= new Point( ActualWidth - PnlMap.N_LANE_VERTICALGAP, N_LANE_MARGIN );
			OnRenderLaneDrawLineSolid( dc, m_penScoreSub, ptUpper0, ptUpper1 );

			Point			ptLower0		= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, ActualHeight - N_LANE_MARGIN );
			Point			ptLower1		= new Point( ActualWidth - PnlMap.N_LANE_VERTICALGAP, ActualHeight - N_LANE_MARGIN );
			OnRenderLaneDrawLineSolid( dc, m_penScoreSub, ptLower0, ptLower1 );
						
			Point			ptVertical0		= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, N_SCOREVERTICAL_GAP );
			Point			ptVertical1		= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, ActualHeight - N_SCOREVERTICAL_GAP );
			OnRenderLaneDrawLineSolid( dc, m_penLine, ptVertical0, ptVertical1 );

			double			dMax			= DataTypeSelected.Scale == true ? DataTypeSelected.ScaleMax : DataTypeSelected.StackLayer;
			Point			ptUpper			= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, N_LANE_MARGIN );
			OnRenderLaneText( dc, ptUpper, dMax );

			double			dMin			= DataTypeSelected.Scale == true ? DataTypeSelected.ScaleMin : 1.0f;
			Point			ptLower			= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, ActualHeight - N_LANE_MARGIN );
			OnRenderLaneText( dc, ptLower, dMin );
			
			if( m_bSelected == true )
			{
				dc.DrawRectangle( m_bshHeadSelected, null, rtHead );				
			}			
			
			TranslateTransform	tt			= m_splInfo.RenderTransform as TranslateTransform;
			tt.X							= PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP;
			tt.Y							= Height - N_SCOREVERTICAL_GAP + 2.0f;			
			m_splInfo.Visibility			= Visibility.Visible;		
	
			DoTypeFill();
		}

		private void OnRenderLane( DrawingContext dc )
		{			
			PnlMap			pnlMap			= Parent as PnlMap;

			Rect			rtBack			= new Rect( 0.0f, 0.0f, ActualWidth, ActualHeight );
			Rect			rtHead			= new Rect( 1.0f, PnlMap.N_LANE_VERTICALGAP, 
												PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP - 1, ActualHeight - 2 * PnlMap.N_LANE_VERTICALGAP );
			
			if( m_bEditable == true )
			{
				OnRenderLaneDrawRectSolid( dc, m_bshBack, m_penBack, rtBack );
			}
			else
			{
				OnRenderLaneDrawRectSolid( dc, m_bshBack, null, rtBack );				
			}
						
			Point			ptVertical0		= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, N_SCOREVERTICAL_GAP );
			Point			ptVertical1		= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, ActualHeight - N_SCOREVERTICAL_GAP );
			OnRenderLaneDrawLineSolid( dc, m_penLine, ptVertical0, ptVertical1 );
			
			Point			ptUpper0		= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, N_LANE_MARGIN );
			Point			ptUpper1		= new Point( ActualWidth - PnlMap.N_LANE_VERTICALGAP, N_LANE_MARGIN );
			OnRenderLaneDrawLineSolid( dc, m_penScoreSub, ptUpper0, ptUpper1 );

			Point			ptLower0		= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, ActualHeight - N_LANE_MARGIN );
			Point			ptLower1		= new Point( ActualWidth - PnlMap.N_LANE_VERTICALGAP, ActualHeight - N_LANE_MARGIN );
			OnRenderLaneDrawLineSolid( dc, m_penScoreSub, ptLower0, ptLower1 );

			double			dYOffsetScore0	= GetYOffsetScoreBase();
			Point			ptBase0			= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, dYOffsetScore0 );
			Point			ptBase1			= new Point( ActualWidth - PnlMap.N_LANE_VERTICALGAP, dYOffsetScore0 );
			OnRenderLaneDrawLineSolid( dc, m_penLine, ptBase0, ptBase1 );
			
			double			dMax			= DataTypeSelected.Scale == true ? DataTypeSelected.ScaleMax : Math.Max( 0, DataTypeSelected.ScoreMax );
			Point			ptUpper			= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, N_LANE_MARGIN );
			OnRenderLaneText( dc, ptUpper, dMax );

			double			dMin			= DataTypeSelected.Scale == true ? DataTypeSelected.ScaleMin : DataTypeSelected.ScoreMin;
			Point			ptLower			= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, ActualHeight - N_LANE_MARGIN );
			OnRenderLaneText( dc, ptLower, dMin );

			Point			ptBase			= new Point( PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP, dYOffsetScore0 );
			if( ptBase.Y >= ptUpper.Y + 8.0f && ptBase.Y <= ptLower.Y - 8.0f )
				OnRenderLaneText( dc, ptBase, 0.0f );

			if( m_bSelected == true )
			{
				dc.DrawRectangle( m_bshHeadSelected, null, rtHead );				
			}			
			
			TranslateTransform	tt			= m_splInfo.RenderTransform as TranslateTransform;
			tt.X							= PnlMap.N_RULER_LEFT - PnlMap.N_LANE_VERTICALGAP;
			tt.Y							= Height - N_SCOREVERTICAL_GAP + 2.0f;			
			m_splInfo.Visibility			= Visibility.Visible;		
	
			DoTypeFill();
		}

		private void OnRenderLaneText( DrawingContext dc, Point ptOrigin, double dScore )
		{
			string			strText			= string.Format( "{0:F1}", dScore );

			FormattedText	ft				= new FormattedText( strText,
																 CultureInfo.GetCultureInfo( "en-us" ), FlowDirection.LeftToRight,
																 m_tfScore, PnlMap.N_RULERTEXT_FONTSIZE, Brushes.Black,
																 VisualTreeHelper.GetDpi( this ).PixelsPerDip );
					
			dc.DrawText( ft, new Point( ptOrigin.X - ft.Width - 4.0f, ptOrigin.Y - ft.Height / 2 ) );			
		}

		private void MakeLaneImage()
		{
			double			dWidth			= double.IsNaN( Width ) ? ActualWidth : Width;
			double			dHeight			= double.IsNaN( Height ) ? ActualHeight : Height;

			if( m_bmpLane == null )
			{
				m_bmpLane		= new RenderTargetBitmap( ( int ) dWidth, ( int ) dHeight, 96, 96, PixelFormats.Pbgra32 );				
			}				
			else if( m_bmpLane.Width != ( int ) Width || m_bmpLane.Height != ( int ) Height )
			{				
				m_bmpLane		= new RenderTargetBitmap( ( int ) dWidth, ( int ) dHeight, 96, 96, PixelFormats.Pbgra32 );
			}
		}

		public void DoLayoutUpdate()
		{
			/*
			MakeLaneImage();

			if( m_bmpLane == null )
			{
				return;
			}
			
			DrawingVisual	dv				= new DrawingVisual();			
			DrawingContext	dc				= dv.RenderOpen();
							
			OnRenderLane( dc );
			OnRenderFeature( dc );

			dc.Close();						
			m_bmpLane.Render( dv );
			*/

			InvalidateVisual();	
		}	

		public static double LaneMargin
		{
			get
			{
				return N_LANE_MARGIN;
			}
		}

		public int PositionMax
		{
			get
			{
				return m_nPositionMax;
			}

			set
			{
				m_nPositionMax	= value;
			}
		}

		public int PositionMin
		{
			get
			{
				return m_nPositionMin;
			}

			set
			{
				m_nPositionMin	= value;
			}
		}

		public int PositionDisplayMax
		{
			get
			{
				return m_nPosDispMax;
			}

			set
			{
				m_nPosDispMax	= value;
			}
		}

		public int PositionDisplayMin
		{
			get
			{
				return m_nPosDispMin;
			}

			set
			{
				m_nPosDispMin	= value;
			}
		}
		
		private void BuildElementMenu()
		{
			BuildElementMenuHead();	
			BuildElementMenuBack();
			BuildElementMenuFeature();
		}

		private void BuildElementMenuBack()
		{
			m_cmBack						= new ContextMenu();

			MenuItem		miAdd			= new MenuItem();
			miAdd.Header					= "Add";
			miAdd.Click						+= delegate( object obj, RoutedEventArgs ea )
			{
				DoBackAddClick();
			};

			MenuItem		miBookmark		= new MenuItem();
			miBookmark.Header				= "Bookmark";
			miBookmark.Click				+= delegate( object obj, RoutedEventArgs ea )
			{
				DoBookmarkAdd();
			};

			m_cmBack.ContextMenuOpening	+= delegate( object obj, ContextMenuEventArgs ea )
			{
				if( m_bEditable == true )
				{
					miAdd.IsEnabled				= true;					
				}
				else
				{
					miAdd.IsEnabled				= false;
				}
			};

			m_cmBack.Items.Add( miAdd );
			m_cmBack.Items.Add( new Separator() );
			m_cmBack.Items.Add( miBookmark );
		}

		private void DoBookmarkAdd()
		{
			DialogBookmarkAdd	dlg			= new DialogBookmarkAdd();
			dlg.Owner						= MainWindow.GetMainWindow();

			PnlMap			pm				= Parent as PnlMap;

			string			strTitle		= string.Format( "{0}_{1}", "Bookmark", pm.Position );							

			dlg.SequenceId					= DataTypeSelected.SequenceId;
			dlg.BookmarkTitle				= strTitle;
			dlg.Position					= pm.Position;
			dlg.Zoom						= pm.Zoom;

			Nullable< bool >	b			= dlg.ShowDialog();
			if( b == true )
			{
				DataBookmark		db			= dlg.MakeBookmark();

				ManagerBookmark		mb			= ManagerBookmark.GetManager();

				mb.DoBookmarkAdd( db );
				mb.DoBookmarkUpdate();
			}
		}

		private void DoBackAddClick()
		{
			PnlMap			pnlMap			= Parent as PnlMap;

			DialogFeatureAdd	dlg			= new DialogFeatureAdd( this );
			dlg.Owner						= MainWindow.GetMainWindow();
			
			DataFeature		dfFirst			= DataTypeSelected.GetFeatureByStart( 0 );
			string			strSource		= dfFirst == null ? null : dfFirst.Source;
			int				nPosition		= UtilityMath.DoRound( pnlMap.GetPositionFromPixel( m_ptRrightClick.X + PnlMap.N_LANE_VERTICALGAP ) );
			double			dScore			= GetScoreFromYOffset( m_ptRrightClick.Y );			

			dlg.Source						= strSource;
			dlg.Start						= nPosition;
			dlg.End							= nPosition;
			dlg.Score						= dScore;	
							
			dlg.ShowDialog();
		}

		private void BuildElementMenuFeature()
		{
			m_cmFeature		= new ContextMenu();
			
			MenuItem		miBookmark		= new MenuItem();
			miBookmark.Header				= "Bookmark";
			miBookmark.Click				+= delegate( object obj, RoutedEventArgs ea )
			{
				DoBookmarkAdd();
			};
						
			MenuItem		miEdit			= new MenuItem();
			miEdit.Header					= "Edit";
			miEdit.Click					+= delegate( object obj, RoutedEventArgs ea )
			{
				DoFeatureEditClick();
			};

			MenuItem		miUnite			= new MenuItem();
			miUnite.Header					= "Unite";
			miUnite.Click					+= delegate( object obj, RoutedEventArgs ea )
			{
				PnlMap			pnlMap			= Parent as PnlMap;
				pnlMap.DoLaneFeatureUniteSelected();
			};			

			MenuItem		miMerge			= new MenuItem();
			miMerge.Header					= "Merge";
			miMerge.Click					+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadFeatOpMergeClick();
			};			

			MenuItem		miFilter		= new MenuItem();
			miFilter.Header					= "Filter";
			miFilter.Click					+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadFeatOpFilterClick();
			};			

			MenuItem		miMove			= new MenuItem();
			miMove.Header					= "Move";
			miMove.Click					+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadFeatOpMoveClick();
			};			

			MenuItem		miCopy			= new MenuItem();
			miCopy.Header					= "Copy";
			miCopy.Click					+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadFeatOpCopyClick();
			};	

			MenuItem		miDelete		= new MenuItem();
			miDelete.Header					= "Delete";
			miDelete.Click					+= delegate( object obj, RoutedEventArgs ea )
			{
				DoFeatureDeleteClick();
			};

			m_cmFeature.ContextMenuOpening	+= delegate( object obj, ContextMenuEventArgs ea )
			{
				if( m_bEditable == true )
				{
					if( ListFeatureSelected.Count == 0 )
					{
						miBookmark.IsEnabled			= true;
						miEdit.IsEnabled				= false;
						miUnite.IsEnabled				= false;
						miMerge.IsEnabled				= false;
						miFilter.IsEnabled				= false;
						miMove.IsEnabled				= false;
						miCopy.IsEnabled				= false;
						miDelete.IsEnabled				= false;
					}
					else if( ListFeatureSelected.Count == 1 )
					{
						miBookmark.IsEnabled			= true;
						miEdit.IsEnabled				= true;
						miUnite.IsEnabled				= false;
						miMerge.IsEnabled				= true;
						miFilter.IsEnabled				= false;
						miMove.IsEnabled				= true;
						miCopy.IsEnabled				= true;
						miDelete.IsEnabled				= true;
					}
					else
					{
						miBookmark.IsEnabled			= true;
						miEdit.IsEnabled				= false;
						miUnite.IsEnabled				= true;
						miMerge.IsEnabled				= true;
						miFilter.IsEnabled				= true;
						miMove.IsEnabled				= true;
						miCopy.IsEnabled				= true;
						miDelete.IsEnabled				= true;
					}					
				}
				else
				{
					miBookmark.IsEnabled			= true;
					miEdit.IsEnabled				= false;
					miUnite.IsEnabled				= false;
					miMerge.IsEnabled				= false;
					miFilter.IsEnabled				= false;
					miMove.IsEnabled				= false;
					miCopy.IsEnabled				= false;
					miDelete.IsEnabled				= false;
				}
			};

			m_cmFeature.Items.Add( miBookmark );
			m_cmFeature.Items.Add( new Separator() );
			m_cmFeature.Items.Add( miUnite );
			m_cmFeature.Items.Add( miMerge );
			m_cmFeature.Items.Add( miFilter );
			m_cmFeature.Items.Add( miMove );
			m_cmFeature.Items.Add( miCopy );
			m_cmFeature.Items.Add( new Separator() );			
			m_cmFeature.Items.Add( miEdit );
			m_cmFeature.Items.Add( miDelete );
		}

		private void BuildElementMenuHead()
		{			
			MenuItem		miSelect		= new MenuItem();
			miSelect.Header					= "Select to Edit";
			miSelect.Click					+= delegate( object obj, RoutedEventArgs ea )
			{				
				DoHeadSelectToEditClick();
			};			

			MenuItem		miSelectAll		= new MenuItem();
			miSelectAll.Header				= "Select All Features";
			miSelectAll.Click				+= delegate( object obj, RoutedEventArgs ea )
			{				
				DoHeadSelectAllClick();
			};			

			MenuItem		miSetColor		= new MenuItem();
			miSetColor.Header				= "Set Color";
			miSetColor.Click				+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadSetColorClick();
			};

			MenuItem		miSetHeight		= new MenuItem();
			miSetHeight.Header				= "Set Height";
			miSetHeight.Click				+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadSetHeightClick();			
			};

			MenuItem		miDisplayBox	= new MenuItem();
			miDisplayBox.Header				= "Bar";
			miDisplayBox.Click				+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadDisplayBox();			
			};

			MenuItem		miDisplayPoint	= new MenuItem();
			miDisplayPoint.Header			= "Point";
			miDisplayPoint.Click			+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadDisplayPoint();			
			};

			MenuItem		miDisplayLine	= new MenuItem();
			miDisplayLine.Header			= "Line";
			miDisplayLine.Click				+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadDisplayLine();			
			};

			MenuItem		miDisplayStack	= new MenuItem();
			miDisplayStack.Header			= "Stack";
			miDisplayStack.Click			+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadDisplayStack();			
			};

			MenuItem		miDisplay		= new MenuItem();
			miDisplay.Header				= "Display";

			miDisplay.Items.Add( miDisplayBox );
			miDisplay.Items.Add( miDisplayPoint );
			miDisplay.Items.Add( miDisplayLine );
			miDisplay.Items.Add( miDisplayStack );

			MenuItem		miAutoScale		= new MenuItem();
			miAutoScale.Header				= "Auto Scale";
			miAutoScale.Click				+= delegate( object obj, RoutedEventArgs ea )
			{
				double			dScaleMax		= DataTypeSelected.ScoreMax / 2;
				double			dScaleMin		= DataTypeSelected.ScoreMin / 2;
				SetScale( dScaleMax, dScaleMin );
			};

			MenuItem		miManualScale	= new MenuItem();
			miManualScale.Header			= "Manual Scale";
			miManualScale.Click				+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadManualScaleClick();
			};

			MenuItem		miChangeType	= new MenuItem();
			miChangeType.Header				= "Change Type";
			miChangeType.Click				+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadChangeTypeClick();
			};
						
			MenuItem		miMoveUp		= new MenuItem();
			miMoveUp.Header					= "Move Up";
			miMoveUp.Click					+= delegate( object obj, RoutedEventArgs ea )
			{
				PnlMap			pnlMap			= Parent as PnlMap;

				pnlMap.DoLaneMoveUp( this );
			};

			MenuItem		miMoveDown		= new MenuItem();
			miMoveDown.Header				= "Move Down";
			miMoveDown.Click				+= delegate( object obj, RoutedEventArgs ea )
			{
				PnlMap			pnlMap			= Parent as PnlMap;

				pnlMap.DoLaneMoveDown( this );
			};

			MenuItem		miUngroup		= new MenuItem();
			miUngroup.Header				= "Ungroup";
			miUngroup.Click					+= delegate( object obj, RoutedEventArgs ea )
			{
				PnlMap			pnlMap			= Parent as PnlMap;

				pnlMap.DoLaneUngroup();
			};

			MenuItem		miGroup			= new MenuItem();
			miGroup.Header					= "Group";
			miGroup.Click					+= delegate( object obj, RoutedEventArgs ea )
			{
				PnlMap			pnlMap			= Parent as PnlMap;

				pnlMap.DoLaneGroup();
			};

			MenuItem		miHide			= new MenuItem();
			miHide.Header					= "Hide Track";
			miHide.Click					+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadHideClick();
			};

			MenuItem		miClose			= new MenuItem();
			miClose.Header					= "Close File";
			miClose.Click					+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadCloseClick();
			};
			
			MenuItem		miFeatOp		= new MenuItem();
			miFeatOp.Header					= "Feature Operation";

			MenuItem		miOpFeatFilter	= new MenuItem();
			miOpFeatFilter.Header			= "Filter";
			miOpFeatFilter.Click			+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadFeatOpFilterClick();
			};

			MenuItem		miOpFeatMerge	= new MenuItem();
			miOpFeatMerge.Header			= "Merge";
			miOpFeatMerge.Click				+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadFeatOpMergeClick();
			};

			MenuItem		miOpFeatMove	= new MenuItem();
			miOpFeatMove.Header				= "Move";
			miOpFeatMove.Click				+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadFeatOpMoveClick();
			};

			MenuItem		miOpFeatCopy	= new MenuItem();
			miOpFeatCopy.Header				= "Copy";
			miOpFeatCopy.Click				+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadFeatOpCopyClick();
			};

			miFeatOp.Items.Add( miOpFeatFilter );
			miFeatOp.Items.Add( miOpFeatMerge );
			miFeatOp.Items.Add( miOpFeatMove );
			miFeatOp.Items.Add( miOpFeatCopy );

			MenuItem		miTrackOp		= new MenuItem();
			miTrackOp.Header				= "Track Operation";
		
			MenuItem		miOpAverage		= new MenuItem();
			miOpAverage.Header				= "Average";
			miOpAverage.Click				+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadOpeartionAverageClick();
			};

			MenuItem		miOpDiff		= new MenuItem();
			miOpDiff.Header					= "Difference";
			miOpDiff.Click					+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadOpeartionDiffClick();
			};

			MenuItem		miOpSum			= new MenuItem();
			miOpSum.Header					= "Summation";
			miOpSum.Click					+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadOpeartionSumClick();
			};

			MenuItem		miOpMerge		= new MenuItem();
			miOpMerge.Header				= "Merge";
			miOpMerge.Click					+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadOpeartionMergeClick();
			};

			MenuItem		miOpFilter		= new MenuItem();
			miOpFilter.Header				= "Filter";
			miOpFilter.Click				+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadOpeartionFilterClick();
			};

			MenuItem		miOpAdjust		= new MenuItem();
			miOpAdjust.Header				= "Adjust";
			miOpAdjust.Click				+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadOpeartionAdjustClick();
			};

			MenuItem		miOpAssignId	= new MenuItem();
			miOpAssignId.Header				= "Assign ID";
			miOpAssignId.Click				+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadOpeartionAssignIdClick();
			};

			miTrackOp.Items.Add( miOpAverage );
			miTrackOp.Items.Add( miOpDiff );
			miTrackOp.Items.Add( miOpSum );
			miTrackOp.Items.Add( miOpMerge );
			miTrackOp.Items.Add( miOpFilter );
			miTrackOp.Items.Add( miOpAdjust );
			miTrackOp.Items.Add( miOpAssignId );

			MenuItem		miIntegration	= new MenuItem();
			miIntegration.Header			= "Integration";

			MenuItem		miIntegPorf		= new MenuItem();
			miIntegPorf.Header				= "pORF";
			miIntegPorf.Click				+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadIntegrationPorfClick();
			};

			MenuItem		miIntegRts		= new MenuItem();
			miIntegRts.Header				= "RTS";
			miIntegRts.Click				+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadIntegrationRtsClick();
			};

			MenuItem		miIntegTu		= new MenuItem();
			miIntegTu.Header				= "TU";			
			miIntegTu.Click					+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadIntegrationTuClick();
			};

			MenuItem		miIntegTrn		= new MenuItem();
			miIntegTrn.Header				= "TRN";
			miIntegTrn.Visibility			= Visibility.Collapsed;
			miIntegTrn.Click				+= delegate( object obj, RoutedEventArgs ea )
			{
				DoHeadIntegrationTrnClick();
			};

			miIntegration.Items.Add( miIntegPorf );
			miIntegration.Items.Add( miIntegRts );
			miIntegration.Items.Add( miIntegTu );
			miIntegration.Items.Add( miIntegTrn );

			m_cmHead						= new ContextMenu();
			m_cmHead.ContextMenuOpening		+= delegate( object obj, ContextMenuEventArgs ea )
			{
				PnlMap			pnlMap			= Parent as PnlMap;

				if( pnlMap.GetCountLaneSelected() > 1 )
				{
					miSetColor.IsEnabled			= true;
					miSetHeight.IsEnabled			= true;
					miDisplay.IsEnabled				= true;
					miDisplayBox.IsEnabled			= true;
					miDisplayPoint.IsEnabled		= true;
					miMoveUp.IsEnabled				= false;
					miMoveDown.IsEnabled			= false;
					miGroup.IsEnabled				= true;
					miSelect.IsEnabled				= false;
					miSelectAll.IsEnabled			= false;
					miAutoScale.IsEnabled			= true;
					miManualScale.IsEnabled			= true;
					miChangeType.IsEnabled			= false;
					miHide.IsEnabled				= true;					
					miOpAverage.IsEnabled			= true;
					miOpDiff.IsEnabled				= true;
					miOpSum.IsEnabled				= true;
					miOpMerge.IsEnabled				= true;
					miOpFilter.IsEnabled			= true;
					miOpAdjust.IsEnabled			= true;
					miOpAssignId.IsEnabled			= true;										
					miIntegPorf.IsEnabled			= pnlMap.GetCountLaneSelected() == 3 ? true : false;
					miIntegRts.IsEnabled			= pnlMap.GetCountLaneSelected() == 2 ? true : false;;
					miIntegTu.IsEnabled				= pnlMap.GetCountLaneSelected() == 3 ? true : false;;
					miIntegTrn.IsEnabled			= true;
				}
				else if( pnlMap.GetCountLaneSelected() == 1 )
				{
					miSetColor.IsEnabled			= true;
					miSetHeight.IsEnabled			= true;
					miDisplay.IsEnabled				= true;
					miDisplayBox.IsEnabled			= true;
					miDisplayPoint.IsEnabled		= true;
					miMoveUp.IsEnabled				= true;
					miMoveDown.IsEnabled			= true;
					miGroup.IsEnabled				= false;
					miSelect.IsEnabled				= true;
					miSelectAll.IsEnabled			= true;
					miAutoScale.IsEnabled			= true;
					miManualScale.IsEnabled			= true;
					miChangeType.IsEnabled			= true;
					miHide.IsEnabled				= true;					
					miOpAverage.IsEnabled			= false;
					miOpDiff.IsEnabled				= false;
					miOpSum.IsEnabled				= false;
					miOpMerge.IsEnabled				= false;
					miOpFilter.IsEnabled			= false;
					miOpAdjust.IsEnabled			= true;
					miOpAssignId.IsEnabled			= true;
					miIntegPorf.IsEnabled			= false;
					miIntegRts.IsEnabled			= false;
					miIntegTu.IsEnabled				= false;
					miIntegTrn.IsEnabled			= false;
				}

				if( GetCountDataType() > 1 )
				{
					miUngroup.IsEnabled				= true;
				}
				else
				{
					miUngroup.IsEnabled				= false;
				}

				if( ListFeatureSelected.Count > 1 )
				{
					miOpFeatFilter.IsEnabled		= true;
					miOpFeatMerge.IsEnabled			= true;
					miOpFeatMove.IsEnabled			= true;
					miOpFeatCopy.IsEnabled			= true;					
				}
				else
				{
					miOpFeatFilter.IsEnabled		= false;
					miOpFeatMerge.IsEnabled			= false;
					miOpFeatMove.IsEnabled			= false;
					miOpFeatCopy.IsEnabled			= false;
				}

				if( DataTypeSelected.Display == EDataTypeDisplay.BAR )
				{
					miDisplayBox.IsChecked			= true;
					miDisplayPoint.IsChecked		= false;
					miDisplayLine.IsChecked			= false;
					miDisplayStack.IsChecked		= false;
				}
				else if( DataTypeSelected.Display == EDataTypeDisplay.POINT )
				{
					miDisplayBox.IsChecked			= false;
					miDisplayPoint.IsChecked		= true;
					miDisplayLine.IsChecked			= false;
					miDisplayStack.IsChecked		= false;
				}
				else if( DataTypeSelected.Display == EDataTypeDisplay.LINE )
				{
					miDisplayBox.IsChecked			= false;
					miDisplayPoint.IsChecked		= false;
					miDisplayLine.IsChecked			= true;
					miDisplayStack.IsChecked		= false;
				}
				else if( DataTypeSelected.Display == EDataTypeDisplay.STACK )
				{
					miDisplayBox.IsChecked			= false;
					miDisplayPoint.IsChecked		= false;
					miDisplayLine.IsChecked			= false;
					miDisplayStack.IsChecked		= true;
				}
			};

			m_cmHead.Items.Add( miSetColor );
			m_cmHead.Items.Add( miSetHeight );
			m_cmHead.Items.Add( miDisplay );
			m_cmHead.Items.Add( miMoveUp );
			m_cmHead.Items.Add( miMoveDown );			
			m_cmHead.Items.Add( miGroup );		
			m_cmHead.Items.Add( miUngroup );		
			m_cmHead.Items.Add( new Separator() );
			m_cmHead.Items.Add( miSelect );
			m_cmHead.Items.Add( miSelectAll );
			//m_cmHead.Items.Add( miAutoScale );
			m_cmHead.Items.Add( miManualScale );
			m_cmHead.Items.Add( miChangeType );
			m_cmHead.Items.Add( new Separator() );
			m_cmHead.Items.Add( miHide );
			m_cmHead.Items.Add( miClose );
			m_cmHead.Items.Add( new Separator() );
			m_cmHead.Items.Add( miFeatOp );
			m_cmHead.Items.Add( miTrackOp );
			m_cmHead.Items.Add( miIntegration );
		}

		public void DoHeadOpeartionDiffClick()
		{
			DialogLaneOperation	dlg			= new DialogLaneOperation( this );
			dlg.Owner						= MainWindow.GetMainWindow();						
			dlg.DoFillDifference();

			Nullable< bool >	b			= dlg.ShowDialog();
			if( b == true )
			{
			}
		}

		public void DoHeadFeatOpCopyClick()
		{
			DialogFeatureOperation	dlg		= new DialogFeatureOperation( this );
			dlg.Owner						= MainWindow.GetMainWindow();						
			dlg.DoFillCopy();

			Nullable< bool >	b			= dlg.ShowDialog();
			if( b == true )
			{
			}
		}

		public void DoHeadFeatOpMoveClick()
		{
			DialogFeatureOperation	dlg		= new DialogFeatureOperation( this );
			dlg.Owner						= MainWindow.GetMainWindow();						
			dlg.DoFillMove();

			Nullable< bool >	b			= dlg.ShowDialog();
			if( b == true )
			{
			}
		}

		public void DoHeadFeatOpMergeClick()
		{
			DialogFeatureOperation	dlg		= new DialogFeatureOperation( this );
			dlg.Owner						= MainWindow.GetMainWindow();						
			dlg.DoFillMerge();

			Nullable< bool >	b			= dlg.ShowDialog();
			if( b == true )
			{
			}
		}

		public void DoHeadFeatOpFilterClick()
		{
			DialogFeatureOperation	dlg		= new DialogFeatureOperation( this );
			dlg.Owner						= MainWindow.GetMainWindow();						
			dlg.DoFillFilter();

			Nullable< bool >	b			= dlg.ShowDialog();
			if( b == true )
			{
			}
		}

		public void DoHeadOpeartionAverageClick()
		{
			DialogLaneOperation	dlg			= new DialogLaneOperation( this );
			dlg.Owner						= MainWindow.GetMainWindow();						
			dlg.DoFillAverage();

			Nullable< bool >	b			= dlg.ShowDialog();
			if( b == true )
			{
			}
		}

		public void DoHeadOpeartionFilterClick()
		{
			DialogLaneOperation	dlg			= new DialogLaneOperation( this );
			dlg.Owner						= MainWindow.GetMainWindow();						
			dlg.DoFillFilter();

			Nullable< bool >	b			= dlg.ShowDialog();
			if( b == true )
			{
			}
		}

		public void DoHeadOpeartionAdjustClick()
		{
			DialogLaneOperation	dlg			= new DialogLaneOperation( this );
			dlg.Owner						= MainWindow.GetMainWindow();						
			dlg.DoFillAdjust();

			Nullable< bool >	b			= dlg.ShowDialog();
			if( b == true )
			{
			}
		}

		public void DoHeadOpeartionAssignIdClick()
		{
			DialogLaneOperation	dlg			= new DialogLaneOperation( this );
			dlg.Owner						= MainWindow.GetMainWindow();						
			dlg.DoFillAssignId();

			Nullable< bool >	b			= dlg.ShowDialog();
			if( b == true )
			{
			}
		}

		public void DoHeadOpeartionSumClick()
		{
			DialogLaneOperation	dlg			= new DialogLaneOperation( this );
			dlg.Owner						= MainWindow.GetMainWindow();						
			dlg.DoFillSummation();

			Nullable< bool >	b			= dlg.ShowDialog();
			if( b == true )
			{
			}
		}

		public void DoHeadOpeartionMergeClick()
		{
			DialogLaneOperation	dlg			= new DialogLaneOperation( this );
			dlg.Owner						= MainWindow.GetMainWindow();						
			dlg.DoFillMerge();

			Nullable< bool >	b			= dlg.ShowDialog();
			if( b == true )
			{
			}
		}
		
		public void DoHeadIntegrationPorfClick()
		{
			DialogIntegrationOperation
							dlg				= new DialogIntegrationOperation( this );

			dlg.Owner						= MainWindow.GetMainWindow();						
			dlg.DoFillPorf();

			Nullable< bool >	b			= dlg.ShowDialog();
			if( b == true )
			{
			}
		}

		public void DoHeadIntegrationRtsClick()
		{
			DialogIntegrationOperation
							dlg				= new DialogIntegrationOperation( this );

			dlg.Owner						= MainWindow.GetMainWindow();						
			dlg.DoFillRts();

			Nullable< bool >	b			= dlg.ShowDialog();
			if( b == true )
			{
			}
		}

		public void DoHeadIntegrationTuClick()
		{
			DialogIntegrationOperation
							dlg				= new DialogIntegrationOperation( this );

			dlg.Owner						= MainWindow.GetMainWindow();						
			dlg.DoFillTu();

			Nullable< bool >	b			= dlg.ShowDialog();
			if( b == true )
			{
			}
		}

		public void DoHeadIntegrationTrnClick()
		{
			DialogIntegrationOperation
							dlg				= new DialogIntegrationOperation( this );

			dlg.Owner						= MainWindow.GetMainWindow();						
			dlg.DoFillTrn();

			Nullable< bool >	b			= dlg.ShowDialog();
			if( b == true )
			{
			}
		}

		public void BuildElementBack()
		{
			m_gmClip						= new RectangleGeometry();

			m_bshBack						= new SolidColorBrush( CLR_BACK );
			m_bshBack.Freeze();

			m_penBack						= new Pen( new SolidColorBrush( CLR_BACKSELECTED ), 1.0f );
			m_penBack.Freeze();

			m_bshHeadSelected				= ManagerBrush.GetManager().GetBrush( CLR_HEADSELECTED, 125 );
		}

		private void DoFeatureEditClick()
		{
			MainWindow		mw				= MainWindow.GetMainWindow();
			ManagerEdit		me				= ManagerEdit.GetManager();

			PnlMap			pnlMap			= Parent as PnlMap;
			int				nPosition		= UtilityMath.DoRound( pnlMap.GetPositionFromPixel( m_ptRrightClick.X + PnlMap.N_LANE_VERTICALGAP ) );
			double			dScore			= GetScoreFromYOffset( m_ptRrightClick.Y );
			DataFeature		df				= m_dtSelected.GetFeatureContaining( nPosition, dScore );

			DialogFeatureEdit	dlg			= new DialogFeatureEdit( this );
			dlg.Owner						= mw;
			dlg.SetFeature( df );
			Nullable<bool>	b				= dlg.ShowDialog();
			if( b == true )
			{
				DataFeature		dfEdited		= dlg.MakeFeatureEdited();
				if( dfEdited.ColorBrush == null )
					dfEdited.ColorBrush			= df.ColorBrush;

				CommandEdit		cmd				= me.MakeCommandEdit();
				cmd.DoFeatureAdd( this, df, dfEdited );
				
				Cursor			cur				= mw.Cursor;
				mw.Cursor						= Cursors.Wait;
				
				m_dtSelected.DoFeatureRemove( df );
				m_dtSelected.DoFeatureAdd( dfEdited );

				mw.Cursor						= cur;

				mw.DoEditUpdate();
				DoLayoutUpdate();

				mw.DoAutoSaveImmediate();
			}
		}
				
		private void DoFeatureDeleteClick()
		{		
			DoFeatureDeleteMouseOver();
		}

		public void DoFeatureDeleteMouseOver()
		{			
			PnlMap			pnlMap			= Parent as PnlMap;
			int				nPosition		= UtilityMath.DoRound( pnlMap.GetPositionFromPixel( m_ptRrightClick.X + PnlMap.N_LANE_VERTICALGAP ) );
			double			dScore			= GetScoreFromYOffset( m_ptRrightClick.Y );
			DataFeature		df				= m_dtSelected.GetFeatureContaining( nPosition, dScore );

			ListFeature		lst				= new ListFeature();
			lst.Add( df );

			MainWindow		mw				= MainWindow.GetMainWindow();
			ManagerEdit		me				= ManagerEdit.GetManager();
			CommandDelete	cmd				= me.MakeCommandDelete();

			cmd.DoFeatureAdd( this, lst, null );

			DoFeatureDelete( lst );

			mw.DoEditUpdate();
			mw.DoExplorerUpdate();

			mw.DoAutoSaveImmediate();
		}
		
		public void DoFeatureUniteSelected( CommandReplace cmd )
		{
			if( m_lstFeatSelected.Count == 0 )
			{
				return;
			}

			MainWindow		mw				= MainWindow.GetMainWindow();
			
			DataFeature		dfNew			= DataFeature.MakeFeatureByMerge( m_lstFeatSelected );
			
			ListFeature		lstOld			= new ListFeature();
			lstOld.AddRange(  m_lstFeatSelected );

			ListFeature		lstNew			= new ListFeature();
			lstNew.Add( dfNew );
			
			cmd.DoFeatureAdd( this, lstOld, lstNew );
			
			Cursor			cur				= mw.Cursor;
			mw.Cursor						= Cursors.Wait;

			m_dtSelected.DoFeatureRemove( m_lstFeatSelected );
			m_dtSelected.DoFeatureAdd( dfNew );
			
			mw.Cursor						= cur;

			m_lstFeatSelected.Clear();
			m_lstFeatSelected.Add( dfNew );			
		}

		public void DoFeatureDeleteSelected( CommandDelete cmd )
		{
			cmd.DoFeatureAdd( this, m_lstFeatSelected, null );

			DoFeatureDelete( m_lstFeatSelected );

			m_lstFeatSelected.Clear();			
		}

		public void DoFeatureAdd( DataFeature df )
		{
			MainWindow		mw				= MainWindow.GetMainWindow();
			ManagerEdit		me				= ManagerEdit.GetManager();
			
			CommandAdd		cmd				= me.MakeCommandAdd();

			cmd.DoFeatureAdd( this, null, df );

			m_dtSelected.DoFeatureAdd( df );
			DoLayoutUpdate();
						
			mw.DoExplorerUpdate();
			mw.DoEditUpdate();
		}

		private void DoFeatureDelete( ListFeature lst )
		{
			MainWindow		mw				= MainWindow.GetMainWindow();
			
			Cursor			cur				= mw.Cursor;
			mw.Cursor						= Cursors.Wait;

			m_dtSelected.DoFeatureRemove( lst );
			
			mw.Cursor						= cur;

			DoLayoutUpdate();			
		}

		private void OnHeadNoScaleClick( object obj, RoutedEventArgs ea )
		{
			DataTypeSelected.Scale			= false;											
			DataTypeSelected.ScaleMax		= 0.0f;
			DataTypeSelected.ScaleMin		= 0.0f;

			DoLayoutUpdate();
		}
				
		public void SetScale( double dScaleMax, double dScaleMin )
		{
			DataTypeSelected.ScaleMax		= dScaleMax;
			DataTypeSelected.ScaleMin		= dScaleMin;			
			DataTypeSelected.Scale			= true;			

			DoLayoutUpdate();
		}
		
		public void DoHeadManualScaleClick()
		{
			PnlMap			pnlMap			= Parent as PnlMap;
			DialogSetScale	dlg				= new DialogSetScale( this );
			dlg.Owner						= MainWindow.GetMainWindow();

			dlg.SetNone( DataTypeSelected.ScoreMax, DataTypeSelected.ScoreMin );

			if( DataTypeSelected.Scale == true )
			{
				dlg.SetManual( DataTypeSelected.ScaleMax, DataTypeSelected.ScaleMin );
			}
			else
			{
				dlg.SetManual( DataTypeSelected.ScoreMax, 0 );
			}

			Nullable< bool >	b			= dlg.ShowDialog();
			if( b == true )
			{
				double			dScaleMax		= 0.0f;
				double			dScaleMin		= 0.0f;
				string			strMax;
				string			strMin;

				if( dlg.IsNone == true )
				{
					strMax			= dlg.NoneMax;
					strMin			= dlg.NoneMin;
				}
				else
				{
					strMax			= dlg.ManualMax;
					strMin			= dlg.ManualMin;
				}

				if( double.TryParse( strMax, out dScaleMax ) == false ||
					double.TryParse( strMin, out dScaleMin ) == false )
				{
					MessageBox.Show( "Scale values must be valid numbers.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning );
					return;
				}

				foreach( PnlMapLane pnl in pnlMap.LaneSelected )
				{
					pnl.SetScale( dScaleMax, dScaleMin );
				}
			}
		}

		public void DoHeadSetColorClick()
		{
			ColorDialog		dlg				= new ColorDialog();
			dlg.Owner						= MainWindow.GetMainWindow();

			if( dlg.ShowDialog()  == true )
			{
				PnlMap			pnlMap			= Parent as PnlMap;
				Color			clr				= dlg.SelectedColor;
                
				foreach( PnlMapLane pnl in pnlMap.LaneSelected )
				{
					pnl.DoFeatureSetColor( clr );
					pnl.DoLayoutUpdate();
				}					
			}
		}

		public void DoHeadSetHeightClick()
		{
			DialogSetHeight	dlg				= new DialogSetHeight( this );
			dlg.Owner						= MainWindow.GetMainWindow();			
			dlg.SetElementValue();
			
			Nullable< bool >	b			= dlg.ShowDialog();
			if( b == true )
			{
				PnlMap			pnlMap			= Parent as PnlMap;
				double			dHeight			= 0.0f;

				if( dlg.IsAutomatic == true )
				{
					// Auto
					dHeight							= 0.0f;					
				}
				else
				{
					// Manual
					dHeight							= Double.Parse( dlg.LaneHeight );					
				}

				foreach( PnlMapLane pnl in pnlMap.LaneSelected )
				{
					pnl.LaneHeight					= dHeight;
				}

				pnlMap.DoUpdateSize();				
			}
		}

		public void DoFeatureSetColor( Color clr )
		{
			m_dtSelected.DoColorSet( clr );
		}

		public void DoHeadDisplayBox()
		{
			PnlMap			pnlMap			= Parent as PnlMap;

			foreach( PnlMapLane pnl in pnlMap.LaneSelected )
			{
				if( pnl.DataTypeSelected.Display != EDataTypeDisplay.BAR )
				{
					pnl.DataTypeSelected.Display	= EDataTypeDisplay.BAR;

					pnl.DoLayoutUpdate();
				}
			}		
		}

		public void DoHeadDisplayPoint()
		{
			PnlMap			pnlMap			= Parent as PnlMap;

			foreach( PnlMapLane pnl in pnlMap.LaneSelected )
			{
				if( pnl.DataTypeSelected.Display != EDataTypeDisplay.POINT )
				{
					pnl.DataTypeSelected.Display	= EDataTypeDisplay.POINT;

					pnl.DoLayoutUpdate();
				}
			}		
		}

		public void DoHeadDisplayLine()
		{
			PnlMap			pnlMap			= Parent as PnlMap;

			foreach( PnlMapLane pnl in pnlMap.LaneSelected )
			{
				if( pnl.DataTypeSelected.Display != EDataTypeDisplay.LINE )
				{
					pnl.DataTypeSelected.Display	= EDataTypeDisplay.LINE;

					pnl.DoLayoutUpdate();
				}
			}		
		}

		public void DoHeadDisplayStack()
		{
			PnlMap			pnlMap			= Parent as PnlMap;

			foreach( PnlMapLane pnl in pnlMap.LaneSelected )
			{
				if( pnl.DataTypeSelected.Display != EDataTypeDisplay.STACK )
				{
					pnl.DataTypeSelected.Display	= EDataTypeDisplay.STACK;

					pnl.DoLayoutUpdate();
				}
			}		
		}
				
		public void DoHeadChangeTypeClick()
		{
			DialogChangeType	dlg			= new DialogChangeType( this );
			dlg.Owner						= MainWindow.GetMainWindow();	
			dlg.SetElementValue();
			dlg.ShowDialog();
		}

		public void DoTypeChange( string strType )
		{
			m_dtSelected.Type				= strType;
			m_dtSelected.IsEdited			= true;

			DoLayoutUpdate();											
		}

		public void DoDataTypeRemove()
		{
			ListDataType	lst				= new ListDataType();
			ManagerData		md				= ManagerData.GetManager();
			PnlMap			pm				= Parent as PnlMap;

			foreach( DataType dt in m_lstDataType )
			{
				DataType		dtExist			= md.GetDataType( dt.SequenceId, dt.Type );

				if( dtExist == null )
				{
					lst.Add( dt );					
				}
			}

			foreach( DataType dt in lst )
			{
				DoDataTypeRemove( dt );
			}
		}

		public void DoDataTypeCloseAll()
		{
			foreach( DataType dt in m_lstDataType )
			{
				dt.DoClose();
			}
		}

		public void DoHeadSelectToEditClick()
		{
			PnlMap			pnlMap			= Parent as PnlMap;

			foreach( PnlMapLane pnl in pnlMap.LaneSelected )
			{
				pnlMap.DoLaneSetEditable( pnl );
			}
		}

		public void DoHeadSelectAllClick()
		{		
			SetEditable( true );
			DoFeatureSelect( m_nPositionMin, m_nPositionMax );
		}

		public void DoHeadCloseClick()
		{
			PnlMap			pnlMap			= Parent as PnlMap;

			foreach( PnlMapLane pnl in pnlMap.LaneSelected )
			{
				pnl.DoDataTypeCloseAll();
				//pnlMap.DoLaneRemove( pnl );				
			}		

			//pnlMap.DoLaneRemoveAll();
			//pnlMap.DoLaneAdd();
			//pnlMap.DoUpdateView();

			MainWindow		mw				= MainWindow.GetMainWindow();
			mw.DoMapUpdate( null );
			mw.DoExplorerUpdate();
		}

		public void DoHeadHideClick()
		{			
			PnlMap			pnlMap			= Parent as PnlMap;

			pnlMap.DoLaneRemoveSelected();
			pnlMap.DoUpdateView();
		}

		private void OnBackgroundMouseDown( object obj, MouseButtonEventArgs ea )
		{
			if( ea.ClickCount == 1 )
			{
				if( ea.LeftButton == MouseButtonState.Pressed )
				{
				}
				else
				{
					// Released
				}
			}
			if( ea.ClickCount == 2 )
			{
				PnlMap			pnlMap			= Parent as PnlMap;
				pnlMap.DoLaneSetEditable( this );

				//SetEditable( !m_bEditable );				
			}
		}

		public void BuildElementLine()
		{
			m_penLine			= new Pen( Brushes.Black, 1.0f );
			m_penLine.Freeze();

			m_penScoreSub		= new Pen( Brushes.DarkGray, 1.0f );
			m_penScoreSub.Freeze();

			m_tfScore			= new Typeface( "calibri" );			
		}

		private void DoTypeFill()
		{
			foreach( Label lbl in m_lstType )
			{
				lbl.Padding						= new Thickness( 0.0f );
				lbl.BorderThickness				= new Thickness( 0.0f );
				lbl.BorderBrush					= null;

				ManagerLabel.GetManager().ReleaseLabel( lbl );

				m_splInfo.Children.Remove( lbl );
			}

			m_lstType.Clear();

			foreach( DataType dt in m_lstDataType )
			{
				Label			lbl				= ManagerLabel.GetManager().GetLabel();

				if( dt == m_dtSelected )
					lbl.Background					= m_brsTypeBackSelected;
				else
					lbl.Background					= m_brsTypeBack;

				lbl.Content						= dt.Type as string;				
				lbl.Padding						= new Thickness( 2.0f, 0.0f, 2.0f, 0.0f );
				lbl.BorderThickness				= new Thickness( 2.0f, 0.0f, 0.0f, 0.0f );
				lbl.BorderBrush					= dt.DoBrushGet();
				lbl.MouseDoubleClick			+= delegate( object obj, MouseButtonEventArgs ea )
				{
					Label			lblClick		= obj as Label;
					string			strType			= lblClick.Content as string;
					
					DoDataTypeSelect( strType );
					DoLayoutUpdate();

					ea.Handled						= true;
				};

				Label			lblEmpty		= ManagerLabel.GetManager().GetLabel();
				lblEmpty.Content				= " ";

				m_splInfo.Children.Add( lbl );
				m_splInfo.Children.Add( lblEmpty );

				m_lstType.Add( lbl );
				m_lstType.Add( lblEmpty );
			}	
		}
		
		public void BuildElementType()
		{
			m_brsTypeBack					= new SolidColorBrush( CLR_TYPEBACK );
			m_brsTypeBackSelected			= new SolidColorBrush( CLR_TYPEBACKSELECTED );

			m_splInfo						= new StackPanel();
			m_splInfo.RenderTransform		= new TranslateTransform();
			m_splInfo.Orientation			= Orientation.Horizontal;
			
			m_lstType						= new ListLabel();			
						
			Label			lblEmpty		= ManagerLabel.GetManager().GetLabel();
			lblEmpty.Content				= "  ";
			
			m_lblMouse						= ManagerLabel.GetManager().GetLabel();			
			m_lblMouse.Background			= m_brsTypeBack;
			m_lblMouse.Content				= "Mouse";											
			m_lblMouse.Visibility			= Visibility.Collapsed;
						
			m_splInfo.Children.Add( m_lblMouse );
			m_splInfo.Children.Add( lblEmpty );
						
			Children.Add( m_splInfo );
		}
				
		public void BuildElementFeature( int nCount )
		{
			m_dicRectFeature				= new DicRectFeature();			
			m_lstFeatSelected				= new ListFeature( nCount );
		}			
		
		public double GetYOffsetScoreBase()
		{
			// ScoreMax - ScoreMin : ScoreMax = m_dLaneHeightActual : x
			// x = ScoreMax * m_dLaneHeightActual / ( ScoreMax - ScoreMin )
			/*
			double			dYOffset		= m_dScoreMax * ( m_dLaneHeightActual - 2 * N_LANE_MARGIN ) / ( m_dScoreMax - m_dScoreMin )
											+ N_LANE_MARGIN; */
			double			dScaleMax		= DataTypeSelected.ScaleMax;
			double			dScaleMin		= DataTypeSelected.ScaleMin;
			if( DataTypeSelected.Scale == false )
			{
				dScaleMax		= DataTypeSelected.ScoreMax;
				dScaleMin		= DataTypeSelected.ScoreMin;

				dScaleMin						= Math.Min( dScaleMin, 0 );
				dScaleMax						= Math.Max( dScaleMax, 0 );
			}			

			double			dScaleBase		= Math.Max( Math.Min( dScaleMax, 0 ), dScaleMin );

			double			dYOffset		= ( dScaleMax - dScaleBase ) * ( m_dLaneHeightActual - 2 * N_LANE_MARGIN ) / ( dScaleMax - dScaleMin )
										+ N_LANE_MARGIN;
			return dYOffset;
		}

		public double GetScoreFromYOffset( double dYOffset )
		{
			// ScoreMax - ScoreMin : dScore - ScoreBase = m_dLaneHeightActual : x
			// x = ( dScore - ScoreBase ) / ( ScoreMax - ScoreMin ) * m_dLaneHeightActual
			/*
			double			dYOffset		= -1 * ( dScore - dScaleBase ) * ( m_dLaneHeightActual - 2 * N_LANE_MARGIN ) 
											/ ( m_dScoreMax - m_dScoreMin ) + N_LANE_MARGIN;
			 */
			double			dScaleMax		= DataTypeSelected.ScaleMax;
			double			dScaleMin		= DataTypeSelected.ScaleMin;
			if( DataTypeSelected.Scale == false )
			{
				dScaleMax		= DataTypeSelected.ScoreMax;
				dScaleMin		= DataTypeSelected.ScoreMin;

				dScaleMin						= Math.Min( dScaleMin, 0 );
				dScaleMax						= Math.Max( dScaleMax, 0 );
			}			

			double			dScaleBase		= Math.Max( Math.Min( dScaleMax, 0 ), dScaleMin );
			double			dOffsetBase		= GetYOffsetScoreBase();

			double			dScore			= ( dScaleMax - dScaleMin ) * ( dOffsetBase - dYOffset ) / ( m_dLaneHeightActual - 2 * N_LANE_MARGIN ) 
											+ dScaleBase;

			return dScore;
		}

		public double GetYOffsetFromScore( double dScore )
		{
			// ScoreMax - ScoreMin : dScore - ScoreBase = m_dLaneHeightActual : x
			// x = ( dScore - ScoreBase ) / ( ScoreMax - ScoreMin ) * m_dLaneHeightActual
			/*
			double			dYOffset		= -1 * ( dScore - dScaleBase ) * ( m_dLaneHeightActual - 2 * N_LANE_MARGIN ) 
											/ ( m_dScoreMax - m_dScoreMin ) + N_LANE_MARGIN;
			 */
			double			dScaleMax		= DataTypeSelected.ScaleMax;
			double			dScaleMin		= DataTypeSelected.ScaleMin;
			if( DataTypeSelected.Scale == false )
			{
				dScaleMax		= DataTypeSelected.ScoreMax;
				dScaleMin		= DataTypeSelected.ScoreMin;

				dScaleMin						= Math.Min( dScaleMin, 0 );
				dScaleMax						= Math.Max( dScaleMax, 0 );
			}			

			double			dScaleBase		= Math.Max( Math.Min( dScaleMax, 0 ), dScaleMin );

			double			dYOffset		= -1 * ( dScore - dScaleBase ) * ( m_dLaneHeightActual - 2 * N_LANE_MARGIN ) 
											/ ( dScaleMax - dScaleMin ) + N_LANE_MARGIN;

			return dYOffset;
		}	

		public ListDataType DataTypeList
		{
			get {	return m_lstDataType; }
		}

		public DataType DataTypeSelected
		{
			get {	return m_dtSelected; }
		}

		public double LaneWidth
		{
			get {	return m_dLaneWidth; }
			set {	m_dLaneWidth	= value; }
		}

		public double LaneHeight
		{
			get {	return m_dLaneHeight; }
			set	{	m_dLaneHeight	= value; }
		}

		public double LaneHeightActual
		{
			get {	return m_dLaneHeightActual; }
			set {	m_dLaneHeightActual	= value; }
		}

		protected override Size MeasureOverride( Size szAvailable )
		{	
			Size			szResult		= new Size( 0,0 );

			foreach( UIElement ue in Children )
			{
				ue.Measure( szAvailable );
				szResult.Width	= Math.Max( szResult.Width, ue.DesiredSize.Width );
				szResult.Height	= Math.Max( szResult.Height, ue.DesiredSize.Height );
			}
			
			szResult.Width	= double.IsPositiveInfinity( szAvailable.Width ) ? szResult.Width : szAvailable.Width;
			szResult.Height = double.IsPositiveInfinity( szAvailable.Height ) ? szResult.Height : szAvailable.Height;
			
			szResult.Width	= Math.Max( szResult.Width, N_LANE_WIDTHMINIMUM );
			szResult.Height	= Math.Max( szResult.Height, N_LANE_HEIGHTMINIMUM );
			
			return szResult;
		}		
				
		protected override Size ArrangeOverride( Size szFinal )
		{
			foreach( UIElement ue in Children )
			{
				ue.Arrange( new Rect( 0, 0, ue.DesiredSize.Width, ue.DesiredSize.Height ) );
			}
			
			return szFinal;
		}		
	}
}
