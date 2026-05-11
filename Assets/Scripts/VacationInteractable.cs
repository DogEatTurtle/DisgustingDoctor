using UnityEngine;

public class VacationInteractable : MonoBehaviour
{
    [SerializeField] private VacationUI vacationUI;

    public void Interact()
    {
        if (vacationUI == null) return;
        vacationUI.OpenUI();
    }
}