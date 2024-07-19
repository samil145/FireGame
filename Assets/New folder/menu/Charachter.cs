using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Characters/Character")]
public class Charachter : ScriptableObject
{
    [SerializeField] private int id = -1;
    [SerializeField] private string displayName = "New Display Name";
    [SerializeField] private Sprite icon;
    [SerializeField] private int health;
    [SerializeField] private int damage;
    [SerializeField] private int speed;
    public int Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public int Health => health;
    public int Damage => damage;
    public int Speed => speed;
}