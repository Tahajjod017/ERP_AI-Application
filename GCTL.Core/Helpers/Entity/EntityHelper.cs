using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.Helpers.Entity
{
    public static class EntityHelper
    {
        public static void Create<TEntity, TModel>(TEntity entity, TModel model)
            where TEntity : class
            where TModel : class
        {
            if (model == null || entity == null)
                return; // Exit silently if either is null

            var modelType = typeof(TModel);
            var entityType = typeof(TEntity);

            // Define specific properties to update
            var propertiesToUpdate = new string[] { "CreatedBy", "CreatedAt", "LIP", "LMAC" };

            foreach (var propertyName in propertiesToUpdate)
            {
                var entityProperty = entityType.GetProperty(propertyName);
                var modelProperty = modelType.GetProperty(propertyName);

                if (entityProperty != null && entityProperty.CanWrite)
                {
                    object value = propertyName == "CreatedAt"
                        ? DateTime.UtcNow
                        : modelProperty?.GetValue(model);

                    entityProperty.SetValue(entity, value);
                }
            }
        }


        public static void Update<TEntity, TModel>(TEntity entity, TModel model)
            where TEntity : class
            where TModel : class
        {
            if (model == null || entity == null)
                return; // Exit silently if either is null

            var modelType = typeof(TModel);
            var entityType = typeof(TEntity);

            // Define specific properties to update
            var propertiesToUpdate = new string[] { "UpdatedBy", "UpdatedAt", "LIP", "LMAC" };

            foreach (var propertyName in propertiesToUpdate)
            {
                var entityProperty = entityType.GetProperty(propertyName);
                var modelProperty = modelType.GetProperty(propertyName);

                if (entityProperty != null && entityProperty.CanWrite)
                {
                    object value = propertyName == "UpdatedAt"
                        ? DateTime.UtcNow
                        : modelProperty?.GetValue(model);

                    entityProperty.SetValue(entity, value);
                }
            }
        }



    }
}
