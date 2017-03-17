using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media;

namespace BorderEditor
{
    /// <summary>
    /// 应用状态模式实现图形状态编辑
    /// </summary>
    public class ShapeEditor
    {
        public Canvas ParentControl { get; private set; }
        public RectShape RectShape { get; set; }
        public IStatus Status { get; set; }

        public ShapeEditor(Canvas parent)
        {
            ParentControl = parent;
            Status = new EdittingStatus(this);
            ParentControl.MouseDown += (o, e) =>
            {
                Status.OnMouseDown(o, e);
            };
            ParentControl.MouseMove += (o, e) =>
            {
                Status.OnMouseMove(o, e);
            };
            ParentControl.MouseUp += (o, e) =>
            {
                Status.OnMouseUp(o, e);
            };
        }

        #region 校准函数

        /// <summary>
        /// 校准图形位置：使图形处于宿主控件内
        /// </summary>
        /// <param name="left">期望设置的LeftProperty值</param>
        /// <returns>校准结果</returns>
        public double AdjustShapeLeft(double left)
        {
            left = left < 0 ? 0 : left;
            left = left > ParentControl.ActualWidth - RectShape.Width ? ParentControl.ActualWidth - RectShape.Width : left;

            return left;
        }

        /// <summary>
        /// 校准图形位置：使图形处于宿主控件内
        /// </summary>
        /// <param name="top">期望设置的TopProperty值</param>
        /// <returns>校准结果</returns>
        public double AdjustShapeTop(double top)
        {
            top = top < 0 ? 0 : top;
            top = top > ParentControl.ActualHeight - RectShape.Height ? ParentControl.ActualHeight - RectShape.Height : top;

            return top;
        }

        /// <summary>
        /// 校准鼠标位置：使鼠标位置处于宿主控件内
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <returns>校准结果</returns>
        public double AdjustMouseX(double x)
        {
            x = x > ParentControl.ActualWidth ? ParentControl.ActualWidth : x;
            x = x < 0 ? 0 : x;
            return x;
        }

        /// <summary>
        /// 校准鼠标位置：使鼠标位置处于宿主控件内
        /// </summary>
        /// <param name="y">Y坐标</param>
        /// <returns>校准结果</returns>
        public double AdjustMouseY(double y)
        {
            y = y > ParentControl.ActualHeight ? ParentControl.ActualHeight : y;
            y = y < 0 ? 0 : y;

            return y;
        }

        #endregion
    }

    public class RectShape : Shape
    {
        /// <summary>
        /// 选择边界拖拽时的，可选择的空间
        /// </summary>
        public double SelectSpace { get; set; }
        public FocusMode FocusMode { get; private set; }

        public bool IsEditting { get; set; }

        public RectShape()
        {
            this.Height = 0.0;
            this.Width = 0.0;
            this.SelectSpace = 12.0;
            this.IsEditting = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!IsEditting) return;

