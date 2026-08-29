using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationStateController : MonoBehaviour
{
    private Animator animator;
    private int currentState = -1;

    private readonly string[] stateNames =
    {
        "0LCup",
        "1LCup",
        "2LCup",
        "3LCup",
        "4LCup",
        "5LCup",
        "6LCup",
        "7LCup",
        "8LCup"
    };

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError(
                gameObject.name + "Ç…AnimatorÇ™Ç†ÇËÇ‹ÇπÇÒ",
                gameObject
            );
        }
    }

    public void ChangeState(int stateNumber)
    {
        if (animator == null)
        {
            Debug.LogError(
                gameObject.name + "ÇÃAnimatorÇéÊìæÇ≈Ç´Ç‹ÇπÇÒ",
                gameObject
            );

            return;
        }

        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError(
                gameObject.name +
                "ÇÃAnimator ControllerÇ™ñ¢ìoò^Ç≈Ç∑",
                gameObject
            );

            return;
        }

        if (stateNumber < 0 ||
            stateNumber >= stateNames.Length)
        {
            Debug.LogError(
                "ë∂ç›ÇµÇ»Ç¢èÛë‘î‘çÜÇ≈Ç∑: " + stateNumber,
                gameObject
            );

            return;
        }

        string stateName = stateNames[stateNumber];

        int stateHash =
            Animator.StringToHash(stateName);

        int fullPathHash =
            Animator.StringToHash(
                "Base Layer." + stateName
            );

        if (!animator.HasState(0, stateHash) &&
            !animator.HasState(0, fullPathHash))
        {
            Debug.LogError(
                "AnimatorÇ…èÛë‘Ç™å©Ç¬Ç©ÇËÇ‹ÇπÇÒ: " +
                stateName,
                gameObject
            );

            return;
        }

        if (currentState == stateNumber)
        {
            return;
        }

        currentState = stateNumber;

        animator.CrossFade(
            fullPathHash,
            0.1f,
            0
        );

        Debug.Log(
            gameObject.name +
            "Ç" +
            stateName +
            "Ç÷êÿÇËë÷Ç¶Ç‹ÇµÇΩ",
            gameObject
        );
    }
}