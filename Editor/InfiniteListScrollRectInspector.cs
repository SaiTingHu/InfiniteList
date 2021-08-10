using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace HT.InfiniteList
{
    [CustomEditor(typeof(InfiniteListScrollRect), true)]
    [CanEditMultipleObjects]
    public class InfiniteListScrollRectInspector : ScrollRectEditor
    {
        private InfiniteListScrollRect _target;
        private InfiniteListScrollRect.Direction _lastDirection;

        protected override void OnEnable()
        {
            base.OnEnable();

            _target = target as InfiniteListScrollRect;
            _lastDirection = _target.ListingDirection;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            GameObject element = EditorGUILayout.ObjectField("Element Template", _target.ElementTemplate, typeof(GameObject), true) as GameObject;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Set InfiniteList ElementTemplate");
                _target.ElementTemplate = element;
                EditorUtility.SetDirty(target);
            }

            EditorGUI.BeginChangeCheck();
            InfiniteListScrollRect.Direction direction = (InfiniteListScrollRect.Direction)EditorGUILayout.EnumPopup("Listing Direction", _target.ListingDirection);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Set InfiniteList ListingDirection");
                _target.ListingDirection = direction;
                EditorUtility.SetDirty(target);

                if (_lastDirection != _target.ListingDirection)
                {
                    _lastDirection = _target.ListingDirection;
                    if (_lastDirection == InfiniteListScrollRect.Direction.Vertical)
                    {
                        _target.content.offsetMin = Vector2.zero;
                        _target.content.offsetMax = new Vector2(0, 200);
                        _target.content.anchorMin = new Vector2(0, 1);
                        _target.content.anchorMax = new Vector2(1, 1);
                        _target.content.pivot = new Vector2(0, 1);
                        _target.content.anchoredPosition = Vector2.zero;
                    }
                    else
                    {
                        _target.content.offsetMin = Vector2.zero;
                        _target.content.offsetMax = new Vector2(200, 0);
                        _target.content.anchorMin = new Vector2(0, 0);
                        _target.content.anchorMax = new Vector2(0, 1);
                        _target.content.pivot = new Vector2(0, 0.5f);
                        _target.content.anchoredPosition = Vector2.zero;
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            int height = EditorGUILayout.IntField(_target.ListingDirection == InfiniteListScrollRect.Direction.Vertical ? "Height" : "Width", _target.Height);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Set InfiniteList Height");
                _target.Height = height;
                EditorUtility.SetDirty(target);
            }

            EditorGUI.BeginChangeCheck();
            int interval = EditorGUILayout.IntField("Interval", _target.Interval);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Set InfiniteList Interval");
                _target.Interval = interval;
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.Space();

            base.OnInspectorGUI();
        }
    }
}