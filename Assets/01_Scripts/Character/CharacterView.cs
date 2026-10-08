using UnityEngine;

public class CharacterView : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform handSocket;

    public Animator Animator => animator;
    public Transform HandSocket => handSocket;
}
