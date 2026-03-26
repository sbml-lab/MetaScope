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

	public class CommandEdit : CommandBase
	{
		//				.								.								.
		private			DataFeature						m_dfOriginal					= null;
		private			DataFeature						m_dfCurrent						= null;
		private			PnlMapLane						m_pnlOwner						= null;

		public DataFeature FeatureOriginal
		{
			get {	return m_dfOriginal; }
		}

		public DataFeature FeatureCurrent
		{
			get {	return m_dfCurrent; }
		}

		public PnlMapLane LaneOwner
		{
			get {	return m_pnlOwner; }
		}

		public void SetAdjustInfo( PnlMapLane pnl, DataFeature dfOriginal, DataFeature dfCurrent )
		{
			m_pnlOwner		= pnl;
			m_dfOriginal	= dfOriginal;
			m_dfCurrent		= dfCurrent;
		}

		public void UpdateAdjust( DataFeature dfOld, DataFeature dfNew )
		{
			DataType		dtSelected		= m_pnlOwner.DataTypeSelected;
			ListFeature		lstAdd			= GetFeatureListAdd( dtSelected );
			lstAdd.Remove( dfOld );
			lstAdd.Add( dfNew );

			m_dfCurrent		= dfNew;
		}

		public override string GetString()
		{
			if( m_dfOriginal != null && m_dfCurrent != null )
			{
				return string.Format( "{0:yyyy}/{1:MM}/{2:dd} {3:HH}:{4:mm}:{5:ss}, EDIT, [{6},{7}] > [{8},{9}]",
					m_dtCommand, m_dtCommand, m_dtCommand, m_dtCommand, m_dtCommand, m_dtCommand,
					m_dfOriginal.Start, m_dfOriginal.End,
					m_dfCurrent.Start, m_dfCurrent.End );
			}

			string			str				= string.Format( "{0:yyyy}/{1:MM}/{2:dd} {3:HH}:{4:mm}:{5:ss}, {6}, {7}->{8}",
												m_dtCommand, m_dtCommand, m_dtCommand, m_dtCommand, m_dtCommand, m_dtCommand,
												"EDIT", GetCountFeatureRemove(), GetCountFeatureAdd() );

			return str;
		}
	}
}
