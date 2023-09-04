using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum QuestState
{
    Inactive,
    Running,Complete,
    Cancel,
    WatingForCompletion
}

[CreateAssetMenu(menuName = "Quest/Quest", fileName = "Quest_")]
public class Quest : ScriptableObject
{
    #region Events
    public delegate void TaskSuccessChangedHandler(Quest quest, Task task, int currentSuccess, int prevSuccess);
    public delegate void CompletedHandler(Quest quest);
    public delegate void CancelHandler(Quest quest);
    public delegate void NewTaskGroupHandler(Quest quest, TaskGroup currenTaskGroup, TaskGroup prevTaskGroup);
    #endregion
    //Categhory, Icon, Quest의 이름,Quest 이름의 설명
    [SerializeField]
    private Category category;
    [SerializeField]
    private Sprite icon;

    [SerializeField]
    private string codeName;
    [SerializeField]
    private string displayName;
    [SerializeField, TextArea]
    private string description;

    [Header("Task")]
    [SerializeField]
    private TaskGroup[] taskGroups;

    [Header("Option")]
    [SerializeField]
    private bool useAutoComplete;

    private int currentTaskGroupIndex;

    public Category Category => category;
    public Sprite Icon => icon; 
    public string CodeName => codeName;
    public string DisplayName => displayName;
    public string Description => description;
    public QuestState State { get; private set; }
    public TaskGroup CurrentTaskGroup => taskGroups[currentTaskGroupIndex];
    public IReadOnlyList<TaskGroup> TaskGroups => taskGroups;

    //현재 상태를 알기 쉽게 하기 위해 bool형 프로퍼티 선언.
    public bool isRegistered => State!=QuestState.Inactive;
    public bool isComplatble => State == QuestState.WatingForCompletion;
    public bool isComplete => State == QuestState.Complete; 
    public bool isCancel => State == QuestState.Cancel;

    //event 생성 4가지(보고 받았을 때 실행할 event, Quest를 완료했을 때 실행할 event,취소 했을 때  새로운 TaskGroup이 시작되었을 때 실행할 event,
    public event TaskSuccessChangedHandler onTaskSuccessChanged;
    public event CompletedHandler onCompleted;
    public event CancelHandler onCanceled;
    public event NewTaskGroupHandler onNewTaskGroup;

    public void OnRegister()
    {
        Debug.Assert(!isRegistered, "This quest has already been registerd.");

        foreach(var taskGroup in taskGroups)
        {
            taskGroup.Setup(this);
            foreach (var task in taskGroup.Tasks)
                task.onSuccessChanged += OnSuccessChanged;
        }

        State = QuestState.Running;
        CurrentTaskGroup.Start();
    }

    public void ReceiveReport(string category, object target, int successCount)
    {
        Debug.Assert(!isRegistered, "This quest has already been registerd.");
        Debug.Assert(!isCancel, "This quest has been canceld.");

        if (isComplete)
            return;

        if (CurrentTaskGroup.IsAllTaskComplete)
        {
            if (currentTaskGroupIndex + 1 == taskGroups.Length)
            {
                State = QuestState.WatingForCompletion;
                if (useAutoComplete)
                    Complete();
            }
            else
            {
                var prevTaskGroup = taskGroups[currentTaskGroupIndex++];
                prevTaskGroup.End();
                CurrentTaskGroup.Start();
                onNewTaskGroup?.Invoke(this, CurrentTaskGroup, prevTaskGroup);
            }
        }
        else
            State = QuestState.Running;
    }

    public void Complete()
    {

    }

    public void Cancel()
    {

    }

    private void OnSuccessChanged(Task task, int currentSuccess, int prevSuccess)
        => onTaskSuccessChanged?.Invoke(this, task, currentSuccess, prevSuccess);
}
