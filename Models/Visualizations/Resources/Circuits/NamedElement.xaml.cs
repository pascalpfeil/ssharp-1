﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SafetySharp.CaseStudies.Visualizations.Resources.Circuits
{
	/// <summary>
	/// Interaktionslogik für LoadCircuitContact.xaml
	/// </summary>
	public partial class NamedElement : UserControl
	{
		public NamedElement()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Gets or sets the NameOfElement
		/// </summary>
		public string NameOfElement
		{
			get { return (string)GetValue(NameOfElementProperty); }
			set { SetValue(NameOfElementProperty, value); }
		}

		/// <summary>
		/// Identified the NameOfElement dependency property
		/// </summary>
		public static readonly DependencyProperty NameOfElementProperty =
			DependencyProperty.Register("NameOfElement", typeof(string),
			  typeof(NamedElement), new PropertyMetadata("?"));
	}
}