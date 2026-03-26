using System;

namespace MetaScope.Services
{
	public class UtilityMessage
	{
		//			.								.								.
		public		const string					STR_MESSAGE_FILESAVEWORKSPACE	= "The workspace has been saved to \"{0}\".";
		public		const string					STR_MESSAGE_FILESAVELAYOUT		= "The layout has been saved to \"{0}\".";
		public		const string					STR_MESSAGE_FILESAVE			= "The file \"{0}\" has been saved.";
		public		const string					STR_MESSAGE_FILESSAVE			= "The files below have been saved.\r\n{0}";
		public		const string					STR_MESSAGE_NOFILESSAVED		= "No files have been saved.";

		/// <summary>
		/// UI layer subscribes to this event to display informational messages.
		/// Falls back to Console.WriteLine if no subscriber is attached.
		/// </summary>
		public static event Action<string>			OnMessage;

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
			if( OnMessage != null )
			{
				OnMessage.Invoke( strMessage );
			}
			else
			{
				Console.WriteLine( strMessage );
			}
		}
	}
}
