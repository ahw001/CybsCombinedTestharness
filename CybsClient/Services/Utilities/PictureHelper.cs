using Microsoft.AspNetCore.Components;

namespace CybsClient.Services.Utilities;

public static class PictureHelper
{
    /// <summary>
    /// Renders a product picture as an &lt;img&gt; element.
    /// SVG markup is base64-encoded into a data URI so it is sandboxed from
    /// the host document (no CSS variable leakage, no duplicate filter IDs,
    /// and standard img sizing via CSS).
    /// Plain URLs are used directly as the src.
    /// </summary>
    public static MarkupString RenderProductImage(string? picture, string style)
    {
        if (string.IsNullOrWhiteSpace(picture))
            return default;

        string src;
        if (picture.TrimStart().StartsWith('<'))
        {
            var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(picture));
            src = $"data:image/svg+xml;base64,{base64}";
        }
        else
        {
            src = picture;
        }

        return new MarkupString($"<img src=\"{src}\" style=\"{style}\" alt=\"Product image\" />");
    }
}
