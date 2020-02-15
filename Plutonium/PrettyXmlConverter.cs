using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;

namespace Plutonium
{
    internal class PrettyXmlConverter : DependencyObject
    {
        private static Style MakeStyle(Color color, FontStyle? fontStyle = null)
        {
            var style = new Style();
            style.Setters.Add(new Setter(TextElement.ForegroundProperty, new SolidColorBrush(color)));
            if (fontStyle.HasValue) style.Setters.Add(new Setter(TextElement.FontStyleProperty, fontStyle.Value));
            return style;
        }

        public static IEnumerable<Block> RenderElement(string type, XElement element, double indent)
        {
            const double childIndent = 25d;
            const double hanging = childIndent / 2;

            yield return RenderLine(type, RenderElement(element), indent, hanging);

            var hasMultiLineText = HasText(element) && IsMultiLine(element.Value);

            if (!element.HasElements && !hasMultiLineText) yield break;
            
            foreach (var e in element.Elements())
            {
                foreach (var b in RenderElement(null, e, indent + childIndent)) yield return b;
            }

            if (hasMultiLineText)
            {
                yield return RenderLine(type, RenderContent(element, 200), indent + childIndent, 0);
            }

            yield return RenderLine(type, RenderEndElement(element), indent, hanging);
        }

        private static Paragraph RenderLine(string type, IEnumerable<Inline> inlines, double indent, double hanging)
        {
            var paragraph = new Paragraph
            {
                TextAlignment = TextAlignment.Left,
                IsHyphenationEnabled = false,
                TextIndent = -hanging,
                Margin = new Thickness(indent + hanging, 0, 0, 0)
            };
            if (type != null)
            {
                paragraph.Inlines.Add(new Run("[ => " + type + "]")
                {
                    Foreground = type == "SERVER" ? Brushes.Blue : Brushes.Green
                });
            }

            paragraph.Inlines.AddRange(inlines);

            return paragraph;
        }

        private static IEnumerable<Inline> RenderElement(XElement element)
        {
            yield return Bracket("<");
            yield return ElementName(element.Name.LocalName);

            foreach (var a in element.Attributes())
            {
                yield return Space();
                yield return AttributeName(a.Name.LocalName);
                yield return Assignment();
                yield return Quote();
                yield return AttributeValue(a.Value);
                yield return Quote();
            }

            var hasText = HasText(element);

            yield return Bracket(element.HasElements || hasText ? ">" : "/>");

            if (!hasText || IsMultiLine(element.Value)) yield break;
            
            yield return ElementValue(element.Value);

            foreach (var i in RenderEndElement(element)) yield return i;
        }

        private static IEnumerable<Inline> RenderEndElement(XElement element)
        {
            yield return Bracket("</");
            yield return ElementName(element.Name.LocalName);
            yield return Bracket(">");
        }

        private static IEnumerable<Inline> RenderContent(XElement element, int maxLength)
        {
            var trimmed = element.Value.Trim();
            var text = trimmed.Substring(0, Math.Min(maxLength, trimmed.Length));
            var first = true;

            foreach (var line in text.Split('\n').Select(l => l.Trim()))
            {
                if (!first) yield return new LineBreak();
                else first = false;

                yield return ElementValue(line.Trim());
            }

            if (text.Length <= maxLength) yield break;
            
            yield return new LineBreak();
            yield return new Run("(Content truncated)") {Style = MakeStyle(Colors.DarkGreen, FontStyles.Italic)};
        }

        private static Inline Bracket(string text)
        {
            return new Run(text) {Style = MakeStyle(Colors.Blue)};
        }

        private static Inline ElementName(string text)
        {
            return new Run(text) {Style = MakeStyle(Colors.DarkRed)};
        }

        private static Inline Space()
        {
            return new Run(" ") {Style = null};
        }

        private static Inline AttributeName(string text)
        {
            return new Run(text) {Style = MakeStyle(Colors.Red)};
        }

        private static Inline Assignment()
        {
            return new Run("=") {Style = MakeStyle(Colors.DarkBlue)};
        }

        private static Inline Quote()
        {
            return new Run("\"") {Style = MakeStyle(Colors.DarkBlue)};
        }

        private static Inline AttributeValue(string text)
        {
            return new Run(text) {Style = MakeStyle(Colors.Black)};
        }

        private static Inline ElementValue(string text)
        {
            return new Run(text) {Style = MakeStyle(Colors.Black)};
        }
        
        private static bool IsMultiLine(string text)
        {
            return text.Trim().Contains('\n');
        }

        private static bool HasText(XContainer element)
        {
            return element.Nodes().Any(n => n.NodeType == XmlNodeType.Text);
        }
    }
}