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
	using		DicAttribute					= Dictionary< string, string >;

	[ TypeConverter( typeof( PropertyGridOrderer ) ) ]
	public class PropertyFeature
	{
		//				.								.								.
		public			const string					STR_CATEGORY_FEATURE			= "Feature";
					
		public static PropertyFeature BuildProperty( DataFeature df )
		{
			DicAttribute	dic				= df.DoAttributeParse();
			if( dic == null )
			{
				return null;
			}

			// create a dynamic assembly and mb 
			AssemblyName	an				= new AssemblyName();
			an.Name							= "PropertyFeatureExtension";
			AssemblyBuilder	ab				= AppDomain.CurrentDomain.DefineDynamicAssembly( an, AssemblyBuilderAccess.RunAndSave );			
			ModuleBuilder	mb				= ab.DefineDynamicModule( "PropertyFeatureModule" );
			
			// create a new type builder
			string			strClass		= string.Format( "PropertyFeatureExtendsion" );
			TypeBuilder		tb				= mb.DefineType( strClass, TypeAttributes.Public | TypeAttributes.Class );
			tb.SetParent( typeof( PropertyFeature ) );
						
			foreach( KeyValuePair< string, string > kv in dic )
			{
				if( kv.Key == "" )				continue;

				// Generate a private field
				FieldBuilder	fb				= tb.DefineField( "_" + kv.Key, typeof( string ), FieldAttributes.Private );			
				PropertyBuilder	pb				= tb.DefineProperty( kv.Key, PropertyAttributes.None, typeof( string ), new Type[] { typeof( string ) } );

				// The property set and property get methods require a special set of attributes:
				MethodAttributes attGetSet		= MethodAttributes.Public | MethodAttributes.HideBySig;

				// Define the "get" accessor method for current private fb.
				MethodBuilder	mbGet			= tb.DefineMethod( "get_value", attGetSet, typeof( string ), Type.EmptyTypes );

				// Intermediate Language stuff...
				ILGenerator		ilgGet			= mbGet.GetILGenerator();
				ilgGet.Emit( OpCodes.Ldarg_0 );
				ilgGet.Emit( OpCodes.Ldfld, fb );
				ilgGet.Emit( OpCodes.Ret );

				// Define the "set" accessor method for current private fb.
				MethodBuilder	mbSet			= tb.DefineMethod( "set_value", attGetSet, null, new Type[] { typeof(string) } );

				// Again some Intermediate Language stuff...
				ILGenerator		ilgSet			= mbSet.GetILGenerator();
				ilgSet.Emit( OpCodes.Ldarg_0 );
				ilgSet.Emit( OpCodes.Ldarg_1 );
				ilgSet.Emit( OpCodes.Stfld, fb );
				ilgSet.Emit( OpCodes.Ret );

				// Last, we must map the two methods created above to our PropertyBuilder to 
				// their corresponding behaviors, "get" and "set" respectively. 
				pb.SetGetMethod( mbGet );
				pb.SetSetMethod( mbSet );				

				//ConstructorInfo	ctor			= tb.GetConstructor( Type.EmptyTypes ); // default ctor
				//CustomAttributeBuilder cab		= new CustomAttributeBuilder( ctor, null, new PropertyInfo[]{ new PropertyInfo( "Category", "Attribute" );						
				//pb.SetCustomAttribute( cab );
			}			
			
			Type			type			= tb.CreateType();

			// Now we have our type. Let's create an instance from it:			
			PropertyFeature	pf				= Activator.CreateInstance( type ) as PropertyFeature;

			pf.SetFeature( df );

			foreach( KeyValuePair< string, string > kv in dic )
			{		
				if( kv.Key == "" )				continue;

				PropertyInfo	pi				= type.GetProperty( kv.Key );				
				pi.SetValue( pf, kv.Value, null );

				//TypeDescriptor.AddAttributes( , new CategoryAttribute( "Attribute" ) );				
			}

			return pf;
		}

		private			DataFeature						m_df							= null;
		
		public PropertyFeature()
		{
		}

		public PropertyFeature( DataFeature df )
		{
			m_df			= df;
		}

		public void SetFeature( DataFeature df )
		{
			m_df			= df;
		}

		[ Category( STR_CATEGORY_FEATURE ), PropertyOrder( 10 ), Description( "Source of the feature" ) ]
		public string Source
		{
			get {			return m_df.Source; }			
		}

		[ Category( STR_CATEGORY_FEATURE ), PropertyOrder( 11 ), Description( "Starting position" ) ]
		public string Start
		{
			get {			return string.Format( "{0:N0}", m_df.Start ); }			
		}

		[ Category( STR_CATEGORY_FEATURE ), PropertyOrder( 12 ) ]
		public string End
		{
			get {			return string.Format( "{0:N0}", m_df.End );; }			
		}

		[ Category( STR_CATEGORY_FEATURE ), PropertyOrder( 13 ) ]
		public string Score
		{
			get {			return m_df.ScoreString; }			
		}

		[ Category( STR_CATEGORY_FEATURE ), PropertyOrder( 14 ) ]
		public string Strand
		{
			get {			return m_df.Strand; }
		}

		[ Category( STR_CATEGORY_FEATURE ), PropertyOrder( 15 ) ]
		public string Phase
		{
			get {			return m_df.Phase; }			
		}

		[ Category( STR_CATEGORY_FEATURE ), PropertyOrder( 16 ) ]
		public string Attribute
		{
			get {			return m_df.Attribute; }			
		}
	}
}
