using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "FPS/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [SerializeField]
    List<GunData> gunDatas = new List<GunData>();

    public int GetGunDatasCount()
    {
        return gunDatas.Count;
    }

    public GunData GetGunDataByIndex(int index)
    {
        if(index < 0 || index >= gunDatas.Count)
        {
            return null;
        }

        return gunDatas[index];
    }
}
