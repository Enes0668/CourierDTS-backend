namespace CourierDTS.Models
{
    // Jenerik taban: her entity kendi Id tipini (int, long, Guid...) seçebilir.
    public abstract class BaseEntity<TKey>
    {
        public TKey Id { get; set; } = default!;
    }

    // Varsayılan/pratik hâl: aksi belirtilmedikçe entity'ler int Id kullanır.
    // Mevcut kodda hiçbir değişiklik gerektirmeden "Courier : BaseEntity" yazmaya devam edilebiliyor.
    public abstract class BaseEntity : BaseEntity<int>
    {
    }
}
