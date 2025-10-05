using UnityEngine.UIElements;

namespace QbGameLib_UI.Extension
{
    public static class VisualElementExtension
    {
        public static VisualElement CreateChild(this VisualElement element,  params string[] classNames)
        {
            return element.CreateChild<VisualElement>(classNames);
        }
        
        public static T CreateChild<T>(this VisualElement element, params string[] classNames) where T : VisualElement, new()
        {
            var visualElement = new T();
            visualElement.AddToClassList(classNames).AddTo(element);
            return visualElement;
        }

        public static T AddTo<T>(this T element, VisualElement parent) where T : VisualElement
        {
            parent.Add(element);
            return element;
        }

        public static T AddToClassList<T>(this T element, params string[] classNames) where T : VisualElement
        {
            if (classNames != null)
            {
                foreach (string className in classNames)
                {
                    element.AddToClassList(className);
                }
            }
            return element;
        }
        
        public static T WithManipulator<T>(this T element, IManipulator manipulator) where T : VisualElement
        {
            element.AddManipulator(manipulator);
            return element;
        }
        
    }
}