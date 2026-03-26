using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace MetaScope.Controls
{
	/// <summary>
	/// In WPF v1.1.11, ComboBoxPopup overrode popup placement via
	/// CustomPopupPlacementCallback. Avalonia uses standard placement.
	/// </summary>
	public class ComboBoxPopup : ComboBox
	{
		public ComboBoxPopup()
		{
		}

		protected override void OnApplyTemplate( TemplateAppliedEventArgs e )
		{
			base.OnApplyTemplate( e );
		}
	}
}
