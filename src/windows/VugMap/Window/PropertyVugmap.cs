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
	[ TypeConverter( typeof( PropertyGridOrderer ) ) ]
	class PropertyVugmap
	{
		//				.								.								.
		private			const string					STR_CATEGORY_WINDOW				= "Window";
		private			const string					STR_CATEGORY_DOCUMENT			= "Document";

		private			double							m_dVugmapScaleX					= 1.0f;
		private			double							m_dVugmapScaleY					= 1.0f;
		private			double							m_dDocumentScaleX				= 1.0f;
		private			double							m_dDocumentScaleY				= 1.0f;

		public PropertyVugmap()
		{			
		}

		[ Category( STR_CATEGORY_DOCUMENT ), PropertyOrder( 10 ) ]
		public double DocumentScaleX
		{
			get {	return m_dDocumentScaleX; }
			set {	m_dDocumentScaleX				= value; }													
		}

		[ Category( STR_CATEGORY_DOCUMENT ), PropertyOrder( 11 ) ]
		public double DocumentScaleY
		{
			get {	return m_dDocumentScaleY; }
			set {	m_dDocumentScaleY				= value; }													
		}
	
		[ Category( STR_CATEGORY_WINDOW ), PropertyOrder( 10 ), Description( "Main Window, Scale X" ) ]
		public double WindowScaleX
		{
			get {	return m_dVugmapScaleX; }
			set {	m_dVugmapScaleX					= value; }													
		}

		[ Category( STR_CATEGORY_WINDOW ), PropertyOrder( 11 ), Description( "Main Window, Scale X" ) ]
		public double WindowScaleY
		{
			get {	return m_dVugmapScaleY; }
			set {	m_dVugmapScaleY					= value; }													
		}
	}
}
