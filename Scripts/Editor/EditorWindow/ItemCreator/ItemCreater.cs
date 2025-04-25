#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using LJS.Item;
using LJS.Item.Effect;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemCreater : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    [SerializeField]
    private StyleSheet _styleSheet;
    [SerializeField]
    private ItemDataTableSO _itemDataTable;
    
    public static Action<string> ShowItemEvent;
    
    private readonly string _itemVisual = "item-visual";
    private readonly string _itemLabel = "item-label";
    private readonly string _itemVisualSelect = "item-visual-select";
    private readonly string _itemFilePath = "Assets/00Work/LJS/05_SO/Item";
    
    private static float width = 900;
    private static float height = 700;

    #region Toolkit Elements

    private VisualElement _root;
    private VisualElement _itemView;
    private Label _selectedLabel;
    private TextField _fileNameField;
    private EnumFlagsField _typeOfItem;
    private TextField _descriptionField;
    private ObjectField _spriteField;
    private Toggle _endImmediatelyToggle;
    private IntegerField _EffectEndTime;
    private IntegerField _healCountField;
    private IntegerField _damageCountField;
    private IntegerField _defenceDecreaseField;
    private IntegerField _defenceIncreaseField;
    private IntegerField _strengthField;
    private IntegerField _weaknessField;
    private ScrollView _valueList;
    private ListView _itemListView;
    
    #endregion
    
    [HideInInspector] public VisualElement _selected;
    protected Dictionary<string, Label> _viewLabelDictionary;

    private Editor _cachedEditor;
    
    public void CreateGUI()
    {
        _root = rootVisualElement;
        
        rootVisualElement.style.flexGrow = 1f;
        
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        _root.Add(labelFromUXML);

        #region Get

        Button makeBtn = _root.Q<Button>("MakeBtn");
        Button deleteBtn = _root.Q<Button>("DeleteBtn");
        Button renameBtn = _root.Q<Button>("RenameBtn");
        Button addItemBtn = _root.Q<Button>("AddItemBtn");
        
        _itemView = _root.Q<VisualElement>("ItemVisualList");
        _selectedLabel = _root.Q<Label>("NameLabel");
        _fileNameField = _root.Q<TextField>("FileName");
        _typeOfItem = _root.Q<EnumFlagsField>("TypeOfItem");
        _descriptionField = _root.Q<TextField>("Description");
        _spriteField = _root.Q<ObjectField>("SpriteField");
        _endImmediatelyToggle = _root.Q<Toggle>("EndImmediatelyToggle");
        _EffectEndTime = _root.Q<IntegerField>("EffectEndTime");
        _healCountField = _root.Q<IntegerField>("HealCount");
        _damageCountField = _root.Q<IntegerField>("DamageCount");
        _defenceDecreaseField = _root.Q<IntegerField>("DefenceDesrease");
        _defenceIncreaseField = _root.Q<IntegerField>("DefenceIncrease");
        _strengthField = _root.Q<IntegerField>("Strength");
        _weaknessField = _root.Q<IntegerField>("Weakness");
        _valueList = _root.Q<ScrollView>("ValueList");
        _itemListView = _root.Q<ListView>("ItemListView");
        
        _fileNameField.bindingPath = "itemName";
        _typeOfItem.bindingPath = "typeOfItem";
        _descriptionField.bindingPath = "description";
        _spriteField.bindingPath = "icon";
        _endImmediatelyToggle.bindingPath =  "endImmediately";
        _EffectEndTime.bindingPath = "effectEndTime";
        _healCountField.bindingPath = "healCount";
        _damageCountField.bindingPath = "damageCount";
        _defenceDecreaseField.bindingPath = "defenceDecrease";
        _defenceIncreaseField.bindingPath = "defenceIncrease";
        _strengthField.bindingPath = "strength";
        _weaknessField.bindingPath = "weakness";
        _itemListView.bindingPath = "effectList";
        #endregion

        _valueList.visible = false;
        
        // ViewItem을 Table에 들어있는 Prefab의 갯수만큼 실행
        if (_itemDataTable.ItemList.Count > 0)
        {
            foreach(var item in _itemDataTable.ItemList){
                ViewItem(item.name);
            }
        }

        makeBtn.clicked += HandleMakeBtnClickEvent;
        deleteBtn.clicked += HandleDeleteBtnClickEvent;
        renameBtn.clicked += HandleRenameBtnClickEvent;
        // addItemBtn.clicked += HandleAddItemBtnClickEvent;
    }

    private void HandleAddItemBtnClickEvent()
    {
        ObjectField objectField = new ObjectField();
        objectField.objectType = typeof(ItemEffectBase);
    }

    private void HandleRenameBtnClickEvent()
    {
        ItemSOBase exist =
            AssetDatabase.LoadAssetAtPath<ItemSOBase>(
                $"{_itemFilePath}/{_selected.name}.asset");
        
        exist.itemName = _fileNameField.text;
        AssetDatabase.RenameAsset($"{_itemFilePath}/{_selected.name}.asset",
            _fileNameField.text);
        
        _itemView.Remove(_selected);
        ViewItem(_fileNameField.text);
    }

    private void HandleDeleteBtnClickEvent()
    {
        if (_selected == null)
        {
            Debug.LogError("Select None");
            return;
        }
        
        // 선택된 Item 삭제
        ItemSOBase item = AssetDatabase.LoadAssetAtPath<ItemSOBase>($"{_itemFilePath}/{_selected.name}.asset");

        VisualElement deleteElement = _itemView.Q<VisualElement>(_selected.name);
        _itemView.Remove(deleteElement);

        _itemDataTable.ItemList.Remove(item);
        AssetDatabase.DeleteAsset($"{_itemFilePath}/{_selected.name}.asset");
        EditorUtility.SetDirty(_itemDataTable);
        AssetDatabase.SaveAssets();
        
        _valueList.visible = false;
    }

    private void HandleMakeBtnClickEvent()
    {
        Guid typeGuid = Guid.NewGuid();
        ItemSOBase newItem = ScriptableObject.CreateInstance<ItemSOBase>();
        newItem.name = typeGuid.ToString();
        newItem.itemName = typeGuid.ToString();
        
        AssetDatabase.CreateAsset(newItem,
            $"{_itemFilePath}/{newItem.name}.asset");
        
        
        _itemDataTable.ItemList.Add(newItem);
        
        Debug.
            Log($"Scucess Create Item, Name : {newItem.name} Path : {_itemFilePath}/{newItem.name}.asset");
        
        EditorUtility.SetDirty(_itemDataTable);
        AssetDatabase.SaveAssets();
        
        ViewItem(newItem.name);
    }

    private void OnEnable()
    {
        ShowItemEvent += ViewItem;
        _viewLabelDictionary = new();
    }
    
    private void OnDisable()
    {
        ShowItemEvent -= ViewItem;
        DestroyImmediate(_cachedEditor);
    }

    [MenuItem("Tools/LJS/ItemCreater")]
    public static void ShowExample()
    {
        ItemCreater wnd = GetWindow<ItemCreater>();
        wnd.titleContent = new GUIContent("ItemCreater");
        
        Rect main = EditorGUIUtility.GetMainWindowPosition();
        
        wnd.minSize = new Vector2(width, height);
        wnd.maxSize = new Vector2(width, height);
        wnd.position = new Rect((main.width - width) * 0.5f, (main.height - height) * 0.5f, width, height);
    }
    
    // TableSO에 들어있는 Prefab을 VisualElement으로 표현하는 함수이다.
    public void ViewItem(string vName){
        VisualElement element = new VisualElement();
        element.AddToClassList(_itemVisual);
        element.name = vName;
        
        // Mouse Down Event
        element.RegisterCallback<PointerDownEvent>(ElementPointerDownEvent);

        Label label = new Label();
        label.name = "label";
        label.AddToClassList(_itemLabel);
        label.text = vName;
        
        _viewLabelDictionary.Add(vName, label);
        element.Add(label);
        _itemView.Add(element);
    }
    
    private void ElementPointerDownEvent(PointerDownEvent evt)
    {
        bool selected = false;
        foreach(var item in _itemDataTable.ItemList){
            VisualElement element = _root.Q<VisualElement>(item.name);
            List<string> classNames = element.GetClasses().ToList();
            
            // 모든 SelectEffect가 적용된 VisualElement에 Effect를 제거한다.
            foreach(string str in classNames){
                if(str == _itemVisualSelect){
                    element.AddToClassList(_itemVisual);
                    element.RemoveFromClassList(_itemVisualSelect);
                }
            }
            
            // 클릭한 위치의 VisualElement와 현재 Element가 같다면? 그 VisualElement에 Effect를 부여하고
            // Toolbar Label의 Value를 바꾸어 준다.
            if(evt.currentTarget.GetHashCode() == element.GetHashCode()){
                _selected = element;
                element.RemoveFromClassList(_itemVisual);
                element.AddToClassList(_itemVisualSelect);
                
                _selectedLabel.text = element.name;
                selected = true;
                ValueListBinding(item);
            }
        }

        if (selected) _valueList.visible = true;
        else _valueList.visible = false;
    }

    private void ValueListBinding(ItemSOBase item)
    {
        // Binding
        SerializedObject serializedObject = new SerializedObject(item);
        _root.Bind(serializedObject);
        
        _fileNameField.Bind(serializedObject);
        _typeOfItem.Bind(serializedObject);
        _descriptionField.Bind(serializedObject);
        _spriteField.Bind(serializedObject);
        _endImmediatelyToggle.Bind(serializedObject);
        _EffectEndTime.Bind(serializedObject);
        _healCountField.Bind(serializedObject);
        _damageCountField.Bind(serializedObject);
        _defenceDecreaseField.Bind(serializedObject);
        _defenceIncreaseField.Bind(serializedObject);
        _strengthField.Bind(serializedObject);
        _weaknessField.Bind(serializedObject);
        _itemListView.Bind(serializedObject);
    }
}
#endif
