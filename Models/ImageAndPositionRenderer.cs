using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Renderer;
using iText.Layout;

namespace MergePdfWebApp.Models
{
    public class ImageAndPositionRenderer : CellRenderer
    {
        private readonly Image img;

        private readonly string content;

        private readonly TextAlignment? alignment;

        private readonly float wPct;

        private readonly float hPct;

        public ImageAndPositionRenderer(Cell modelElement, float wPct, float hPct, Image img,
            string content, TextAlignment? alignment)
            : base(modelElement)
        {
            this.img = img;
            this.content = content;
            this.alignment = alignment;
            this.wPct = wPct;
            this.hPct = hPct;
        }

        // If renderer overflows on the next area, iText uses getNextRender() method to create a renderer for the overflow part.
        // If getNextRenderer isn't overriden, the default method will be used and thus a default rather than custom
        // renderer will be created
        public override IRenderer GetNextRenderer()
        {
            return new ImageAndPositionRenderer((Cell)modelElement, wPct, hPct, img, content, alignment);
        }

        public override void Draw(DrawContext drawContext)
        {
            base.Draw(drawContext);
            drawContext.GetCanvas()
                .AddXObjectFittedIntoRectangle(img.GetXObject(), GetOccupiedAreaBBox())
                .Stroke();

            UnitValue fontSizeUv = GetPropertyAsUnitValue(Property.FONT_SIZE);
            float x = GetOccupiedAreaBBox().GetX() + wPct * GetOccupiedAreaBBox().GetWidth();
            float y = GetOccupiedAreaBBox().GetY() + hPct *
                      (GetOccupiedAreaBBox().GetHeight() - (fontSizeUv.IsPointValue()
                           ? fontSizeUv.GetValue()
                           : 12f) * 1.5f);
            new Canvas(drawContext.GetDocument().GetFirstPage(), drawContext.GetDocument().GetDefaultPageSize())
                .ShowTextAligned(content, x, y, alignment);
        }
    }
}