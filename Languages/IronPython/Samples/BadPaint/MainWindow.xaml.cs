/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Timers;
using System.Windows.Media.Animation;
using Microsoft.Scripting.Hosting;
using System.Dynamic;

namespace BadPaint {
    public partial class MainWindow : Window {
        private readonly ScriptRuntime _runtime = ScriptRuntime.CreateFromConfiguration();
        private ScriptScope _scope;

        public MainWindow() {
            InitializeComponent();

            // setup the timer for when we run the scripts
            _timer.Elapsed += TimerElapsed;
            _timer.Enabled = true;
            _timer.Start();
            InitializeHosting();
            _canvas.LayoutUpdated += _canvas_LayoutUpdated;
            _scope = _runtime.CreateScope();
            _scope.SetVariable("Application", new DynamicAPI(this));

            _runtime.LoadAssembly(typeof(Canvas).Assembly);
            _runtime.LoadAssembly(typeof(Brushes).Assembly);
            _runtime.LoadAssembly(GetType().Assembly);
        }

        /// <summary>
        /// Gets the languages registered with the ScriptRuntime
        /// </summary>
        public IEnumerable<string> LanguageNames {
            get {
                return from setup in _runtime.Setup.LanguageSetups select setup.DisplayName;
            }
        }

        private void InitializeHosting() {
        }

        private void UpdateClick(object sender, RoutedEventArgs e) {
            if (_currentLanguage.SelectedIndex != -1) {
                var language = _runtime.Setup.LanguageSetups[_currentLanguage.SelectedIndex];
                var engine = _runtime.GetEngine(language.Names[0]);

                engine.Execute(_codeTextBox.Text, _scope);

                Action callbackAction;
                if (_scope.TryGetVariable("callback", out callbackAction)) {
                    _callback = callbackAction;
                } else {
                    _callback = null;
                }

                Func<object, dynamic> tracker;
                if (_scope.TryGetVariable("tracker", out tracker)) {
                    _trackerMaker = tracker;
                }
            }
        }

        private void AddSelectedItemToScope(string oldName) {
            _namedControls.Remove(oldName);
            _namedControls[_selected.Name] = _selected;
        }

        public Canvas Painting {
            get {
                return _canvas;
            }
        }

        public object GetControl(string name) {
            foreach (UIElement element in _canvas.Children) {
                if (((Shape)element).Name == name) {
                    return element;
                }
            }
            return null;
        }

        /// <summary>
        /// Processes each tick to update the canvas
        /// </summary>
        private void TimerElapsed(object sender, ElapsedEventArgs e) {
            _canvas.Dispatcher.BeginInvoke(new Action(RunCallbacks));
        }

        private void RunCallbacks() {            
            try {
                if (_callback != null) {
                    _callback();
                }

                foreach (UIElement element in _canvas.Children) {
                    dynamic tracker = element.GetValue(TrackerProperty);

                    if (tracker == null && _trackerMaker != null) {
                        tracker = _trackerMaker(element);
                        element.SetValue(TrackerProperty, tracker);
                    }

                    if (tracker != null) {
                        tracker.Update(element);
                    }
                }
            } catch (Exception e) {
                _callback = null;
                _trackerMaker = null;
                MessageBox.Show("Error during callback: " + e.ToString());
                pauseOrResume_Click(pauseOrResume, new RoutedEventArgs());
            }
        }

        #region Canvas Drag and Drop

        private void ShapePreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            BeginMouseDown(e,
                (dropPoint) => {
                    var newShape = CopyShape(sender as Shape);
                    _canvas.Children.Add(newShape);
                    Canvas.SetTop(newShape, dropPoint.Y);
                    Canvas.SetLeft(newShape, dropPoint.X);
                }
            );
        }

        private Shape CopyShape(Shape shape) {
            
            Rectangle rect = shape as Rectangle;
            if (rect != null) {
                Rectangle res = new Rectangle();
                res.Width = rect.Width;
                res.Height = rect.Height;
                res.Fill = rect.Fill;
                res.Stroke = rect.Stroke;
                res.PreviewMouseLeftButtonDown += LiveObjectMouseLeftButtonDown;
                return res;
            }

            Ellipse ellipse = shape as Ellipse;
            if (ellipse != null) {
                Ellipse res = new Ellipse();
                res.Width = ellipse.Width;
                res.Height = ellipse.Height;
                res.Fill = ellipse.Fill;
                res.Stroke = ellipse.Stroke;
                res.PreviewMouseLeftButtonDown += LiveObjectMouseLeftButtonDown;
                return res;
            }

            Polygon polygon = shape as Polygon;
            if (polygon != null) {
                Polygon res = new Polygon();
                res.Points = polygon.Points;
                res.Height = polygon.Height;
                res.Width = polygon.Width;
                res.Fill = polygon.Fill;
                res.Stroke = polygon.Stroke;
                res.PreviewMouseLeftButtonDown += LiveObjectMouseLeftButtonDown;
                return res;
            }

            throw new InvalidOperationException();
        }

