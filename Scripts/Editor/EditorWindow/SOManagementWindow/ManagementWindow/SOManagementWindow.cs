using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Animancer.Editor;
using Editors.SO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SOManagementWindow : EditorWindow
{

    #region Path OR UssClassName
    
    private readonly string _itemVisual = "item-visual";
    private readonly string _itemLabel = "item-label";
    private readonly string _itemVisualSelect = "item-visual-select";
    private readonly string _itemFilePath = "Assets/00Work/LJS/05_SO/Item";

    #endregion
    
    #region UxmlTemplate

    [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;
    [SerializeField] private VisualTreeAsset _tabSplitView;

    #endregion
    
    /// <summary>
    /// 생성할 SO의 Type들을 모은 Table
    /// </summary>
    [SerializeField] private SOTypeTable _soTypeTable;
    /// <summary>
    /// ScriptableObject를 생성하는 Window
    /// </summary>
    private SOCreator _soCreator;
    
    /// <summary>
    /// 현재 SO Data
    /// </summary>
    private ScriptableObject _currentData;

    #region MyElements

    /// <summary>
    /// CachedEditor 그리는 용도
    /// </summary>
    private IMGUIContainer _cachedGUI;
    private TabView _tabView;
    private Tab _currentTab;
    private VisualElement _itemView;
    private Label _selectedLabel;
    private TextField _fileNameField;
    private VisualElement _selected;
    #endregion
    
    /// <summary>
    /// 생성된 element가 들고 있는 SOData Dict
    /// </summary>
    private Dictionary<string, ScriptableObject> _elementDataDictionary = new();
    /// <summary>
    /// 각각의 Tab이 들고 SOData List Dict
    /// </summary>
    private Dictionary<string, List<ScriptableObject>> _soDataDictionary = new();
    /// <summary>
    /// SO의 실제 경로를 담고 있는 Dict
    /// </summary>
    private Dictionary<ScriptableObject, string> _soPathDict = new();
    /// <summary>
    /// 생성된 Cached Editor을 담고 있는 Dict
    /// </summary>
    private Dictionary<ScriptableObject, Editor> _cachedEditorDict = new();
    /// <summary>
    /// Tab에 따른 Type을 들고 올 수 있는 Dict
    /// </summary>
    private Dictionary<Tab, Type> _typeDict = new();

    /// <summary>
    /// 현재 생성된 Tab이 들어오는 List
    /// </summary>
    private List<Tab> _createdTabList = new();
    /// <summary>
    /// Child가 전부 생성된 TabList 새롭게 자식이 추가 된다면 Remove 됨
    /// 따라서 두번 Child 생성하는 일을 방지함
    /// </summary>
    private List<Tab> _createdChildTabList = new();

    /// <summary>
    /// 기초 설정이 종료 됬는가?
    /// </summary>
    private bool _isEndStartSetting = false;

    public SOManagementWindow()
    {
        if (_soTypeTable == null)
        {
            string[] soGuidArray = AssetDatabase.FindAssets("t:SOTypeTable");
            string path = "s";
            if (soGuidArray.Length > 2)
            {
                Debug.LogError("SOTypeTable More than one has been created. Please reduce it to one.");
                Close();
                return;
            }
        }
    }
    
    public static void ShowExample()
    {
        SOManagementWindow wnd = GetWindow<SOManagementWindow>();
        wnd.titleContent = new GUIContent("SOManagementWindow");
        wnd.maxSize = new Vector2(900, 600);
        wnd.minSize = new Vector2(900, 600);
    }

    /// <summary>
    /// 기본 설정
    /// </summary>
    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        root.style.flexGrow = 1;

        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        _tabView = root.Q<TabView>("SOTypeToolbar");
        _tabView.activeTabChanged += ChangeCurrentTabEvent;

        Tab tab = null;
        for (int i = 0; i < _soTypeTable._typeList.Count; ++i)
        {
            if(_soTypeTable._typeList[i] == null) continue;
            
            var template = _tabSplitView.Instantiate().Q<VisualElement>();
            string tabName = _soTypeTable._typeList[i].Name;

            tab = new Tab();
            tab.Add(template);
            tab.label = tabName;
            tab.name = tabName;

            tab.Q<Button>("MakeBtn").clicked += HandleMakeBtnClickEvent;
            tab.Q<Button>("DeleteBtn").clicked += HandleDeleteBtnClickEvent;
            tab.Q<Button>("RenameBtn").clicked += HandleRenameBtnClickEvent;

            if (_currentTab == null) _currentTab = tab;
            _tabView.Add(tab);
            _typeDict.Add(tab, _soTypeTable._typeList[i]);
            _createdTabList.Add(tab);
            
            ViewTabSetting(tab, _soTypeTable._typeList[i].Name);
        }

        _isEndStartSetting = true;
    }

    /// <summary>
    /// Tab의 그릴 것을 모두 그려주는 함수 FindAssets 함수를 활용하여 특정 SO를 전부 가져온다.
    /// 그리고 가져온 GUID를 경로로 변환하여 불러온뒤 List와 Dict에 저장시켜준다.
    /// 그리고 가져온 SO을 통해서 ViewItem 함수를 호출함
    /// </summary>
    /// <param name="tab">그릴 Tab</param>
    /// <param name="type">Scriptable Object Type to String</param>
    private void ViewTabSetting(Tab tab, string type)
    {
        if(_createdChildTabList.Contains(tab)) return;
        
        _createdChildTabList.Add(tab);
        List<ScriptableObject> spawnSoList = new();
        _itemView = tab.Q<VisualElement>("ItemVisualList");
        
        StringBuilder strBuilder = new StringBuilder();
        strBuilder.Append("t:");
        strBuilder.Append(type);
        string[] soPathArray = AssetDatabase.FindAssets(strBuilder.ToString());
        string path = "";
        for (int i = 0; i < soPathArray.Length; ++i)
        {
            path = AssetDatabase.GUIDToAssetPath(soPathArray[i]);
            ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            spawnSoList.Add(so);
            _soPathDict.Add(so, path);
            if(so != null)
                ViewItem(so.name, so);
        }
        _soDataDictionary.Add(type, spawnSoList);
    }
    
    /// <summary>
    /// 실제 표기될 SO 항목들을 그려주는 함수 실제로 Element과 Label를 생성하고 USS Class를 넣어준다.
    /// 그 뒤 마우스 다운 이벤트를 등록해준다.
    /// </summary>
    /// <param name="vName">이름</param>
    /// <param name="so">데이터</param>
    public void ViewItem(string vName, ScriptableObject so){
        VisualElement element = new VisualElement();
        element.AddToClassList(_itemVisual);
        element.name = vName;
        element.tooltip = GUID.Generate().ToString();
        _elementDataDictionary.Add(element.tooltip, so);
        
        // Mouse Down Event
        element.RegisterCallback<PointerDownEvent>(OnPointerDownInElementEvent);
        
        Label label = new Label();
        label.name = "label";
        label.AddToClassList(_itemLabel);
        label.text = vName;
        
        element.Add(label);
        _itemView.Add(element);
    }

    #region EventMethod
    
    /// <summary>
    /// Tab이 변경 되었을 때 호출되는 함수
    /// 현재 탭을 변경해준다.
    /// </summary>
    /// <param name="arg1">변경 되기 전 Tab</param>
    /// <param name="arg2">변경 후 Tab</param>
    private void ChangeCurrentTabEvent(Tab arg1, Tab arg2)
    {
        if (!_isEndStartSetting) return;
        _currentTab = arg2;
        _selectedLabel = _currentTab.Q<Label>("NameLabel");
        
        // ViewTabSetting(_tabView.activeTab, 
        //     _soDataDictionary[_tabView.activeTab.label][0].GetType().Name);
    }
    
    /// <summary>
    /// 선택 되었을 때 USS Class를 선택 전용 USS Class로 바꿔준다.
    /// 그 뒤 CachedEditor와 연결시켜줌
    /// </summary>
    /// <param name="evt">이벤트에서 반환된 정보들</param>
    private void OnPointerDownInElementEvent(PointerDownEvent evt)
    {
        if (_selectedLabel == null)
            _selectedLabel = _currentTab.Q<Label>("NameLabel");
        
        foreach(var item in _soDataDictionary[_currentTab.label]){
            VisualElement element = _currentTab.Q<VisualElement>(item.name);
            
            _cachedGUI = _currentTab.Q<IMGUIContainer>();
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
                _selectedLabel.text = item.name;
                _selected = element;
                
                ValueListBinding(item);
            }
        }
    }
    
    private void HandleRenameBtnClickEvent()
    {
        if (_currentTab == null)
        {
            Debug.LogError("Tab is not select");
            return;
        }
        
        if(_selected == null) return;
        
        string path = _soPathDict[_currentData];
        ScriptableObject changeTarget = AssetDatabase.LoadAssetAtPath<ScriptableObject>(
            path);

        AssetDatabase.RenameAsset($"path",
            _fileNameField.text);
        
        _itemView.Remove(_selected);
        ViewItem(changeTarget.name, changeTarget);
    }

    private void HandleDeleteBtnClickEvent()
    {
        if (_currentTab == null)
        {
            Debug.LogError("Tab is not select");
            return;
        }
        
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

        VisualElement deleteElement = _itemView.Q<VisualElement>(_selected.name);
        _itemView.Remove(deleteElement);

        _elementDataDictionary.Remove(_selected.name);
        _soDataDictionary[_currentTab.label].Remove(changeTarget);
        
        AssetDatabase.DeleteAsset(path);
        EditorUtility.SetDirty(changeTarget);
        AssetDatabase.SaveAssets();

        Editor cachedEditor = _cachedEditorDict[changeTarget];
        if (cachedEditor != null)
        {
            _cachedEditorDict.Remove(changeTarget);
            DestroyImmediate(cachedEditor);
        }
    }

    private void HandleMakeBtnClickEvent()
    {
        if (_currentTab == null)
        {
            Debug.LogError("Tab is not select");
            return;
        }
        
        Type soType = _typeDict[_currentTab];
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
        _soDataDictionary[_currentTab.label].Add(item);
        _soPathDict.Add(item, AssetDatabase.GetAssetPath(item));
        ViewItem(item.name, item);
    }
    
    #endregion
    
    /// <summary>
    /// SO를 가져와 CachedEditor를 사용해서 IMGUIConationer에 표기 시켜줌
    /// </summary>
    /// <param name="item"> 현재 표기 할 SO Data</param>
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

    public void OnDisable()
    {
        for (int i = 0; i < _createdTabList.Count; i++)
        {
            _createdTabList[i].Q<Button>("MakeBtn").clicked -= HandleMakeBtnClickEvent;
            _createdTabList[i].Q<Button>("DeleteBtn").clicked -= HandleDeleteBtnClickEvent;
            _createdTabList[i].Q<Button>("RenameBtn").clicked -= HandleRenameBtnClickEvent;
        }
    }
}
