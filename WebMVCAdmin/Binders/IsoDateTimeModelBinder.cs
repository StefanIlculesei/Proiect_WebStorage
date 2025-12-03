using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace WebMVCAdmin.Binders
{
    public class IsoDateTimeModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(value))
            {
                return Task.CompletedTask;
            }

            // Try parsing ISO 8601 format (yyyy-MM-ddTHH:mm) which is what datetime-local sends
            // We also try standard formats just in case
            var formats = new[] 
            { 
                "yyyy-MM-ddTHH:mm", 
                "yyyy-MM-ddTHH:mm:ss", 
                "yyyy-MM-dd" 
            };

            if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
            {
                bindingContext.Result = ModelBindingResult.Success(dateTime);
            }
            else if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                bindingContext.Result = ModelBindingResult.Success(dateTime);
            }
            else
            {
                // Let the default binder handle it or fail
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Invalid date format");
            }

            return Task.CompletedTask;
        }
    }
}
