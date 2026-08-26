namespace BoomDog.Data
{
    public enum CardType
    {
        ExplodingDog, // Giống Exploding Kitten
        Defuse,       // Gỡ mìn
        Attack,       // Kết thúc lượt không bốc bài, bắt người sau đi 2 lượt
        Skip,         // Kết thúc lượt không bốc bài
        SeeTheFuture, // Nhìn 3 lá trên cùng
        Shuffle,      // Trộn lại nọc
        Nope,         // Chặn tác dụng lá bài người khác
        Favor,        // Xin bài người khác
        Normal        // Các lá bài thường (thu thập theo bộ)
    }
}
