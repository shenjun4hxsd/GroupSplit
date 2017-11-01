/*
 *  created by shenjun
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachineControl : MonoBehaviour {

    public GameObject selectFrame;
	public GameObject victoryImage;

    List<string> names = new List<string>();

    List<string> currentNames = new List<string>();

    Text[] nameTxts;

    Animator m_Ani;

    int countHash = Animator.StringToHash("count");

    public bool m_Run = false;
    float m_TimeCount;
    float m_MaxTime;

    void Awake()
    {
        m_Ani = GetComponent<Animator>();


        nameTxts = new Text[transform.childCount];
        for (int i = 0; i < nameTxts.Length; i++)
        {
            nameTxts[i] = transform.GetChild(i).GetComponentInChildren<Text>();
        }
    }

    float GetSpeed()
    {
        return Mathf.Max(0, Mathf.Sin(Mathf.PI * m_TimeCount / m_MaxTime)) * 4 + 0.8f;
    }


    void OnEnable()
    {
        StartCoroutine(LoadAsync());
    }

    private void OnDisable()
    {
        //Debug.Log("OnDisable");
        StopAllCoroutines();
        m_Run = false;
        m_TimeCount = 0;
        m_Ani.SetInteger(countHash, (int)m_TimeCount);
        m_Ani.Play("Idle");
        m_Ani.speed = 1;
        m_Ani.enabled = false;

    }

    IEnumerator LoadAsync()
    {
        selectFrame.SetActive(false);
        victoryImage.SetActive(false);
        m_Ani.enabled = true;
        AnimatorClipInfo[] clips = m_Ani.GetCurrentAnimatorClipInfo(0);
        for (int i = 0; i < clips.Length; i++)
        {
            clips[i].clip.wrapMode = WrapMode.Default;
        }
        while (NameListLoad.Instance == null || !NameListLoad.Instance.m_Loaded)
		{
			yield return null;
		}
		names = NameListLoad.Instance.GetCurrentNameList();

		m_TimeCount = m_MaxTime = Random.Range(5, 15.0f);
		m_Ani.SetInteger(countHash, (int)m_TimeCount);

		InitNameList();
		m_Run = true;
    }

    void Update()
    {
        if(m_Run)
        {
            m_TimeCount -= Time.deltaTime;
			m_Ani.SetInteger(countHash, (int)m_TimeCount);
            m_Ani.speed = GetSpeed();
            if(m_TimeCount < 0)
            {
                m_Run = false;
                selectFrame.transform.position = nameTxts[2].transform.position;
                selectFrame.SetActive(true);
                victoryImage.SetActive(true);
                NameListLoad.Instance.randomNameResultText.text = GetSlotMachineName();
            }
        }
    }

    void InitNameList()
    {
		currentNames = new List<string>();
		for (int i = 0; i < nameTxts.Length; i++)
		{
			currentNames.Add(names[Random.Range(0, names.Count)]);
			nameTxts[i].text = currentNames[i];
		}
    }

    public void OnUpdateNameList()
    {
        currentNames.RemoveAt(0);
        currentNames.Add(names[Random.Range(0, names.Count)]);
		for (int i = 0; i < nameTxts.Length; i++)
		{
			nameTxts[i].text = currentNames[i];
		}

        //if(m_TimeCount < 0)
        //{
        //    selectFrame.SetActive(true);
        //}
    }

    public string GetSlotMachineName()
    {
        return currentNames[2];
    }
}
