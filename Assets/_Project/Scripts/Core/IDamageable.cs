namespace DeadZone.Core
{
    public interface IDamageable
    {
        void TakeDamage(float damage);
        void TakeDamage(DamageInfo info);
    }
}
