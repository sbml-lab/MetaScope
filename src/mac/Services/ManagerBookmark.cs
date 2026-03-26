using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using MetaScope.Models;

namespace MetaScope.Services
{
	using			ListBookmark					= List< DataBookmark >;

	public class ManagerBookmark
	{
		//			.								.								.
		private		static ManagerBookmark			S_MANAGER						= null;

		/// <summary>
		/// Raised when bookmarks change and the UI needs to refresh.
		/// The UI layer subscribes to this instead of direct MainWindow coupling.
		/// </summary>
		public static event Action					OnBookmarkUpdated;

		static ManagerBookmark()
		{
			S_MANAGER			= new ManagerBookmark();
		}

		public static ManagerBookmark GetManager()
		{
			if( S_MANAGER == null )
			{
				S_MANAGER			= new ManagerBookmark();
			}

			return S_MANAGER;
		}

		private		ListBookmark					m_lstBookmark					= null;
		private		BookmarkComparer				m_cmpBookmark					= null;

		public ManagerBookmark()
		{
			m_cmpBookmark	= new BookmarkComparer();
			m_lstBookmark	= new ListBookmark();
		}

		public ManagerBookmark( ListBookmark lstBookmark )
			: this()
		{
			m_lstBookmark.AddRange( lstBookmark );
		}

		public ListBookmark ListBookmark
		{
			get {	return m_lstBookmark; }
			set {	m_lstBookmark = value; }
		}

		public void DoBookmarkUpdate()
		{
			// Decoupled via event — UI layer subscribes to OnBookmarkUpdated
			OnBookmarkUpdated?.Invoke();
		}

		public void DoBookmarkAdd( DataBookmark db )
		{
			m_lstBookmark.Add( db );
			m_lstBookmark.Sort( m_cmpBookmark );
		}

		public void DoBookmarkRead( string strFile )
		{

		}

		public class BookmarkComparer: IComparer< DataBookmark >
		{
			public int Compare( DataBookmark db0, DataBookmark db1 )
			{
				if( db0.Position < db1.Position )
				{
					return -1;
				}
				else if( db0.Position == db1.Position )
				{
					return db0.Title.CompareTo( db1.Title );
				}
				else
				{
					return 1;
				}
			}
		}
	}
}
