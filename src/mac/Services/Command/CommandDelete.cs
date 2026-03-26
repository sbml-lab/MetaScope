using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

using MetaScope.Models;

namespace MetaScope.Services.Command
{
	using				ListFeature						= List< DataFeature >;

	public class CommandDelete : CommandBase
	{
		//				.								.								.

		public override string GetString()
		{
			string			str				= string.Format( "{0:yyyy}/{1:MM}/{2:dd} {3:HH}:{4:mm}:{5:ss}, {6}, {7} ({8})",
												m_dtCommand, m_dtCommand, m_dtCommand, m_dtCommand, m_dtCommand, m_dtCommand,
												"DELETE", GetCountFeatureRemove(), GetCountLaneRemove() );

			return str;
		}
	}
}
