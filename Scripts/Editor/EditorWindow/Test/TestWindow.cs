using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Editors.SO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public struct TabInfo
{
    public VisualElement tabElement;
    public Type tabType;
    public Dictionary<VisualElement ,ScriptableObject> getSODict;
}

public class TestWindow : EditorWindow
{
    #region PathORUSSClass

    private readonly string _tabSelectClassName = "tab-select";
    private readonly string _tabClassName = "tab";
    private readonly string _tabLabelClassName = "tab-label";
    private readonly string _itemVisual = "item-visual";
    private readonly string _itemLabel = "item-label";
    private readonly string _itemVisualSelect = "item-visual-select";
    
    #endregion
    
    #region MyElements
    
    private ScrollView _tabScrollView;
    private VisualElement _rootElement;
    private VisualElement _currentSOView;
    private VisualElement _currentSelectTab;
    private IMGUIContainer _cachedGUI;
    private Label _selectedLabel;
    private VisualElement _selected;
    private TextField _fileNameField;
    
    #endregion
    
    #region UxmlTemplate
    
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    [SerializeField] private VisualTreeAsset _tabSplitView;
    
    #endregion
    
    /// <summary>
    /// 생성할 SO의 Type들을 모은 Table
    /// </summary>
    [SerializeField] private SOTypeTable _soTypeTable;

    [SerializeField] private SOCreator _soCreator; 

    private List<TabInfo> _tabList = new();
    private Dictionary<TabInfo, VisualElement> _soInfoViewDict = new();
    /// <summary>
    /// 생성된 Cached Editor을 담고 있는 Dict
    /// </summary>
    private Dictionary<ScriptableObject, Editor> _cachedEditorDict = new();
    /// <summary>
    /// SO의 실제 경로를 담고 있는 Dict
    /// </summary>
    private Dictionary<ScriptableObject, string> _soPathDict = new();
    /// <summary>
    /// 현재 SO Data
    /// </summary>
    private ScriptableObject _currentData;
    /// <summary>
    /// 현재 선택된 TabInfo
    /// </summary>
    private TabInfo _currentTab;

    [MenuItem("Editor/LJS/SoManagementWindow")]
    public static void ShowExample()
    {
        TestWindow wnd = GetWindow<TestWindow>();
        wnd.titleContent = new GUIContent("TestWindow");
        wnd.minSize = new Vector2(900, 600);
        wnd.maxSize = new Vector2(900, 600);
    }

    /// <summary>
    /// 기본 초기화 함수
    /// 기본적인 세팅과 Tab 생성 Button 이벤트 구독 ViewElement 생성 등 여러가지 작업을 한다.
    /// </summary>
    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        root.style.flexGrow = 1;
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        _tabScrollView = root.Q<ScrollView>("TabScrollView");
        _rootElement = root.Q<VisualElement>("SoManagement");

