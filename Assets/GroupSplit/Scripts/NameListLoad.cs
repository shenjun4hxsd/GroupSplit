/*
 *  created by shenjun
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.UI;
using System;

public class NameListLoad : MonoBehaviour
{

    public static NameListLoad Instance;

    #region StartPanel
    [Header("开始面板")]
    public GameObject startPanel;       // 开始界面
    public InputField input;            // 输入框
    public Button startBtn;             // 开始按钮
    public Text warningText;            // 显示警告信息
    public Dropdown fileNameDropdown;
    public Text currentFileNameText;
    public bool m_Loaded = false;       // 文件是否加载完毕
    int currentFileIndex = 0;           // 当前选中的文件列表索引
    bool showFileNamesDropdown = false;
    List<string> fileList = new List<string>();     // 文件列表
    const string warningNoFile = "Warning : 没有找到名单文件，请确认当前目录中包含文件名中带有名单的文本文件";
    const string warningGroupNum = "Warning : 分组数必须小于10";
    //string m_Path
    //{
    //    get
    //    {
    //        List<string> fileNames;
    //        if (TryGetFilesName(out fileNames))
    //        {
    //            return Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + fileNames[0];
    //        }
    //        else
    //        {
    //            return null;
    //        }
    //    }
    //}
    #endregion

    #region GroupPanel
    [Header("分组面板")]
    public GameObject groupPanel;           // 分组界面
    public Text tipsText;                   // 提示文本
    public Button createBtn;                // 生成名单按钮
    public Button quitBtn;                  // 退出按钮
    public Dropdown m_DropDown;             // 名单下拉列表
    public Transform m_DropDownStartTrans;  // 分组定位点
    public Transform m_DropDownParent;      // 分组对象容器
	GroupItem currentGroupItem;             // 当前分组
	GameObject m_GroupPrefab;               // 分组对象预制体
    #endregion

    #region RandomPanel
    [Header("随机面板")]
    public GameObject randomPanel;  // 随机界面
    public Text randomGroupText;
    public Text randomNameResultText;
    public SlotMachineControl slotMachineControl;
    public Button randomOKBtn;
    public Button randomCancelBtn;
    #endregion

    /** 名单原始列表 */
    public List<string> m_NameList = new List<string>();

    public List<GroupItem> dropDowns = new List<GroupItem>();


    #region Message Function
    void Awake()
    {
        Instance = this;

        InitStartPanel();
        InitGroupPanel();
        InitRandomPanel();
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    void Start()
    {
        //StartCoroutine(LoadGropNames());
    }



    void Update()
    {
        if(showFileNamesDropdown)
        {
            currentFileIndex = fileNameDropdown.value;
        }
        currentFileNameText.text = "当前选择分组名单文件 ：<Color=yellow>" + fileNameDropdown.options[currentFileIndex].text + "</Color>";
    }

    #endregion

    #region StartPanel
    /// <summary>
    /// 初始化开始面板
    /// </summary>
    void InitStartPanel()
    {
        startPanel.SetActive(true);
        WarningHide();
        startBtn.onClick.AddListener(OnStartBtn);

        InitFileNamesDropdown();
    }

    /// <summary>
    /// 初始化文件下拉列表
    /// </summary>
    void InitFileNamesDropdown()
    {
        if (TryGetFilesName(out fileList))
        {
            fileNameDropdown.options.Clear();
            List<Dropdown.OptionData> datas = new List<Dropdown.OptionData>();
            foreach (var item in fileList)
            {
                datas.Add(new Dropdown.OptionData(item));
            }
            fileNameDropdown.options.AddRange(datas);
            fileNameDropdown.value = 0;
            fileNameDropdown.RefreshShownValue();

            if (fileList.Count() == 1)
            {
                currentFileIndex = 0;
                fileNameDropdown.gameObject.SetActive(false);
                showFileNamesDropdown = false;
                currentFileNameText.text = "当前选择分组名单文件 ：<Color=yellow>" + fileList[0] + "</Color>";
            }
            else if (fileList.Count() > 1)
            {
                showFileNamesDropdown = true;
                fileNameDropdown.gameObject.SetActive(true);

            }
            else
            {
                Debug.LogError("No File");
            }
        }
        else
        {
            WarningShow(warningNoFile);
        }
    }

    /// <summary>
    /// 初始化界面 根据输入的组 初始化分组界面
    /// </summary>
    void OnStartBtn()
    {
        // 如果没有分组名单，则退出
        if (showFileNamesDropdown == false)
        {
			Application.Quit();
            return;
        }

        int result;
        if (int.TryParse(input.text, out result))
        {
            if (result > 0 && result < 10)
            {
                startPanel.SetActive(false);
                groupPanel.SetActive(true);
                WarningHide();

                for (int i = 0; i < result; i++)
                {
                    RectTransform rTrans = (m_DropDownStartTrans as RectTransform);
                    Vector3 pos = rTrans.localPosition + new Vector3(i % 3 * 185, -i / 3 * 140, 0);
                    GameObject groupItemObj = Instantiate(m_GroupPrefab);
                    GroupItem item = groupItemObj.GetComponent<GroupItem>();
                    item.SetGroupName("小组" + (i + 1));

                    groupItemObj.transform.SetParent(m_DropDownParent);
                    groupItemObj.transform.localPosition = pos;
                    dropDowns.Add(item);
                }

                // 加载文件
                LoadGropNames();
                return;
            }
        }
        WarningShow();
    }

    void LoadGropNames()
    {
        string path = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + fileNameDropdown.options[currentFileIndex].text;
        Debug.Log("Path : " + path);

        m_NameList = File.ReadAllLines(path).ToList();
        m_NameList.RemoveAll(
            item =>
            {
                return item == "" || item.Trim().StartsWith("//", StringComparison.Ordinal) || item.Trim().StartsWith("--", StringComparison.Ordinal);
            });

        for (int i = 0; i < m_NameList.Count(); i++)
        {
            m_NameList[i] = m_NameList[i].Trim();
        }

        m_DropDown.ClearOptions();
        m_DropDown.AddOptions(m_NameList);
        m_Loaded = true;
    }

    bool TryGetFilesName(out List<string> fileNames)
    {
        bool flag = false;
        fileNames = new List<string>();

        string[] filePaths = Directory.GetFiles(Directory.GetCurrentDirectory());
        for (int i = 0; i < filePaths.Length; i++)
        {
            filePaths[i] = filePaths[i].Substring(filePaths[i].LastIndexOf(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) + 1);
        }

        foreach (var item in filePaths)
        {
            if (item.Contains("名单") && item.Contains(".txt"))
            {
                fileNames.Add(item);
                flag = true;
            }
        }

        return flag;
    }

    void WarningShow(string warningMsg = warningGroupNum)
    {
        warningText.gameObject.SetActive(true);
        warningText.text = warningMsg;
    }

    void WarningHide()
    {
        warningText.gameObject.SetActive(false);
    }

    #endregion StartPanel

    #region GroupPanel
    void InitGroupPanel()
    {
        //m_DropDown = GameObject.Find("Canvas/GroupPanel/Dropdown").GetComponent<Dropdown>();
		groupPanel.SetActive(false);
        m_GroupPrefab = Resources.Load<GameObject>("Prefabs/Group");

        tipsText.text = "Tips : 红色<color=red> + </color>号按钮为随机抽取成员！白色<color=white> + </color>号按钮为从<color=yellow>成员列表</color>中选择成员！";

        createBtn.onClick.AddListener(() =>
        {
            string filePath = Directory.GetCurrentDirectory() + "/生成分组" + DateTime.Now.Ticks + ".txt";
            List<string> contents = new List<string>();
            for (int i = 0; i < dropDowns.Count(); i++)
            {
                if (dropDowns[i].Count > 0)
                {

                    string groupName = dropDowns[i].m_GroupName;
                    contents.Add(groupName);
                    foreach (var item in dropDowns[i].GetCurrentNameList())
                    {
                        contents.Add("\t" + item);
                    }
                    //contents.AddRange(dropDowns[i].GetCurrentNameList());
                }
            }
            if (contents.Count() > 0)
            {
                File.WriteAllLines(filePath, contents.ToArray(), System.Text.Encoding.UTF8);
                tipsText.text = "Tips : 分组文件 生成成功！";
            }
            else
            {
                tipsText.text = "Tips : 分组名单生成失败，请确认分组人员名单是否为空！";
            }
        });

        quitBtn.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }

    void ClampDrapdownValue()
    {
        m_DropDown.value = Mathf.Clamp(m_DropDown.value, 0, Count - 1);
    }
    #endregion GroupPanel

    #region RandomPanel
    void InitRandomPanel()
    {
        randomPanel.SetActive(false);

        randomOKBtn = randomPanel.transform.Find("OKBtn").GetComponent<Button>();
        randomCancelBtn = randomPanel.transform.Find("CancelBtn").GetComponent<Button>();
        randomOKBtn.onClick.AddListener(() =>
        {

            if (slotMachineControl.m_Run) return;

            string name = slotMachineControl.GetSlotMachineName();
            if (RemoveName(name))
            {
                currentGroupItem.AddItem(name);
                tipsText.text = "Tips : " + currentGroupItem.m_GroupName + " 随机抽取到了成员：" + name;
            }
            randomPanel.SetActive(false);
        });

        randomCancelBtn.onClick.AddListener(() =>
        {
            randomPanel.SetActive(false);
            tipsText.text = "Tips : " + currentGroupItem.m_GroupName + "取消了此次随机抽取的结果！";
        });
    }
    #endregion RandomPanel

    #region Public Function
    /// <summary>
    /// 往列表中添加成员
    /// </summary>
    public void AddName(string name)
    {
        m_DropDown.enabled = true;
        m_DropDown.options.Add(new Dropdown.OptionData(name));
        m_DropDown.value = Count - 1;
        m_DropDown.RefreshShownValue();
    }

    /// <summary>
    /// 从列表中取出元素
    /// </summary>
    public bool TryRemoveName(out string item)
    {
        if (Count == 0)
        {
            m_DropDown.value = 0;
            item = "";
            return false;
        }
        else
        {
            ClampDrapdownValue();
            Dropdown.OptionData data = m_DropDown.options[m_DropDown.value];
            m_DropDown.options.RemoveAt(m_DropDown.value);
            if (Count == 0)
            {
                m_DropDown.enabled = false;
            }
            m_DropDown.RefreshShownValue();

            item = data.text;
            return true;
        }
    }

    /// <summary>
    /// 移除列表中的名字
    /// </summary>
    public bool RemoveName(string name)
    {
        for (int i = 0; i < m_DropDown.options.Count(); i++)
        {
            if (m_DropDown.options[i].text == name)
            {
                m_DropDown.options.RemoveAt(i);
                ClampDrapdownValue();
                m_DropDown.RefreshShownValue();
                return true;
            }
        }
        return false;
    }

    public int Count { get { return m_DropDown.options.Count(); } }

    public List<string> GetCurrentNameList()
    {
        List<string> names = new List<string>();
        foreach (var item in m_DropDown.options)
        {
            names.Add(item.text);
        }
        return names;
    }

    public void OnStartRandom(GroupItem currentGroupItem)
    {
        this.currentGroupItem = currentGroupItem;
        randomGroupText.text = currentGroupItem.m_GroupName;
        randomNameResultText.text = "";
        randomPanel.SetActive(true);
    }
    #endregion

}