            Point p = e.GetPosition(this);
            if (p.X >= 0.0 && p.X <= SelectSpace)
            {
                if (p.Y >= SelectSpace && p.Y <= this.Height - SelectSpace)
                {
                    this.Cursor = Cursors.IBeam;
                    this.FocusMode = BorderEditor.FocusMode.Left;
                }
                else if (p.Y < SelectSpace)
                {
                    this.Cursor = Cursors.IBeam;
                    this.FocusMode = BorderEditor.FocusMode.LeftTop;
                }
                else
                {
                    this.Cursor = Cursors.IBeam;
                    this.FocusMode = BorderEditor.FocusMode.LeftBottom;
                }
            }
            else if (p.X >= this.Width - SelectSpace)
            {
                if (p.Y >= SelectSpace && p.Y <= this.Height - SelectSpace)
                {
                    this.Cursor = Cursors.IBeam;
                    this.FocusMode = BorderEditor.FocusMode.Right;
                }
                else if (p.Y < SelectSpace)
                {
                    this.Cursor = Cursors.IBeam;
                    this.FocusMode = BorderEditor.FocusMode.RightTop;
                }
                else
                {
                    this.Cursor = Cursors.IBeam;
                    this.FocusMode = BorderEditor.FocusMode.RightBottom;
                }
            }
            else if (p.X >= SelectSpace && p.X <= this.Width - SelectSpace)
            {
                if (p.Y >= 0.0 && p.Y <= SelectSpace)
                {
                    this.Cursor = Cursors.IBeam;
                    this.FocusMode = BorderEditor.FocusMode.Top;
                }
                else if (p.Y >= this.Height - SelectSpace)
                {
                    this.Cursor = Cursors.IBeam;
                    this.FocusMode = BorderEditor.FocusMode.Bottom;
                }
                else
                {
                    this.Cursor = Cursors.Hand;
                    this.FocusMode = BorderEditor.FocusMode.Center;
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
            this.FocusMode = BorderEditor.FocusMode.None;
            base.OnMouseLeave(e);
        }

        protected override System.Windows.Media.Geometry DefiningGeometry
        {
            get
            {
                GeometryGroup group = new GeometryGroup();

                PathGeometry pathGeometry = new PathGeometry();

                // 添加线
                PathFigure figure = new PathFigure();
                double x = 3.0;
                double y = 3.0;
                double actualWidth = this.Width - x * 2;
                double actualHeight = this.Height - y * 2;
                figure.StartPoint = new Point(x, y);
                PolyLineSegment poly = new PolyLineSegment();
                poly.Points.Add(new Point(x, actualHeight + y));
                poly.Points.Add(new Point(x + actualWidth, actualHeight + y));
                poly.Points.Add(new Point(x + actualWidth, y));
                poly.Points.Add(new Point(x, y));
                figure.Segments.Add(poly);
                pathGeometry.Figures.Add(figure);
                group.Children.Add(pathGeometry);

                EllipseGeometry ellipse = new EllipseGeometry(new Point(x, y), x, y);
                group.Children.Add(ellipse);
                ellipse = new EllipseGeometry(new Point(x + actualWidth / 2, y), x, y);
                group.Children.Add(ellipse);
                ellipse = new EllipseGeometry(new Point(x + actualWidth, y), x, y);
                group.Children.Add(ellipse);
                ellipse = new EllipseGeometry(new Point(x + actualWidth, y + actualHeight / 2), x, y);
                group.Children.Add(ellipse);
                ellipse = new EllipseGeometry(new Point(x + actualWidth, y + actualHeight), x, y);
                group.Children.Add(ellipse);
                ellipse = new EllipseGeometry(new Point(x + actualWidth / 2, y + actualHeight), x, y);
                group.Children.Add(ellipse);
                ellipse = new EllipseGeometry(new Point(x, y + actualHeight), x, y);
                group.Children.Add(ellipse);
                ellipse = new EllipseGeometry(new Point(x, y + actualHeight / 2), x, y);
                group.Children.Add(ellipse);
                group.Children.Add(ellipse);

                return group;
            }
        }
    }

    public enum FocusMode
    {
        None,
        Left,
        Top,
        Right,
        Bottom,
        Center,
        LeftTop,
        LeftBottom,
        RightBottom,
        RightTop
    }

    public interface IStatus
    {
        void OnMouseDown(object sender, MouseButtonEventArgs e);
        void OnMouseUp(object sender, MouseButtonEventArgs e);
        void OnMouseMove(object sender, MouseEventArgs e);
    }

    class EdittingStatus : IStatus
    {
        ShapeEditor _editor;
        public EdittingStatus(ShapeEditor editor)
        {
            _editor = editor;
        }

        public void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (_editor.RectShape == null)
                {
                    _editor.Status = new CreatingStatus(_editor);
                    _editor.Status.OnMouseDown(sender, e);
                }
                else
                {
                    switch (_editor.RectShape.FocusMode)
                    {
                        case FocusMode.Center:
                            _editor.Status = new DragDropStatus(_editor);
                            _editor.Status.OnMouseDown(sender, e);
                            break;
                        case FocusMode.None:
                            break;
                        default:
                            _editor.Status = new BorderStatus(_editor);
                            _editor.Status.OnMouseDown(sender, e);
                            break;
                    }
                }
            }
            else if (e.ChangedButton == MouseButton.Right && e.Source == _editor.ParentControl)
            {
                _editor.ParentControl.Children.Remove(_editor.RectShape);
                _editor.RectShape = null;
                _editor.Status = new CreatingStatus(_editor);
                _editor.Status.OnMouseDown(sender, e);
            }
        }

