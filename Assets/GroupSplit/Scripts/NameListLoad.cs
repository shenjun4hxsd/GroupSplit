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

    public GameObject startPanel;   // 开始界面
    public GameObject groupPanel;   // 分组界面
    public GameObject randomPanel;  // 随机界面
    public Text randomGroupText;
    public Text randomNameResultText;

    GroupItem currentGroupItem;
    public SlotMachineControl slotMachineControl;

    public InputField input;
    public Button startBtn;
    public Text warningText;

    public Text tipsText;

    public Button randomOKBtn;
    public Button randomCancelBtn;

    public Button createBtn;
    public Button quitBtn;

    Dropdown m_DropDown;

    GameObject m_GroupPrefab;
    public Transform m_DropDownStartTrans;
    public Transform m_DropDownParent;

    string m_Path
    {
        get
        {
            List<string> fileNames;
            if(TryGetFileName(out fileNames))
            {
                return Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + fileNames[0];
            }
            else
            {
                return null;
            }
        }
    }

    /** 名单原始列表 */
    public List<string> m_NameList = new List<string>();

    public List<GroupItem> dropDowns = new List<GroupItem>();

    public bool m_Loaded = false;

    void Awake()
    {
        Instance = this;

        m_DropDown = GameObject.Find("Canvas/Panel/Dropdown").GetComponent<Dropdown>();
        startPanel.SetActive(true);
        groupPanel.SetActive(false);
        randomPanel.SetActive(false);
        warningText.gameObject.SetActive(false);
        startBtn.onClick.AddListener(StartGroup);

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

    bool TryGetFileName(out List<string> fileNames)
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
            if (item.Contains("名单"))
            {
                fileNames.Add(item);
                flag = true;
            }
        }

        return flag;
    }


    /// <summary>
    /// 初始化界面
    /// </summary>
    void StartGroup()
    {
        int result;
        if (int.TryParse(input.text, out result))
        {
            if (result > 0 && result < 10)
            {
                startPanel.SetActive(false);
                groupPanel.SetActive(true);
                warningText.gameObject.SetActive(false);

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
                return;
            }
        }
        warningText.gameObject.SetActive(true);
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    IEnumerator Start()
    {
        yield return null;
        if (m_Path == null) yield break;
        Debug.Log("Path : " + m_Path);
        m_NameList = File.ReadAllLines(m_Path).ToList();
        m_NameList.RemoveAll(
            item =>
            {
                return item == "" || item.Trim().StartsWith("//", StringComparison.Ordinal) || item.Trim().StartsWith("--", StringComparison.Ordinal);
            });
        yield return null;

        for (int i = 0; i < m_NameList.Count(); i++)
        {
            m_NameList[i] = m_NameList[i].Trim();
        }

        m_DropDown.ClearOptions();
        m_DropDown.AddOptions(m_NameList);
        m_Loaded = true;
    }

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

    public void ClampDrapdownValue()
    {
        m_DropDown.value = Mathf.Clamp(m_DropDown.value, 0, Count - 1);
    }

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
}
