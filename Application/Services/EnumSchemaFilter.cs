using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            var enumValues = context.Type
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(field => new
                {
                    Value = field.GetValue(null)?.ToString(), // Enum value (e.g., pm_card_visa)
                    DisplayName = field.GetCustomAttribute<DisplayAttribute>()?.Name ?? field.Name // Friendly name (e.g., Visa)
                });

            schema.Enum.Clear();

            foreach (var enumValue in enumValues)
            {
                schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(enumValue.DisplayName));
            }
        }
    }
}