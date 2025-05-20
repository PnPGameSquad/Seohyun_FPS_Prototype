public enum WeaponName { AssaultRifle = 0 }

[System.Serializable]
public struct WeaponSetting
{
    public WeaponName weaponName; // ���� �̸�
    public int damage; // ���� ���ݷ�
    public int currentMagazine; // ���� ������ źâ ��
    public int maxMagazine; // �ִ� ���� ������ źâ ��
    public int currentAmmo; // ���� ������ �Ѿ� ��
    public int maxAmmo; // �ִ� ���� ������ �Ѿ� ��
    public float attackRate; // ���� �ӵ�
    public float attackDistance; // ���� ��Ÿ�
    public float isAutomaticAttack; // ���� ���� ����
}