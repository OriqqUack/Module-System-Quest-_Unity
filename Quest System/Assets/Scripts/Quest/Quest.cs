using System.Collections;
using System.Collections.Generic;
using System.Diagnostics; //assert���� �Լ��� �����ϱ� ���� ���.
using System.Linq;
using UnityEngine;

using Debug = UnityEngine.Debug; //�⺻������ Debug class�� unity�� debug class�� ����ϰڴ� ����.

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
    //Categhory, Icon, Quest�� �̸�,Quest �̸��� ����
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

    [Header("Reward")]
    [SerializeField]
    private Reward[] rewards;

    [Header("Option")]
    [SerializeField]
    private bool useAutoComplete;
    [SerializeField]
    private bool isCancelable;

    [Header("Condition")]
    [SerializeField]
    private Condition[] acceptionConditions;
    [SerializeField]
    private Condition[] cancelConditions;

    private int currentTaskGroupIndex;

    public Category Category => category;
    public Sprite Icon => icon;
    public string CodeName => codeName;
    public string DisplayName => displayName;
    public string Description => description;
    public QuestState State { get; private set; }
    public TaskGroup CurrentTaskGroup => taskGroups[currentTaskGroupIndex];
    public IReadOnlyList<TaskGroup> TaskGroups => taskGroups;
    public IReadOnlyList<Reward> Rewards => rewards;

    //���� ���¸� �˱� ���� �ϱ� ���� bool�� ������Ƽ ����.
    public bool isRegistered => State != QuestState.Inactive;
    public bool isComplatble => State == QuestState.WatingForCompletion;
    public bool isComplete => State == QuestState.Complete;
    public bool isCancel => State == QuestState.Cancel;
    public virtual bool IsCancelable => isCancelable && cancelConditions.All(x=>x.IsPass(this));
    public bool IsAcceptable => acceptionConditions.All(x => x.IsPass(this));

    //event ���� 4����(���� �޾��� �� ������ event, Quest�� �Ϸ����� �� ������ event,��� ���� ��  ���ο� TaskGroup�� ���۵Ǿ��� �� ������ event,
    public event TaskSuccessChangedHandler onTaskSuccessChanged;
    public event CompletedHandler onCompleted;
    public event CancelHandler onCanceled;
    public event NewTaskGroupHandler onNewTaskGroup;

    public void OnRegister()
    {
        Debug.Assert(!isRegistered, "This quest has already been registerd.");

        foreach (var taskGroup in taskGroups)
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

        CurrentTaskGroup.ReceiveReport(category, target, successCount);
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
            State = QuestState.Running; // watingforcompletion ��Ȳ���� �ٽ� ����Ʈ�� ���� �� �� �ֱ⿡
    }

    public void Complete()
    {
        CheckIsRunning();

        foreach (var taskGroup in taskGroups)
            taskGroup.Complete();

        State = QuestState.Complete;

        foreach (var reward in rewards)
            reward.Give(this);

        onCompleted?.Invoke(this);

        onTaskSuccessChanged = null;
        onCompleted = null;
        onCanceled = null;
        onNewTaskGroup = null;
    }

    public virtual void Cancel()
    {
        CheckIsRunning();
        Debug.Assert(IsCancelable, "This quest cna't be canceled");

        State = QuestState.Cancel;
        onCanceled?.Invoke(this);
    }

    public Quest Clone()
    {
        var clone = Instantiate(this);
        clone.taskGroups = taskGroups.Select(x => new TaskGroup(x)).ToArray();

        return clone;
    }

    private void OnSuccessChanged(Task task, int currentSuccess, int prevSuccess)
        => onTaskSuccessChanged?.Invoke(this, task, currentSuccess, prevSuccess);

    [Conditional("UNITY_EDITOR")]
    private void CheckIsRunning()
    {
        Debug.Assert(!isRegistered, "This quest has already been registerd.");
        Debug.Assert(!isCancel, "This quest has been canceld.");
        Debug.Assert(!isComplatble, "This quest has already been completed");
    }
}
