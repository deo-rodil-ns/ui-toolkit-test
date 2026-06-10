namespace GothicVampire.Jobs
{
    public interface IAssignee
    {
        public int Tier { get; }
        public Job Job { get; set; }
    }
}