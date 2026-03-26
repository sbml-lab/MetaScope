using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

using MetaScope.Models;
using MetaScope.Controls;

namespace MetaScope.Services.Command
{
	using				ListFeature						= List< DataFeature >;
	using				DicListFeature					= Dictionary< DataType, List< DataFeature > >;
	using				ListMapLane						= List< PnlMapLane >;

	abstract public class CommandBase
	{
		//				.								.								.
		protected		DateTime						m_dtCommand						= DateTime.Now;
		protected		DicListFeature					m_dicAdd						= null;
		protected		DicListFeature					m_dicRemove						= null;
		protected		ListMapLane						m_lstLane						= null;

		public CommandBase()
		{
			m_dicAdd		= new DicListFeature();
			m_dicRemove		= new DicListFeature();
			m_lstLane		= new ListMapLane();
		}

		public void DoFeatureAdd( PnlMapLane pnl, DataFeature dfOld, DataFeature dfNew )
		{
			m_lstLane.Add( pnl );

			DataType		dtSelected		= pnl.DataTypeSelected;
			ListFeature		lstAdd			= GetFeatureListAdd( dtSelected );
			ListFeature		lstRemove		= GetFeatureListRemove( dtSelected );

			if( dfNew != null )				lstAdd.Add( dfNew );
			if( dfOld != null )				lstRemove.Add( dfOld );
		}

		public void DoFeatureAdd( PnlMapLane pnl, ListFeature lstOld, ListFeature lstNew )
		{
			m_lstLane.Add( pnl );

			DataType		dtSelected		= pnl.DataTypeSelected;
			ListFeature		lstAdd			= GetFeatureListAdd( dtSelected );
			ListFeature		lstRemove		= GetFeatureListRemove( dtSelected );

			if( lstNew != null )			lstAdd.AddRange( lstNew );
			if( lstOld != null )			lstRemove.AddRange( lstOld );
		}

		public void DoLaneUpdate()
		{
			foreach( PnlMapLane pnl in m_lstLane )
			{
				pnl.DoLayoutUpdate();
			}
		}

		public ListFeature GetFeatureListAdd( DataType dt )
		{
			if( m_dicAdd.ContainsKey( dt ) == true )
			{
				return m_dicAdd[ dt ];
			}
			else
			{
				ListFeature		lst				= new ListFeature();
				m_dicAdd[ dt ]					= lst;
				return lst;
			}
		}

		public ListFeature GetFeatureListRemove( DataType dt )
		{
			if( m_dicRemove.ContainsKey( dt ) == true )
			{
				return m_dicRemove[ dt ];
			}
			else
			{
				ListFeature		lst				= new ListFeature();
				m_dicRemove[ dt ]				= lst;
				return lst;
			}
		}

		public int GetCountLaneAdd()		{	return m_dicAdd.Keys.Count; }
		public int GetCountLaneRemove()		{	return m_dicRemove.Keys.Count; }

		public int GetCountFeatureAdd()
		{
			int				nCount			= 0;
			foreach( KeyValuePair< DataType, ListFeature > kv in m_dicAdd )
				nCount			+= kv.Value.Count;
			return nCount;
		}

		public int GetCountFeatureRemove()
		{
			int				nCount			= 0;
			foreach( KeyValuePair< DataType, ListFeature > kv in m_dicRemove )
				nCount			+= kv.Value.Count;
			return nCount;
		}

		public void DoUndo()
		{
			foreach( KeyValuePair< DataType, ListFeature > kv in m_dicAdd )
			{
				kv.Key.DoFeatureRemove( kv.Value );
			}

			foreach( KeyValuePair< DataType, ListFeature > kv in m_dicRemove )
			{
				kv.Key.DoFeatureAdd( kv.Value );
			}
		}

		abstract public string GetString();

		public DateTime Time
		{
			get {	return m_dtCommand; }
		}
	}
}
