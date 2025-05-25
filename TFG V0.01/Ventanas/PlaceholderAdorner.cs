using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TFG_V0._01.Ventanas
{
    public class PlaceholderAdorner : Adorner
    {
        private readonly TextBlock _placeholderTextBlock;
        private readonly Control _control;
        private readonly VisualCollection _visuals;
        private bool _isUp = false;
        private readonly double _fontSize;
        private readonly string _placeholder;
        private readonly Brush _placeholderBrush;
        private readonly Brush _placeholderBrushFocused;

        public PlaceholderAdorner(Control adornedElement, string placeholder, Brush brush, Brush brushFocused, double fontSize = 14) : base(adornedElement)
        {
            _control = adornedElement;
            _placeholder = placeholder;
            _placeholderBrush = brush;
            _placeholderBrushFocused = brushFocused;
            _fontSize = fontSize;
            _placeholderTextBlock = new TextBlock
            {
                Text = placeholder,
                Foreground = brush,
                FontSize = fontSize,
                Margin = new Thickness(15, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false,
                Opacity = 1
            };
            _visuals = new VisualCollection(this) { _placeholderTextBlock };
            AddHandlers();
            UpdateState();
        }

        private void AddHandlers()
        {
            if (_control is TextBox tb)
            {
                tb.TextChanged += (s, e) => UpdateState();
                tb.GotFocus += (s, e) => AnimateUp();
                tb.LostFocus += (s, e) => UpdateState();
            }
            else if (_control is PasswordBox pb)
            {
                pb.PasswordChanged += (s, e) => UpdateState();
                pb.GotFocus += (s, e) => AnimateUp();
                pb.LostFocus += (s, e) => UpdateState();
            }
        }

        private bool IsEmpty()
        {
            if (_control is TextBox tb)
                return string.IsNullOrEmpty(tb.Text);
            if (_control is PasswordBox pb)
                return string.IsNullOrEmpty(pb.Password);
            return true;
        }

        private void UpdateState()
        {
            if (!IsEmpty())
            {
                _placeholderTextBlock.Opacity = 0;
            }
            else if (_control.IsFocused)
            {
                AnimateUp();
                _placeholderTextBlock.Opacity = 1;
            }
            else
            {
                AnimateDown();
                _placeholderTextBlock.Opacity = 1;
            }
        }

        private void AnimateUp()
        {
            if (_isUp) return;
            _isUp = true;
            var anim = new DoubleAnimation(0, -22, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase() };
            var scaleAnim = new DoubleAnimation(_fontSize, _fontSize * 0.8, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase() };
            _placeholderTextBlock.Foreground = _placeholderBrushFocused;
            _placeholderTextBlock.BeginAnimation(Canvas.TopProperty, anim);
            _placeholderTextBlock.BeginAnimation(TextBlock.FontSizeProperty, scaleAnim);
        }

        private void AnimateDown()
        {
            if (!_isUp) return;
            _isUp = false;
            var anim = new DoubleAnimation(-22, 0, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase() };
            var scaleAnim = new DoubleAnimation(_fontSize * 0.8, _fontSize, TimeSpan.FromMilliseconds(200)) { EasingFunction = new QuadraticEase() };
            _placeholderTextBlock.Foreground = _placeholderBrush;
            _placeholderTextBlock.BeginAnimation(Canvas.TopProperty, anim);
            _placeholderTextBlock.BeginAnimation(TextBlock.FontSizeProperty, scaleAnim);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _placeholderTextBlock.Arrange(new Rect(new Point(0, 0), finalSize));
            return finalSize;
        }

        protected override int VisualChildrenCount => _visuals.Count;
        protected override Visual GetVisualChild(int index) => _visuals[index];
    }
}