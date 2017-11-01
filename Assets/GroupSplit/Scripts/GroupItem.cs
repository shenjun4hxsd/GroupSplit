/*
 *  created by shenjun
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GroupItem : MonoBehaviour
{

    Dropdown m_DropDown;
    Button m_AddBtn;
    Button m_SubBtn;
    Button m_RandomAddBtn;
    Text m_GroupNameText;       // 显示小组名
    Text m_NumText;             // 显示人数
    Text m_CaptionText;        // 下拉列表名单

    public string m_GroupName = "";

    void Awake()
    {
        m_DropDown = transform.GetComponentInChildren<Dropdown>();
        m_DropDown.options.Clear();
        m_DropDown.enabled = false;
		m_CaptionText = m_DropDown.captionText;

        m_AddBtn = transform.Find("AddBtn").GetComponent<Button>();
        m_SubBtn = transform.Find("SubBtn").GetComponent<Button>();
        m_RandomAddBtn = transform.Find("RandomAddBtn").GetComponent<Button>();

        m_GroupNameText = transform.Find("GroupName").GetComponent<Text>();
        m_NumText = transform.Find("numTxt").GetComponent<Text>();

        m_AddBtn.onClick.AddListener(() => {
			string item;
            if (NameListLoad.Instance.TryRemoveName(out item))
			{
                AddItem(item);
			}
        });

        m_SubBtn.onClick.AddListener(() => {
            string item;
            if(TryRemoveCurrentItem(out item))
            {
                NameListLoad.Instance.AddName(item);
            }
        });

        m_RandomAddBtn.onClick.AddListener(() => {
            if (NameListLoad.Instance.Count <= 1)
            {
				NameListLoad.Instance.tipsText.text = "Tips : 少于2名成员，不用随机了吧！";
                return;
            }
                
            NameListLoad.Instance.OnStartRandom(this);
        });

        m_SubBtn.enabled = false;
    }

    public void SetGroupName(string name)
    {
        m_DropDown.RefreshShownValue();
        m_GroupName = name;
        m_GroupNameText.text = name;
        m_CaptionText.text = name;
    }

    public int Count { get{ return m_DropDown.options.Count; }}

    public void AddItem(string name)
    {
        m_DropDown.options.Add(new Dropdown.OptionData(name));

        m_DropDown.value = Count - 1;
        m_DropDown.RefreshShownValue();
        UpdateNum();
        m_DropDown.enabled = true;
        m_SubBtn.enabled = true;

        NameListLoad.Instance.tipsText.text = "Tips : " + m_GroupName + " 选择了成员 : " + name;
    }

    /// <summary>
    /// 移除当前项
    /// </summary>
    public bool TryRemoveCurrentItem(out string item)
    {
        if (Count == 0)
        {
            ClampDrapdownValue();
            item = "";
            m_CaptionText.text = m_GroupName;
            return false;
        }
        else
        {
            ClampDrapdownValue();
            Debug.Log("Count : " + Count + ", value = " + m_DropDown.value);
			Dropdown.OptionData data = m_DropDown.options[m_DropDown.value];
			m_DropDown.options.RemoveAt(m_DropDown.value);
			m_DropDown.RefreshShownValue();
            if (Count == 0)
            {
                //m_SubBtn.enabled = false;
                m_DropDown.enabled = false;
                m_CaptionText.text = m_GroupName;
            }
			UpdateNum();
			item = data.text;
            NameListLoad.Instance.tipsText.text = "Tips : " + m_GroupName + " 移除了成员 : " + item;
            return true;
        }
    }

    /// <summary>
    /// 更新列表人数
    /// </summary>
    void UpdateNum()
    {
        m_NumText.text = Count.ToString();
    }

	public void ClampDrapdownValue()
	{
        if(Count == 0)
        {
            m_SubBtn.enabled = false;
        }
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

}
