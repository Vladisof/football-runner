using Tracks;
using UnityEngine;
namespace Characters
{
    public class RestartRun : StateMachineBehaviour
    {
        static int s_DeadHash = Animator.StringToHash("Dead");

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animator.GetBool(s_DeadHash))
                return; 
        
            TracksManager.instance.StartMove();
        }

    }
}
