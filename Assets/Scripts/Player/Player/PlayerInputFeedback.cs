using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputFeedback : MonoBehaviour
{
    [SerializeField] private PlayerSpriteColor _outline1;
    [SerializeField] private PlayerSpriteColor _outline2;
    [SerializeField] private PlayerSpriteColor _stickIndicator;
    // Input parameters
    public bool Moving { get; set; }
    public bool Thrusting { get; set; }
    public bool Shooting { get; set; }

    public void Update()
    {
        if (Thrusting || Shooting)
        {
            if (Thrusting && Shooting)
            {
                _outline1.State = PlayerSpriteColor.SpriteState.Thrusting;
                _outline2.State = PlayerSpriteColor.SpriteState.Shooting;
                _stickIndicator.State = Moving ? PlayerSpriteColor.SpriteState.Thrusting : PlayerSpriteColor.SpriteState.InactiveThrusting;
            }
            else if (Thrusting)
            {
                _outline1.State = PlayerSpriteColor.SpriteState.Thrusting;
                _outline2.State = PlayerSpriteColor.SpriteState.Thrusting;
                _stickIndicator.State = Moving ? PlayerSpriteColor.SpriteState.Thrusting : PlayerSpriteColor.SpriteState.InactiveThrusting;
            }
            else
            {
                _outline1.State = PlayerSpriteColor.SpriteState.Shooting;
                _outline2.State = PlayerSpriteColor.SpriteState.Shooting;
                _stickIndicator.State = Moving ? PlayerSpriteColor.SpriteState.Shooting : PlayerSpriteColor.SpriteState.InactiveShooting;
            }
        }
        else
        {
            _outline1.State = PlayerSpriteColor.SpriteState.Inactive;
            _outline2.State = PlayerSpriteColor.SpriteState.Inactive;
            _stickIndicator.State = Moving ? PlayerSpriteColor.SpriteState.Active : PlayerSpriteColor.SpriteState.Inactive;
        }
    }
}
