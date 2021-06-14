using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Shield : MonoBehaviourPun
{
    public const float ShieldShowTime = 0.4f;
    
    [SerializeField] private Ship _ship;
    [SerializeField] private ShipHitPool _shipHitPool;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private Collider2D _bodyCollider;

    [SerializeField] private SpriteRenderer _shieldHitRenderer;
    [SerializeField] private SpriteRenderer _shieldPermRenderer;

    private Color _defaultShieldColor;
    private float _shieldMultiplier;
    private float _lastHitTime = 0;
    

    private void Start()
    {
        _defaultShieldColor = _shieldPermRenderer.color;
    }

    private void Update()
    {
        _collider.enabled = !_ship.ShieldDisabled;
        _bodyCollider.enabled = _ship.ShieldDisabled;
        _shieldMultiplier = _ship.Shield / Ship.MaxShield;
        _shieldPermRenderer.color = new Color(_defaultShieldColor.r, _defaultShieldColor.g, _defaultShieldColor.b, _defaultShieldColor.a*_shieldMultiplier);

        if (_lastHitTime < ShieldShowTime) _lastHitTime += Time.deltaTime;
        float shieldAlpha = Mathf.Lerp(1, 0, _lastHitTime / ShieldShowTime);
        _shieldHitRenderer.color = new Color(1, 1, 1, shieldAlpha*_shieldMultiplier);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (photonView.IsMine)
        {
            //TODO: Don't allow consecutive hits from same collider
            if (other.otherRigidbody != null)
            {
                _lastHitTime = 0;
                Vector2 direction = other.collider.transform.position - transform.position;
                ShieldHit(other.contacts[0].point, direction);
                _ship.HandleCollision(other.rigidbody.mass, other.relativeVelocity.magnitude);
            }
        }
    }

    public void ShieldHit(Vector2 position, Vector2 direction)
    {
        photonView.RPC("ShieldHitRPC", RpcTarget.All, position, direction);
    }
    
    [PunRPC]
    private void ShieldHitRPC(Vector2 position, Vector2 direction)
    {
        _shipHitPool.Spawn(position, direction);
    }
}
