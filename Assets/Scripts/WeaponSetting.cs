public enum WeaponName { AssaultRifle = 0 }

[System.Serializable]
public struct WeaponSetting
{
    public WeaponName weaponName; // 무기 이름
    public int damage; // 무기 공격력
    public int currentMagazine; // 현재 장착된 탄창 수
    public int maxMagazine; // 최대 장착 가능한 탄창 수
    public int currentAmmo; // 현재 장착된 총알 수
    public int maxAmmo; // 최대 장착 가능한 총알 수
    public float attackRate; // 공격 속도
    public float attackDistance; // 공격 사거리
    public float isAutomaticAttack; // 연속 공격 여부
}