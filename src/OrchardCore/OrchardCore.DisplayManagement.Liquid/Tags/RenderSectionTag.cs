using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Microsoft.AspNetCore.Html;
using OrchardCore.Liquid.Ast;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class RenderSectionTag : ArgumentsTag
    {
        public override async ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, FilterArgument[] args)
        {
            if (!context.AmbientValues.TryGetValue("ThemeLayout", out dynamic layout))
            {
                throw new ArgumentException("ThemeLayout missing while invoking 'render_section'");
            }

            if (!context.AmbientValues.TryGetValue("DisplayHelper", out var item) || !(item is IDisplayHelper displayHelper))
            {
                throw new ArgumentException("DisplayHelper missing while invoking 'render_section'");
            }

            var arguments = (FilterArguments)(await new ArgumentsExpression(args).EvaluateAsync(context)).ToObjectValue();

            var name = arguments["name"].Or(arguments.At(0)).ToStringValue();
            var required = arguments.HasNamed("required") && Convert.ToBoolean(arguments["required"].ToStringValue());
            var zone = layout[name];

            if (required && zone != null && zone.Items.Count == 0)
            {
                throw new InvalidOperationException("Zone not found while invoking 'render_section': " + name);
            }

            IHtmlContent htmlContent = await displayHelper.ShapeExecuteAsync(zone);
            htmlContent.WriteTo(writer, (HtmlEncoder)encoder);
            return Completion.Normal;
        }
    }
}
