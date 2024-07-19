using Gameplay;
using Unity.Netcode;
using UnityEngine;

public class FireballLogic : NetworkBehaviour
{
    [SerializeField] ParticleSystem fireball;
    [SerializeField] ParticleSystem explosion;
    float speed = 20f;
    private int damage;
    private Belonging belonging;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsClient || !IsOwner)
        {
            enabled = false;
            return;
        }
        fireball.Play();
    }

    void FixedUpdate()
    {
        transform.position += transform.forward * speed * Time.fixedDeltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Teammate teammate) && teammate.Side != this.Side)
        {
            //Should be checked
            Debug.Log("Fireball Damage: " + damage);
            fireball.Stop();
            explosion.Play();
            speed = 0;
            Destroy(gameObject, explosion.main.duration);
        }

    }

    public int Damage { get => damage; set => damage = value; }

    public Belonging Side { get => belonging; set => belonging = value; }
}