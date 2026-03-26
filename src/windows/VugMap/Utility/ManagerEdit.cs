using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VugMap.Utility.Command;
using VugMap.Utility.Data;
using VugMap.Window;

namespace VugMap.Utility
{
	using				ListFeature						= List< DataFeature >;
	using				ListCommand						= List< CommandBase >;

	public class ManagerEdit
	{
		//				.								.								.
		private			static ManagerEdit				S_MANAGER						= null;

		static ManagerEdit()
		{
			S_MANAGER				= new ManagerEdit();
		}

		public static ManagerEdit GetManager()
		{
			if( S_MANAGER == null )
			{
				S_MANAGER				= new ManagerEdit();
			}

			return S_MANAGER;
		}

		private			ListCommand						m_lstCommand					= null;

		public ManagerEdit()
		{
			m_lstCommand	= new ListCommand();							
		}

		public CommandBase GetCommandLast()
		{
			int				nCount			= GetCount();
			CommandBase		cbLast			= GetCommand( nCount - 1 );

			return cbLast;
		}

		public CommandBase RemoveCommandLast()
		{
			CommandBase		cbLast			= GetCommandLast();
			m_lstCommand.Remove( cbLast );

			return cbLast;
		}

		public CommandAdd MakeCommandAdd()
		{
			CommandAdd		cmd				= new CommandAdd();
			
			DoCommandAdd( cmd );

			return cmd;
		}

		public CommandReplace MakeCommandReplace()
		{
			CommandReplace	cmd				= new CommandReplace();
			
			DoCommandAdd( cmd );

			return cmd;
		}

		public CommandEdit MakeCommandEdit()
		{
			CommandEdit		cmd				= new CommandEdit();
			
			DoCommandAdd( cmd );

			return cmd;
		}

		public CommandDelete MakeCommandDelete()
		{
			CommandDelete	cmd				= new CommandDelete();
			
			DoCommandAdd( cmd );

			return cmd;
		}

		public int GetCount()
		{
			int				nCount			= m_lstCommand.Count;

			return nCount;
		}

		public CommandBase GetCommand( int nIndex )
		{
			CommandBase		cb				= m_lstCommand[ nIndex ];

			return cb;
		}

		public CommandBase RemoveCommand( int nIndex )
		{
			CommandBase		cb				= GetCommand( nIndex );
			m_lstCommand.Remove( cb );

			return cb;
		}

		public ListCommand GetCommand()
		{
			return m_lstCommand;
		}

		public void DoEditUpdate()
		{
			MainWindow		mw				= MainWindow.GetMainWindow();

			mw.DoEditUpdate();
		}

		private void DoCommandAdd( CommandBase cb )
		{
			lock( m_lstCommand )
			{
				m_lstCommand.Add( cb );
			}
		}
	}
}
