#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TagSelectorAttribute))]
public class TagSelectorPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.String)
        {
            EditorGUI.BeginProperty(position, label, property);

            var attrib = attribute as TagSelectorAttribute;

            if (attrib.UseDefaultTagFieldDrawer)
            {
                property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
            }
            else
            {
                // Generate the tag list + custom options
                var tagList = UnityEditorInternal.InternalEditorUtility.tags;
                var tagListWithCustom = new string[tagList.Length + 2];
                tagListWithCustom[0] = "<No Tag>";
                tagListWithCustom[1] = "<All Tags>";
                for (int i = 0; i < tagList.Length; i++)
                {
                    tagListWithCustom[i + 2] = tagList[i];
                }

                // Get the current index of the selected tag
                int index = 0;
                for (int i = 0; i < tagListWithCustom.Length; i++)
                {
                    if (tagListWithCustom[i] == property.stringValue)
                    {
                        index = i;
                        break;
                    }
                }

                // Draw the dropdown
                index = EditorGUI.Popup(position, label.text, index, tagListWithCustom);
                property.stringValue = tagListWithCustom[index];
            }

            EditorGUI.EndProperty();
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
#endif