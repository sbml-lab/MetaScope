using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using MetaScope.Models;

namespace MetaScope.ViewModels
{
	using				ListFeature						= List< DataFeature >;

	[TypeConverter( typeof( PropertyGridOrderer ) )]
	public class PropertyFeatureSelected
	{
		//				.								.								.
		public			const string					STR_CATEGORY_FEATURE			= "Feature";
		public			const string					STR_CATEGORY_STATISTICS			= " Statistics";

		private			ListFeature						m_lstFeature					= null;

		public PropertyFeatureSelected()
		{
			m_lstFeature	= new ListFeature();
		}

		public void SetFeature( DataFeature df )
		{
			Debug.Assert( m_lstFeature != null );

			m_lstFeature.Clear();
			m_lstFeature.Add( df );
		}

		public void SetFeature( ListFeature lst )
		{
			m_lstFeature	= lst;
		}

		[Category( STR_CATEGORY_STATISTICS ), PropertyOrder( 1 ), Description( "Number of features selected" )]
		public string Count
		{
			get
			{
				int				nCount			= m_lstFeature.Count;

				return string.Format( "{0:N0}", nCount );
			}
		}

		[Category( STR_CATEGORY_FEATURE ), PropertyOrder( 10 ), Description( "Starting position" )]
		public string Start
		{
			get
			{
				if( m_lstFeature.Count == 0 )
				{
					return "0";
				}
				else
				{
					DataFeature		df				= m_lstFeature.First();
					return string.Format( "{0:N0}", df.Start );
				}
			}
		}

		[Category( STR_CATEGORY_FEATURE ), PropertyOrder( 11 )]
		public string End
		{
			get
			{
				if( m_lstFeature.Count == 0 )
				{
					return "0";
				}
				else
				{
					DataFeature		df				= m_lstFeature.Last();
					return string.Format( "{0:N0}", df.End );
				}
			}
		}

		[Category( STR_CATEGORY_FEATURE ), PropertyOrder( 12 )]
		public string Score
		{
			get
			{
				if( m_lstFeature.Count == 0 )
				{
					return "0.0";
				}
				else
				{
					DataFeature		df				= m_lstFeature.First();
					return string.Format( "{0:N0}", df.Score );
				}
			}
		}

		[Category( STR_CATEGORY_FEATURE ), PropertyOrder( 13 )]
		public string Strand
		{
			get
			{
				if( m_lstFeature.Count == 0 )
				{
					return null;
				}
				else
				{
					DataFeature		df				= m_lstFeature.First();
					return df.Strand;
				}
			}
		}

		[Category( STR_CATEGORY_FEATURE ), PropertyOrder( 14 )]
		public string Phase
		{
			get
			{
				if( m_lstFeature.Count == 0 )
				{
					return ".";
				}
				else
				{
					DataFeature		df				= m_lstFeature.First();
					return df.Phase;
				}
			}
		}

		[Category( STR_CATEGORY_FEATURE ), PropertyOrder( 15 )]
		public string Attribute
		{
			get
			{
				if( m_lstFeature.Count == 0 )
				{
					return null;
				}
				else
				{
					DataFeature		df				= m_lstFeature.First();
					return df.Attribute;
				}
			}
		}
	}
}
