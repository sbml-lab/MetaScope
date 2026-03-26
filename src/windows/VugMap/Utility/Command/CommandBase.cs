using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

using VugMap.Utility.Data;
using VugMap.Window;

namespace VugMap.Utility.Command
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

			ListFeature		lstAdd			= GetFeatureListAdd( pnl.DataTypeSelected );
			ListFeature		lstRemove		= GetFeatureListRemove( pnl.DataTypeSelected );

			if( dfNew != null )				lstAdd.Add( dfNew );
			if( dfOld != null )				lstRemove.Add( dfOld );
		}		

		public void DoFeatureAdd( PnlMapLane pnl, ListFeature lstOld, ListFeature lstNew )
		{
			m_lstLane.Add( pnl );

			ListFeature		lstAdd			= GetFeatureListAdd( pnl.DataTypeSelected );
			ListFeature		lstRemove		= GetFeatureListRemove( pnl.DataTypeSelected );

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
				ListFeature		lst				= m_dicAdd[ dt ];

				return lst;								
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
				ListFeature		lst				= m_dicRemove[ dt ];

				return lst;								
			}
			else
			{
				ListFeature		lst				= new ListFeature();

				m_dicRemove[ dt ]				= lst;

				return lst;
			}
		}

		public int GetCountLaneAdd()
		{
			int				nCount			= m_dicAdd.Keys.Count;

			return nCount;
		}

		public int GetCountLaneRemove()
		{
			int				nCount			= m_dicRemove.Keys.Count;

			return nCount;
		}

		public int GetCountFeatureAdd()
		{
			int				nCount			= 0;

			foreach( KeyValuePair< DataType, ListFeature > kv in m_dicAdd )
			{
				nCount			+= kv.Value.Count;
			}

			return nCount;
		}

		public int GetCountFeatureRemove()
		{
			int				nCount			= 0;

			foreach( KeyValuePair< DataType, ListFeature > kv in m_dicRemove )
			{
				nCount			+= kv.Value.Count;
			}

			return nCount;
		}

		public void DoUndo()
		{			
			foreach( KeyValuePair< DataType, ListFeature > kv in m_dicAdd )
			{
				DataType		dt				= kv.Key;
				ListFeature		lst				= kv.Value;

				dt.DoFeatureRemove( lst );
			}			

			foreach( KeyValuePair< DataType, ListFeature > kv in m_dicRemove )
			{
				DataType		dt				= kv.Key;
				ListFeature		lst				= kv.Value;

				dt.DoFeatureAdd( lst );
			}		
		}

		abstract public string GetString();		

		public DateTime Time
		{
			get {	return m_dtCommand; }
		}
	}
}
