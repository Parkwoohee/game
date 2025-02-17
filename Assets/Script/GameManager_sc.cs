﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GameManager_sc : MonoBehaviour
{

    public GameObject notePrefab;
    public GameObject barPrefab;

    float beatPerBar = 32f; // Bar 당 비트수.
    int defaultSpeed = 10;
    int timeRateBySpeed = 2; // 거리 계수

    GameObject note;
    NoteObj_sc note_sc;

    public string bmsName = "Only_you";

    // 메트로놈 재생 여부
    public bool isTic;

    //Firebase에서 Goolge Play서비스가 최신상태인지 확인한다.
    void Awake()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                //   app = Firebase.FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }
        // Use this for initialization
        IEnumerator Start()
    {
        // bms 파일 로드.
        string[] lineData = File.ReadAllLines(Application.dataPath + "/BmsFiles/" + "test.bms");
        //string[] lineData = File.ReadAllLines("Assets/Resources/" + "Only_you.bms");

        // BMS 파일 찾기.
        //TextAsset ta = Resources.Load("BmsFiles/" + bmsName) as TextAsset;
        //string strData = "" + ta.text;
        //string[] lineData = strData.Split('\n');

        // BMS 노트데이터 파싱.
        BmsLoader_sc bmsLoader = GameObject.Find("BmsLoader").GetComponent<BmsLoader_sc>();
        bmsLoader.BmsLoad(lineData);

        Bms bms = bmsLoader.getBms();

        // 시작선.
        GameObject plane_Top = GameObject.Find("Start_Line");
        float startPositionZ = plane_Top.transform.position.z;

        // 판정선.
        GameObject lineJudgment = GameObject.Find("Cut_Line");
        float judgmentPositionZ = lineJudgment.transform.position.z;

        // 시작선 ~ 판정선 거리.
        //print(Vector3.Distance(plane_Top.transform.position, lineJudgment.transform.position));
        //float distance = 17.81831f;

        // 노트 라인.
        GameObject lineCenter = GameObject.Find("LineCenter");

        // 노트 소멸 위치.
        float destroyDelayPositionZ = 1f;

        // 노트 간격 비율.
        //float noteWidthRate = 1.8f;

        // 노트 프리팹 생성 및 리스트 저장.
        List<NoteObj_sc> noteObj_Line_1 = new List<NoteObj_sc>();
        List<NoteObj_sc> noteObj_Line_2 = new List<NoteObj_sc>();
        List<NoteObj_sc> noteObj_Line_3 = new List<NoteObj_sc>();
        List<NoteObj_sc> noteObj_Line_4 = new List<NoteObj_sc>();
        List<NoteObj_sc> noteObj_Line_5 = new List<NoteObj_sc>();
        List<NoteObj_sc> bar_Line = new List<NoteObj_sc>();

        bool isLongNoteStart_1 = true;
        bool isLongNoteStart_2 = true;
        bool isLongNoteStart_3 = true;
        bool isLongNoteStart_4 = true;
        bool isLongNoteStart_5 = true;

        float preNoteTime_Ln1 = 0f;
        float preNoteTime_Ln2 = 0f;
        float preNoteTime_Ln3 = 0f;
        float preNoteTime_Ln4 = 0f;
        float preNoteTime_Ln5 = 0f;

        // 노트 소멸 딜레이 타임.
        float destroyDelayTime = bms.getTotalPlayTime() + 1;

        //Bar 생성.
        float secondPerBar = 60.0f / (float)bms.getBpm() * 4.0f; // Bar 당 시간(초).
        int barCount = 0;
        for (int i = 0; i < bms.totalBarCount; i++)
        {
            // barLine 생성
            float barTime = barCount * secondPerBar; // Bar 시작시간
            note = (GameObject)Instantiate(barPrefab, new Vector3(0, 0, startPositionZ), Quaternion.identity);
            note_sc = note.GetComponent<NoteObj_sc>();
            note_sc.speed = defaultSpeed;
            note_sc.destroyPositionZ = judgmentPositionZ - destroyDelayPositionZ;
            note_sc.destroyDelayTime = destroyDelayTime;
            note_sc.noteTime = barTime;
            bar_Line.Add(note_sc);
            barCount++;
        }

        // 노트 생성.
        foreach (BarData barData in bms.getBarDataList())
        {
            float linePositionX = lineCenter.transform.position.x;
            float linePositionY = lineCenter.transform.position.y;
            float noteRotZ = lineCenter.transform.rotation.z;
            bool isLongChannel = false;

            //GameObject parent = GameObject.Find("punching bag fbx");

            int channel = barData.getChannel();
            if (channel == 11 || channel == 51)
            {
                linePositionX = lineCenter.transform.position.x - 0.15f;
                linePositionY = lineCenter.transform.position.y + 1.2f;
                noteRotZ = lineCenter.transform.rotation.z - 90;
            }
            else if (channel == 12 || channel == 52)
            {
                linePositionX = lineCenter.transform.position.x - 0.15f;
                linePositionY = lineCenter.transform.position.y + 0.7f;
                noteRotZ = lineCenter.transform.rotation.z - 90;
            }
            else if (channel == 13 || channel == 53)
            {
                linePositionX = lineCenter.transform.position.x + 0.02f;
                linePositionY = lineCenter.transform.position.y + 0.98f;
            }
            else if (channel == 14 || channel == 54)
            {
                linePositionX = lineCenter.transform.position.x + 0.17f;
                linePositionY = lineCenter.transform.position.y + 1.2f;
                noteRotZ = lineCenter.transform.rotation.z + 90;
            }
            else if (channel == 15 || channel == 55)
            {
                linePositionX = lineCenter.transform.position.x + 0.17f;
                linePositionY = lineCenter.transform.position.y + 0.7f;
                noteRotZ = lineCenter.transform.rotation.z + 90;
            }

            if (channel == 51 || channel == 52 || channel == 53 || channel == 54 || channel == 55)
            {
                isLongChannel = true;
            }

            foreach (Dictionary<int, float> noteData in barData.getNoteDataList())
            {
                foreach (int key in noteData.Keys)
                {
                    // 단노트.
                    if (isLongChannel == false && key != 0 && channel != 16)
                    {
                        float noteTime = noteData[key];
                        Quaternion rotation = Quaternion.identity;
                        rotation.eulerAngles = new Vector3(0, 0, noteRotZ);

                        note = (GameObject)Instantiate(notePrefab, new Vector3(linePositionX, linePositionY, startPositionZ), rotation);
                        //note.transform.SetParent(parent.transform);
                        note_sc = note.GetComponent<NoteObj_sc>();
                        note_sc.speed = defaultSpeed;
                        note_sc.destroyPositionZ = judgmentPositionZ - destroyDelayPositionZ;
                        note_sc.destroyDelayTime = destroyDelayTime;
                        note_sc.noteTime = noteTime;
                        note_sc.channel = channel;

                        if (channel == 11)
                        {
                            noteObj_Line_1.Add(note_sc);
                        }
                        else if (channel == 12)
                        {
                            noteObj_Line_2.Add(note_sc);
                        }
                        else if (channel == 13)
                        {
                            noteObj_Line_3.Add(note_sc);
                        }
                        else if (channel == 14)
                        {
                            noteObj_Line_4.Add(note_sc);
                        }
                        else if (channel == 15)
                        {
                            noteObj_Line_5.Add(note_sc);
                        }
                    }

                    // 롱노트.
                    if (isLongChannel == true && key != 0)
                    {
                        float secondPerBeat = 60.0f / (float)bms.getBpm() * 4.0f / beatPerBar; // Beat당 시간(초)
                        float longHeightRate = 0f; // 롱노트 높이 배율.

                        bool isLongNoteStart = false;
                        if (channel == 51)
                        {
                            isLongNoteStart = isLongNoteStart_1;
                        }
                        else if (channel == 52)
                        {
                            isLongNoteStart = isLongNoteStart_2;
                        }
                        else if (channel == 53)
                        {
                            isLongNoteStart = isLongNoteStart_3;
                        }
                        else if (channel == 54)
                        {
                            isLongNoteStart = isLongNoteStart_4;
                        }
                        else if (channel == 55)
                        {
                            isLongNoteStart = isLongNoteStart_5;
                        }

                        if (isLongNoteStart == true)
                        {
                            if (channel == 51)
                            {
                                preNoteTime_Ln1 = noteData[key];
                                isLongNoteStart_1 = false;
                            }
                            else if (channel == 52)
                            {
                                preNoteTime_Ln2 = noteData[key];
                                isLongNoteStart_2 = false;
                            }
                            else if (channel == 53)
                            {
                                preNoteTime_Ln3 = noteData[key];
                                isLongNoteStart_3 = false;
                            }
                            else if (channel == 54)
                            {
                                preNoteTime_Ln4 = noteData[key];
                                isLongNoteStart_4 = false;
                            }
                            else if (channel == 55)
                            {
                                preNoteTime_Ln5 = noteData[key];
                                isLongNoteStart_5 = false;
                            }
                        }
                        else if (isLongNoteStart == false)
                        {
                            float noteTime = noteData[key];

                            float preNoteTime_Ln = 0f;
                            if (channel == 51)
                            {
                                preNoteTime_Ln = preNoteTime_Ln1;
                            }
                            else if (channel == 52)
                            {
                                preNoteTime_Ln = preNoteTime_Ln2;
                            }
                            else if (channel == 53)
                            {
                                preNoteTime_Ln = preNoteTime_Ln3;
                            }
                            else if (channel == 54)
                            {
                                preNoteTime_Ln = preNoteTime_Ln4;
                            }
                            else if (channel == 55)
                            {
                                preNoteTime_Ln = preNoteTime_Ln5;
                            }

                            longHeightRate = (noteTime - preNoteTime_Ln) / secondPerBeat;

                            //print("preNoteTime_Ln = " + preNoteTime_Ln);
                            //print("noteTime = " + noteTime);
                            //print("longHeightRate = " + Mathf.Round(longHeightRate));

                            note = (GameObject)Instantiate(notePrefab, new Vector3(linePositionX, linePositionY, startPositionZ), Quaternion.identity);
                            float originalScaleX = note.transform.localScale.x;
                            float originalScaleY = note.transform.localScale.y;
                            float originalScaleZ = note.transform.localScale.z;
                            note.transform.localScale = new Vector3(originalScaleX, originalScaleY, originalScaleZ * Mathf.Round(longHeightRate) + originalScaleZ);

                            note_sc = note.GetComponent<NoteObj_sc>();
                            note_sc.destroyPositionZ = judgmentPositionZ - destroyDelayPositionZ;
                            note_sc.destroyDelayTime = destroyDelayTime;
                            note_sc.noteTime = preNoteTime_Ln;

                            if (channel == 51)
                            {
                                noteObj_Line_1.Add(note_sc);
                                preNoteTime_Ln1 = 0;
                                isLongNoteStart_1 = true;
                            }
                            else if (channel == 52)
                            {
                                noteObj_Line_2.Add(note_sc);
                                preNoteTime_Ln2 = 0;
                                isLongNoteStart_2 = true;
                            }
                            else if (channel == 53)
                            {
                                noteObj_Line_3.Add(note_sc);
                                preNoteTime_Ln3 = 0;
                                isLongNoteStart_3 = true;
                            }
                            else if (channel == 54)
                            {
                                noteObj_Line_4.Add(note_sc);
                                preNoteTime_Ln4 = 0;
                                isLongNoteStart_4 = true;
                            }
                            else if (channel == 55)
                            {
                                noteObj_Line_5.Add(note_sc);
                                preNoteTime_Ln5 = 0;
                                isLongNoteStart_5 = true;
                            }
                        }
                    }
                }
            }
        }

        noteObj_Line_1.Sort(delegate (NoteObj_sc a, NoteObj_sc b)
        {
            return a.noteTime.CompareTo(b.noteTime);
        });
        noteObj_Line_2.Sort(delegate (NoteObj_sc a, NoteObj_sc b)
        {
            return a.noteTime.CompareTo(b.noteTime);
        });
        noteObj_Line_3.Sort(delegate (NoteObj_sc a, NoteObj_sc b)
        {
            return a.noteTime.CompareTo(b.noteTime);
        });
        noteObj_Line_4.Sort(delegate (NoteObj_sc a, NoteObj_sc b)
        {
            return a.noteTime.CompareTo(b.noteTime);
        });
        noteObj_Line_5.Sort(delegate (NoteObj_sc a, NoteObj_sc b)
        {
            return a.noteTime.CompareTo(b.noteTime);
        });
        bar_Line.Sort(delegate (NoteObj_sc a, NoteObj_sc b)
        {
            return a.noteTime.CompareTo(b.noteTime);
        });

        // 비트 크리에이터.
        BeatCreator_sc beatCreator = GameObject.Find("BeatCreator").GetComponent<BeatCreator_sc>();
        beatCreator.noteObj_Line_1 = noteObj_Line_1;
        beatCreator.noteObj_Line_2 = noteObj_Line_2;
        beatCreator.noteObj_Line_3 = noteObj_Line_3;
        beatCreator.noteObj_Line_4 = noteObj_Line_4;
        beatCreator.noteObj_Line_5 = noteObj_Line_5;
        beatCreator.bar_Line = bar_Line;

        beatCreator.bpm = (float)bms.getBpm();
        beatCreator.beatPerBar = beatPerBar;
        beatCreator.timeRateBySpeed = timeRateBySpeed;

        //AudioClip bgm = Resources.Load<AudioClip>("Sound/Only_you");
        AudioClip bgm = Resources.Load("Sound/" + "Only_you") as AudioClip;
        beatCreator.bgmSound = bgm;
        Debug.Log(beatCreator.bgmSound);
        beatCreator.StartSetting();

        // 모든 렌더링작업이 끝날 때까지 대기
        yield return new WaitForEndOfFrame();

        beatCreator.isStart = true; // 시작
    }

    // Update is called once per frame
    void Update()
    {

    }

}