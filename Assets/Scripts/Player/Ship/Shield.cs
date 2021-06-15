using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Shield : MonoBehaviourPun
{
    public const float ShieldShowTime = 0.6f;
    
    [SerializeField] private Ship _ship;
    [SerializeField] private ShipHitPool _shipHitPool;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private Collider2D _bodyCollider;

    [SerializeField] private SpriteRenderer _shieldHitRenderer;
    [SerializeField] private SpriteRenderer _shieldPermRenderer;

    private Color _defaultShieldColor;
    private float _shieldMultiplier;
    private float _lastHitTime = 0;

    private List<RecentlyHitBodys> _recentlyHitBodys = new List<RecentlyHitBodys>();
    private List<RecentlyHitBodys> _toRemove = new List<RecentlyHitBodys>();

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

        foreach (var body in _recentlyHitBodys)
        {
            body.TimeToIgnore -= Time.deltaTime;
            if (body.TimeToIgnore < 0)
            {
                _toRemove.Add(body);
            }
        }

        foreach (var toRemove in _toRemove)
        {
            _recentlyHitBodys.Remove(toRemove);
        }
        _toRemove.Clear();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (photonView.IsMine)
        {
            //TODO: Don't allow consecutive hits from same collider
            Rigidbody2D otherBody = other.rigidbody;
            if (otherBody != null)
            {
                Vector2 direction = other.collider.transform.position - transform.position;
                ShieldHit(other.contacts[0].point, direction);
                
                _lastHitTime = 0;

                if (HasNotCollidedRecently(otherBody))
                {
                    float relativeVelocityMag = other.relativeVelocity.magnitude;
                    _ship.HandleCollision(otherBody.mass, relativeVelocityMag);

                    IHittable hittable = other.transform.GetComponent<IHittable>();
                    if (hittable != null)
                    {
                        hittable.Hit(other.relativeVelocity, other.contacts[0].point,
                            Mathf.Clamp(relativeVelocityMag / 3, 1, 9));
                    }
                    
                    _recentlyHitBodys.Add(new RecentlyHitBodys {Rigidbody = otherBody, TimeToIgnore = 0.3f});
                }
            }
        }
    }

    private bool HasNotCollidedRecently(Rigidbody2D otherBody)
    {
        foreach (var body in _recentlyHitBodys)
        {
            if (body.Rigidbody.Equals(otherBody)) return false;
        }

        return true;
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

public class RecentlyHitBodys
{
    public Rigidbody2D Rigidbody;
    public float TimeToIgnore;
}
