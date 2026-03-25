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

namespace VugMap.Utility.Error
{
	public class ErrorMessage
	{
		//			.								.								.
		public		const string					STR_ERROR_SELECTLANEFIRST		= "The lane to edit should be selected first.";
		public		const string					STR_ERROR_EMPTYFEATURE			= "Feature cannot be an empty string.";
		public		const string					STR_ERROR_FILEALREADYOPEN		= "The file \"{0}\" is already loaded.";
		public		const string					STR_ERROR_FILENOTSUPPORTED		= "The file extention \"{0}\" is not supported.";
		public		const string					STR_ERROR_FILEWORKSPACE			= "The workspace file should be open alone.";
		public		const string					STR_ERROR_FILEINVALID			= "The file \"{0}\" is in an invalid format.";
		public		const string					STR_ERROR_FILETYPEEXISTS		= "The type \"{0}\" is aleady used.";
		public		const string					STR_ERROR_FILENOTFOUND			= "The file \"{0}\" is not found.";
		public		const string					STR_ERROR_SEARCHNOTINTRODUCTION	= "You cannot search in the introduction page.";
		public		const string					STR_ERROR_SEARCHNODOCUMENTOPEN	= "The map with a sequence ID \"{0}\" should be open first.";
		public		const string					STR_ERROR_FILENOTSELECTED		= "The file should be selected first.";
		public		const string					STR_ERROR_AVERAGENOTPOSSIBLE	= "Data lanes selected to make average are not compatible in format.";
		public		const string					STR_ERROR_OPACITYINVALID		= "The opacity value \"{0}\" is not valid.\r\nIt should be between 0 and 255 or 0x00 and 0xFF.";
		public		const string					STR_ERROR_ZOOMTOINVALID			= "The zoom value \"{0}\" is not valid.\r\nIt should be between {1} and {2}.";
		public		const string					STR_ERROR_POSITIONTOINVALID		= "The position value \"{0}\" is not valid.\r\nIt should be between {1} and {2}.";
		public		const string					STR_ERROR_SCOREINVALID			= "The value \"{0}\" is not valid.";
		public		const string					STR_ERROR_ASSIGNIDINVALID		= "The ID pattern is not valid";

		public static void ShowErrorAssignIdInvalid()
		{
			ShowError( STR_ERROR_ASSIGNIDINVALID );
		}

		public static void ShowErrorScoreInvalid( string strScore )
		{
			string			strMessage		= string.Format( STR_ERROR_SCOREINVALID, strScore );

			ShowError( strMessage );
		}

		public static void ShowErrorPositionToInvalid( string strPosition, int nMin, int nMax )
		{
			string			strMessage		= string.Format( STR_ERROR_POSITIONTOINVALID, strPosition, nMin, nMax );

			ShowError( strMessage );
		}

		public static void ShowErrorZoomToInvalid( string strZoom, double dMin, double dMax )
		{
			string			strMessage		= string.Format( STR_ERROR_ZOOMTOINVALID, strZoom, dMin, dMax );

			ShowError( strMessage );
		}

		public static void ShowErrorOpacityInvalid( string strOpacity )
		{
			string			strMessage		= string.Format( STR_ERROR_OPACITYINVALID, strOpacity );

			ShowError( strMessage );
		}

		public static void ShowErrorAverageNotPossible()
		{
			ShowError( STR_ERROR_AVERAGENOTPOSSIBLE );
		}

		public static void ShowErrorSearchNoDocumentOpen( string strSequenceId )
		{
			string			strMessage		= string.Format( STR_ERROR_SEARCHNODOCUMENTOPEN, strSequenceId );

			ShowError( strMessage );
		}

		public static void ShowErrorFileNotSelected()
		{
			ShowError( STR_ERROR_FILENOTSELECTED );
		}

		public static void ShowErrorSearchNotIntroduction()
		{
			ShowError( STR_ERROR_SEARCHNOTINTRODUCTION );
		}

		public static void ShowErrorFileNotFound( string strFile )
		{
			string			strMessage		= string.Format( STR_ERROR_FILENOTFOUND, strFile );

			ShowError( strMessage );
		}

		public static void ShowErrorFileTypeExists( string strType )
		{
			string			strMessage		= string.Format( STR_ERROR_FILETYPEEXISTS, strType );

			ShowError( strMessage );
		}

		public static void ShowErrorFileInvalid( string strFile )
		{
			string			strMessage		= string.Format( STR_ERROR_FILEINVALID, strFile );

			ShowError( strMessage );
		}

		public static void ShowErrorFileInvalid( string strFile, string strReason )
		{
			string			strMessage		= string.Format( STR_ERROR_FILEINVALID, strFile );
			strMessage						+= string.Format( "\r\n({0})", strReason );

			ShowError( strMessage );
		}

		public static void ShowErrorFileWorkspace()
		{
			ShowError( STR_ERROR_FILEWORKSPACE );
		}

		public static void ShowErrorFileAlreadyOpen( string strFile )
		{
			string			strMessage		= string.Format( STR_ERROR_FILEALREADYOPEN, strFile );

			ShowError( strMessage );
		}

		public static void ShowErrorFileNotSupported( string strExt )
		{
			string			strMessage		= string.Format( STR_ERROR_FILENOTSUPPORTED, strExt );

			ShowError( strMessage );
		}

		public static void ShowErrorEmptyFeature()
		{
			ShowError( STR_ERROR_EMPTYFEATURE );
		}

		public static void ShowErrorSelectLaneFirst()
		{
			ShowError( STR_ERROR_SELECTLANEFIRST );
		}

		public static void ShowError( string strMessage )
		{
			MessageBox.Show( strMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error );
		}
	}
}
