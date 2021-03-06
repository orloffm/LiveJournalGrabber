﻿using System.Windows;
using System.Windows.Controls;

namespace OrlovMikhail.LJ.Grabber.Client.Controls
{
    public sealed class StackPanelLayout : StackPanel
    {
        public static readonly DependencyProperty ChildMarginProperty = DependencyProperty.Register(
            "ChildMargin"
            , typeof(Thickness)
            , typeof(StackPanelLayout)
            , new FrameworkPropertyMetadata(new Thickness(8))
            {
                AffectsArrange = true, AffectsMeasure = true
            }
        );

        // The child margin defines a margin that will be automatically applied to all children of this StackPanel.
        // However, the children at the edges will have the respective margins remove. E.g. the leftmost children will have
        // a Margin.Left of 0 and the children in the first row will have a Margin.Top of 0.
        // The margins that are not set to 0 are set to half the ChildMargin's value, since it's neighbour will also apply it,
        // effectively doubling it.
        public Thickness ChildMargin
        {
            get => (Thickness) GetValue(ChildMarginProperty);
            set
            {
                SetValue(ChildMarginProperty, value);
                UpdateChildMargins();
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            UpdateChildMargins();
            return base.MeasureOverride(constraint);
        }

        private void UpdateChildMargins()
        {
            bool isHorizontal = Orientation == Orientation.Horizontal;
            bool isFirst = true;
            FrameworkElement lastOne = null;

            foreach (UIElement element in InternalChildren)
            {
                if (element.Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                FrameworkElement fe = element as FrameworkElement;
                if (null != fe)
                {
                    double factorLeft = isHorizontal ? 0.5 : 0;
                    double factorTop = isHorizontal ? 0 : 0.5;
                    double factorRight = isHorizontal ? 0.5 : 0;
                    double factorBottom = isHorizontal ? 0 : 0.5;

                    if (isFirst)
                    {
                        if (isHorizontal)
                        {
                            factorLeft = 0;
                        }
                        else
                        {
                            factorTop = 0;
                        }
                    }

                    fe.Margin = new Thickness(
                        ChildMargin.Left * factorLeft
                        , ChildMargin.Top * factorTop
                        , ChildMargin.Right * factorRight
                        , ChildMargin.Bottom * factorBottom
                    );
                    lastOne = fe;
                }

                isFirst = false;
            }

            if (lastOne != null)
            {
                if (isHorizontal)
                {
                    lastOne.Margin = new Thickness(lastOne.Margin.Left, 0, 0, 0);
                }
                else
                {
                    lastOne.Margin = new Thickness(0, lastOne.Margin.Top, 0, 0);
                }
            }
        }
    }
}