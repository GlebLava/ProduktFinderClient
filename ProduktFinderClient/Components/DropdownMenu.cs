using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ProduktFinderClient.Components
{
    public class DropdownMenu : Control
    {
        #region DependenyProperties

        [TypeConverterAttribute(typeof(LengthConverter))]
        public new double Width
        {
            get
            {
                if (Visibility == System.Windows.Visibility.Hidden)
                {
                    return 0;
                }
                return base.Width;
            }
            set
            {
                base.Width = value;
            }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(DropdownMenu), new PropertyMetadata(""));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty
                = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(DropdownMenu), new PropertyMetadata(new CornerRadius() { BottomLeft = 0, BottomRight = 0, TopLeft = 0, TopRight = 0 }));

        public Brush HoverBackGround
        {
            get { return (Brush)GetValue(HoverBackGroundProperty); }
            set { SetValue(HoverBackGroundProperty, value); }
        }

        public static readonly DependencyProperty HoverBackGroundProperty =
                DependencyProperty.Register("HoverBackGround", typeof(Brush), typeof(DropdownMenu), new PropertyMetadata(Brushes.White));

        public Brush HoverForeGround
        {
            get { return (Brush)GetValue(HoverForeGroundProperty); }
            set { SetValue(HoverForeGroundProperty, value); }
        }

        public static readonly DependencyProperty HoverForeGroundProperty =
                DependencyProperty.Register("HoverForeGround", typeof(Brush), typeof(DropdownMenu), new PropertyMetadata(Brushes.White));




        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(DropdownMenu), new PropertyMetadata(null));



        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(DropdownMenu), new PropertyMetadata(null));


        public SelectionMode SelectionMode
        {
            get { return (SelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        public static readonly DependencyProperty SelectionModeProperty =
             DependencyProperty.Register("SelectionMode", typeof(SelectionMode), typeof(DropdownMenu), new PropertyMetadata(null));


        #endregion

        #region fields

        private bool isToggled = true;
        private Popup popup;

        #endregion

        static DropdownMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropdownMenu), new FrameworkPropertyMetadata(typeof(DropdownMenu)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.GetTemplateChild("PART_ToggleButton") is Button button)
            {
                button.Click += OnToggle;
            }

            if (this.GetTemplateChild("PART_Popup") is PopupNonTopmost popup)
            {
                this.popup = popup;
            }


            this.LayoutUpdated += OnLayoutUpdated;

            //LostFocus += OnLostFocus;
        }

        public void OnToggle(object sender, RoutedEventArgs e)
        {
            if (popup != null)
                popup.IsOpen = isToggled;

            isToggled = !isToggled;
        }

        public void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (popup != null)
            {
                popup.IsOpen = false;
                isToggled = true;
            }
        }

        public void ClosePopup()
        {
            if (popup != null)
            {
                popup.IsOpen = false;
                isToggled = true;
            }
        }

        private void OnLayoutUpdated(object? sender, EventArgs e)
        {
            if (popup.IsOpen)
            {
                var offset = popup.HorizontalOffset;
                popup.HorizontalOffset = offset + 1;
                popup.HorizontalOffset = offset;
            }
        }

    }

}









