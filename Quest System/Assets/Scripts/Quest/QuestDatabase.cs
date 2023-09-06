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

        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T)}"); //GUID란 유니티가 에셋을 관리하기 위해 부여한 id
        foreach(var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var quest = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            if (quest.GetType() == typeof(Quest))
                quests.Add(quest); //quest와 achivement를 모두 찾아와 버리기 때문에 한번더 if문을 통해 관리

            EditorUtility.SetDirty(this); //더티라는 플래그를 세우고 실제 그 작업이 필요할 때 플래그가 세워진 값을 반영.
            AssetDatabase.SaveAssets(); //에셋을 최신으로 갱신
        }
    }
    #endif
}
