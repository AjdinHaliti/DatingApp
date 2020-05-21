namespace DatingApp.API.Models
{
    public class Like
    {
        //id of the user that is liking another user
        public int LikerId { get; set; }
        //id of the user that is beeing liked by another user
        public int LikeeId { get; set; }
        public virtual User Liker { get; set; }
        public virtual User Likee { get; set; }
    }
}