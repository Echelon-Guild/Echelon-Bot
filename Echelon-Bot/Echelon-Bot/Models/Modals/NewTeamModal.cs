using Discord.Interactions;

namespace EchelonBot.Models.Modals
{
    public class NewTeamModal : IModal
    {
        public string Title => "Create a new team";

        [InputLabel("Name your team")]
        [ModalTextInput("newteam_name")]
        public string Name { get; set; }

        [InputLabel("Describe your team")]
        [ModalTextInput("newteam_description")]
        public string Description { get; set; }
    }
}
