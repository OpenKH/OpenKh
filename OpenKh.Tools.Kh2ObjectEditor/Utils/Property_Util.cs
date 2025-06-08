using System.Linq;

namespace OpenKh.Tools.Kh2ObjectEditor.Utils
{
    public class Property_Util
    {
        // Copies the properties of the source object into the target object given they share name and type
        public static void CopyProperties<TSource, TTarget>(TSource source, TTarget target)
        {
            var sourceProps = typeof(TSource).GetProperties();
            var targetProps = typeof(TTarget).GetProperties();

            foreach (var sourceProp in sourceProps)
            {
                var targetProp = targetProps.FirstOrDefault(p => p.Name == sourceProp.Name && p.PropertyType == sourceProp.PropertyType);
                if (targetProp != null && targetProp.CanWrite)
                {
                    targetProp.SetValue(target, sourceProp.GetValue(source));
                }
            }
        }
    }
}
