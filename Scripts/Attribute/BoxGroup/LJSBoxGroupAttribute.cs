using UnityEditor.UIElements;
using UnityEngine;

namespace LJS.Editing
{
    public class LJSBoxGroupAttribute : PropertyAttribute
    {
        public ObjectField Field { get; private set; }

        public LJSBoxGroupAttribute(ObjectField field)
        {
            Field = field;
        }
    }
}
