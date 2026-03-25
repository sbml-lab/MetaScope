using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

using VugMap.Utility.Data;
using VugMap.Utility.Window;

namespace VugMap.Window
{
	using				ListFeature						= List< DataFeature >;

	public class PropertyFeatureGroup
	{
		//				.								.								.
		public			const string					STR_CATEGORY_GROUP				= "Feature Group";
	
		private			int								m_nStart						= 0;
		private			int								m_nEnd							= 0;
		private			ListFeature						m_lstFeature					= null;

		public PropertyFeatureGroup()
		{
			m_lstFeature	= new ListFeature();
		}

		public void SetFeature( ListFeature lstFeature )
		{
			m_lstFeature	= lstFeature;

			if( lstFeature.Count == 0 )		return;

			DataFeature		dfFirst			= lstFeature.First();
			DataFeature		dfLast			= lstFeature.Last();

			m_nStart		= dfFirst.Start;
			m_nEnd			= dfLast.End;
		}

		private double GetScoreMax()
		{
			double			dMax			= double.MinValue;

			foreach( DataFeature df in m_lstFeature )
			{
				dMax			= Math.Max( dMax, df.Score );
			}

			return dMax;
		}

		private double GetScoreMin()
		{
			double			dMin			= double.MaxValue;

			foreach( DataFeature df in m_lstFeature )
			{
				dMin			= Math.Min( dMin, df.Score );
			}

			return dMin;
		}

		[ Category( STR_CATEGORY_GROUP ), PropertyOrder( 10 ), Description( "# of features" ) ]
		public string Count
		{
			get {			return string.Format( "{0:N0}", m_lstFeature.Count ); }			
		}

		[ Category( STR_CATEGORY_GROUP ), PropertyOrder( 11 ), Description( "Starting position" ) ]
		public string Start
		{
			get {			return string.Format( "{0:N0}", m_nStart ); }			
		}

		[ Category( STR_CATEGORY_GROUP ), PropertyOrder( 12 ), Description( "Ending position" ) ]
		public string End
		{
			get {			return string.Format( "{0:N0}", m_nEnd ); }			
		}		

		[ Category( STR_CATEGORY_GROUP ), PropertyOrder( 13 ), Description( "Highest score" ) ]
		public double ScoreMax
		{
			get {			return GetScoreMax(); }			
		}

		[ Category( STR_CATEGORY_GROUP ), PropertyOrder( 14 ), Description( "Lowest score" ) ]
		public double ScoreMin
		{
			get {			return GetScoreMin(); }			
		}
	}
}
