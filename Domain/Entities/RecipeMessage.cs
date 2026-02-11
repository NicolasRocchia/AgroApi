namespace APIAgroConnect.Domain.Entities
{
    public class RecipeMessage
    {
        public long Id { get; set; }
        public long RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;

        public long SenderUserId { get; set; }
        public User Sender { get; set; } = null!;

        public string Message { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
