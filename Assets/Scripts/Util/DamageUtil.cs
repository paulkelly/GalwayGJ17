using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DamageUtil
{
    public const float MinMass = 1;
    public const float MaxMass = 40;
    public const float MinMassDamageMulti = 1;
    public const float MaxMassDamageMulti = 4;

    public const float MinImpactVelocity = 1;
    public const float MaxImpactVelocity = 40;
    public const float MinImpactVelocityDamageMulti = 1;
    public const float MaxImpactVelocityDamageMulti = 4;

    public const float BaseShieldDamage = 5f;

    public static float GetDamage(float mass, float impactVelocity)
    {
        float massDamage =
            Mathf.Lerp(MinMassDamageMulti, MaxMassDamageMulti, Mathf.InverseLerp(MinMass, MaxMass, mass));
        float impactDamage = Mathf.Lerp(MinImpactVelocityDamageMulti, MaxImpactVelocityDamageMulti, Mathf.InverseLerp(MinImpactVelocity, MaxImpactVelocity, impactVelocity));

        return BaseShieldDamage * massDamage * impactDamage;
    }
}
