using UnityEngine;

public class HoeScript : Usable
{
    public override UseInfo Use(Interactable targetInteractable)
    {
        if (targetInteractable == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Hoes needs to be used on farm land, you used it on nothing");
#endif
        }
        else
        {
            if (targetInteractable is FarmTile farmTile) farmTile.TillSoil();
        }
        return new UseInfo() { FinishedUsing = false }; // Never destroy
    }
}
