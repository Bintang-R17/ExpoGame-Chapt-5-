/// <summary>
/// Interface untuk semua entity yang bisa menerima damage
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Menerima damage
    /// </summary>
    /// <param name="damage">Jumlah damage yang diterima</param>
    void TakeDamage(float damage);
}
