﻿using Mapsui.Geometries;
using Mapsui.Widgets.ScaleBar;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class ScaleBarWidgetRenderer
    {
        private const float strokeExternal = 4;
        private const float strokeInternal = 2;

        private static SKPaint paintScaleBar;
        private static SKPaint paintScaleBarStroke;
        private static SKPaint paintScaleText;
        private static SKPaint paintScaleTextStroke;

        public static void Draw(SKCanvas canvas, double screenWidth, double screenHeight, ScaleBarWidget scaleBar,
            float layerOpacity)
        {
            // If this widget belongs to no viewport, than stop drawing
            if (scaleBar.Map == null)
                return;

            // If this is the first time, we call this renderer, ...
            if (paintScaleBar == null)
            {
                // ... than create the paints
                paintScaleBar = CreateScaleBarPaint(scaleBar.TextColor.ToSkia(layerOpacity), strokeInternal, SKPaintStyle.Fill, scaleBar.Scale);
                paintScaleBarStroke = CreateScaleBarPaint(scaleBar.Halo.ToSkia(layerOpacity), strokeExternal, SKPaintStyle.Stroke, scaleBar.Scale);
                paintScaleText = CreateTextPaint(scaleBar.TextColor.ToSkia(layerOpacity), 2, SKPaintStyle.Fill, scaleBar.Scale);
                paintScaleTextStroke = CreateTextPaint(scaleBar.Halo.ToSkia(layerOpacity), 2, SKPaintStyle.Stroke, scaleBar.Scale);
            }
            else
            {
                // Update paints with new values
                paintScaleBar.Color = scaleBar.TextColor.ToSkia(layerOpacity);
                paintScaleBar.StrokeWidth = strokeInternal * scaleBar.Scale;
                paintScaleBarStroke.Color = scaleBar.Halo.ToSkia(layerOpacity);
                paintScaleBarStroke.StrokeWidth = strokeExternal * scaleBar.Scale;
                paintScaleText.Color = scaleBar.TextColor.ToSkia(layerOpacity);
                paintScaleText.StrokeWidth = strokeInternal * scaleBar.Scale;
                paintScaleText.Typeface = SKTypeface.FromFamilyName(scaleBar.Font.FontFamily, SKTypefaceStyle.Bold);
                paintScaleText.TextSize = (float)scaleBar.Font.Size * scaleBar.Scale;
                paintScaleTextStroke.Color = scaleBar.Halo.ToSkia(layerOpacity);
                paintScaleTextStroke.StrokeWidth = strokeInternal * scaleBar.Scale;
                paintScaleTextStroke.Typeface = SKTypeface.FromFamilyName(scaleBar.Font.FontFamily, SKTypefaceStyle.Bold);
                paintScaleTextStroke.TextSize = (float)scaleBar.Font.Size * scaleBar.Scale;
            }

            float scaleBarLength1;
            string scaleBarText1;
            float scaleBarLength2;
            string scaleBarText2;

            (scaleBarLength1, scaleBarText1, scaleBarLength2, scaleBarText2) = scaleBar.GetScaleBarLengthAndText();

            // Calc height of scale bar
            SKRect textSize;

            // Do this, because height of text changes sometimes (e.g. from 2 m to 1 m)
            paintScaleTextStroke.MeasureText("9999 m", ref textSize);

            var scaleBarHeight = textSize.Height + (scaleBar.TickLength + strokeExternal * 0.5f + scaleBar.TextMargin) * scaleBar.Scale;

            if (scaleBar.ScaleBarMode == ScaleBarMode.Both && scaleBar.SecondaryUnitConverter != null)
            {
                scaleBarHeight *= 2;
            }
            else
            {
                scaleBarHeight += strokeExternal * 0.5f * scaleBar.Scale;
            }

            scaleBar.Height = scaleBarHeight;

            // Draw lines

            // Get lines for scale bar
            var points = scaleBar.GetScaleBarLinePositions(scaleBarLength1, scaleBarLength2, strokeExternal);

            // BoundingBox for scale bar
            BoundingBox envelop = new BoundingBox();

            if (points != null)
            {
                // Draw outline of scale bar
                for (int i = 0; i < points.Length; i += 2)
                {
                    canvas.DrawLine((float)points[i].X, (float)points[i].Y, (float)points[i + 1].X, (float)points[i + 1].Y, paintScaleBarStroke);
                }

                // Draw scale bar
                for (int i = 0; i < points.Length; i += 2)
                {
                    canvas.DrawLine((float)points[i].X, (float)points[i].Y, (float)points[i + 1].X, (float)points[i + 1].Y, paintScaleBar);
                }

                envelop = points[0].GetBoundingBox();

                for (int i = 1; i < points.Length; i++)
                {
                    envelop = envelop.Join(points[i].GetBoundingBox());
                }

                envelop = envelop.Grow(strokeExternal * 0.5f * scaleBar.Scale);
            }

            // Draw text

            // Calc text height
            SKRect textSize1;
            SKRect textSize2;

            scaleBarText1 = scaleBarText1 ?? string.Empty;
            scaleBarText2 = scaleBarText2 ?? string.Empty;

            paintScaleTextStroke.MeasureText(scaleBarText1, ref textSize1);
            paintScaleTextStroke.MeasureText(scaleBarText2, ref textSize2);

            var (posX1, posY1, posX2, posY2) = scaleBar.GetScaleBarTextPositions(textSize.ToMapsui(), textSize1.ToMapsui(), textSize2.ToMapsui(), strokeExternal);

            // Now draw text
            canvas.DrawText(scaleBarText1, posX1, posY1 - textSize1.Top, paintScaleTextStroke);
            canvas.DrawText(scaleBarText1, posX1, posY1 - textSize1.Top, paintScaleText);

            envelop = envelop.Join(new BoundingBox(posX1, posY1, posX1 + textSize1.Width, posY1 + textSize1.Height));

            if (scaleBar.ScaleBarMode == ScaleBarMode.Both && scaleBar.SecondaryUnitConverter != null)
            {
                // Now draw second text
                canvas.DrawText(scaleBarText2, posX2, posY2 - textSize2.Top, paintScaleTextStroke);
                canvas.DrawText(scaleBarText2, posX2, posY2 - textSize2.Top, paintScaleText);

                envelop = envelop.Join(new BoundingBox(posX2, posY2, posX2 + textSize2.Width, posY2 + textSize2.Height));
            }

            scaleBar.Envelope = envelop;

            if (scaleBar.ShowEnvelop)
            {
                // Draw a rect around the scale bar for testing
                var tempPaint = new SKPaint() { StrokeWidth = 1, Color = SKColors.Blue, IsStroke = true };
                canvas.DrawRect(new SKRect((float)envelop.MinX, (float)envelop.MinY, (float)envelop.MaxX, (float)envelop.MaxY), tempPaint);
            }
        }

        private static SKPaint CreateScaleBarPaint(SKColor color, float strokeWidth, SKPaintStyle style, float scale)
        {
            SKPaint paint = new SKPaint();

            paint.LcdRenderText = true;
            paint.Color = color;
            paint.StrokeWidth = strokeWidth * scale;
            paint.Style = style;
            paint.StrokeCap = SKStrokeCap.Square;

            return paint;
        }

        private static SKPaint CreateTextPaint(SKColor color, float strokeWidth, SKPaintStyle style, float scale)
        {
            SKPaint paint = new SKPaint();

            paint.LcdRenderText = true;
            paint.Color = color;
            paint.StrokeWidth = strokeWidth * scale;
            paint.Style = style;
            paint.Typeface = SKTypeface.FromFamilyName("Arial", SKTypefaceStyle.Bold);
            paint.TextSize = 10 * scale;
            paint.IsAntialias = true;

            return paint;
        }
    }
}