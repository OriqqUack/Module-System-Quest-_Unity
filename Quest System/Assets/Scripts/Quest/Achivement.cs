using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Quest/Achivement", fileName = "Achivement_")]
public class Achivement : Quest
{
    public override bool IsCancelable => false;

    public override void Cancel()
    {
        Debug.LogAssertion("Achivement cna't be cancled");
    }
}
