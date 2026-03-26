using System;
using System.Windows.Input;

namespace MetaScope.Services
{
	/// <summary>
	/// Minimal ICommand implementation for use in the MainWindow code-behind.
	/// Replaces WPF RoutedCommand with a simple delegate-based command.
	/// </summary>
	public class RelayCommand : ICommand
	{
		private readonly Action<object>				m_actExecute;
		private readonly Func<object, bool>			m_fnCanExecute;

		public RelayCommand( Action<object> actExecute, Func<object, bool> fnCanExecute = null )
		{
			m_actExecute		= actExecute ?? throw new ArgumentNullException( nameof( actExecute ) );
			m_fnCanExecute		= fnCanExecute;
		}

		public RelayCommand( Action actExecute, Func<bool> fnCanExecute = null )
		{
			if( actExecute == null )
				throw new ArgumentNullException( nameof( actExecute ) );

			m_actExecute		= _ => actExecute();
			m_fnCanExecute		= fnCanExecute != null ? _ => fnCanExecute() : null;
		}

		public event EventHandler CanExecuteChanged;

		public bool CanExecute( object parameter )
		{
			return m_fnCanExecute == null || m_fnCanExecute( parameter );
		}

		public void Execute( object parameter )
		{
			m_actExecute( parameter );
		}

		public void RaiseCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke( this, EventArgs.Empty );
		}
	}
}
