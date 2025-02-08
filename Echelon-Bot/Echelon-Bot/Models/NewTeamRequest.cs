using EchelonBot.Models.WoW;

namespace EchelonBot.Models
{
    public class NewTeamRequest
    {
        public Guid Id { get; set; }
        public InstanceType ForInstanceType { get; set; }
    }
}
