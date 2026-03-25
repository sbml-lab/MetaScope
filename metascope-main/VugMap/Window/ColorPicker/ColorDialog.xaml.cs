using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using VugMap.Utility;
using VugMap.Utility.Data;
using VugMap.Utility.Error;
using VugMap.Utility.Logger;

namespace VugMap.Window.ColorPicker
{
    /// <summary>
    /// Holds a ColorPicker control, and exposes
    /// the ColorPicker.SelectedColor
    /// </summary>
    public partial class ColorDialog : System.Windows.Window
    {
        #region Ctor
        public ColorDialog()
        {
            InitializeComponent();
        }
        #endregion

        #region Public Properties
        public Color SelectedColor
        {
            get {	return colorPicker.SelectedColor; }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Closes the dialog on Enter key pressed
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Close();
            }
        }

        /// <summary>
        /// User is happy with choice
        /// </summary>
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>
        /// User is not happy with choice
        /// </summary>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        #endregion
    }
}