        private void LiveObjectMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            BeginMouseDown(e,
                (dropPoint) => {
                    Canvas.SetTop(sender as UIElement, dropPoint.Y);
                    Canvas.SetLeft(sender as UIElement, dropPoint.X);
                }
            );
        }

        private void ColorPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            BeginMouseDown(e, (point) => {
                foreach (Shape element in _canvas.Children) {
                    var top = Canvas.GetTop(element);
                    var left = Canvas.GetLeft(element);

                    if (top <= point.Y && (top + element.Height) >= point.Y &&
                        left <= point.X && (left + element.Width) >= point.X) {
                        ((Shape)element).Fill = ((Shape)element).Stroke= ((Shape)sender).Fill;
                        break;
                    }
                }
            });
        }

        private void BeginMouseDown(MouseButtonEventArgs e, Action<Point> dropAction) {
            if (_selected != null) {
                _selected.Stroke = _selected.Fill;
                _selected.StrokeThickness = 1;
                _selected = null;
            }
            _isMouseDown = true;
            _dragStart = e.GetPosition(TopCanvas);
            _dragElement = e.Source as UIElement;
            TopCanvas.CaptureMouse();
            e.Handled = true;
            _dropAction = dropAction;
        }

        #endregion

        #region Drag and Drop Support

        /// <summary>
        /// Initiates a drag and drop session.
        /// </summary>
        private void DragStarted() {
            _isDragging = true;

            _dragAdornment = new SimpleAdorner(_dragElement);
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(_dragElement);
            layer.Add(_dragAdornment);
        }

        /// <summary>
        /// Completes a drag and drop either cancelling it or committing it.
        /// </summary>
        private void DragFinished(bool cancelled, Point? dropPoint) {
            Mouse.Capture(null);
            if (_isDragging) {
                AdornerLayer.GetAdornerLayer(_dragAdornment.AdornedElement).Remove(_dragAdornment);

                if (cancelled == false) {
                    if (_dropAction != null) {
                        _dropAction(dropPoint.Value);
                        _dropAction = null;
                    }
                    //Canvas.SetTop(_dragElement, _dragTop + _overlayElement.TopOffset);
                    //Canvas.SetLeft(_dragElement, _dragLeft + _overlayElement.LeftOffset);
                    // commit result
                }
                _dragAdornment = null;

            }
            _isDragging = false;
            _isMouseDown = false;
        }

        /// <summary>
        /// Handles key down events to allow escape to cancel a drag and drop.
        /// </summary>
        private void WindowPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key == System.Windows.Input.Key.Escape && _isDragging) {
                DragFinished(true, null);
            }
        }

        /// <summary>
        /// Handles mouse move events on our top canvas which covers the entire window.  When
        /// the mouse moves we update our drag icon.
        /// </summary>
        private void TopCanvasMouseMove(object sender, MouseEventArgs e) {
            if (_isMouseDown) {
                if ((_isDragging == false) && ((Math.Abs(e.GetPosition(this).X - _dragStart.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs(e.GetPosition(this).Y - _dragStart.Y) > SystemParameters.MinimumVerticalDragDistance))) {
                    DragStarted();
                }
                if (_isDragging) {
                    Point CurrentPosition = System.Windows.Input.Mouse.GetPosition(TopCanvas);

                    _dragAdornment.LeftOffset = CurrentPosition.X - _dragStart.X;
                    _dragAdornment.TopOffset = CurrentPosition.Y - _dragStart.Y;
                }
            }
        }

        /// <summary>
        /// Handles mouse up events on our top canvas which covers our entire window.  When the mouse
        /// event up is raised we process the drop action if we're dragging.
        /// </summary>
        private void TopCanvasMouseUp(object sender, MouseButtonEventArgs e) {
            if (_isMouseDown) {
                if (!_isDragging) {
                    _selected = _dragElement as Shape;
                    if (_selected != null) {                        
                        _selected.Stroke = Brushes.LightBlue;
                        _selected.StrokeThickness = 2;
                        SelectedItemName.Text = _selected.Name;
                    }
                } else {
                    DragFinished(false, Mouse.GetPosition(_canvas));
                }
                e.Handled = true;
                Mouse.Capture(null);
            }
        }

        #endregion

        #region Adornment 

        class SimpleAdorner : Adorner {
            private readonly Rectangle _child;
            private double _leftOffset = 0;
            private double _topOffset = 0;

            public SimpleAdorner(UIElement adornedElement)
                : base(adornedElement) {
                VisualBrush _brush = new VisualBrush(adornedElement);

                _child = new Rectangle();
                _child.Width = adornedElement.RenderSize.Width;
                _child.Height = adornedElement.RenderSize.Height;

                DoubleAnimation animation = new DoubleAnimation(0.3, 1, new Duration(TimeSpan.FromSeconds(1)));
                animation.AutoReverse = true;
                animation.RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever;
                _brush.BeginAnimation(System.Windows.Media.Brush.OpacityProperty, animation);

                _child.Fill = _brush;
            }

            protected override Size MeasureOverride(Size constraint) {
                _child.Measure(constraint);
                return _child.DesiredSize;
            }

            protected override Size ArrangeOverride(Size finalSize) {
                _child.Arrange(new Rect(finalSize));
                return finalSize;
            }

            protected override Visual GetVisualChild(int index) {
                return _child;
            }

            protected override int VisualChildrenCount {
                get {
                    return 1;
                }
            }

            public double LeftOffset {
                get {
                    return _leftOffset;
                }
                set {
                    _leftOffset = value;
                    UpdatePosition();
                }
            }

            public double TopOffset {
                get {
                    return _topOffset;
                }
                set {
                    _topOffset = value;
                    UpdatePosition();
                }
            }

            private void UpdatePosition() {
                AdornerLayer adornerLayer = this.Parent as AdornerLayer;
                if (adornerLayer != null) {
                    adornerLayer.Update(AdornedElement);
                }
            }

            public override GeneralTransform GetDesiredTransform(GeneralTransform transform) {
                GeneralTransformGroup result = new GeneralTransformGroup();
                result.Children.Add(base.GetDesiredTransform(transform));
                result.Children.Add(new TranslateTransform(_leftOffset, _topOffset));
                return result;
            }
        }


        private void pauseOrResume_Click(object sender, RoutedEventArgs e) {
            _timer.Enabled = !_timer.Enabled;
            if (_timer.Enabled) {
                ((Button)sender).Content = "Pause";
            } else {
                ((Button)sender).Content = "Resume";
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) {

        }

        private void SelectedItemName_TextChanged(object sender, TextChangedEventArgs e) {
            if (_selected != null) {
                try {
                    string oldName = _selected.Name;
                    _selected.Name = SelectedItemName.Text;
                    AddSelectedItemToScope(oldName);
                } catch {
                    // invalid name
                }
            }
        }

        /// <summary>
        /// Runs through all of our Canvas's children and makes sure they're clickable.  This enables
        /// script code to add children to the canvas and still allow the user to interact w/ them.
        /// </summary>
        void _canvas_LayoutUpdated(object sender, EventArgs e) {
            foreach (UIElement elem in _canvas.Children) {
                elem.PreviewMouseLeftButtonDown -= LiveObjectMouseLeftButtonDown;
                elem.PreviewMouseLeftButtonDown += LiveObjectMouseLeftButtonDown;
            }
        }

        #endregion

        #region Really Dynamic

        /*
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(System.Linq.Expressions.Expression parameter) {
            return new Meta(parameter, this);
        }

        private bool TryGetMember(string name, out object res) {
            res = GetControl(name);
            return res != null;
        }

        private delegate bool TryGet(string name, out object res);

        private class Meta : DynamicMetaObject {
            public Meta(Ast parameter, object value)
                : base(parameter, BindingRestrictions.Empty, value) {
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder) {
                var res = binder.FallbackGetMember(this);
                var param = Ast.Parameter(typeof(object));
                return binder.FallbackGetMember(
                    this,
                    new DynamicMetaObject(
                        Ast.Block(
                            new[] { param },
                            Ast.Condition(
                                Ast.Invoke(
                                    Ast.Constant(new TryGet(((MainWindow)Value).TryGetMember)),
                                    Ast.Constant(binder.Name),
                                    param
                                ),
                                param,
                                res.Expression
                            )
                        ),
                        BindingRestrictions.GetInstanceRestriction(Expression, Value)                    
                    )                    
                );
            }
        }*/

        #endregion

        private void button1_Click(object sender, RoutedEventArgs e) {
            _canvas.Children.Clear();
        }

        private readonly Timer _timer = new Timer(1000 / 30);   // 30 frames per second
        private bool _isMouseDown, _isDragging;
        private Point _dragStart;
        private UIElement _dragElement;
        private Shape _selected;
        private SimpleAdorner _dragAdornment;
        private Action<Point> _dropAction;
        private Action _callback;
        private Func<object, dynamic> _trackerMaker;
        private Dictionary<string, UIElement> _namedControls = new Dictionary<string, UIElement>();
        private static DependencyProperty TrackerProperty = DependencyProperty.RegisterAttached("TrackerProperty", typeof(object), typeof(UIElement));

        class DynamicAPI : DynamicObject {
            private readonly MainWindow _window;

            public DynamicAPI(MainWindow window) {
                _window = window;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result) {

                if (binder.Name == "Painting") {
                    result = _window.Painting;
                    return true;
                }

                result = _window.GetControl(binder.Name);
                return result != null;
            }
        }
    }
}
