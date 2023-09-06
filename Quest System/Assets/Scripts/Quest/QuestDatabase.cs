using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor;
    #endif

[CreateAssetMenu(menuName = "Quest/QuestDatabase")]
public class QuestDatabase : ScriptableObject
{
    [SerializeField]
    private List<Quest> quests;

    public IReadOnlyList<Quest> Quests => quests;

    public Quest FindQuestBy(string codeName) => quests.FirstOrDefault(x => x.CodeName == codeName);

#if UNITY_EDITOR
    [ContextMenu("FindQuests")]
    private void FindQuests()
    {
        FindQuestsBy<Quest>();
    }

    [ContextMenu("FindAchivments")]
    private void FindAchivements()
    {
        FindQuestsBy<Achivement>();
    }

    private void FindQuestsBy<T>() where T : Quest
    {
        quests = new List<Quest>();

        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T)}"); //GUID�� ����Ƽ�� ������ �����ϱ� ���� �ο��� id
        foreach(var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var quest = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            if (quest.GetType() == typeof(Quest))
                quests.Add(quest); //quest�� achivement�� ��� ã�ƿ� ������ ������ �ѹ��� if���� ���� ����

            EditorUtility.SetDirty(this); //��Ƽ��� �÷��׸� ����� ���� �� �۾��� �ʿ��� �� �÷��װ� ������ ���� �ݿ�.
            AssetDatabase.SaveAssets(); //������ �ֽ����� ����
        }
    }
    #endif
}
