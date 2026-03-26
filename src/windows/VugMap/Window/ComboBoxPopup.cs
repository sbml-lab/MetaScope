using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;


namespace VugMap.Window
{
	public class ComboBoxPopup : ComboBox
	{
		public override void OnApplyTemplate()
        {
			base.OnApplyTemplate();

			var				vPopup			= ( Popup ) Template.FindName( "PART_Popup", this );
			
			//vPopup.Placement				= PlacementMode.Custom;
			//vPopup.PlacementTarget
			vPopup.CustomPopupPlacementCallback				= new CustomPopupPlacementCallback( PlacePopup );
			//vPopup.PlacementTarget			= this;
			//vPopup.Placement				= PlacementMode.Bottom;
        }

		public CustomPopupPlacement[] PlacePopup( Size szPopup, Size szTarget, Point ptOffset )
		{
			CustomPopupPlacement			cpp1			= new CustomPopupPlacement( new Point( -50, 100 ), PopupPrimaryAxis.Vertical );

			CustomPopupPlacement			cpp2			= new CustomPopupPlacement( new Point( 10, 20 ), PopupPrimaryAxis.Horizontal );

			CustomPopupPlacement[]			cppA			= new CustomPopupPlacement[] { cpp1, cpp2 };
			
			return cppA;
		}
	}
}
