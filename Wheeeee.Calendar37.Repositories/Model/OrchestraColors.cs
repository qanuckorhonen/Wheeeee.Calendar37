using System.Drawing;
using Wheeeee.Calendar37.Core.Extensions;
using Wheeeee.Calendar37.Core.Interfaces;

namespace Wheeeee.Calendar37.Repositories.Model
{
    internal class OrchestraColors : IOrchestraColors
    {
        private readonly string _color1;

        public OrchestraColors(string color1)
        {
            _color1 = color1;

            ColorHeaderHtml = _color1;
            var c1 = ColorTranslator.FromHtml("#" + _color1);
            RowColor0Html = ColorTranslator.ToHtml(c1.Brighten(70)).Trim('#');
            RowColor1Html = ColorTranslator.ToHtml(c1.Brighten(50)).Trim('#');
        }

        public string ColorHeaderHtml { get; }
        public string RowColor0Html { get; }
        public string RowColor1Html { get; }
    }
}
