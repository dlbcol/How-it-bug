using UnityEngine;

[CreateAssetMenu (fileName = "NewWeaponEntity", menuName = "Weapons/WeaponProperties")]
public class WeaponProperties : ScriptableObject
{
    public string weaponName;

    public LayerMask targetLayerMask;

    [Header("Firing configurations")]
    public float weaponRange;
    public float cycleRate;

    [Header("Firing configurations")]
    [Tooltip("For melee weapons, keep the following two values at -1")]
    public float magSize;
    public float reloadTime;
}
    