        public void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        public void OnMouseMove(object sender, MouseEventArgs e)
        {
        }
    }

    class CreatingStatus : IStatus
    {
        ShapeEditor _editor;
        Point _pInitialPoint;
        public CreatingStatus(ShapeEditor editor)
        {
            _editor = editor;
        }

        public void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _editor.RectShape = new RectShape()
                {
                    Fill = System.Windows.Media.Brushes.Transparent,
                    Stroke = System.Windows.Media.Brushes.Blue,
                    StrokeThickness = 1
                };
                _editor.ParentControl.Children.Add(_editor.RectShape);
                _pInitialPoint = e.GetPosition(_editor.ParentControl);
                _editor.RectShape.SetValue(Canvas.LeftProperty, _pInitialPoint.X);
                _editor.RectShape.SetValue(Canvas.TopProperty, _pInitialPoint.Y);

                _editor.RectShape.CaptureMouse();
            }
        }

        public void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (_editor.RectShape != null)
                {
                    _editor.RectShape.ReleaseMouseCapture();
                    _editor.Status = new EdittingStatus(_editor);
                }
            }
        }

        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_editor.RectShape != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point curPoint = e.GetPosition(_editor.ParentControl);
                double curX = _editor.AdjustMouseX(curPoint.X);
                double curY = _editor.AdjustMouseY(curPoint.Y);
                _editor.RectShape.Width = Math.Abs(_pInitialPoint.X - curX);
                _editor.RectShape.Height = Math.Abs(_pInitialPoint.Y - curY);
                _editor.RectShape.SetValue(Canvas.LeftProperty, Math.Min(_pInitialPoint.X, curX));
                _editor.RectShape.SetValue(Canvas.TopProperty, Math.Min(_pInitialPoint.Y, curY));
            }
        }
    }

    class DragDropStatus : IStatus
    {
        ShapeEditor _editor;
        Point _pInitialPoint;
        public DragDropStatus(ShapeEditor editor)
        {
            _editor = editor;
        }

        public void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _editor.RectShape.CaptureMouse();
                _pInitialPoint = e.GetPosition(_editor.ParentControl);
            }
        }

        public void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _editor.RectShape.ReleaseMouseCapture();
                _editor.Status = new EdittingStatus(_editor);
            }
        }

        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point curPoint = e.GetPosition(_editor.ParentControl);
                double x = (double)_editor.RectShape.GetValue(Canvas.LeftProperty);
                double y = (double)_editor.RectShape.GetValue(Canvas.TopProperty);
                double xDelta = curPoint.X - _pInitialPoint.X;
                double yDelta = curPoint.Y - _pInitialPoint.Y;

                _editor.RectShape.SetValue(Canvas.LeftProperty, _editor.AdjustShapeLeft(x + xDelta));
                _editor.RectShape.SetValue(Canvas.TopProperty, _editor.AdjustShapeTop(y + yDelta));
                _pInitialPoint = curPoint;
            }
        }
    }

    class BorderStatus : IStatus
    {
        ShapeEditor _editor;
        Point _pInitialPoint;
        FocusMode _currentMode;
        public BorderStatus(ShapeEditor editor)
        {
            _editor = editor;
        }

        public void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _editor.RectShape.CaptureMouse();
                _currentMode = _editor.RectShape.FocusMode;
                double x = (double)_editor.RectShape.GetValue(Canvas.LeftProperty);
                double y = (double)_editor.RectShape.GetValue(Canvas.TopProperty);
                switch (_currentMode)
                {
                    case FocusMode.Right:
                    case FocusMode.Bottom:
                    case FocusMode.RightBottom:
                        _pInitialPoint = new Point(x, y);
                        break;
                    case FocusMode.Left:
                    case FocusMode.Top:
                    case FocusMode.LeftTop:
                        _pInitialPoint = new Point(x + _editor.RectShape.Width, y + _editor.RectShape.Height);
                        break;
                    case FocusMode.RightTop:
                        _pInitialPoint = new Point(x, y + _editor.RectShape.Height);
                        break;
                    case FocusMode.LeftBottom:
                        _pInitialPoint = new Point(x + _editor.RectShape.Width, y);
                        break;
                    default:
                        break;
                }
            }
        }

        public void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _editor.RectShape.ReleaseMouseCapture();
                _editor.Status = new EdittingStatus(_editor);
            }
        }

        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point curPoint = e.GetPosition(_editor.ParentControl);
                double curX = _editor.AdjustMouseX(curPoint.X);
                double curY = _editor.AdjustMouseY(curPoint.Y);

                switch (_currentMode)
                {
                    case FocusMode.Left:
                    case FocusMode.Right:
                        _editor.RectShape.Width = Math.Abs(_pInitialPoint.X - curX);
                        _editor.RectShape.SetValue(Canvas.LeftProperty, Math.Min(_pInitialPoint.X, curX));
                        break;
                    case FocusMode.Top:
                    case FocusMode.Bottom:
                        _editor.RectShape.Height = Math.Abs(_pInitialPoint.Y - curY);
                        _editor.RectShape.SetValue(Canvas.TopProperty, Math.Min(_pInitialPoint.Y, curY));
                        break;
                    case FocusMode.RightBottom:
                    case FocusMode.RightTop:
                    case FocusMode.LeftTop:
                    case FocusMode.LeftBottom:
                        _editor.RectShape.Width = Math.Abs(_pInitialPoint.X - curX);
                        _editor.RectShape.SetValue(Canvas.LeftProperty, Math.Min(_pInitialPoint.X, curX));
                        _editor.RectShape.Height = Math.Abs(_pInitialPoint.Y - curY);
                        _editor.RectShape.SetValue(Canvas.TopProperty, Math.Min(_pInitialPoint.Y, curY));
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
