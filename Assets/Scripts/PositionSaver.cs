using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DefaultNamespace
{
#if UNITY_EDITOR
	[AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyAttribute : PropertyAttribute
    {

    }

    [UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyPropertyDrawer : UnityEditor.PropertyDrawer
    {
	    public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
	    {
		    GUI.enabled = false;
		    UnityEditor.EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
        }

  //      public override VisualElement CreatePropertyGUI(UnityEditor.SerializedProperty property)
		//{
		//	var element = base.CreatePropertyGUI(property)
		//	?? new UnityEditor.UIElements.PropertyField(property);
		//	element.SetEnabled(false);
		//	return element;
		//}
    }
#endif
	public class PositionSaver : MonoBehaviour
	{
        [Serializable]
        public struct Data
		{
			public Vector3 Position;
			public float Time;
		}

		[SerializeField, Tooltip("для заполнения этого поля нужно воспользоваться контекстным меню в инспекторе и командой \"Create File\"")]
		[ReadOnly]
		private TextAsset _json;

        [field: SerializeField, HideInInspector]
        public List<Data> Records { get; private set; }

		private void Awake()
		{

			//todo comment: Что будет, если в теле этого условия не сделать выход из метода?
			//
			if (_json == null)
			{
				gameObject.SetActive(false);
				Debug.LogError("Please, create TextAsset and add in field _json");
				
			}
			
			JsonUtility.FromJsonOverwrite(_json.text, this);
			//todo comment: Для чего нужна эта проверка (что она позволяет избежать)?
			if (Records == null)
				Records = new List<Data>(10);
		}

		private void OnDrawGizmos()
		{
			//todo comment: Зачем нужны эти проверки (что они позволляют избежать)?
			if (Records == null || Records.Count == 0) return;
			var data = Records;
			var prev = data[0].Position;
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(prev, 0.3f);
			//todo comment: Почему итерация начинается не с нулевого элемента?
			for (int i = 1; i < data.Count; i++)
			{
				var curr = data[i].Position;
				Gizmos.DrawWireSphere(curr, 0.3f);
				Gizmos.DrawLine(prev, curr);
				prev = curr;
			}
		}


#if UNITY_EDITOR
		[ContextMenu("Create File")]
		private void CreateFile()
		{
			//todo comment: Что происходит в этой строке?
			var stream = File.Create(Path.Combine(Application.dataPath, "Path.txt"));
			//todo comment: Подумайте для чего нужна эта строка? (а потом проверьте догадку, закомментировав) 
			stream.Dispose();
			UnityEditor.AssetDatabase.Refresh();
			//В Unity можно искать объекты по их типу, для этого используется префикс "t:"
			//После нахождения, Юнити возвращает массив гуидов (которые в мета-файлах задаются, например)
			var guids = UnityEditor.AssetDatabase.FindAssets("t:TextAsset");
			foreach (var guid in guids)
			{
				//Этой командой можно получить путь к ассету через его гуид
				var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
				//Этой командой можно загрузить сам ассет
				var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
				//todo comment: Для чего нужны эти проверки?
				if(asset != null && asset.name == "Path")
				{
					_json = asset;
					UnityEditor.EditorUtility.SetDirty(this);
					UnityEditor.AssetDatabase.SaveAssets();
					UnityEditor.AssetDatabase.Refresh();
					//todo comment: Почему мы здесь выходим, а не продолжаем итерироваться?
					return;
				}
			}
		}

        private void OnDestroy()
        {
		    
        }
#endif
    }

}