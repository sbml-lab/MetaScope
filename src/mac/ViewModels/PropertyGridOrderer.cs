using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MetaScope.ViewModels
{
	/// <summary>
	/// Orders properties by a PropertyOrderAttribute when used with
	/// TypeDescriptor/PropertyGrid patterns.  In Avalonia (which lacks
	/// a built-in PropertyGrid), this helper is retained so that any
	/// future ListBox-based property display can call GetProperties()
	/// and receive an ordered PropertyDescriptorCollection.
	/// </summary>
	public class PropertyGridOrderer : ExpandableObjectConverter
	{
		#region Methods
		public override bool GetPropertiesSupported( ITypeDescriptorContext context )
		{
			return true;
		}

		public override PropertyDescriptorCollection GetProperties( ITypeDescriptorContext context, object value, Attribute[] attributes )
		{
			//
			// This override returns a list of properties in order
			//
			PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties( value, attributes );
			ArrayList orderedProperties = new ArrayList();
			foreach( PropertyDescriptor pd in pdc )
			{
				Attribute attribute = pd.Attributes[ typeof( PropertyOrderAttribute ) ];
				if( attribute != null )
				{
					//
					// If the attribute is found, then create a pair object to hold it
					//
					PropertyOrderAttribute poa = (PropertyOrderAttribute)attribute;
					orderedProperties.Add( new PropertyOrderPair( pd.Name, poa.Order ) );
				}
				else
				{
					//
					// If no order attribute is specified then give it an order of 0
					//
					orderedProperties.Add( new PropertyOrderPair( pd.Name, 0 ) );
				}
			}
			//
			// Perform the actual order using the PropertyOrderPair classes
			// implementation of IComparable to sort
			//
			orderedProperties.Sort();
			//
			// Build a string list of the ordered names
			//
			ArrayList propertyNames = new ArrayList();
			foreach( PropertyOrderPair pop in orderedProperties )
			{
				propertyNames.Add( pop.Name );
			}
			//
			// Pass in the ordered list for the PropertyDescriptorCollection to sort by
			//
			return pdc.Sort( (string[])propertyNames.ToArray( typeof( string ) ) );
		}
		#endregion

		/// <summary>
		/// Returns an ordered list of (name, value, category, description) tuples
		/// for binding to a ListBox or ItemsControl in Avalonia.
		/// </summary>
		public static List< PropertyDisplayItem > GetOrderedItems( object obj )
		{
			var		result		= new List< PropertyDisplayItem >();

			PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties( obj );
			var		pairs		= new List< PropertyOrderPair >();

			foreach( PropertyDescriptor pd in pdc )
			{
				int		nOrder		= 0;
				var		poaAttr		= pd.Attributes[ typeof( PropertyOrderAttribute ) ] as PropertyOrderAttribute;
				if( poaAttr != null )
					nOrder			= poaAttr.Order;

				pairs.Add( new PropertyOrderPair( pd.Name, nOrder ) );
			}

			pairs.Sort();

			foreach( var pop in pairs )
			{
				PropertyDescriptor	pd		= pdc[ pop.Name ];
				if( pd == null )			continue;

				string		strCategory		= pd.Category ?? "";
				string		strDescription	= pd.Description ?? "";
				object		objValue		= null;
				try { objValue = pd.GetValue( obj ); } catch { }

				result.Add( new PropertyDisplayItem
				{
					Name		= pd.Name,
					Value		= objValue?.ToString() ?? "",
					Category	= strCategory,
					Description	= strDescription,
				} );
			}

			return result;
		}
	}

	/// <summary>
	/// Simple DTO for displaying a property row in a ListBox.
	/// </summary>
	public class PropertyDisplayItem
	{
		public string		Name			{ get; set; }
		public string		Value			{ get; set; }
		public string		Category		{ get; set; }
		public string		Description		{ get; set; }
	}

	#region Helper Class - PropertyOrderAttribute
	[AttributeUsage( AttributeTargets.Property )]
	public class PropertyOrderAttribute : Attribute
	{
		//
		// Simple attribute to allow the order of a property to be specified
		//
		private		int		m_nOrder;

		public PropertyOrderAttribute( int order )
		{
			m_nOrder = order;
		}

		public int Order
		{
			get
			{
				return m_nOrder;
			}
		}
	}
	#endregion

	#region Helper Class - PropertyOrderPair
	public class PropertyOrderPair : IComparable
	{
		private		int		m_nOrder;
		private		string	m_strName;

		public string Name
		{
			get
			{
				return m_strName;
			}
		}

		public PropertyOrderPair( string name, int order )
		{
			m_nOrder	= order;
			m_strName	= name;
		}

		public int CompareTo( object obj )
		{
			//
			// Sort the pair objects by ordering by order value
			// Equal values get the same rank
			//
			int otherOrder = ((PropertyOrderPair)obj).m_nOrder;
			if( otherOrder == m_nOrder )
			{
				//
				// If order not specified, sort by name
				//
				string otherName = ((PropertyOrderPair)obj).m_strName;
				return string.Compare( m_strName, otherName );
			}
			else if( otherOrder > m_nOrder )
			{
				return -1;
			}
			return 1;
		}
	}
	#endregion
}
