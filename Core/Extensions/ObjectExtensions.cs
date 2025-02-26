namespace Core.Extensions
{
    public static class ObjectExtensions
    {
        public static Dictionary<string, object> GetParameters<T>(this T entity)
        {
            var parameters = new Dictionary<string, object>();
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                var propertyName = property.Name;
                var propertyValue = property.GetValue(entity);
                if (propertyValue != null) parameters[propertyName] = propertyValue;
            }

            return parameters;
        }
    }
}