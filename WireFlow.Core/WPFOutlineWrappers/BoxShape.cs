using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using MS.Internal;


namespace WireFlow
{

    // Too complicated. Not being used right now.

    public class BoxShape: Shape, INotifyPropertyChanged
    {
        public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register(nameof(RadiusX), typeof(double), typeof(BoxShape),
            (PropertyMetadata)new FrameworkPropertyMetadata((object)0.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register(nameof(RadiusY), typeof(double), typeof(BoxShape),
            (PropertyMetadata)new FrameworkPropertyMetadata((object)0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(BoxShape), (PropertyMetadata)new FrameworkPropertyMetadata((object)string.Empty,
            FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(BoxShape.OnTextChanged), new CoerceValueCallback(BoxShape.CoerceText)));

        public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(BoxShape));
        public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(typeof(BoxShape));
        public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(typeof(BoxShape));
        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(BoxShape));
        public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(typeof(BoxShape));
        //public static readonly DependencyProperty BackgroundProperty = TextElement.BackgroundProperty.AddOwner(typeof(Box),
        //    (PropertyMetadata)new FrameworkPropertyMetadata((object)null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty LineHeightProperty = Block.LineHeightProperty.AddOwner(typeof(BoxShape));
        public static readonly DependencyProperty PaddingProperty = Block.PaddingProperty.AddOwner(typeof(BoxShape),
            (PropertyMetadata)new FrameworkPropertyMetadata((object)new Thickness(), FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty TextAlignmentProperty = Block.TextAlignmentProperty.AddOwner(typeof(BoxShape));

        //public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(nameof(TextWrapping), typeof(TextWrapping), typeof(TextBlock),
        //    (PropertyMetadata)new FrameworkPropertyMetadata((object)TextWrapping.NoWrap, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender),
        //    new ValidateValueCallback(TextBlock.IsValidTextWrap));

        private Rect rect = Rect.Empty;
        private TextBlock textBlock;


        public BoxShape()
        {
            textBlock = new TextBlock()
            {
                Text = "Text in a Box",
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            AddLogicalChild(textBlock);
        }


        protected override void OnInitialized(EventArgs e)
        {
            Stretch = Stretch.Fill;
            Stroke = Brushes.Gray;
            StrokeThickness = 2;
            Fill = Brushes.LightGray;

        }



        public string Text
        {
            get
            {
                return (string)this.GetValue(TextBlock.TextProperty);
            }
            set
            {
                this.SetValue(TextBlock.TextProperty, (object)value);
            }
        }


        public FontFamily FontFamily
        {
            get
            {
                return (FontFamily)this.GetValue(TextBlock.FontFamilyProperty);
            }
            set
            {
                this.SetValue(TextBlock.FontFamilyProperty, (object)value);
            }
        }


        public FontStyle FontStyle
        {
            get
            {
                return (FontStyle)this.GetValue(TextBlock.FontStyleProperty);
            }
            set
            {
                this.SetValue(TextBlock.FontStyleProperty, (object)value);
            }
        }


        public FontWeight FontWeight
        {
            get
            {
                return (FontWeight)this.GetValue(TextBlock.FontWeightProperty);
            }
            set
            {
                this.SetValue(TextBlock.FontWeightProperty, (object)value);
            }
        }


        [TypeConverter(typeof(LengthConverter))]
        public double RadiusX
        {
            get
            {
                return (double)this.GetValue(BoxShape.RadiusXProperty);
            }
            set
            {
                this.SetValue(BoxShape.RadiusXProperty, (object)value);
                //OnPropertyChanged(nameof(RadiusY));
            }
        }


        [TypeConverter(typeof(LengthConverter))]
        public double RadiusY
        {
            get
            {
                return (double)this.GetValue(BoxShape.RadiusYProperty);
            }
            set
            {
                this.SetValue(BoxShape.RadiusYProperty, (object)value);
                //OnPropertyChanged(nameof(RadiusY));
            }
        }


        public double FontSize
        {
            get
            {
                return (double)this.GetValue(TextBlock.FontSizeProperty);
            }
            set
            {
                this.SetValue(TextBlock.FontSizeProperty, (object)value);
            }
        }

        public Brush Foreground
        {
            get
            {
                return (Brush)this.GetValue(TextBlock.ForegroundProperty);
            }
            set
            {
                this.SetValue(TextBlock.ForegroundProperty, (object)value);
            }
        }


        protected override Geometry DefiningGeometry => (Geometry)new RectangleGeometry(this.rect, this.RadiusX, this.RadiusY);
        public override Geometry RenderedGeometry => (Geometry)new RectangleGeometry(this.rect, this.RadiusX, this.RadiusY);
        public override Transform GeometryTransform => Transform.Identity;



        private static object CoerceText(DependencyObject d, object value)
        {
            BoxShape textBlock = (BoxShape)d;
            if (value == null)
                value = (object)string.Empty;
            //if (textBlock._complexContent != null && !textBlock.CheckFlags(TextBlock.Flags.TextContentChanging) && (string)value == (string)textBlock.GetValue(TextBlock.TextProperty))
            //    TextBlock.OnTextChanged(d, (string)value);
            return value;
        }



        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BoxShape.OnTextChanged(d, (string)e.NewValue);
        }


        private static void OnTextChanged(DependencyObject d, string newText)
        {
             //textBlock._contentCache = newText != null ? newText : string.Empty;
        }



        protected override void OnRender(DrawingContext drawingContext)
        {
            //base.OnRender(drawingContext);
            drawingContext.DrawRoundedRectangle(
                this.Fill,
                new Pen(Stroke, StrokeThickness),
                this.rect,
                this.RadiusX,
                this.RadiusY
            );


            //drawingContext.DrawEllipse(
            //    Brushes.Green,
            //    new Pen(Brushes.Yellow, 2),
            //    center: new Point(size.Width / 2, size.Height / 2),
            //    radiusX: size.Width / 4,
            //    radiusY: size.Height / 4);
            //var txt = new FormattedText(
            //    "Foobar",
            //    System.Globalization.CultureInfo.CurrentCulture,
            //    FlowDirection.LeftToRight,
            //    new Typeface("Courier new"),
            //    21,
            //    Brushes.Red);
            //drawingContext.DrawText(txt,
            //    new Point(size.Width / 2, size.Height / 2)
            //    );
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        protected override Size MeasureOverride(Size constraint)
        {
            if (this.Stretch != Stretch.UniformToFill)
                return this.GetNaturalSize();
            double width = constraint.Width;
            double height = constraint.Height;
            if (double.IsInfinity(width) && double.IsInfinity(height))
                return this.GetNaturalSize();
            double num = double.IsInfinity(width) || double.IsInfinity(height) ? Math.Min(width, height) : Math.Max(width, height);
            return new Size(num, num);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double strokeThickness = StrokeThickness;

            double num = strokeThickness / 2.0;
            double width = Math.Max(0.0, finalSize.Width - strokeThickness);
            double height = Math.Max(0.0, finalSize.Height - strokeThickness);
            this.rect = new Rect(num, num, width, height);
            switch (this.Stretch)
            {
                case Stretch.None:
                    this.rect.Width = this.rect.Height = 0.0;
                    break;
                case Stretch.Uniform:
                    if (this.rect.Width > this.rect.Height)
                    {
                        this.rect.Width = this.rect.Height;
                        break;
                    }
                    this.rect.Height = this.rect.Width;
                    break;
                case Stretch.UniformToFill:
                    if (this.rect.Width < this.rect.Height)
                    {
                        this.rect.Width = this.rect.Height;
                        break;
                    }
                    this.rect.Height = this.rect.Width;
                    break;
            }
            return finalSize;
        }



        protected Size GetNaturalSize()
        {
            double strokeThickness = StrokeThickness;
            return new Size(strokeThickness, strokeThickness);
        }

    }
}

