// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

// This is a very special primitive control that works around a limitation in
// the core animation subsystem of Silverlight: there is no way to declare in
// VSM states relative properties, such as animating from 0 to 33% the width of
// the control, using double animations for translation.
//
// It's a tough problem to solve property, but this primitive, unsupported
// control does offer a solution based on magic numbers that still allows a
// designer to make alterations to their animation values to present their 
// vision for custom templates.
//
// This is instrumental in offering a Windows Phone ProgressBar implementation
// that uses the render thread instead of animating UI thread-only properties.
//
// For questions, please see
// http://www.jeff.wilcox.name/performanceprogressbar/
//
// This control is licensed Ms-PL and as such comes with no warranties or
// official support.
//
// Style Note
// - - -
// The style that must be used with this is present at the bottom of this file.
//

namespace Microsoft.Phone.Controls.Unsupported
{
    /// <summary>
    /// A very specialized primitive control that works around a specific visual
    /// state manager issue. The platform does not support relative sized
    /// translation values, and this special control walks through visual state
    /// animation storyboards looking for magic numbers to use as percentages.
    /// This control is not supported, unofficial, and is a hack in many ways.
    /// It is used to enable a Windows Phone native platform-style progress bar
    /// experience in indeterminate mode that remains performant.
    /// </summary>
    public class RelativeAnimatingContentControl : ContentControl
    {
        /// <summary>
        /// A simple Epsilon-style value used for trying to determine the magic 
        /// state, if any, of a double.
        /// </summary>
        private const double SimpleDoubleComparisonEpsilon = 0.000009;

        /// <summary>
        /// The last known width of the control.
        /// </summary>
        private double _knownWidth;

        /// <summary>
        /// The last known height of the control.
        /// </summary>
        private double _knownHeight;

        /// <summary>
        /// A set of custom animation adapters used to update the animation
        /// storyboards when the size of the control changes.
        /// </summary>
        private List<AnimationValueAdapter> _specialAnimations;

        /// <summary>
        /// Initializes a new instance of the RelativeAnimatingContentControl
        /// type.
        /// </summary>
        public RelativeAnimatingContentControl()
        {
            SizeChanged += OnSizeChanged;
        }

        /// <summary>
        /// Handles the size changed event.
        /// </summary>
        /// <param name="name">The source object.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e != null && e.NewSize.Height > 0 && e.NewSize.Width > 0)
            {
                _knownWidth = e.NewSize.Width;
                _knownHeight = e.NewSize.Height;

                Clip = new RectangleGeometry { Rect = new Rect(0, 0, _knownWidth, _knownHeight), };

                UpdateAnyAnimationValues();
            }
        }

