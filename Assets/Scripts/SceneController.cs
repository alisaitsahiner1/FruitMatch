using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SceneController : MonoBehaviour
{
    public Animator[] anim;
    private bool state = true;
    private Text buttonText;

    //private string m_ClipName;
    private AnimatorClipInfo[] m_CurrentClipInfo;

    public void PlayAllAnimations()
    {
        for (int i = 0; i < anim.Length; i++)
        {
            m_CurrentClipInfo = anim[i].GetCurrentAnimatorClipInfo(0);

            if (m_CurrentClipInfo[0].clip.name == "StartAnimation")
            {
                state = false;
                break;
            }

            state = true;
        }
        if (state)
        {
            for (int i = 0; i < anim.Length; i++)
            {
                anim[i].Play("StartAnimation");
            }
            state = false;
        }
    }
  
}
