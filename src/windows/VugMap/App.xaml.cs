using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace VugMap
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

			DispatcherUnhandledException += OnDispatcherUnhandledException;
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
		}

		private void OnDispatcherUnhandledException( object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e )
		{
			string		strMessage		= string.Format( "An unexpected error occurred.\r\n\r\n{0}", e.Exception.Message );

			VugMap.Utility.Logger.Logger.PrintLine( "# ERROR, App:OnDispatcherUnhandledException - {0}", e.Exception.ToString() );
			VugMap.Utility.Error.ErrorMessage.ShowError( strMessage );

			e.Handled		= true;
		}

		private static void OnUnhandledException( object sender, UnhandledExceptionEventArgs e )
		{
			Exception		ex				= e.ExceptionObject as Exception;
			string			strMessage		= ( ex != null ) ? ex.Message : "Unknown error";

			VugMap.Utility.Logger.Logger.PrintLine( "# ERROR, App:OnUnhandledException - {0}", ( ex != null ) ? ex.ToString() : strMessage );
			VugMap.Utility.Error.ErrorMessage.ShowError( string.Format( "A fatal error occurred.\r\n\r\n{0}", strMessage ) );
		}

		private static Assembly OnAssemblyResolve( object sender, ResolveEventArgs args )
		{
			string			strName			= new AssemblyName( args.Name ).Name + ".dll";

			using( Stream stm = Assembly.GetExecutingAssembly().GetManifestResourceStream( strName ) )
			{
				if( stm == null )			return null;

				byte[]			data			= new byte[ stm.Length ];
				stm.Read( data, 0, data.Length );
				return Assembly.Load( data );
			}
		}
	}
}
