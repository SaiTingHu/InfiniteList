using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HT.InfiniteList
{
    public static class InfiniteListEditorUtility
    {
        private const string UILayerName = "UI";
        private const string StandardSpritePath = "UI/Skin/UISprite.psd";
        private const string BackgroundSpritePath = "UI/Skin/Background.psd";
        private const string InputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";
        private const string KnobPath = "UI/Skin/Knob.psd";
        private const string CheckmarkPath = "UI/Skin/Checkmark.psd";
        private const string DropdownArrowPath = "UI/Skin/DropdownArrow.psd";
        private const string MaskPath = "UI/Skin/UIMask.psd";

        private static DefaultControls.Resources StandardResources;
        
        private static DefaultControls.Resources GetStandardResources()
        {
            if (StandardResources.standard == null)
            {
                StandardResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>(StandardSpritePath);
                StandardResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>(BackgroundSpritePath);
                StandardResources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>(InputFieldBackgroundPath);
                StandardResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>(KnobPath);
                StandardResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>(CheckmarkPath);
                StandardResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>(DropdownArrowPath);
                StandardResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>(MaskPath);
            }
            return StandardResources;
        }

        private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null && SceneView.sceneViews.Count > 0)
                sceneView = SceneView.sceneViews[0] as SceneView;

            if (sceneView == null || sceneView.camera == null)
                return;

            Vector2 localPlanePosition;
            Camera camera = sceneView.camera;
            Vector3 position = Vector3.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPlanePosition))
            {
                localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
                localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;

                localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
                localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);

                position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
                position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;

                Vector3 minLocalPosition;
                minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
                minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;

                Vector3 maxLocalPosition;
                maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
                maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;

                position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
                position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
            }

            itemTransform.anchoredPosition = position;
            itemTransform.localRotation = Quaternion.identity;
            itemTransform.localScale = Vector3.one;
        }

        private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                parent = GetOrCreateCanvasGameObject();
            }

            string uniqueName = GameObjectUtility.GetUniqueNameForSibling(parent.transform, element.name);
            element.name = uniqueName;
            Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);
            Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
            GameObjectUtility.SetParentAndAlign(element, parent);
            if (parent != menuCommand.context)
                SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());

            Selection.activeGameObject = element;
        }

        private static GameObject GetOrCreateCanvasGameObject()
        {
            GameObject selectedGo = Selection.activeGameObject;

            Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
            if (canvas != null && canvas.gameObject.activeInHierarchy)
                return canvas.gameObject;

            canvas = Object.FindObjectOfType(typeof(Canvas)) as Canvas;
            if (canvas != null && canvas.gameObject.activeInHierarchy)
                return canvas.gameObject;

            return CreateNewUI();
        }

        private static GameObject CreateNewUI()
        {
            var root = new GameObject("Canvas");
            root.layer = LayerMask.NameToLayer(UILayerName);
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);

            CreateEventSystem(false);
            return root;
        }

        private static void CreateEventSystem(bool select)
        {
            CreateEventSystem(select, null);
        }

        private static void CreateEventSystem(bool select, GameObject parent)
        {
            var esys = Object.FindObjectOfType<EventSystem>();
            if (esys == null)
            {
                var eventSystem = new GameObject("EventSystem");
                GameObjectUtility.SetParentAndAlign(eventSystem, parent);
                esys = eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();

                Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
            }

            if (select && esys != null)
            {
                Selection.activeGameObject = esys.gameObject;
            }
        }

        /// <summary>
        /// 新建无限列表滚动视野
        /// </summary>
        [MenuItem("GameObject/UI/Scroll View - Infinite List", false, 2063)]
        private static void AddInfiniteListScrollView(MenuCommand menuCommand)
        {
            GameObject go = DefaultControls.CreateScrollView(GetStandardResources());
            go.name = "Scroll View - Infinite List";
            PlaceUIElementRoot(go, menuCommand);

            ScrollRect scrollRect = go.GetComponent<ScrollRect>();
            Object.DestroyImmediate(scrollRect);
            InfiniteListScrollRect ilScrollRect = go.AddComponent<InfiniteListScrollRect>();
            ilScrollRect.content = scrollRect.content;
            ilScrollRect.content.anchoredPosition = Vector2.zero;
            ilScrollRect.viewport = scrollRect.viewport;
            ilScrollRect.horizontalScrollbar = scrollRect.horizontalScrollbar;
            ilScrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            ilScrollRect.horizontalScrollbarSpacing = -3;
            ilScrollRect.verticalScrollbar = scrollRect.verticalScrollbar;
            ilScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            ilScrollRect.verticalScrollbarSpacing = -3;

            GameObject elementTemplate = new GameObject("ElementTemplate");
            elementTemplate.AddComponent<RectTransform>();
            elementTemplate.AddComponent<InfiniteListElement>();
            elementTemplate.transform.SetParent(ilScrollRect.content);
            elementTemplate.GetComponent<RectTransform>().DockToLeftUp();
            elementTemplate.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
            elementTemplate.SetActive(false);
            ilScrollRect.ElementTemplate = elementTemplate;
        }

        /// <summary>
        /// 设置RectTransform为全屏拉伸
        /// </summary>
        public static void DockToStretchFull(this RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        /// <summary>
        /// 设置RectTransform为靠左上角停靠
        /// </summary>
        public static void DockToLeftUp(this RectTransform rectTransform)
        {
            rectTransform.anchoredPosition3D = Vector3.zero;
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
        }
    }
}