        foreach (var type in _soTypeTable._typeList)
        {
            TabInfo tab = new TabInfo();
            tab.tabType = type;
            tab.getSODict = new();
            
            var template = _tabSplitView.Instantiate().Q<VisualElement>();
            VisualElement itemVisualList = template.Q<VisualElement>("ItemVisualList"); 
            
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append("t:");
            strBuilder.Append(type);
            string[] soPathArray = AssetDatabase.FindAssets(strBuilder.ToString());
            string path = "";
            for (int i = 0; i < soPathArray.Length; ++i)
            {
                path = AssetDatabase.GUIDToAssetPath(soPathArray[i]);
                ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                VisualElement view = ViewElementMake(so);
                tab.getSODict.Add(view, so);
                _soPathDict.Add(so, path);
                itemVisualList.Add(view);
            }

            VisualElement tabElement = TabVisualMake(type.Name);
            
            tab.tabElement = tabElement;
            
            _tabList.Add(tab);
            _tabScrollView.Add(tabElement);
            _soInfoViewDict.Add(tab, template);            
            template.Q<Button>("MakeBtn").clicked += HandleMakeBtnClickEvent;
            template.Q<Button>("DeleteBtn").clicked += HandleDeleteBtnClickEvent;
            template.Q<Button>("RenameBtn").clicked += HandleRenameBtnClickEvent;
        }
    }

    /// <summary>
    /// Tab의 Visual를 만드어주는 함수
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private VisualElement TabVisualMake(string name)
    {
        VisualElement tabVisual = new VisualElement();

        tabVisual.AddToClassList(_tabClassName);
        tabVisual.name = name;
        tabVisual.tooltip = GUID.Generate().ToString();
        
        // Mouse Down Event
        tabVisual.RegisterCallback<PointerDownEvent>(OnPointerDownInTabEvent);
        
        Label label = new Label();
        label.name = "label";
        label.AddToClassList(_tabLabelClassName);
        label.text = name;
        
        tabVisual.Add(label);

        return tabVisual;
    }

    /// <summary>
    /// 실제 보이는 ViewElement를 만들어주는 함수
    /// </summary>
    /// <param name="soData"> 만들 View의 Data </param>
    /// <returns></returns>
    private VisualElement ViewElementMake(ScriptableObject soData)
    {
        VisualElement element = new VisualElement();
        element.AddToClassList(_itemVisual);
        element.name = soData.name;
        element.tooltip = GUID.Generate().ToString();
        
        // Mouse Down Event
        element.RegisterCallback<PointerDownEvent>(OnPointerDownInElementEvent);
        
        Label label = new Label();
        label.name = "label";
        label.AddToClassList(_itemLabel);
        label.text = soData.name;
        
        element.Add(label);

        return element;
    }

    #region EventMethod
    
        /// <summary>
        /// Tab 위에 마우스가 클릭 되었을 때 발생되는 이벤트
        /// 현재 Tab을 변경시켜주고 Select USSClass 넣어준다.
        /// </summary>
        /// <param name="evt"></param>
        private void OnPointerDownInTabEvent(PointerDownEvent evt)
        {
            TabInfo tab = _tabList.Find(x => x.tabElement.GetHashCode()
                                             == evt.currentTarget.GetHashCode());
            _currentTab = tab;
            VisualElement tabElement = tab.tabElement;
            
            if (_currentSOView != null)
            {
                _currentSelectTab.RemoveFromClassList(_tabSelectClassName);
                _currentSelectTab.AddToClassList(_tabClassName);
                _rootElement.Remove(_currentSOView);
            }
            
            _currentSOView = _soInfoViewDict[tab];
            _currentSelectTab = tabElement;
            _rootElement.Add(_currentSOView);
            
            _currentSelectTab.RemoveFromClassList(_tabClassName);
            _currentSelectTab.AddToClassList(_tabSelectClassName);
            
            _fileNameField = _currentSOView.Q<TextField>("FileNameField");
            _selectedLabel = _currentSOView.Q<Label>("NameLabel");
        }
        
        /// <summary>
        /// Tab 내에 항목을 클릭했을 때 발행되는 이벤트
        /// Cached Editor 연결해주고 Select 상태로 USSClass 넣어줌
        /// </summary>
        /// <param name="evt"></param>
        private void OnPointerDownInElementEvent(PointerDownEvent evt)
        {
            if (_selectedLabel == null)
                _selectedLabel = _currentSelectTab.Q<Label>("NameLabel");
        
            foreach(var item in _currentTab.getSODict){
                VisualElement element = _currentSOView.Q<VisualElement>(item.Value.name);
            
                _cachedGUI = _currentSOView.Q<IMGUIContainer>();
                List<string> classNames = element.GetClasses().ToList();
            
                // 모든 SelectEffect가 적용된 VisualElement에 Effect를 제거한다.
                foreach(string str in classNames){
                    if(str == _itemVisualSelect){
                        element.AddToClassList(_itemVisual);
                        element.RemoveFromClassList(_itemVisualSelect);
                    }
                }

                // 클릭한 위치의 VisualElement에 Effect를 부여하고
                // Toolbar Label의 Value를 바꾸어 준다.
                if (evt.currentTarget.GetHashCode() == element.GetHashCode())
                {
                    element.RemoveFromClassList(_itemVisual);
                    element.AddToClassList(_itemVisualSelect);
                    _selectedLabel.text = item.Value.name;
                    _selected = element;
                
                    ValueListBinding(item.Value);
                    _fileNameField.value = "";
                }
            }

        }
        
        /// <summary>
        /// 이름을 변경 버튼 클릭 시 발행하는 이벤트
        /// 경로를 찾고 추적해서 변경해줌 그리고 다시 그려줌
        /// </summary>
        private void HandleRenameBtnClickEvent()
        {
            if (_fileNameField.text.Length <= 0)
            {
                Debug.LogError("The name must be at least 1 character long, and if it is only 1 character, special characters are not allowed.");    
                return;
            }
            
            if(_selected == null) return;
            
            string path = _soPathDict[_currentData];
            ScriptableObject changeTarget = AssetDatabase.LoadAssetAtPath<ScriptableObject>(
                path);
            VisualElement viewTable = _currentSOView.Q<VisualElement>("ItemVisualList");
            
            AssetDatabase.RenameAsset(path, _fileNameField.text);
            changeTarget.name = _fileNameField.text;
            
            viewTable.Remove(_selected);
            viewTable.Add(ViewElementMake(changeTarget));
        }

        /// <summary>
        /// 삭제 버튼 클릭시 발행되는 이벤트
        /// 역으로 경로를 추척하고 그 경로에 있는 SO를 삭제 해준다.
        /// 그리고 현재 Tab에서도 삭제해주고 비주얼도 지워준다.
        /// </summary>
        private void HandleDeleteBtnClickEvent()
        {
            if (_selected == null)
            {
                Debug.LogError("Select None");
                return;
            }
        
            // 선택된 Item 삭제
            string path = _soPathDict[_currentData];
            Debug.Log(path);
            ScriptableObject changeTarget = AssetDatabase.LoadAssetAtPath<ScriptableObject>(
                path);
            VisualElement viewTable = _currentSOView.Q<VisualElement>("ItemVisualList");
            VisualElement deleteElement = viewTable.Q<VisualElement>(_selected.name);
            viewTable.Remove(deleteElement);

            _currentTab.getSODict.Remove(deleteElement);
        
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();

            Editor cachedEditor = _cachedEditorDict[changeTarget];
            if (cachedEditor != null)
            {
                _cachedEditorDict.Remove(changeTarget);
                DestroyImmediate(cachedEditor);
            }

            _selected = null;
            _selectedLabel.text = string.Empty;
        }

        /// <summary>
        /// 만들기 버튼을 클릭 시 발행되는 이벤트
        /// SOCreator를 열어주고 기본 정보들을 전달함.
        /// </summary>
        private void HandleMakeBtnClickEvent()
        {
            Type soType = _currentTab.tabType;
            string path = _soTypeTable.ReturnPath(soType);

            _soCreator = GetWindow<SOCreator>();
            _soCreator.titleContent = new GUIContent("SOCreator");
            _soCreator.minSize = new Vector2(300f, 75f);
            _soCreator.maxSize = new Vector2(300f, 75f);
            _soCreator.SettingInfo(soType, path, HandleCreateSOEvent);
        }

        /// <summary>
        /// 새로운 SO를 생성할 때 호출되는 Method
        /// SO Data Dict에 넣어주고 비주얼을 그려줌
        /// </summary>
        /// <param name="item"></param>
        private void HandleCreateSOEvent(ScriptableObject item)
        {
            VisualElement newView = ViewElementMake(item);
            _currentSOView.Q<VisualElement>("ItemVisualList").Add(newView);
            _currentTab.getSODict.Add(newView, item);
            _soPathDict.Add(item, AssetDatabase.GetAssetPath(item));
        }

        #endregion
    
    private void ValueListBinding(ScriptableObject item)
    {
        Editor cachedEditor = null;
        _cachedGUI.onGUIHandler = () =>
        {
            if (item != null)
            {
                Editor.CreateCachedEditor(item, null, ref cachedEditor);
                if (cachedEditor != null)
                {
                    _currentData = item;
                    cachedEditor.OnInspectorGUI();
                    _cachedEditorDict.TryAdd(item, cachedEditor);
                }
            }
        };
    }

    private void OnDisable()
    {
        foreach (var item in _soInfoViewDict)
        {
            item.Value.Q<Button>("MakeBtn").clicked -= HandleMakeBtnClickEvent;
            item.Value.Q<Button>("DeleteBtn").clicked -= HandleDeleteBtnClickEvent;
            item.Value.Q<Button>("RenameBtn").clicked -= HandleRenameBtnClickEvent;
        }

        foreach (var item in _cachedEditorDict)
        {
            DestroyImmediate(item.Value);
        }
    }
}
