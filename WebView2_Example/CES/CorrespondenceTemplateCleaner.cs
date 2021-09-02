using HtmlAgilityPack;
using System.Linq;
using System.Text.RegularExpressions;

namespace WebView2_Example.CES
{
    public class CorrespondenceTemplateCleaner
    {
        private const string EDITABLE_CLASS = "editable";
        private const string NONEDITABLE_CLASS = "non-editable";
        private const string TOREMOVE_CLASS = "toRemove";
        private const string CONTENTEDITABLE_ATTR = "contenteditable";

        public string CleanMail(string body)
        {
            var html = new HtmlDocument();
            html.LoadHtml(body);
            var nodesToRemove = html.DocumentNode
                .Descendants()
                .Where(x => x.HasClass(EDITABLE_CLASS) || x.HasClass(NONEDITABLE_CLASS) || x.HasClass(TOREMOVE_CLASS))
                .ToList();

            var nodesWithContentEditableAttribute = html.DocumentNode.SelectNodes($"//@{CONTENTEDITABLE_ATTR}");

            if (nodesWithContentEditableAttribute != null)
            {
                foreach (var node in nodesWithContentEditableAttribute)
                {
                    node.Attributes[CONTENTEDITABLE_ATTR].Remove();
                }
            }

            foreach (var node in nodesToRemove)
            {
                if (node.HasClass(EDITABLE_CLASS))
                {
                    node.Attributes.Remove("style");
                }
                node.ParentNode.RemoveChild(node, true);
            }

            return html.DocumentNode.OuterHtml;
        }

        private string SetNewLines(string body)
        {
            var regexBr = new Regex(@"<br.*?\/>");
            var result = regexBr.Replace(body, "\n")
                .Replace("&nbsp;", string.Empty);
            return result;
        }
    }
}
