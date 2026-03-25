using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace VugMap.Utility
{
	public class UtilityMessage
	{
		//			.								.								.
		public		const string					STR_MESSAGE_FILESAVEWORKSPACE	= "The workspace has been saved to \"{0}\".";
		public		const string					STR_MESSAGE_FILESAVELAYOUT		= "The layout has been saved to \"{0}\".";
		public		const string					STR_MESSAGE_FILESAVE			= "The file \"{0}\" has been saved.";
		public		const string					STR_MESSAGE_FILESSAVE			= "The files below have been saved.\r\n{0}";
		public		const string					STR_MESSAGE_NOFILESSAVED		= "No files have been saved.";

		public static void ShowMessageFilesSave( string strFile )
		{
			if( strFile != "" )
			{
				string			strMessage		= string.Format( STR_MESSAGE_FILESSAVE, strFile );

				ShowMessage( strMessage );
			}
			else
			{
				ShowMessage( STR_MESSAGE_NOFILESSAVED );
			}
		}

		public static void ShowMessageFileSave( string strFile )
		{
			string			strMessage		= string.Format( STR_MESSAGE_FILESAVE, strFile );

			ShowMessage( strMessage );
		}

		public static void ShowMessageFileSaveLayout( string strFile )
		{
			string			strMessage		= string.Format( STR_MESSAGE_FILESAVELAYOUT, strFile );

			ShowMessage( strMessage );
		}

		public static void ShowMessageFileSaveWorkspace( string strFile )
		{
			string			strMessage		= string.Format( STR_MESSAGE_FILESAVEWORKSPACE, strFile );

			ShowMessage( strMessage );
		}

		public static void ShowMessage( string strMessage )
		{
			MessageBox.Show( strMessage, "Message", MessageBoxButton.OK, MessageBoxImage.Information );
		}
	}
}