        /// <summary>
        /// Walks through the known storyboards in the control's template that
        /// may contain magic double animation values, storing them for future
        /// use and updates.
        /// </summary>
        private void UpdateAnyAnimationValues()
        {
            if (_knownHeight > 0 && _knownWidth > 0)
            {
                // Initially, before any special animations have been found,
                // the visual state groups of the control must be explored. 
                // By definition they must be at the implementation root of the
                // control, and this is designed to not walk into any other
                // depth.
                if (_specialAnimations == null)
                {
                    _specialAnimations = new List<AnimationValueAdapter>();

                    foreach (VisualStateGroup group in VisualStateManager.GetVisualStateGroups(this))
                    {
                        if (group == null)
                        {
                            continue;
                        }
                        foreach (VisualState state in group.States)
                        {
                            if (state != null)
                            {
                                Storyboard sb = state.Storyboard;
                                if (sb != null)
                                {
                                    // Examine all children of the storyboards,
                                    // looking for either type of double
                                    // animation.
                                    foreach (Timeline timeline in sb.Children)
                                    {
                                        DoubleAnimation da = timeline as DoubleAnimation;
                                        DoubleAnimationUsingKeyFrames dakeys = timeline as DoubleAnimationUsingKeyFrames;
                                        if (da != null)
                                        {
                                            ProcessDoubleAnimation(da);
                                        }
                                        else if (dakeys != null)
                                        {
                                            ProcessDoubleAnimationWithKeys(dakeys);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Update special animation values relative to the current size.
                UpdateKnownAnimations();
            }
        }

        /// <summary>
        /// Walks through all special animations, updating based on the current
        /// size of the control.
        /// </summary>
        private void UpdateKnownAnimations()
        {
            foreach (AnimationValueAdapter adapter in _specialAnimations)
            {
                adapter.UpdateWithNewDimension(_knownWidth, _knownHeight);
            }
        }

        /// <summary>
        /// Processes a double animation with keyframes, looking for known 
        /// special values to store with an adapter.
        /// </summary>
        /// <param name="da">The double animation using key frames instance.</param>
        private void ProcessDoubleAnimationWithKeys(DoubleAnimationUsingKeyFrames da)
        {
            // Look through all keyframes in the instance.
            foreach (DoubleKeyFrame frame in da.KeyFrames)
            {
                var d = DoubleAnimationFrameAdapter.GetDimensionFromMagicNumber(frame.Value);
                if (d.HasValue)
                {
                    _specialAnimations.Add(new DoubleAnimationFrameAdapter(d.Value, frame));
                }
            }
        }

        /// <summary>
        /// Processes a double animation looking for special values.
        /// </summary>
        /// <param name="da">The double animation instance.</param>
        private void ProcessDoubleAnimation(DoubleAnimation da)
        {
            // Look for a special value in the To property.
            if (da.To.HasValue)
            {
                var d = DoubleAnimationToAdapter.GetDimensionFromMagicNumber(da.To.Value);
                if (d.HasValue)
                {
                    _specialAnimations.Add(new DoubleAnimationToAdapter(d.Value, da));
                }
            }

            // Look for a special value in the From property.
            if (da.From.HasValue)
            {
                var d = DoubleAnimationFromAdapter.GetDimensionFromMagicNumber(da.To.Value);
                if (d.HasValue)
                {
                    _specialAnimations.Add(new DoubleAnimationFromAdapter(d.Value, da));
                }
            }
        }

        #region Private animation updating system
        /// <summary>
        /// A selection of dimensions of interest for updating an animation.
        /// </summary>
        private enum DoubleAnimationDimension
        {
            /// <summary>
            /// The width (horizontal) dimension.
            /// </summary>
            Width,

            /// <summary>
            /// The height (vertical) dimension.
            /// </summary>
            Height,
        }

        /// <summary>
        /// A simple class designed to store information about a specific 
        /// animation instance and its properties. Able to update the values at
        /// runtime.
        /// </summary>
        private abstract class AnimationValueAdapter
        {
            /// <summary>
            /// Gets or sets the original double value.
            /// </summary>
            protected double OriginalValue { get; set; }

            /// <summary>
            /// Initializes a new instance of the AnimationValueAdapter type.
            /// </summary>
            /// <param name="dimension">The dimension of interest for updates.</param>
            public AnimationValueAdapter(DoubleAnimationDimension dimension)
            {
                Dimension = dimension;
            }

            /// <summary>
            /// Gets the dimension of interest for the control.
            /// </summary>
            public DoubleAnimationDimension Dimension { get; private set; }

            /// <summary>
            /// Updates the original instance based on new dimension information
            /// from the control. Takes both and allows the subclass to make the
            /// decision on which ratio, values, and dimension to use.
            /// </summary>
            /// <param name="width">The width of the control.</param>
            /// <param name="height">The height of the control.</param>
            public abstract void UpdateWithNewDimension(double width, double height);
        }

        private abstract class GeneralAnimationValueAdapter<T> : AnimationValueAdapter
        {
            /// <summary>
            /// Stores the animation instance.
            /// </summary>
            protected T Instance { get; set; }

            /// <summary>
            /// Gets the value of the underlying property of interest.
            /// </summary>
            /// <returns>Returns the value of the property.</returns>
            protected abstract double GetValue();

            /// <summary>
            /// Sets the value for the underlying property of interest.
            /// </summary>
            /// <param name="newValue">The new value for the property.</param>
            protected abstract void SetValue(double newValue);

            /// <summary>
            /// Gets the initial value (minus the magic number portion) that the
            /// designer stored within the visual state animation property.
            /// </summary>
            protected double InitialValue { get; private set; }

            /// <summary>
            /// The ratio based on the original magic value, used for computing
            /// the updated animation property of interest when the size of the
            /// control changes.
            /// </summary>
            private double _ratio;

            /// <summary>
            /// Initializes a new instance of the GeneralAnimationValueAdapter
            /// type.
            /// </summary>
            /// <param name="d">The dimension of interest.</param>
            /// <param name="instance">The animation type instance.</param>
            public GeneralAnimationValueAdapter(DoubleAnimationDimension d, T instance)
                : base(d)
            {
                Instance = instance;

                InitialValue = StripMagicNumberOff(GetValue());
                _ratio = InitialValue / 100;
            }

            /// <summary>
            /// Approximately removes the magic number state from a value.
            /// </summary>
            /// <param name="number">The initial number.</param>
            /// <returns>Returns a double with an adjustment for the magic
            /// portion of the number.</returns>
            public double StripMagicNumberOff(double number)
            {
                return Dimension == DoubleAnimationDimension.Width ? number - .1 : number - .2;
            }

            /// <summary>
            /// Retrieves the dimension, if any, from the number. If the number
            /// is not magic, null is returned instead.
            /// </summary>
            /// <param name="number">The double value.</param>
            /// <returns>Returs a double animation dimension, if the number was
            /// partially magic; otherwise, returns null.</returns>
            public static DoubleAnimationDimension? GetDimensionFromMagicNumber(double number)
            {
                double floor = Math.Floor(number);
                double remainder = number - floor;

                if (remainder >= .1 - SimpleDoubleComparisonEpsilon && remainder <= .1 + SimpleDoubleComparisonEpsilon)
                {
                    return DoubleAnimationDimension.Width;
                }
                if (remainder >= .2 - SimpleDoubleComparisonEpsilon && remainder <= .2 + SimpleDoubleComparisonEpsilon)
                {
                    return DoubleAnimationDimension.Height;
                }
                return null;
            }

            /// <summary>
            /// Updates the animation instance based on the dimensions of the
            /// control.
            /// </summary>
            /// <param name="width">The width of the control.</param>
            /// <param name="height">The height of the control.</param>
            public override void UpdateWithNewDimension(double width, double height)
            {
                double size = Dimension == DoubleAnimationDimension.Width ? width : height;
                UpdateValue(size);
            }

            /// <summary>
            /// Updates the value of the property.
            /// </summary>
            /// <param name="sizeToUse">The size of interest to use with a ratio
            /// computation.</param>
            private void UpdateValue(double sizeToUse)
            {
                SetValue(sizeToUse * _ratio);
            }
        }

        /// <summary>
        /// Adapter for DoubleAnimation's To property.
        /// </summary>
        private class DoubleAnimationToAdapter : GeneralAnimationValueAdapter<DoubleAnimation>
        {
            /// <summary>
            /// Gets the value of the underlying property of interest.
            /// </summary>
            /// <returns>Returns the value of the property.</returns>
            protected override double GetValue()
            {
                return (double)Instance.To;
            }

            /// <summary>
            /// Sets the value for the underlying property of interest.
            /// </summary>
            /// <param name="newValue">The new value for the property.</param>
            protected override void SetValue(double newValue)
            {
                Instance.To = newValue;
            }

            /// <summary>
            /// Initializes a new instance of the DoubleAnimationToAdapter type.
            /// </summary>
            /// <param name="dimension">The dimension of interest.</param>
            /// <param name="instance">The instance of the animation type.</param>
            public DoubleAnimationToAdapter(DoubleAnimationDimension dimension, DoubleAnimation instance)
                : base(dimension, instance)
            {
            }
        }

        /// <summary>
        /// Adapter for DoubleAnimation's From property.
        /// </summary>
        private class DoubleAnimationFromAdapter : GeneralAnimationValueAdapter<DoubleAnimation>
        {
            /// <summary>
            /// Gets the value of the underlying property of interest.
            /// </summary>
            /// <returns>Returns the value of the property.</returns>
            protected override double GetValue()
            {
                return (double)Instance.From;
            }

            /// <summary>
            /// Sets the value for the underlying property of interest.
            /// </summary>
            /// <param name="newValue">The new value for the property.</param>
            protected override void SetValue(double newValue)
            {
                Instance.From = newValue;
            }

            /// <summary>
            /// Initializes a new instance of the DoubleAnimationFromAdapter 
            /// type.
            /// </summary>
            /// <param name="dimension">The dimension of interest.</param>
            /// <param name="instance">The instance of the animation type.</param>
            public DoubleAnimationFromAdapter(DoubleAnimationDimension dimension, DoubleAnimation instance)
                : base(dimension, instance)
            {
            }
        }

        /// <summary>
        /// Adapter for double key frames.
        /// </summary>
        private class DoubleAnimationFrameAdapter : GeneralAnimationValueAdapter<DoubleKeyFrame>
        {
            /// <summary>
            /// Gets the value of the underlying property of interest.
            /// </summary>
            /// <returns>Returns the value of the property.</returns>
            protected override double GetValue()
            {
                return Instance.Value;
            }

            /// <summary>
            /// Sets the value for the underlying property of interest.
            /// </summary>
            /// <param name="newValue">The new value for the property.</param>
            protected override void SetValue(double newValue)
            {
                Instance.Value = newValue;
            }

            /// <summary>
            /// Initializes a new instance of the DoubleAnimationFrameAdapter
            /// type.
            /// </summary>
            /// <param name="dimension">The dimension of interest.</param>
            /// <param name="instance">The instance of the animation type.</param>
            public DoubleAnimationFrameAdapter(DoubleAnimationDimension dimension, DoubleKeyFrame frame)
                : base(dimension, frame)
            {
            }
        }
        #endregion
    }

    /*
    This is the style that should be used with the control. Make sure to define
    the XMLNS at the top of the style file similar to this:
    xmlns:unsupported="clr-namespace:Microsoft.Phone.Controls.Unsupported"

    <!--
// Performance Progress Bar
// - - -
// To use this progress bar at runtime, make sure to set the Style
// value to this key. Since the control visually is identical to control
// in the Windows Phone runtime, you will not be able to visually tell
// the difference: except this style will not use the UI thread at
// runtime when IsIndeterminate=true.
// 
// <ProgressBar Style="{StaticResource PerformanceProgressBar}"
//              IsIndeterminate="true"/>
//
-->
<Style x:Key="PerformanceProgressBar" TargetType="ProgressBar">
    <Setter Property="Foreground" Value="{StaticResource PhoneAccentBrush}"/>
    <Setter Property="Background" Value="{StaticResource PhoneAccentBrush}"/>
    <Setter Property="Maximum" Value="100"/>
    <Setter Property="IsHitTestVisible" Value="False"/>
    <Setter Property="Padding" Value="{StaticResource PhoneHorizontalMargin}"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="ProgressBar">
                <unsupported:RelativeAnimatingContentControl HorizontalContentAlignment="Stretch" VerticalContentAlignment="Stretch">
                    <unsupported:RelativeAnimatingContentControl.Resources>
                        <ExponentialEase EasingMode="EaseOut" Exponent="1" x:Key="ProgressBarEaseOut"/>
                        <ExponentialEase EasingMode="EaseIn" Exponent="1" x:Key="ProgressBarEaseIn"/>
                    </unsupported:RelativeAnimatingContentControl.Resources>
                    <VisualStateManager.VisualStateGroups>
                        <VisualStateGroup x:Name="CommonStates">
                            <VisualState x:Name="Determinate"/>
                            <VisualState x:Name="Indeterminate">
                                <Storyboard RepeatBehavior="Forever" Duration="00:00:04.4">
                                    <ObjectAnimationUsingKeyFrames Storyboard.TargetProperty="Visibility" Storyboard.TargetName="IndeterminateRoot">
                                        <DiscreteObjectKeyFrame KeyTime="0">
                                            <DiscreteObjectKeyFrame.Value>
                                                <Visibility>Visible</Visibility>
                                            </DiscreteObjectKeyFrame.Value>
                                        </DiscreteObjectKeyFrame>
                                    </ObjectAnimationUsingKeyFrames>
                                    <ObjectAnimationUsingKeyFrames Storyboard.TargetProperty="Visibility" Storyboard.TargetName="DeterminateRoot">
                                        <DiscreteObjectKeyFrame KeyTime="0">
                                            <DiscreteObjectKeyFrame.Value>
                                                <Visibility>Collapsed</Visibility>
                                            </DiscreteObjectKeyFrame.Value>
                                        </DiscreteObjectKeyFrame>
                                    </ObjectAnimationUsingKeyFrames>
                                    <DoubleAnimationUsingKeyFrames BeginTime="00:00:00.0" Storyboard.TargetProperty="X" Storyboard.TargetName="R1TT">
                                        <LinearDoubleKeyFrame KeyTime="00:00:00.0" Value="0.1"/>
                                        <EasingDoubleKeyFrame KeyTime="00:00:00.5" Value="33.1" EasingFunction="{StaticResource ProgressBarEaseOut}"/>
                                        <LinearDoubleKeyFrame KeyTime="00:00:02.0" Value="66.1"/>
                                        <EasingDoubleKeyFrame KeyTime="00:00:02.5" Value="100.1" EasingFunction="{StaticResource ProgressBarEaseIn}"/>
                                    </DoubleAnimationUsingKeyFrames>
                                    <DoubleAnimationUsingKeyFrames BeginTime="00:00:00.2" Storyboard.TargetProperty="X" Storyboard.TargetName="R2TT">
                                        <LinearDoubleKeyFrame KeyTime="00:00:00.0" Value="0.1"/>
                                        <EasingDoubleKeyFrame KeyTime="00:00:00.5" Value="33.1" EasingFunction="{StaticResource ProgressBarEaseOut}"/>
                                        <LinearDoubleKeyFrame KeyTime="00:00:02.0" Value="66.1"/>
                                        <EasingDoubleKeyFrame KeyTime="00:00:02.5" Value="100.1" EasingFunction="{StaticResource ProgressBarEaseIn}"/>
                                    </DoubleAnimationUsingKeyFrames>
                                    <DoubleAnimationUsingKeyFrames BeginTime="00:00:00.4" Storyboard.TargetProperty="X" Storyboard.TargetName="R3TT">
                                        <LinearDoubleKeyFrame KeyTime="00:00:00.0" Value="0.1"/>
                                        <EasingDoubleKeyFrame KeyTime="00:00:00.5" Value="33.1" EasingFunction="{StaticResource ProgressBarEaseOut}"/>
                                        <LinearDoubleKeyFrame KeyTime="00:00:02.0" Value="66.1"/>
                                        <EasingDoubleKeyFrame KeyTime="00:00:02.5" Value="100.1" EasingFunction="{StaticResource ProgressBarEaseIn}"/>
                                    </DoubleAnimationUsingKeyFrames>
                                    <DoubleAnimationUsingKeyFrames BeginTime="00:00:00.6" Storyboard.TargetProperty="X" Storyboard.TargetName="R4TT">
                                        <LinearDoubleKeyFrame KeyTime="00:00:00.0" Value="0.1"/>
                                        <EasingDoubleKeyFrame KeyTime="00:00:00.5" Value="33.1" EasingFunction="{StaticResource ProgressBarEaseOut}"/>
                                        <LinearDoubleKeyFrame KeyTime="00:00:02.0" Value="66.1"/>
                                        <EasingDoubleKeyFrame KeyTime="00:00:02.5" Value="100.1" EasingFunction="{StaticResource ProgressBarEaseIn}"/>
                                    </DoubleAnimationUsingKeyFrames>
                                    <DoubleAnimationUsingKeyFrames BeginTime="00:00:00.8" Storyboard.TargetProperty="X" Storyboard.TargetName="R5TT">
                                        <LinearDoubleKeyFrame KeyTime="00:00:00.0" Value="0.1"/>
                                        <EasingDoubleKeyFrame KeyTime="00:00:00.5" Value="33.1" EasingFunction="{StaticResource ProgressBarEaseOut}"/>
                                        <LinearDoubleKeyFrame KeyTime="00:00:02.0" Value="66.1"/>
                                        <EasingDoubleKeyFrame KeyTime="00:00:02.5" Value="100.1" EasingFunction="{StaticResource ProgressBarEaseIn}"/>
                                    </DoubleAnimationUsingKeyFrames>
                                    <DoubleAnimationUsingKeyFrames BeginTime="00:00:00.0" Storyboard.TargetProperty="Opacity" Storyboard.TargetName="R1">
                                        <DiscreteDoubleKeyFrame KeyTime="0" Value="1"/>
                                        <DiscreteDoubleKeyFrame KeyTime="00:00:02.5" Value="0"/>
                                    </DoubleAnimationUsingKeyFrames>
                                    <DoubleAnimationUsingKeyFrames BeginTime="00:00:00.2" Storyboard.TargetProperty="Opacity" Storyboard.TargetName="R2">
                                        <DiscreteDoubleKeyFrame KeyTime="0" Value="1"/>
                                        <DiscreteDoubleKeyFrame KeyTime="00:00:02.5" Value="0"/>
                                    </DoubleAnimationUsingKeyFrames>
                                    <DoubleAnimationUsingKeyFrames BeginTime="00:00:00.4" Storyboard.TargetProperty="Opacity" Storyboard.TargetName="R3">
                                        <DiscreteDoubleKeyFrame KeyTime="0" Value="1"/>
                                        <DiscreteDoubleKeyFrame KeyTime="00:00:02.5" Value="0"/>
                                    </DoubleAnimationUsingKeyFrames>
                                    <DoubleAnimationUsingKeyFrames BeginTime="00:00:00.6" Storyboard.TargetProperty="Opacity" Storyboard.TargetName="R4">
                                        <DiscreteDoubleKeyFrame KeyTime="0" Value="1"/>
                                        <DiscreteDoubleKeyFrame KeyTime="00:00:02.5" Value="0"/>
                                    </DoubleAnimationUsingKeyFrames>
                                    <DoubleAnimationUsingKeyFrames BeginTime="00:00:00.8" Storyboard.TargetProperty="Opacity" Storyboard.TargetName="R5">
                                        <DiscreteDoubleKeyFrame KeyTime="0" Value="1"/>
                                        <DiscreteDoubleKeyFrame KeyTime="00:00:02.5" Value="0"/>
                                    </DoubleAnimationUsingKeyFrames>
                                </Storyboard>
                            </VisualState>
                        </VisualStateGroup>
                    </VisualStateManager.VisualStateGroups>
                    <Grid>
                        <Grid x:Name="DeterminateRoot" Margin="{TemplateBinding Padding}" Visibility="Visible">
                            <Rectangle x:Name="ProgressBarTrack" Fill="{TemplateBinding Background}" Height="4" Opacity="0.1"/>
                            <Rectangle x:Name="ProgressBarIndicator" Fill="{TemplateBinding Foreground}" HorizontalAlignment="Left" Height="4"/>
                        </Grid>
                        <Border x:Name="IndeterminateRoot" Margin="{TemplateBinding Padding}" Visibility="Collapsed">
                            <Grid HorizontalAlignment="Left">
                                <Rectangle Fill="{TemplateBinding Foreground}" Height="4" IsHitTestVisible="False" Width="4" x:Name="R1" Opacity="0" CacheMode="BitmapCache">
                                    <Rectangle.RenderTransform>
                                        <TranslateTransform x:Name="R1TT"/>
                                    </Rectangle.RenderTransform>
                                </Rectangle>
                                <Rectangle Fill="{TemplateBinding Foreground}" Height="4" IsHitTestVisible="False" Width="4" x:Name="R2" Opacity="0" CacheMode="BitmapCache">
                                    <Rectangle.RenderTransform>
                                        <TranslateTransform x:Name="R2TT"/>
                                    </Rectangle.RenderTransform>
                                </Rectangle>
                                <Rectangle Fill="{TemplateBinding Foreground}" Height="4" IsHitTestVisible="False" Width="4" x:Name="R3" Opacity="0" CacheMode="BitmapCache">
                                    <Rectangle.RenderTransform>
                                        <TranslateTransform x:Name="R3TT"/>
                                    </Rectangle.RenderTransform>
                                </Rectangle>
                                <Rectangle Fill="{TemplateBinding Foreground}" Height="4" IsHitTestVisible="False" Width="4" x:Name="R4" Opacity="0" CacheMode="BitmapCache">
                                    <Rectangle.RenderTransform>
                                        <TranslateTransform x:Name="R4TT"/>
                                    </Rectangle.RenderTransform>
                                </Rectangle>
                                <Rectangle Fill="{TemplateBinding Foreground}" Height="4" IsHitTestVisible="False" Width="4" x:Name="R5" Opacity="0" CacheMode="BitmapCache">
                                    <Rectangle.RenderTransform>
                                        <TranslateTransform x:Name="R5TT"/>
                                    </Rectangle.RenderTransform>
                                </Rectangle>
                            </Grid>
                        </Border>
                    </Grid>
                </unsupported:RelativeAnimatingContentControl>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
     */
}

